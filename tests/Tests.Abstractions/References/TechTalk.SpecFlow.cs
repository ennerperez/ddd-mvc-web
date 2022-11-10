using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TechTalk.SpecFlow
{
	public static class Extensions
	{
		public static T GetTagValue<T>(this FeatureInfo source, string name)
		{
			return source.Tags.GetTagValue<T>(name);
		}

		public static T GetTagValue<T>(this ScenarioInfo source, string name)
		{
			return source.Tags.GetTagValue<T>(name);
		}

		private static T GetTagValue<T>(this string[] stringArray, string name)
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

		public static IEnumerable<T> GetTagValues<T>(this FeatureInfo source, string name)
		{
			return source.Tags.GetTagValues<T>(name);
		}

		public static IEnumerable<T> GetTagValues<T>(this ScenarioInfo source, string name)
		{
			return source.Tags.GetTagValues<T>(name);
		}

		private static IEnumerable<T> GetTagValues<T>(this string[] stringArray, string name)
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

		public static T GetValue<T>(this Table table, string key, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
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
			if (table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
			{
				var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];

				if (isNullable)
				{
					if (string.IsNullOrWhiteSpace(value)) return default(T);
					return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
				}

				return (T)Convert.ChangeType(value, type);
			}

			return default(T);
		}

		public static object GetValue(this Table table, string key, Type type, bool ignoreCase = true, string fieldColum = "Field", string valueColum = "Value")
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
			if (table.Rows.Any(m => m[fieldColum].Equals(key, stringComparison)))
			{
				var value = table.Rows.FirstOrDefault(m => m[fieldColum].Equals(key, stringComparison))?[valueColum];
				if (isNullable)
				{
					if (string.IsNullOrWhiteSpace(value)) return default;
					return Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
				}

				return Convert.ChangeType(value, type);
			}

			return default;
		}

		public static T CastTo<T>(this Table table, string fieldColum = "Field")
		{
			T result = Activator.CreateInstance<T>();
			var props = typeof(T).GetProperties().Where(m => m.CanWrite).ToArray();
			string[] fields;
			if (!string.IsNullOrWhiteSpace(fieldColum)) fields = table.Rows.Select(m => m[fieldColum].ToLowerInvariant()).ToArray();
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

		public static DateTime? EvaluateDate(this string @this, DateTime? nullValue = null)
		{
			DateTime? result = null;
			if (!string.IsNullOrWhiteSpace(@this))
			{
				//TODO: Translate keywords
				if (@this.Equals("{Now}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Now;
				else if (@this.Equals("{Today}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today;
				else if (@this.Equals("{Tomorrow}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today.AddDays(1);
				else if (@this.Equals("{Yesterday}", StringComparison.InvariantCultureIgnoreCase)) return DateTime.Today.AddDays(-1);

				//TODO: Translate keywords
				var regex = new Regex(@"\{([0-9]{1,})([y,m,d])(Ago|Ahead)}", RegexOptions.Compiled);
				var match = regex.Match(@this);
				if (match.Success)
				{
					var amount = int.Parse(match.Groups[1].Value);
					var interval = match.Groups[2].Value;
					var direction = match.Groups[3].Value == "Ago" ? -1 : 1;
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

			if (result != null)
			{
				return (DateTime)result;
			}

			return nullValue;
		}

		public static string EvaluateString(this string @this, int maxLenght = 10, string nullValue = null)
		{
			var result = @this;
			if (string.IsNullOrWhiteSpace(@this)) @this = nullValue;

			if (!string.IsNullOrWhiteSpace(@this))
			{
				var randomRegex = new Regex(@"\{(Random)\:?(\d+)?\}", RegexOptions.Compiled);
				var matchRandomRegex = randomRegex.Match(@this);
				if (matchRandomRegex.Success)
				{
					if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
					{
						maxLenght = int.Parse(matchRandomRegex.Groups[2].Value);
					}
					var value = Guid.NewGuid().ToString().Replace("-", "");
					result = value.Substring(0, Math.Min(value.Length, maxLenght));
					return result;
				}

				var guidRegex = new Regex(@"\{(Guid)\}", RegexOptions.Compiled);
				var matchGuidRegex = guidRegex.Match(@this);
				if (matchGuidRegex.Success)
				{
					result = Guid.NewGuid().ToString();
					return result;
				}

				var rangeRegex = new Regex(@"\{\[(.*)\]\}", RegexOptions.Compiled);
				var rangeRegexMatch = rangeRegex.Match(@this);
				if (rangeRegexMatch.Success)
				{
					var values = rangeRegexMatch.Groups[1].Value.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
					if (values.Length > 1)
					{
						var rnd = new Random();
						var index = rnd.Next(-1, values.Length + 1);
						if (index < 0) index = 0;
						if (index > values.Length - 1) index = values.Length - 1;
						result = values[index];
					}
					else
						result = values[0];
				}
			}

			return result ?? nullValue;
		}
	}
}
