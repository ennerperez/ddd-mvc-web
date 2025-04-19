using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Text
{
    public static partial class Extensions
    {
        public static string ToTitleCase(this string @this, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;

            var words = @this.Split([
                "_", " "
            ], StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpper(w[0], culture) + w[1..].ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCase(this string @this, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            var words = @this.Split([
                "_", " "
            ], StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLower(culture);
            var tailWords = words.Skip(1)
                .Select(w => char.ToUpper(w[0], culture) + w[1..].ToLower(culture))
                .ToArray();
            var result = string.Join(string.Empty, tailWords);
            return $"{leadWord}{result}";
        }

        public static string ToTitleCaseInvariant(this string @this)
        {
            var words = @this.Split([
                "_", " "
            ], StringSplitOptions.RemoveEmptyEntries);

            words = words
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, words);
            return result;
        }

        public static string ToCamelCaseInvariant(this string @this)
        {
            var words = @this.Split([
                "_", " "
            ], StringSplitOptions.RemoveEmptyEntries);
            var leadWord = words[0].ToLowerInvariant();
            var tailWords = words.Skip(1)
                .Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant())
                .ToArray();
            var result = string.Join(string.Empty, tailWords);
            return $"{leadWord}{result}";
        }

        public static string ToSentence(this string @this, CultureInfo culture = null)
        {
            culture ??= CultureInfo.CurrentCulture;
            return $"{@this[0].ToString().ToUpper()}{@this[1..].ToLower(culture)}";
        }

        public static string ToSentenceInvariant(this string @this)
        {
            return $"{@this[0].ToString().ToUpperInvariant()}{@this[1..].ToLowerInvariant()}";
        }

        public static string Replace(this string @this, string[] oldValues, string newValues)
        {
            return oldValues.Aggregate(@this, (current, item) => current.Replace(item, newValues));
        }

        [GeneratedRegex("[*'\",_&#^@]", RegexOptions.Compiled)]
        private static partial Regex s_regex1();

        [GeneratedRegex("[ ]", RegexOptions.Compiled)]
        private static partial Regex s_regex2();

        public static string Normalize(this string @this, bool specialsChars = true, bool upperCase = false, bool lowerCase = false)
        {
            if (!specialsChars)
            {
                @this = s_regex1().Replace(@this, string.Empty);
                @this = s_regex2().Replace(@this, replacement: "-");
            }

            if (upperCase && !lowerCase)
            {
                @this = @this.ToUpper();
            }
            else if (lowerCase && !upperCase)
            {
                @this = @this.ToLower();
            }

            return @this;
        }
    }
}
