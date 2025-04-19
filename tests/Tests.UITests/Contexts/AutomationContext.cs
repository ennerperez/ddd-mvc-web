using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Settings;
using Xunit.Sdk;
#if USING_REQNROLL
using Reqnroll;
#endif

namespace Tests.UITests.Contexts
{
    public class AutomationContext : IAutomationContext
    {
        private Dictionary<string, object> _attributeLibrary;
#if USING_REQNROLL
        public AutomationContext(IConfiguration configuration, IAutomationConfiguration automationConfigurations, FeatureContext featureContext, ScenarioContext scenarioContext)
#else
        public AutomationContext(IConfiguration configuration, IAutomationConfiguration automationConfigurations)
#endif
        {
#if USING_REQNROLL
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
#endif
            AutomationConfiguration = automationConfigurations;
            NavigationStack = new Stack<string>();
            ScreenshotConfiguration = new ScreenshotConfiguration();
            configuration.Bind("ScreenshotSettings", ScreenshotConfiguration);
        }

        public IServiceProvider Container => Program.Container;

        public IApplicationFactory ApplicationFactory { get; }
        public IAutomationConfiguration AutomationConfiguration { get; }
        public ScreenshotConfiguration ScreenshotConfiguration { get; }

        public bool IsInitialized { get; set; }

        public object GetAttribute(string attributeKey, bool throwException = true)
        {
            if (_attributeLibrary == null)
            {
                return null;
            }

            if (_attributeLibrary.TryGetValue(attributeKey, out var attributeObject))
            {
                return attributeObject;
            }

            if (throwException)
            {
                throw new KeyNotFoundException($"The {attributeKey} Attribute Key is not defined anywhere within the Feature.");
            }

            return null;
        }

        public void SetAttribute(string attributeKey, object attributeObject)
        {
            _attributeLibrary ??= new Dictionary<string, object>();

            _attributeLibrary.Remove(attributeKey);
            _attributeLibrary.Add(attributeKey, attributeObject);
        }

        public T GetAttribute<T>(string attributeKey, bool throwException = true)
        {
            var value = GetAttribute(attributeKey, throwException);
            if (value is T value1)
            {
                return value1;
            }

            return default;
        }

        public void RemoveAttribute(string attributeKey)
        {
            _attributeLibrary ??= new Dictionary<string, object>();
            _attributeLibrary.Remove(attributeKey);
        }

        public Exception TestError
        {
            get
            {
                if (_exceptions?.Count != 0)
                {
                    return null;
                }

                var i = 0;
                //return new AllException(_exceptions.Count, _exceptions.Select(m => new Tuple<int, object, Exception>(i++, null, m)).ToArray());
                return AllException.ForFailures(_exceptions.Count, _exceptions.Select(m => new Tuple<int, string, Exception>(i++, m.Message, m)).ToArray());

            }
        }

        #region Tags

        public string AutomationType { get; set; }
        public string PlatformTarget { get; set; }

        public string ApplicationTarget { get; set; }
        public string EnvironmentTarget { get; set; }

        public string TestSuiteTarget { get; set; }
        public string TestPlanTarget { get; set; }
        public string TestCaseTarget { get; set; }
        public string CurrentPage { get; set; }
        public Stack<string> NavigationStack { get; }
        public string Priority { get; set; }

        public string Code { get; set; }

        #endregion

#if USING_REQNROLL
        public FeatureContext FeatureContext { get; }
        public ScenarioContext ScenarioContext { get; }
#endif

        #region Exceptions

        private List<Exception> _exceptions;

        public IEnumerable<Exception> GetExceptions() => _exceptions?.ToArray();

        public void AddException(Exception e)
        {
            _exceptions ??= [];

            _exceptions.Add(e);
        }

        public bool HasExceptions() => _exceptions?.Count > 0;

        #endregion
    }
}
