using System.Globalization;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Text
{
    public static class Extensions
    {
        public static string ToTitleCase(this string @this, CultureInfo culture)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpper(w[0], culture) + w.Substring(1).ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCase(this string @this, CultureInfo culture)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLower(culture);
            var tailwords = words.Skip(1)
                .Select(w => char.ToUpper(w[0], culture) + w.Substring(1).ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, tailwords);
            return $"{leadWord}{result}";
        }

        public static string ToTitleCaseInvariant(this string @this)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCaseInvariant(this string @this)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLowerInvariant();
            var tailwords = words.Skip(1)
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, tailwords);
            return $"{leadWord}{result}";
        }

        public static string Replace(this string @this, string[] oldValues, string newValues)
        {
            var result = @this;
            foreach (var item in oldValues)
                result = result.Replace(item, newValues);
            return result;
        }
    }
}
