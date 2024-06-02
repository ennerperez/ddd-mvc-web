using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace System.Text
{
    public static class Extensions
    {
        public static string ToTitleCase(this string @this, CultureInfo culture)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpper(w[0], culture) + w[1..].ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCase(this string @this, CultureInfo culture)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLower(culture);
            var tailwords = words.Skip(1)
                .Select(w => char.ToUpper(w[0], culture) + w[1..].ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, tailwords);
            return $"{leadWord}{result}";
        }

        public static string ToTitleCaseInvariant(this string @this)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCaseInvariant(this string @this)
        {
            var words = @this.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLowerInvariant();
            var tailwords = words.Skip(1)
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, tailwords);
            return $"{leadWord}{result}";
        }

        public static string ToSentence(this string @this) => $"{@this[0].ToString().ToUpper()}{@this[1..].ToLower()}";

        public static string ToSentenceInvariant(this string @this) => $"{@this[0].ToString().ToUpperInvariant()}{@this[1..].ToLowerInvariant()}";

        public static string Replace(this string @this, string[] oldValues, string newValues)
        {
            var result = @this;
            foreach (var item in oldValues)
            {
                result = result.Replace(item, newValues);
            }

            return result;
        }

        public static string Normalize(this string @this, bool specialsChars = true, bool upperCase = false, bool lowerCase = false)
        {
            if (!specialsChars)
            {
                var reg = new Regex("[*'\",_&#^@]", RegexOptions.Compiled);
                @this = reg.Replace(@this, string.Empty);
                reg = new Regex("[ ]");
                @this = reg.Replace(@this, "-");
            }

            if (upperCase)
            {
                @this = @this.ToUpper();
            }
            else if (lowerCase)
            {
                @this = @this.ToLower();
            }

            return @this;
        }
    }
}
