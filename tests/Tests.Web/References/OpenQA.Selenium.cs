using System.Reflection;

namespace OpenQA.Selenium
{
    public static class IWebElementExtensions
    {
        public static string GetClass(this IWebElement @this) => @this.GetAttribute("clss");
        public static string GetText(this IWebElement @this) => @this.GetAttribute("text");
        public static bool GetCheckable(this IWebElement @this) => bool.Parse(@this.GetAttribute("checkable"));

        public static bool GetChecked(this IWebElement @this) => int.Parse(@this.GetAttribute("value")) == 1;

        public static bool GetClickable(this IWebElement @this) => bool.Parse(@this.GetAttribute("clickable"));
        public static bool GetFocusable(this IWebElement @this) => bool.Parse(@this.GetAttribute("focusable"));
        public static bool GetLongClickable(this IWebElement @this) => bool.Parse(@this.GetAttribute("long-clickable"));
        public static bool GetPassword(this IWebElement @this) => bool.Parse(@this.GetAttribute("password"));
        public static bool GetScrollable(this IWebElement @this) => bool.Parse(@this.GetAttribute("scrollable"));

        public static string GetAutomationId(this IWebElement @this) => @this.GetAttribute("content-desc");

        public static string GetElementId(this IWebElement @this)
        {
            if (@this == null)
            {
                return string.Empty;
            }

            var field = @this.GetType().GetField("elementId", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                return string.Empty;
            }

            return field.GetValue(@this)?.ToString();
        }

        public static bool IsOptional(this IWebElement @this) => @this.GetElementId().StartsWith("optional_");
    }
}
