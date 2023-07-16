using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Tests.Abstractions.Entities;
using Tests.Abstractions.Enums;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Services
{
    public class DefinitionService : IDefinitionService<IWebElement>, IDisposable
    {
        private readonly IAutomationContext _automationContext;
        private readonly IConfigurationRoot _globalConfiguration;

        private Definition GlobalDefinitions { get; }
        private Definition ScenarioDefinitions { get; }
        private Definition PageDefinitions { get; set; }

        private Definition[] Definitions => new[] { GlobalDefinitions, ScenarioDefinitions, PageDefinitions }.Where(m => m != null).ToArray();

        internal static IWebDriver Driver => Program.Driver;
        internal IEnumerable<MethodInfo> _methods;

        private readonly IDefinitionConfiguration _definitionConfiguration;

        public DefinitionService(IAutomationContext automationContext, IDefinitionConfiguration definitionConfiguration)
        {
            _automationContext = automationContext;
            _definitionConfiguration = definitionConfiguration;

            var basePath = Program.GetCurrentDirectory();
            var definitionsDirectory = _definitionConfiguration.GetApplicationDefinitionsLocation();
            if (!string.IsNullOrWhiteSpace(_automationContext.ApplicationTarget))
            {
                foreach (var item in _automationContext.ApplicationTarget.Split("-"))
                {
                    definitionsDirectory = Path.Combine(definitionsDirectory, item);
                }
            }

            /* GLOBALS */

            var basePageFile = Path.Combine(basePath, definitionsDirectory, "Global.ini");

            _globalConfiguration = new ConfigurationBuilder()
                .AddIniFile(basePageFile, false, true)
                .Build();

            GlobalDefinitions = new Definition("Global", "Global");
            _globalConfiguration.Bind(GlobalDefinitions);
            GlobalDefinitions.PrepareAndValidate(_definitionConfiguration);

            Console.WriteLine($@"DEFINITION_FILE[ Global.ini ]DEFINITION_FILE");

            ScenarioDefinitions = GetCurrentScenarioDefinitions();

            _methods = Driver.GetType().GetInterface(nameof(IWebDriver)).GetExtensionMethods(GetType().Assembly).ToArray();
        }

        public bool ValidateAllUniqueElements(params Definition[] definitions)
        {
            var source = definitions.SelectMany(m => m.UniqueElements).ToArray();
            foreach (var item in source)
            {
                var element = TryFindElement(item.Key);
                if (element == null)
                {
                    return false;
                }
            }

            return true;
        }

        #region Privates

        public KeyValuePair<SelectBy, string> GetStrategy(string selector, string token = "", string name = "")
        {
            var result = new KeyValuePair<SelectBy, string>(SelectBy.Id, selector);

            var key = selector;

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(name))
            {
                key = name;
            }

            key = key.Replace(new[] { "&", "-", " ", "'" }, "");

            if (selector.StartsWith(_definitionConfiguration.ElementUniquenessIdentifier))
            {
                key = selector;
            }

            var sources = Definitions;
            foreach (var source in sources)
            {
                if (source.Ids.TryGetValue(key, out var id))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.Id, id);
                }
                else if (source.AutomationIds.TryGetValue(key, out var automationId))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.AccessibilityId, automationId);
                }
                else if (source.AutomationNames.TryGetValue(key, out var automationName))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.Name, automationName);
                }
                else if (source.AutomationCSSes.TryGetValue(key, out var se))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.CssSelector, se);
                }
                else if (source.AutomationXPaths.TryGetValue(key, out var path))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.XPath, path);
                }
                else if (source.AutomationClassNames.TryGetValue(key, out var className))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.ClassName, className);
                }
                else if (source.AutomationLinkTexts.TryGetValue(key, out var text))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.LinkText, text);
                }
                else if (source.AutomationPartialLinkTexts.TryGetValue(key, out var linkText))
                {
                    result = new KeyValuePair<SelectBy, string>(SelectBy.PartialLinkText, linkText);
                }
            }

            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(name))
            {
                var value = result.Value.Replace(token, selector);
                result = new KeyValuePair<SelectBy, string>(result.Key, value);
            }

            if (result.Value.Contains(@"@automation-id"))
            {
                var value = result.Value;
                result = new KeyValuePair<SelectBy, string>(result.Key, value);
            }

            Console.WriteLine($@"STRATEGY[ {selector} | {result.Key} | {result.Value} ]STRATEGY");

            return result;
        }

        private T InvokeMethod<T>(KeyValuePair<SelectBy, string> location, [CallerMemberName] string method = "", params object[] values)
        {
            method = method.Replace("Dynamic", "");
            var methodName = $"{method}By{Enum.GetName(location.Key)}";
            var methodInfo = (_methods.FirstOrDefault(m => m.Name == methodName) ?? Driver.GetType().GetMethods().FirstOrDefault(m => m.Name == methodName)) ?? throw new NotImplementedException(methodName);
            var value = location.Value;
            var valuesList = new List<object>(values);
            valuesList.Insert(0, Driver);
            valuesList.Insert(1, value);
            var result = (T)methodInfo.Invoke(Driver, valuesList.ToArray());

            return result;
        }

        #endregion

        // FindElement

        public IWebElement FindElement(string selector, bool waitForElementToBeDisplayed = false)
        {
            IWebElement element = null;
            var explicitElementWaitTime = (60 / _definitionConfiguration.ElementSearchRetryFactor) * 1000;
            var currentWait = 0;
            var location = GetStrategy(selector);

            var isElementLoadAwaitable = Definitions.SelectMany(m => m.MustAwaitElements).Any(m => m.Key == selector);

            if (isElementLoadAwaitable)
            {
                long elapsedMilliseconds = 0;
                var stopWatch = Stopwatch.StartNew();
                while (currentWait <= _definitionConfiguration.MaxSlowLoadingElementWaitTime)
                {
                    stopWatch.Restart();
                    try
                    {
                        element = InvokeMethod<IWebElement>(location);
                        if (waitForElementToBeDisplayed && element != null && element.Displayed)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        element = null;
                    }

                    if (_definitionConfiguration.ImplicitElementWaitTime == 0)
                    {
                        Thread.Sleep(explicitElementWaitTime);
                        currentWait += explicitElementWaitTime;
                    }
                    else
                    {
                        var maxImplicitWaitTimeInMilliseconds = _definitionConfiguration.ImplicitElementWaitTime;
                        if (stopWatch.ElapsedMilliseconds < _definitionConfiguration.ImplicitElementWaitTime)
                        {
                            var extraWaitTimeNeeded = maxImplicitWaitTimeInMilliseconds - (int)stopWatch.ElapsedMilliseconds;
                            if (extraWaitTimeNeeded > 0)
                            {
                                Thread.Sleep(extraWaitTimeNeeded);
                            }
                        }

                        currentWait += _definitionConfiguration.ImplicitElementWaitTime;
                    }

                    stopWatch.Stop();
                    elapsedMilliseconds += stopWatch.ElapsedMilliseconds;
                }

                if (elapsedMilliseconds > 0)
                {
                    Console.WriteLine($@"Spent a total of [{elapsedMilliseconds / 1000}] second(s) waiting to find Element '{selector}'");
                }
            }
            else
            {
                try
                {
                    element = InvokeMethod<IWebElement>(location);
                }
                catch (Exception)
                {
                    element = null;
                }
            }

            if (element == null && IsOptional(selector))
            {
                element = new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}");
            }

            return element;
        }

        public IWebElement FindDynamicElement(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false)
        {
            IWebElement element = null;
            var explicitElementWaitTime = 60 / _definitionConfiguration.ElementSearchRetryFactor;
            var currentWait = 0;

            var dynamicName = $"DynamicallyNamed{type.ToCamelCaseInvariant()}";
            var location = GetStrategy(selector, token, dynamicName);

            var isElementLoadAwaitable = Definitions.SelectMany(m => m.MustAwaitElements).Any(m => m.Key == selector);

            if (isElementLoadAwaitable)
            {
                long elapsedMilliseconds = 0;
                var stopWatch = Stopwatch.StartNew();
                while (currentWait <= _definitionConfiguration.MaxSlowLoadingElementWaitTime)
                {
                    stopWatch.Restart();
                    try
                    {
                        element = InvokeMethod<IWebElement>(location);
                        if (waitForElementToBeDisplayed && element != null && element.Displayed)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        element = null;
                    }

                    if (_definitionConfiguration.ImplicitElementWaitTime == 0)
                    {
                        Thread.Sleep(explicitElementWaitTime);
                        currentWait += explicitElementWaitTime;
                    }
                    else
                    {
                        var maxImplicitWaitTimeInMilliseconds = _definitionConfiguration.ImplicitElementWaitTime;
                        if (stopWatch.ElapsedMilliseconds < _definitionConfiguration.ImplicitElementWaitTime)
                        {
                            var extraWaitTimeNeeded = maxImplicitWaitTimeInMilliseconds - (int)stopWatch.ElapsedMilliseconds;
                            if (extraWaitTimeNeeded > 0)
                            {
                                Thread.Sleep(extraWaitTimeNeeded);
                            }
                        }

                        currentWait += _definitionConfiguration.ImplicitElementWaitTime;
                    }

                    stopWatch.Stop();
                    elapsedMilliseconds += stopWatch.ElapsedMilliseconds;
                }

                if (elapsedMilliseconds > 0)
                {
                    Console.WriteLine($@"Spent a total of [{elapsedMilliseconds / 1000}] second(s) waiting to find Element '{selector}'");
                }
            }
            else
            {
                try
                {
                    element = InvokeMethod<IWebElement>(location);
                }
                catch (NoSuchElementException)
                {
                    element = null;
                }
            }

            if (element == null && IsOptional(selector))
            {
                element = new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}");
            }

            return element;
        }

        // TryFindElement

        public IWebElement TryFindElement(string selector, int timeout = 1500, int maxAttempts = 5)
        {
            try
            {
                IWebElement element = null;

                var attempts = 0;
                while (element == null)
                {
                    if (attempts >= maxAttempts)
                    {
                        break;
                    }

                    try
                    {
                        element = FindElement(selector, true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (element == null)
                    {
                        Thread.Sleep(timeout);
                    }

                    if (maxAttempts > -1)
                    {
                        attempts++;
                    }
                }

                if (element == null && IsOptional(selector))
                {
                    element = new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}");
                }

                if (element == null)
                {
                    throw new NullReferenceException($"Element [{selector}] was not found after {attempts} attempts");
                }

                return element;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return null;
        }

        public IWebElement TryFindDynamicElement(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5)
        {
            try
            {
                IWebElement element = null;

                var attempts = 0;
                while (element == null)
                {
                    if (attempts >= maxAttempts)
                    {
                        break;
                    }

                    try
                    {
                        element = FindDynamicElement(selector, token, type, true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (element == null)
                    {
                        Thread.Sleep(timeout);
                    }

                    if (maxAttempts > -1)
                    {
                        attempts++;
                    }
                }

                if (element == null && IsOptional(selector))
                {
                    element = new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}");
                }

                if (element == null)
                {
                    throw new NullReferenceException($"Element [{selector}] was not found after {attempts} attempts");
                }

                return element;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return null;
        }

        // FindElements

        public IReadOnlyCollection<IWebElement> FindElements(string selector, bool waitForElementToBeDisplayed = false)
        {
            IReadOnlyCollection<IWebElement> elements = null;
            var explicitElementWaitTime = 60 / _definitionConfiguration.ElementSearchRetryFactor;
            var currentWait = 0;
            var location = GetStrategy(selector);

            var isElementLoadAwaitable = Definitions.SelectMany(m => m.MustAwaitElements).Any(m => m.Key == selector);

            if (isElementLoadAwaitable)
            {
                long elapsedMilliseconds = 0;
                var stopWatch = Stopwatch.StartNew();
                while (currentWait <= _definitionConfiguration.MaxSlowLoadingElementWaitTime)
                {
                    stopWatch.Restart();
                    try
                    {
                        elements = InvokeMethod<IReadOnlyCollection<IWebElement>>(location);
                        if (waitForElementToBeDisplayed && elements != null && elements.Any() && elements.All(m => m.Displayed))
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        elements = null;
                    }

                    if (_definitionConfiguration.ImplicitElementWaitTime == 0)
                    {
                        Thread.Sleep(explicitElementWaitTime);
                        currentWait += explicitElementWaitTime;
                    }
                    else
                    {
                        var maxImplicitWaitTimeInMilliseconds = _definitionConfiguration.ImplicitElementWaitTime;
                        if (stopWatch.ElapsedMilliseconds < _definitionConfiguration.ImplicitElementWaitTime)
                        {
                            var extraWaitTimeNeeded = maxImplicitWaitTimeInMilliseconds - (int)stopWatch.ElapsedMilliseconds;
                            if (extraWaitTimeNeeded > 0)
                            {
                                Thread.Sleep(extraWaitTimeNeeded);
                            }
                        }

                        currentWait += _definitionConfiguration.ImplicitElementWaitTime;
                    }

                    stopWatch.Stop();
                    elapsedMilliseconds += stopWatch.ElapsedMilliseconds;
                }

                if (elapsedMilliseconds > 0)
                {
                    Console.WriteLine($@"Spent a total of [{elapsedMilliseconds / 1000}] second(s) waiting to find Element '{selector}'");
                }
            }
            else
            {
                try
                {
                    elements = InvokeMethod<IReadOnlyCollection<IWebElement>>(location);
                }
                catch (NoSuchElementException)
                {
                    elements = null;
                }
            }

            if ((elements == null || !elements.Any()) && IsOptional(selector))
            {
                elements = new[] { new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}") };
            }

            return elements;
        }

        public IReadOnlyCollection<IWebElement> FindDynamicElements(string selector, string token, string type = "item", bool waitForElementToBeDisplayed = false)
        {
            IReadOnlyCollection<IWebElement> elements = null;
            var explicitElementWaitTime = 60 / _definitionConfiguration.ElementSearchRetryFactor;
            var currentWait = 0;
            var dynamicName = $"DynamicallyNamed{type.ToCamelCaseInvariant()}";
            var location = GetStrategy(selector, token, dynamicName);

            var isElementLoadAwaitable = Definitions.SelectMany(m => m.MustAwaitElements).Any(m => m.Key == selector);

            if (isElementLoadAwaitable)
            {
                long elapsedMilliseconds = 0;
                var stopWatch = Stopwatch.StartNew();
                while (currentWait <= _definitionConfiguration.MaxSlowLoadingElementWaitTime)
                {
                    stopWatch.Restart();
                    try
                    {
                        elements = InvokeMethod<IReadOnlyCollection<IWebElement>>(location);
                        if (waitForElementToBeDisplayed && elements != null && elements.Any() && elements.All(m => m.Displayed))
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        elements = null;
                    }

                    if (_definitionConfiguration.ImplicitElementWaitTime == 0)
                    {
                        Thread.Sleep(explicitElementWaitTime);
                        currentWait += explicitElementWaitTime;
                    }
                    else
                    {
                        var maxImplicitWaitTimeInMilliseconds = _definitionConfiguration.ImplicitElementWaitTime;
                        if (stopWatch.ElapsedMilliseconds < _definitionConfiguration.ImplicitElementWaitTime)
                        {
                            var extraWaitTimeNeeded = maxImplicitWaitTimeInMilliseconds - (int)stopWatch.ElapsedMilliseconds;
                            if (extraWaitTimeNeeded > 0)
                            {
                                Thread.Sleep(extraWaitTimeNeeded);
                            }
                        }

                        currentWait += _definitionConfiguration.ImplicitElementWaitTime;
                    }

                    stopWatch.Stop();
                    elapsedMilliseconds += stopWatch.ElapsedMilliseconds;
                }

                if (elapsedMilliseconds > 0)
                {
                    Console.WriteLine($@"Spent a total of [{elapsedMilliseconds / 1000}] second(s) waiting to find Element '{selector}'");
                }
            }
            else
            {
                try
                {
                    elements = InvokeMethod<IReadOnlyCollection<IWebElement>>(location);
                }
                catch (NoSuchElementException)
                {
                    elements = null;
                }
            }

            if ((elements == null || !elements.Any()) && IsOptional(selector))
            {
                elements = new[] { new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}") };
            }

            return elements;
        }

        // TryFindElements

        public IReadOnlyCollection<IWebElement> TryFindElements(string selector, int timeout = 1500, int maxAttempts = 5)
        {
            try
            {
                IReadOnlyCollection<IWebElement> elements = null;

                var attempts = 0;
                while (elements == null)
                {
                    if (attempts >= maxAttempts)
                    {
                        break;
                    }

                    try
                    {
                        elements = FindElements(selector, true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (elements == null)
                    {
                        Thread.Sleep(timeout);
                    }

                    if (maxAttempts > -1)
                    {
                        attempts++;
                    }
                }

                if ((elements == null || !elements.Any()) && IsOptional(selector))
                {
                    elements = new[] { new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}") };
                }

                if (elements == null)
                {
                    throw new NullReferenceException($"Elements [{selector}] was not found after {attempts} attempts");
                }

                return elements;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return null;
        }

        public IReadOnlyCollection<IWebElement> TryFindDynamicElements(string selector, string token, string type = "item", int timeout = 1500, int maxAttempts = 5)
        {
            try
            {
                IReadOnlyCollection<IWebElement> elements = null;

                var attempts = 0;
                while (elements == null)
                {
                    if (attempts >= maxAttempts)
                    {
                        break;
                    }

                    try
                    {
                        elements = FindDynamicElements(selector, token, type, true);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                    if (elements == null)
                    {
                        Thread.Sleep(timeout);
                    }

                    if (maxAttempts > -1)
                    {
                        attempts++;
                    }
                }

                if ((elements == null || !elements.Any()) && IsOptional(selector))
                {
                    elements = new[] { new WebElement((WebDriver)Driver, $"optional_{Guid.NewGuid()}") };
                }

                if (elements == null)
                {
                    throw new NullReferenceException($"Elements [{selector}] was not found after {attempts} attempts");
                }

                return elements;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return null;
        }

        // SetCurrentPage

        public void SetCurrentPage(string name)
        {
            if (_automationContext.CurrentPage != name)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var definitionsDirectory = _definitionConfiguration.GetApplicationDefinitionsLocation();

                    if (!string.IsNullOrWhiteSpace(_automationContext.ApplicationTarget))
                    {
                        foreach (var item in _automationContext.ApplicationTarget.Split("-"))
                        {
                            definitionsDirectory = Path.Combine(definitionsDirectory, item);
                        }
                    }

                    var definitionsPagesDirectory = Path.Combine(definitionsDirectory, "Pages");

                    var basePath = Program.GetCurrentDirectory();
                    var builder = new ConfigurationBuilder();

                    var value = name.Replace(new[] { "&", "-", " ", "'" }, "");
                    var file = _globalConfiguration[$"DefinitionFiles:{value}"];
                    if (string.IsNullOrWhiteSpace(file) && Directory.Exists(Path.Combine(basePath, definitionsPagesDirectory)))
                    {
                        file = Path.GetFileNameWithoutExtension(Path.Combine(basePath, definitionsPagesDirectory, $"{value}.ini"));
                    }

                    if (string.IsNullOrWhiteSpace(file))
                    {
                        throw new FileNotFoundException($"Definition file [{file}.ini] was not found");
                    }

                    var basePageFile = Path.Combine(basePath, definitionsPagesDirectory, $"{file}.ini");

                    builder
                        .AddIniFile(basePageFile, false, true);

                    var config = builder.Build();

                    PageDefinitions = new Definition(name, "Page");
                    config.Bind(PageDefinitions);
                    PageDefinitions.PrepareAndValidate(_definitionConfiguration);

                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        Console.WriteLine($@"DEFINITION_FILE[ {file}.ini ]DEFINITION_FILE");
                    }

                    var isValid = ValidateAllUniqueElements(PageDefinitions);
                    if (!isValid)
                    {
                        PageDefinitions = null;
                    }
                }
                else
                {
                    PageDefinitions = null;
                }

                _automationContext.CurrentPage = name;
            }
        }

        public bool IsOptional(string name)
        {
            foreach (var item in Definitions)
            {
                //var sources = new[] { item.AutomationIds, item.AutomationNames, item.AutomationCSSes, item.AutomationXPaths, item.AutomationClassNames, item.AutomationLinkTexts, item.AutomationPartialLinkTexts };
                var containsKey = item.OptionalElements.ContainsKey(name);
                if (containsKey)
                {
                    return true;
                }
            }

            return false;
        }

        private Definition GetCurrentScenarioDefinitions()
        {
#if USING_SPECFLOW
            var basePath = Program.GetCurrentDirectory();

            var definitionsDirectory = _definitionConfiguration.GetApplicationDefinitionsLocation();
            if (!string.IsNullOrWhiteSpace(_automationContext.ApplicationTarget))
            {
                foreach (var item in _automationContext.ApplicationTarget.Split("-"))
                {
                    definitionsDirectory = Path.Combine(definitionsDirectory, item);
                }
            }

            var featureDefinitionsDirectory = Path.Combine(definitionsDirectory, "Features");

            /* SCENARIOS */

            var regEx = new Regex(@"Code\((.*)\)", RegexOptions.Compiled);
            var tags = _automationContext.FeatureContext.FeatureInfo.Tags.Select(t => regEx.Match(t)).Where(p => p.Success).ToArray();

            var scenarioBuilder = new ConfigurationBuilder();

            foreach (var item in tags)
            {
                var value = item.Groups[1].Value.Replace(new[] { "&", "-", " ", "'" }, "");
                var scenarioFile = _globalConfiguration[$"DefinitionFiles:{value}"];
                if (string.IsNullOrWhiteSpace(scenarioFile) && Directory.Exists(Path.Combine(basePath, featureDefinitionsDirectory)))
                {
                    scenarioFile = Path.GetFileNameWithoutExtension(Directory.GetFiles(Path.Combine(basePath, featureDefinitionsDirectory), $"*{value}_*.ini").FirstOrDefault(m => !(m.Contains("Android") || m.Contains("iOS"))));
                }

                var basePageFile = Path.Combine(basePath, featureDefinitionsDirectory, $"{scenarioFile}.ini");

                scenarioBuilder
                  .AddIniFile(basePageFile, true, true);

                if (!string.IsNullOrWhiteSpace(scenarioFile))
                {
                    Console.WriteLine($@"DEFINITION_FILE[ {scenarioFile}.ini ]DEFINITION_FILE");
                }
            }

            var scenarioConfiguration = scenarioBuilder.Build();

            var name = string.Join(" ", tags.Select(m => m.Groups[1].Value).ToArray());
            var scenarioDefinitions = new Definition(name, "Scenario");
            scenarioConfiguration.Bind(scenarioDefinitions);
            scenarioDefinitions.PrepareAndValidate(_definitionConfiguration);

            return scenarioDefinitions;
#else
            return null;
#endif
        }

        #region IDisposable

        // To detect redundant calls
        private bool _disposed;

        ~DefinitionService() => Dispose(false);

        public void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (var definition in Definitions)
                {
                    definition.Dispose();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
