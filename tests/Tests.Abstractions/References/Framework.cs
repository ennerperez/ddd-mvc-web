using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions
{
    public static class Extensions
    {
        public static T GetTagValue<T>(this string[] stringArray, string name)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                    return stringArray.GetTagValue<string>(name).AsEnum<T>();
                else
                    return (T)stringArray.GetTagValue<string>(name).AsEnum(Nullable.GetUnderlyingType(type));
            }

            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();
            var result = tags.Any() ? tags.First().Groups[1].Value : null;

            if (isNullable)
            {
                if (string.IsNullOrWhiteSpace(result)) return default(T);
                return (T)Convert.ChangeType(result, conversionType: Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
            }
                
            return (T)Convert.ChangeType(result, type);
        }

        public static IEnumerable<T> GetTagValues<T>(this string[] stringArray, string name)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                    return stringArray.GetTagValues<string>(name)?.Select(m => m.AsEnum<T>()).ToArray();
                else
                    return stringArray.GetTagValues<string>(name)?.Select(m => (T)m.AsEnum(Nullable.GetUnderlyingType(type))).ToArray();
            }

            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).SelectMany(o => o.Groups[1].Value.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator)).ToArray();
            var result = tags.Any()
                ? tags.Select(m =>
                {
                    if (type.IsGenericType && isNullable)
                    {
                        if (string.IsNullOrWhiteSpace(m)) return default(T);
                        return (T)Convert.ChangeType(m, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                    }
                    return (T)Convert.ChangeType(m, type);
                }).ToArray()
                : null;

            return result;
        }
        
        public static T GetValue<T>(this Table table, string key, bool ignoreCase = true)
        {
            var type = typeof(T);
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                    return table.GetValue<string>(key).AsEnum<T>();
                else
                    return (T)table.GetValue<string>(key).AsEnum(Nullable.GetUnderlyingType(type));
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (table.Rows.Any(m => m["Field"].Equals(key, stringComparison)))
            {
                var value = table.Rows.FirstOrDefault(m => m["Field"].Equals(key, stringComparison))?["Value"];

                if (isNullable)
                {
                    if (string.IsNullOrWhiteSpace(value)) return default(T);
                    return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }
                return (T)Convert.ChangeType(value, type);                
            }
            return default(T);
        }

        public static object GetValue(this Table table, string key, Type type, bool ignoreCase = true)
        {
            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition();

            if (type.IsEnum || isNullable && Nullable.GetUnderlyingType(type)!.IsEnum)
            {
                if (!isNullable)
                    return table.GetValue<string>(key).AsEnum(type);
                else
                    return table.GetValue<string>(key).AsEnum(Nullable.GetUnderlyingType(type));
            }

            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (table.Rows.Any(m => m["Field"].Equals(key, stringComparison)))
            {
                var value = table.Rows.FirstOrDefault(m => m["Field"].Equals(key, stringComparison))?["Value"];
                if (isNullable)
                {
                    if (string.IsNullOrWhiteSpace(value)) return default;
                    return Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }

                return Convert.ChangeType(value, type);
            }

            return default;
        }

        public static T CastTo<T>(this Table table, string column = "Field")
        {
            T result = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties().Where(m => m.CanWrite).ToArray();
            string[] fields;
            if (!string.IsNullOrWhiteSpace(column)) fields = table.Rows.Select(m => m[column].ToLowerInvariant()).ToArray();
            else fields = table.Rows.Select(m => m[0].ToLowerInvariant()).ToArray();
            var joint = props.Where(m => fields.Contains(m.Name.ToLowerInvariant())).ToArray();
            foreach (var prop in joint)
            {
                try
                {
                    var value = table.GetValue(prop.Name, prop.PropertyType);
                    if (value == null) continue;

                    if (prop.PropertyType.IsEnum)
                        value = value.ToString().AsEnum(prop.PropertyType);
                    prop.SetValue(result, value);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return result;
        }

        public static DateTime? EvaluateDate(this string @this)
        {
            DateTime? result = null;
            if (!string.IsNullOrWhiteSpace(@this))
            {
                //TODO: Translate keywords
                if (@this.Equals("{Now}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Now;
                else if (@this.Equals("{Today}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today;
                else if (@this.Equals("{Tomorrow}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today.AddDays(1);
                else if (@this.Equals("{Yesterday}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today.AddDays(-1);

                //TODO: Include futures dates
                //TODO: Translate keywords
                var regex = new Regex(@"\{([0-9]{1,})([y,m,d])(Ago)}", RegexOptions.Compiled);
                var match = regex.Match(@this);
                if (match.Success)
                {
                    var amount = int.Parse(match.Groups[1].Value);
                    var interval = match.Groups[2].Value;
                    //var direction = birthMatch.Groups[3].Value;
                    var direction = -1;
                    switch (interval)
                    {
                        case "y":
                            result = DateTime.Now.AddYears(direction * amount);
                            break;
                        case "m":
                            result = DateTime.Now.AddMonths(direction * amount);
                            break;
                        case "d":
                            result = DateTime.Now.AddDays(direction * amount);
                            break;
                    }
                }

                if (result == null)
                {
                    DateTime pasedDate;
                    DateTime.TryParse(@this, out pasedDate);
                    if (pasedDate != DateTime.MinValue) return pasedDate;
                }
            }

            return result;
        }

        public static string EvaluateString(this string @this, int maxLenght = 10, string nullValue = null)
        {
            var result = @this;
            if (!string.IsNullOrWhiteSpace(@this))
            {
                if (@this.Equals("{Random}", StringComparison.InvariantCultureIgnoreCase))
                {
                    var value = Guid.NewGuid().ToString().Replace("-", "");
                    result = value.Substring(0, Math.Min(value.Length, maxLenght));
                }
                else if (@this.Equals("{Guid}", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = Guid.NewGuid().ToString();
                }
            }

            return result ?? nullValue;
        }

        public static string GetAllContextInformation(this IAutomationContext @this)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"AutomationType:\t{@this.AutomationType}");
            sb.AppendLine($"Platform:\t{@this.PlatformTarget}");
            sb.AppendLine($"Application:\t{@this.ApplicationTarget}");
            sb.AppendLine($"Environment:\t{@this.EnvironmentTarget}");

            sb.AppendLine($"TestPlan:\t{@this.TestPlanTarget}");
            sb.AppendLine($"TestSuite:\t{@this.TestSuiteTarget}");
            sb.AppendLine($"TestCase:\t{@this.TestCaseTarget}");

            sb.AppendLine($"Priority:\t{@this.Priority}");
            sb.AppendLine($"IsInitialized:\t{@this.IsInitialized}");

            sb.AppendLine("\r\n\r\nAttribute Library Contents:\r\n");
            foreach (var attributeKey in @this.AttributeLibrary.Keys)
            {
                @this.AttributeLibrary.TryGetValue(attributeKey, out var attributeObject);
                var attributeValue = attributeObject?.ToString();
                sb.AppendLine($"{attributeKey}:\t{attributeValue}");
            }

            return sb.ToString();
        }

        public static object GetAttributeFromAttributeLibrary(this IAutomationContext @this, string attributeKey, bool throwException = true)
        {
            if (@this.AttributeLibrary.TryGetValue(attributeKey, out var attributeObject))
            {
                return attributeObject;
            }
            else if (throwException)
            {
                throw new Exception($"The {attributeKey} Attribute Key is not defined anywhere within the Feature.");
            }
            else
            {
                return null;
            }
        }

        public static void SetAttributeInAttributeLibrary(this IAutomationContext @this, string attributeKey, object attributeObject)
        {
            @this.AttributeLibrary.Remove(attributeKey);
            @this.AttributeLibrary.Add(attributeKey, attributeObject);
        }
    }
}
