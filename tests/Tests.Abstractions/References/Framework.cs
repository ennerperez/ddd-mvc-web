﻿using System;
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
            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();
            var result = tags.Any() ? tags.First().Groups[1].Value : null;
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
            {
                if (string.IsNullOrWhiteSpace(result)) return default(T);
                return (T)Convert.ChangeType(result, conversionType: Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
            }
                
            return (T)Convert.ChangeType(result, type);
        }

        public static IEnumerable<T> GetTagValues<T>(this string[] stringArray, string name)
        {
            var regEx = new Regex(@$"{name}\((.*)\)", RegexOptions.Compiled);
            var tags = stringArray.Select(t => regEx.Match(t)).Where(p => p.Success).SelectMany(o => o.Groups[1].Value.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator)).ToArray();
            var result = tags.Any()
                ? tags.Select(m =>
                {
                    var type = typeof(T);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
                    {
                        if (string.IsNullOrWhiteSpace(m)) return default(T);
                        return (T)Convert.ChangeType(m, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                    }
                    return (T)Convert.ChangeType(m, type);
                }).ToArray() : null;
            return result;
        }
        
        public static T GetValue<T>(this Table table, string key, bool ignoreCase = true)
        {
            var stringComparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.Ordinal;
            if (table.Rows.Any(m => m["Field"].Equals(key, stringComparison)))
            {
                var value = table.Rows.FirstOrDefault(m => m["Field"].Equals(key, stringComparison))?["Value"];
                var type = typeof(T);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
                {
                    if (string.IsNullOrWhiteSpace(value)) return default(T);
                    return (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? throw new InvalidOperationException());
                }
                return (T)Convert.ChangeType(value, type);                
            }
            return default(T);
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
