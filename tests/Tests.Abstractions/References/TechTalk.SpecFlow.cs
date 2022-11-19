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

		public static string Evaluate(this string @this, int maxLenght = 10, string nullValue = "")
		{
			return @this.Evaluate<string>(maxLenght, nullValue);
		}
		public static T Evaluate<T>(this string @this, int maxLenght = 10, T nullValue = default)
		{
			if (string.IsNullOrWhiteSpace(@this)) return nullValue;

			var randomRegex = new Regex(@"\{(Random)\:?(\d+)?\}", RegexOptions.Compiled);
			var matchRandomRegex = randomRegex.Match(@this);

			if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
			{
				object result = null;

				//TODO: Translate keywords
				if (@this.Equals("{Now}", StringComparison.InvariantCultureIgnoreCase))
					return (T)Convert.ChangeType(DateTime.Now, typeof(T));
				else if (@this.Equals("{Today}", StringComparison.InvariantCultureIgnoreCase))
					return (T)Convert.ChangeType(DateTime.Today, typeof(T));
				else if (@this.Equals("{Tomorrow}", StringComparison.InvariantCultureIgnoreCase))
					return (T)Convert.ChangeType(DateTime.Today.AddDays(1), typeof(T));
				else if (@this.Equals("{Yesterday}", StringComparison.InvariantCultureIgnoreCase))
					return (T)Convert.ChangeType(DateTime.Today.AddDays(-1), typeof(T));
				else
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
						switch (interval)
						{
							case "y":
								result = DateTime.Now.AddYears(direction * amount);
								break;
							case "M":
								result = DateTime.Now.AddMonths(direction * amount);
								break;
							case "d":
								result = DateTime.Now.AddDays(direction * amount);
								break;
							case "m":
								result = DateTime.Now.AddMinutes(direction * amount);
								break;
						}
					}
				}

				if (matchRandomRegex.Success)
				{
					var rnd = new Random();
					if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
					{
						//TODO: Translate keywords
						if (matchRandomRegex.Groups[2].Value == "Now")
							result = new DateTime(rnd.NextLong(DateTime.Now.AddDays(-1).Ticks, DateTime.Now.Ticks));
						else if (matchRandomRegex.Groups[2].Value == "Today")
							result = new DateTime(rnd.NextLong(DateTime.Today.Ticks, DateTime.Today.AddDays(1).Ticks));
						else if (matchRandomRegex.Groups[2].Value == "Tomorrow")
							result = new DateTime(rnd.NextLong(DateTime.Today.AddDays(1).Ticks, DateTime.Today.AddDays(2).Ticks));
						else if (matchRandomRegex.Groups[2].Value == "Yesterday")
							result = new DateTime(rnd.NextLong(DateTime.Today.AddDays(-1).Ticks, DateTime.Today.Ticks));
						else
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
								switch (interval)
								{
									case "y":
										lv = DateTime.Now.AddYears(direction * amount);
										break;
									case "M":
										lv = DateTime.Now.AddMonths(direction * amount);
										break;
									case "d":
										lv = DateTime.Now.AddDays(direction * amount);
										break;
									case "m":
										lv = DateTime.Now.AddMinutes(direction * amount);
										break;
								}
								ls = direction == -1 ? DateTime.Now : lv;
								li = direction == -1 ? lv : DateTime.Now;
								result = new DateTime(rnd.NextLong(li.Ticks, ls.Ticks));
							}
						}
					}
					else
						result = new DateTime(rnd.NextLong(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks));
				}

				if (result == null)
				{
					DateTime pasedData;
					DateTime.TryParse(@this, out pasedData);
					if (pasedData != DateTime.MinValue)
						return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				if (result != null)
					return (T)result;
			}
			else if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
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

				if (result == null)
				{
					int pasedData;
					int.TryParse(@this, out pasedData);
					return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				return (T)result;
			}
			else if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
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

				if (result == null)
				{
					long pasedData;
					long.TryParse(@this, out pasedData);
					return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				return (T)result;
			}
			else if (typeof(T) == typeof(short) || typeof(T) == typeof(short?))
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

				if (result == null)
				{
					short pasedData;
					short.TryParse(@this, out pasedData);
					return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				return (T)result;
			}
			else if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
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

				if (result == null)
				{
					decimal pasedData;
					decimal.TryParse(@this, out pasedData);
					return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				return (T)result;
			}
			else if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
			{
				object result = null;
				if (matchRandomRegex.Success)
					result = Guid.NewGuid();

				if (result == null)
				{
					Guid pasedData;
					Guid.TryParse(@this, out pasedData);
					if (pasedData != Guid.Empty)
						return (T)Convert.ChangeType(pasedData, typeof(T));
				}

				if (result != null)
					return (T)result;
			}
			else if (typeof(T) == typeof(string))
			{
				var result = string.Empty;
				if (matchRandomRegex.Success)
				{
					//var maxLenght = 10;
					if (!string.IsNullOrWhiteSpace(matchRandomRegex.Groups[2].Value))
						maxLenght = int.Parse(matchRandomRegex.Groups[2].Value);
					var value = Guid.NewGuid().ToString().Replace("-", "");
					while (value.Length < maxLenght)
						value += Guid.NewGuid().ToString().Replace("-", "");
					result = value.Substring(0, Math.Min(value.Length, maxLenght));
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
							var values = arrayRegexMatch.Groups[1].Value.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
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
				}
				if (result == string.Empty)
					result = @this;

				return (T)Convert.ChangeType(result, typeof(T));
			}
			else if (typeof(T) == typeof(char) || typeof(T) == typeof(char?))
			{
				object result = null;
				if (matchRandomRegex.Success)
				{
					var value = Guid.NewGuid().ToString().Replace("-", "");
					result = value.Substring(0, 1)[0];
				}
				else
				{
					var arrayRegex = new Regex(@"\{(.*\,(?:.*\,?){1,})\}", RegexOptions.Compiled);
					var arrayRegexMatch = arrayRegex.Match(@this);
					if (arrayRegexMatch.Success)
					{
						var values = arrayRegexMatch.Groups[1].Value.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
						if (values.Length > 1)
						{
							var rnd = new Random();
							var index = rnd.Next(-1, values.Length + 1);
							if (index < 0) index = 0;
							if (index > values.Length - 1) index = values.Length - 1;
							result = values[index][0];
						}
						else
							result = values[0][0];
					}
				}

				if (result == null)
					result = (char)Convert.ChangeType(@this, typeof(T));

				return (T)result;
			}

			return nullValue;
		}

	}
}
