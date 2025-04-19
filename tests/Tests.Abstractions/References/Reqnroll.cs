#if USING_REQNROLL
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reqnroll
{
    public static class Extensions
    {
        public static T GetTagValue<T>(this FeatureInfo source, string name) => source.Tags.GetTagValue<T>(name);

        public static T GetTagValue<T>(this ScenarioInfo source, string name) => source.Tags.GetTagValue<T>(name);

        private static T GetTagValue<T>(this string[] stringArray, string name)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || (isNullable && Nullable.GetUnderlyingType(type) is { IsEnum: true }))
            {
                if (!isNullable)
                {
                    return stringArray.GetTagValue<string>(name).AsEnum<T>();
                }

                return (T)stringArray.GetTagValue<string>(name).AsEnum(Nullable.GetUnderlyingType(type));
            }

            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();
            var result = tags.Length != 0 ? tags.First().Groups[1].Value : null;

            if (!isNullable)
            {
                return (T)Convert.ChangeType(result, type);
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                return default;
            }

            return (T)Convert.ChangeType(result, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
        }

        public static IEnumerable<T> GetTagValues<T>(this FeatureInfo source, string name) => source.Tags.GetTagValues<T>(name);

        public static IEnumerable<T> GetTagValues<T>(this ScenarioInfo source, string name) => source.Tags.GetTagValues<T>(name);

        private static T[] GetTagValues<T>(this string[] stringArray, string name)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || (isNullable && Nullable.GetUnderlyingType(type) is { IsEnum: true }))
            {
                return !isNullable ? stringArray.GetTagValues<string>(name)?.Select(m => m.AsEnum<T>()).ToArray() : stringArray.GetTagValues<string>(name)?.Select(m => (T)m.AsEnum(Nullable.GetUnderlyingType(type))).ToArray();
            }

            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).SelectMany(o => o.Groups[1].Value.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator)).ToArray();
            var result = tags.Length != 0
                ? tags.Select(m =>
                {
                    if (!type.IsGenericType || !isNullable)
                    {
                        return (T)Convert.ChangeType(m, type);
                    }

                    if (string.IsNullOrWhiteSpace(m))
                    {
                        return default;
                    }

                    return (T)Convert.ChangeType(m, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }).ToArray()
                : null;

            return result;
        }

        public static T GetValue<T>(this Table table, string key, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || (isNullable && Nullable.GetUnderlyingType(type) is { IsEnum: true }))
            {
                if (!isNullable)
                {
                    return table.GetValue<string>(key).AsEnum<T>();
                }

                return (T)table.GetValue<string>(key).AsEnum(Nullable.GetUnderlyingType(type));
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (!table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
            {
                return default;
            }

            var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];

            if (!isNullable)
            {
                return (T)Convert.ChangeType(value, type);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
        }

        public static object GetValue(this Table table, string key, Type type, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
        {
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || (isNullable && Nullable.GetUnderlyingType(type) is { IsEnum: true }))
            {
                return table.GetValue<string>(key).AsEnum(!isNullable ? type : Nullable.GetUnderlyingType(type));
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (!table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
            {
                return null;
            }


            var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];
            if (isNullable)
            {
                return string.IsNullOrWhiteSpace(value) ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
            }

            return Convert.ChangeType(value, type);
        }

        public static T CastTo<T>(this Table table, string fieldColum = "Field")
        {
            var result = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties().Where(m => m.CanWrite).ToArray();
            string[] fields = !string.IsNullOrWhiteSpace(fieldColum) ? table.Rows.Select(m => m[fieldColum].ToLowerInvariant()).ToArray() : table.Rows.Select(m => m[0].ToLowerInvariant()).ToArray();

            var joint = props.Where(m => fields.Contains(m.Name.ToLowerInvariant())).ToArray();
            foreach (var prop in joint)
            {
                try
                {
                    var value = table.GetValue(prop.Name, prop.PropertyType);
                    if (value == null)
                    {
                        continue;
                    }

                    if (prop.PropertyType.IsEnum)
                    {
                        value = value.ToString().AsEnum(prop.PropertyType);
                    }

                    prop.SetValue(result, value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }

        public static string Evaluate(this string @this, int maxLenght = 10, string nullValue = "") => @this.Evaluate<string>(maxLenght, nullValue);

        public static T Evaluate<T>(this string @this, int maxLenght = 10, T nullValue = default) => (T)@this.Evaluate(typeof(T), maxLenght, nullValue);

        public static object Evaluate(this string @this, Type type, int maxLenght = 10, object nullValue = null)
        {
            if (string.IsNullOrWhiteSpace(@this))
            {
                return nullValue;
            }

            var instanceRegex = new Regex(@"\{(Instance)\}", RegexOptions.Compiled);
            var matchInstanceRegex = instanceRegex.Match(@this);
            if (matchInstanceRegex.Success)
            {
                return Activator.CreateInstance(type);
            }

            var randomRegex = new Regex(@"\{(Random)\:?(\d+)?\}", RegexOptions.Compiled);
            var matchRandomRegex = randomRegex.Match(@this);

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                object result = null;

                //TODO: Translate keywords
                if (@this.Equals("{Now}", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(DateTime.Now, type);
                }

                if (@this.Equals("{Today}", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(DateTime.Today, type);
                }

                if (@this.Equals("{Tomorrow}", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(DateTime.Today.AddDays(1), type);
                }

                if (@this.Equals("{Yesterday}", StringComparison.InvariantCultureIgnoreCase))
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(DateTime.Today.AddDays(-1), type);
                }

                {
                    //TODO: Translate keywords
                    var regex = new Regex(@"\{([0-9]{1,})([y,M,d,m])(Ago|Ahead)}", RegexOptions.Compiled);
                    var match = regex.Match(@this);
                    if (match.Success)
                    {
                        var amount = int.Parse(match.Groups[1].Value);
                        var interval = match.Groups[2].Value;
                        //TODO: Translate keywords
                        var direction = match.Groups[3].Value == "Ago" ? -1 : 1;
                        result = interval switch
                        {
                            "y" => DateTime.Now.AddYears(direction * amount),
                            "M" => DateTime.Now.AddMonths(direction * amount),
                            "d" => DateTime.Now.AddDays(direction * amount),
                            "m" => DateTime.Now.AddMinutes(direction * amount),
                            _ => null
                        };
                    }
                }

                if (matchRandomRegex.Success)
                {
                    var rnd = new Random();
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        switch (matchRandomRegex.Groups[2].Value)
                        {
                            //TODO: Translate keywords
                            case "Now":
                                result = new DateTime(rnd.NextLong(DateTime.Now.AddDays(-1).Ticks, DateTime.Now.Ticks));
                                break;
                            case "Today":
                                result = new DateTime(rnd.NextLong(DateTime.Today.Ticks, DateTime.Today.AddDays(1).Ticks));
                                break;
                            case "Tomorrow":
                                result = new DateTime(rnd.NextLong(DateTime.Today.AddDays(1).Ticks, DateTime.Today.AddDays(2).Ticks));
                                break;
                            case "Yesterday":
                                result = new DateTime(rnd.NextLong(DateTime.Today.AddDays(-1).Ticks, DateTime.Today.Ticks));
                                break;
                            default:
                            {
                                //TODO: Translate keywords
                                var regex = new Regex(@"\{([0-9]{1,})([y,M,d,m])(Ago|Ahead)}", RegexOptions.Compiled);
                                var match = regex.Match(@this);
                                if (match.Success)
                                {
                                    DateTime ls;
                                    DateTime li;
                                    var lv = DateTime.Now;
                                    var amount = int.Parse(match.Groups[1].Value);
                                    var interval = match.Groups[2].Value;
                                    //TODO: Translate keywords
                                    var direction = match.Groups[3].Value == "Ago" ? -1 : 1;
                                    lv = interval switch
                                    {
                                        "y" => DateTime.Now.AddYears(direction * amount),
                                        "M" => DateTime.Now.AddMonths(direction * amount),
                                        "d" => DateTime.Now.AddDays(direction * amount),
                                        "m" => DateTime.Now.AddMinutes(direction * amount),
                                        _ => lv
                                    };

                                    ls = direction == -1 ? DateTime.Now : lv;
                                    li = direction == -1 ? lv : DateTime.Now;
                                    result = new DateTime(rnd.NextLong(li.Ticks, ls.Ticks));
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        result = new DateTime(rnd.NextLong(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks));
                    }
                }

                if (result == null)
                {
                    _ = DateTime.TryParse(@this, out var parsedData);
                    if (parsedData != DateTime.MinValue)
                    {
                        type = Nullable.GetUnderlyingType(type) ?? type;
                        return Convert.ChangeType(parsedData, type);
                    }
                }

                if (result != null)
                {
                    return result;
                }
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    var rnd = new Random();
                    var li = 0;
                    var ls = int.MaxValue;
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        var maxValue = int.Parse(matchRandomRegex.Groups[2].Value);
                        if (maxValue < 0)
                        {
                            li = int.MinValue;
                            ls = 0;
                        }
                    }

                    result = rnd.Next(li, ls);
                }
                else
                {
                    var rangeRegex = new Regex(@"\{(.*)\-(.*)\}", RegexOptions.Compiled);
                    var rangeRegexMatch = rangeRegex.Match(@this);
                    if (rangeRegexMatch.Success)
                    {
                        var rnd = new Random();
                        var li = int.Parse(rangeRegexMatch.Groups[1].Value);
                        var ls = int.Parse(rangeRegexMatch.Groups[2].Value);
                        result = rnd.Next(li, ls);
                    }
                }

                if (result != null)
                {
                    return result;
                }

                _ = int.TryParse(@this, out var parsedData);
                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(parsedData, type);
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    var rnd = new Random();
                    long li = 0;
                    var ls = long.MaxValue;
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        var maxValue = long.Parse(matchRandomRegex.Groups[2].Value);
                        if (maxValue < 0)
                        {
                            li = long.MinValue;
                            ls = 0;
                        }
                    }

                    result = rnd.NextLong(li, ls);
                }
                else
                {
                    var rangeRegex = new Regex(@"\{(.*)\-(.*)\}", RegexOptions.Compiled);
                    var rangeRegexMatch = rangeRegex.Match(@this);
                    if (rangeRegexMatch.Success)
                    {
                        var rnd = new Random();
                        var li = long.Parse(rangeRegexMatch.Groups[1].Value);
                        var ls = long.Parse(rangeRegexMatch.Groups[2].Value);
                        result = rnd.NextLong(li, ls);
                    }
                }

                if (result != null)
                {
                    return result;
                }

                _ = long.TryParse(@this, out var parsedData);
                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(parsedData, type);
            }
            else if (type == typeof(short) || type == typeof(short?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    var rnd = new Random();
                    short li = 0;
                    var ls = short.MaxValue;
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        var maxValue = short.Parse(matchRandomRegex.Groups[2].Value);
                        if (maxValue < 0)
                        {
                            li = short.MinValue;
                            ls = 0;
                        }
                    }

                    result = rnd.NextShort(li, ls);
                }
                else
                {
                    var rangeRegex = new Regex(@"\{(.*)\-(.*)\}", RegexOptions.Compiled);
                    var rangeRegexMatch = rangeRegex.Match(@this);
                    if (rangeRegexMatch.Success)
                    {
                        var rnd = new Random();
                        var li = short.Parse(rangeRegexMatch.Groups[1].Value);
                        var ls = short.Parse(rangeRegexMatch.Groups[2].Value);
                        result = rnd.NextShort(li, ls);
                    }
                }

                if (result != null)
                {
                    return result;
                }

                _ = short.TryParse(@this, out var parsedData);
                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(parsedData, type);
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    var rnd = new Random();
                    decimal li = 0;
                    var ls = decimal.MaxValue;
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        var maxValue = decimal.Parse(matchRandomRegex.Groups[2].Value);
                        if (maxValue < 0)
                        {
                            li = decimal.MinValue;
                            ls = 0;
                        }
                    }

                    result = rnd.NextDecimal(li, ls);
                }
                else
                {
                    var rangeRegex = new Regex(@"\{(.*)\-(.*)\}", RegexOptions.Compiled);
                    var rangeRegexMatch = rangeRegex.Match(@this);
                    if (rangeRegexMatch.Success)
                    {
                        var rnd = new Random();
                        var li = decimal.Parse(rangeRegexMatch.Groups[1].Value);
                        var ls = decimal.Parse(rangeRegexMatch.Groups[2].Value);
                        result = rnd.NextDecimal(li, ls);
                    }
                }

                if (result != null)
                {
                    return result;
                }

                _ = decimal.TryParse(@this, out var parsedData);
                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(parsedData, type);
            }
            else if (type == typeof(Guid) || type == typeof(Guid?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    result = Guid.NewGuid();
                }

                if (result == null)
                {
                    _ = Guid.TryParse(@this, out var parsedData);
                    if (parsedData != Guid.Empty)
                    {
                        type = Nullable.GetUnderlyingType(type) ?? type;
                        return Convert.ChangeType(parsedData, type);
                    }
                }

                if (result != null)
                {
                    return result;
                }
            }
            else if (type == typeof(string))
            {
                var result = string.Empty;
                if (matchRandomRegex.Success)
                {
                    //var maxLenght = 10;
                    if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
                    {
                        maxLenght = int.Parse(matchRandomRegex.Groups[2].Value);
                    }

                    var value = Guid.NewGuid().ToString().Replace("-", "");
                    while (value.Length < maxLenght)
                    {
                        value += Guid.NewGuid().ToString().Replace("-", "");
                    }

                    result = value[..Math.Min(value.Length, maxLenght)];
                }
                else
                {
                    var guidRegex = new Regex(@"\{(Guid)\}", RegexOptions.Compiled);
                    var matchGuidRegex = guidRegex.Match(@this);
                    if (matchGuidRegex.Success)
                    {
                        result = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        var arrayRegex = new Regex(@"\{(.*\,(?:.*\,?){1,})\}", RegexOptions.Compiled);
                        var arrayRegexMatch = arrayRegex.Match(@this);
                        if (arrayRegexMatch.Success)
                        {
                            var values = arrayRegexMatch.Groups[1].Value.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                            if (values.Length > 1)
                            {
                                var rnd = new Random();
                                var index = rnd.Next(-1, values.Length + 1);
                                if (index < 0)
                                {
                                    index = 0;
                                }

                                if (index > values.Length - 1)
                                {
                                    index = values.Length - 1;
                                }

                                result = values[index];
                            }
                            else
                            {
                                result = values[0];
                            }
                        }
                    }
                }

                if (result == string.Empty)
                {
                    result = @this;
                }

                type = Nullable.GetUnderlyingType(type) ?? type;
                return Convert.ChangeType(result, type);
            }
            else if (type == typeof(char) || type == typeof(char?))
            {
                object result = null;
                if (matchRandomRegex.Success)
                {
                    var value = Guid.NewGuid().ToString().Replace("-", "");
                    result = value[..1][0];
                }
                else
                {
                    var arrayRegex = new Regex(@"\{(.*\,(?:.*\,?){1,})\}", RegexOptions.Compiled);
                    var arrayRegexMatch = arrayRegex.Match(@this);
                    if (arrayRegexMatch.Success)
                    {
                        var values = arrayRegexMatch.Groups[1].Value.Split(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
                        if (values.Length > 1)
                        {
                            var rnd = new Random();
                            var index = rnd.Next(-1, values.Length + 1);
                            if (index < 0)
                            {
                                index = 0;
                            }

                            if (index > values.Length - 1)
                            {
                                index = values.Length - 1;
                            }

                            result = values[index][0];
                        }
                        else
                        {
                            result = values[0][0];
                        }
                    }
                }

                type = Nullable.GetUnderlyingType(type) ?? type;
                result ??= (char)Convert.ChangeType(@this, type);

                return result;
            }

            return nullValue;
        }
    }
}
#endif
