using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using Tests.Web;

#if XUNIT
namespace Xunit.Framework
{
    internal static class Assert
    {
        public static void Fail(string message = "")
        {
            Xunit.Assert.True(false, message);
        }

        public static void Pass(string message = "")
        {
            Xunit.Assert.True(true, message);
        }
    }
}
#elif SPECRUN
namespace SpecRunner.Framework
{
    internal static class Assert
    {
        public static void Fail(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(false, message);
        }
        public static void Pass(string message = "") 
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true, message);
        }
    }
}
#endif

// ReSharper disable once CheckNamespace
namespace Tests
{
    [DebuggerStepThrough]
    public static class Extensions
    {
        private static string AccessibilityId => Program.SpecFlowConfiguration.AccessibilityTag ?? "data-accessibility-id";

        public static IWebElement FindElementBy(this IWebDriver @this, By by) => @this.FindElement(by);
        public static IWebElement FindElementByAccessibilityId(this IWebDriver @this, string selector) => @this.FindElement(By.CssSelector($"[{AccessibilityId}='{selector}']"));
        public static IWebElement FindElementByClassName(this IWebDriver @this, string className) => @this.FindElement(By.ClassName(className));
        public static IWebElement FindElementByCssSelector(this IWebDriver @this, string cssSelector) => @this.FindElement(By.CssSelector(cssSelector));
        public static IWebElement FindElementById(this IWebDriver @this, string id) => @this.FindElement(By.Id(id));

        public static IWebElement FindElementByLinkText(this IWebDriver @this, string linkText) => @this.FindElement(By.LinkText(linkText));
        public static IWebElement FindElementByName(this IWebDriver @this, string name) => @this.FindElement(By.Name(name));
        public static IWebElement FindElementByPartialLinkText(this IWebDriver @this, string partialLinkText) => @this.FindElement(By.PartialLinkText(partialLinkText));
        public static IWebElement FindElementByTagName(this IWebDriver @this, string tagName) => @this.FindElement(By.TagName(tagName));
        public static IWebElement FindElementByXPath(this IWebDriver @this, string xpath) => @this.FindElement(By.XPath(xpath));

        public static IReadOnlyCollection<IWebElement> FindElementsBy(this IWebDriver @this, By by) => @this.FindElements(by);
        public static IReadOnlyCollection<IWebElement> FindElementsByAccessibilityId(this IWebDriver @this, string selector) => @this.FindElements(By.CssSelector($"[{AccessibilityId}='{selector}']"));
        public static IReadOnlyCollection<IWebElement> FindElementsByClassName(this IWebDriver @this, string className) => @this.FindElements(By.ClassName(className));
        public static IReadOnlyCollection<IWebElement> FindElementsByCssSelector(this IWebDriver @this, string cssSelector) => @this.FindElements(By.CssSelector(cssSelector));
        public static IReadOnlyCollection<IWebElement> FindElementsById(this IWebDriver @this, string id) => @this.FindElements(By.Id(id));

        public static IReadOnlyCollection<IWebElement> FindElementsByLinkText(this IWebDriver @this, string linkText) => @this.FindElements(By.LinkText(linkText));
        public static IReadOnlyCollection<IWebElement> FindElementsByName(this IWebDriver @this, string name) => @this.FindElements(By.Name(name));
        public static IReadOnlyCollection<IWebElement> FindElementsByPartialLinkText(this IWebDriver @this, string partialLinkText) => @this.FindElements(By.PartialLinkText(partialLinkText));
        public static IReadOnlyCollection<IWebElement> FindElementsByTagName(this IWebDriver @this, string tagName) => @this.FindElements(By.TagName(tagName));
        public static IReadOnlyCollection<IWebElement> FindElementsByXPath(this IWebDriver @this, string xpath) => @this.FindElements(By.XPath(xpath));

        public static bool WaitForElementBy(this IWebDriver @this, By by, int timeout = 3000, int attempts = 5)
        {
            var i = 0;
            var found = false;
            while (!found)
            {
                try
                {
                    found = @this.FindElement(by) != null;
                }
                catch (Exception)
                {
                    // ignore
                }

                i++;
                if (i >= attempts)
                    break;
                Thread.Sleep(timeout);
            }

            return found;
        }

        public static bool WaitForElementByAccessibilityId(this IWebDriver @this, string selector, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.CssSelector($"[{AccessibilityId}='{selector}']"), timeout, attempts);
        public static bool WaitForElementByClassName(this IWebDriver @this, string className, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.ClassName(className), timeout, attempts);
        public static bool WaitForElementByCssSelector(this IWebDriver @this, string cssSelector, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.CssSelector(cssSelector), timeout, attempts);
        public static bool WaitForElementById(this IWebDriver @this, string id, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.Id(id), timeout, attempts);

        public static bool WaitForElementByLinkText(this IWebDriver @this, string linkText, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.LinkText(linkText), timeout, attempts);
        public static bool WaitForElementByName(this IWebDriver @this, string name, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.Name(name), timeout, attempts);
        public static bool WaitForElementByPartialLinkText(this IWebDriver @this, string partialLinkText, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.PartialLinkText(partialLinkText), timeout, attempts);
        public static bool WaitForElementByTagName(this IWebDriver @this, string tagName, int timeout = 3000, int attempts = 5) => @this.WaitForElementBy(By.TagName(tagName), timeout, attempts);

        public static bool WaitForElementsBy(this IWebDriver @this, By by, int amount = 1, int timeout = 3000, int attempts = 5)
        {
            var i = 0;
            var found = false;
            while (!found)
            {
                try
                {
                    var elements = @this.FindElements(by);
                    found = elements != null && elements.Count() == amount;
                }
                catch (Exception)
                {
                    // ignore
                }

                i++;
                if (i >= attempts)
                    break;
                Thread.Sleep(timeout);
            }

            return found;
        }

        public static bool WaitForElementsByAccessibilityId(this IWebDriver @this, string selector, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.CssSelector($"[{AccessibilityId}='{selector}']"), amount, timeout, attempts);
        public static bool WaitForElementsByClassName(this IWebDriver @this, string className, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.ClassName(className), amount, timeout, attempts);
        public static bool WaitForElementsByCssSelector(this IWebDriver @this, string cssSelector, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.CssSelector(cssSelector), amount, timeout, attempts);
        public static bool WaitForElementsById(this IWebDriver @this, string id, int timeout = 3000, int amount = 1, int attempts = 5) => @this.WaitForElementsBy(By.Id(id), amount, timeout, attempts);

        public static bool WaitForElementsByLinkText(this IWebDriver @this, string linkText, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.LinkText(linkText), amount, timeout, attempts);
        public static bool WaitForElementsByName(this IWebDriver @this, string name, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.Name(name), amount, timeout, attempts);
        public static bool WaitForElementsByPartialLinkText(this IWebDriver @this, string partialLinkText, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.PartialLinkText(partialLinkText), amount, timeout, attempts);
        public static bool WaitForElementsByTagName(this IWebDriver @this, string tagName, int amount = 1, int timeout = 3000, int attempts = 5) => @this.WaitForElementsBy(By.TagName(tagName), amount, timeout, attempts);

        private static long s_counter = 1;

        public static void SaveScreenshot(this IWebDriver @this, string path)
        {
            var screenshot = @this.TakeScreenshot();
            var directory = Path.GetDirectoryName(path) ?? "";
            var fileName = Path.GetFileName(path);
            path = Path.Combine(directory, $"{s_counter}_{fileName}");
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
            if (screenshot != null) screenshot.SaveAsFile(path);
            s_counter++;
        }
    }
}
