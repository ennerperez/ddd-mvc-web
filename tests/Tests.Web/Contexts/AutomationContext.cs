using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Settings;
using Xunit.Sdk;

namespace Tests.Web.Contexts
{
    public class AutomationContext : IAutomationContext
    {

        public AutomationContext(IConfiguration configuration, IAutomationConfiguration automationConfigurations, FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            AutomationConfiguration = automationConfigurations;
            NavigationStack = new Stack<string>();
            ScreenshotConfiguration = new ScreenshotConfiguration();
            configuration.Bind("ScreenshotSettings", ScreenshotConfiguration);
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

        public static ServiceProvider Container => Program.Container;

        public IAutomationConfiguration AutomationConfiguration { get; }
        public ScreenshotConfiguration ScreenshotConfiguration { get; }
        public FeatureContext FeatureContext { get; }
        public ScenarioContext ScenarioContext { get; }

        #region Exceptions

        private List<Exception> _exceptions;
        public IEnumerable<Exception> GetExceptions()
        {
            return _exceptions?.ToArray();
        }
        public void AddException(Exception e)
        {
            if (_exceptions == null)
            {
                _exceptions = new List<Exception>();
            }

            _exceptions.Add(e);
        }
        public bool HasExceptions()
        {
            return _exceptions?.Any() ?? false;
        }

        #endregion
        public bool IsInitialized { get; set; }

        private Dictionary<string, object> _attributeLibrary;

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
            else if (throwException)
            {
                throw new KeyNotFoundException($"The {attributeKey} Attribute Key is not defined anywhere within the Feature.");
            }
            else
            {
                return null;
            }
        }

        public void SetAttribute(string attributeKey, object attributeObject)
        {
            if (_attributeLibrary == null)
            {
                _attributeLibrary = new Dictionary<string, object>();
            }

            _attributeLibrary.Remove(attributeKey);
            _attributeLibrary.Add(attributeKey, attributeObject);
        }

        public Exception TestError
        {
            get
            {
                if (_exceptions?.Any() ?? false)
                {
                    var i = 0;
                    //return new AllException(_exceptions.Count, _exceptions.Select(m => new Tuple<int, object, Exception>(i++, null, m)).ToArray());
                    return AllException.ForFailures(_exceptions.Count, _exceptions.Select(m => new Tuple<int, string, Exception>(i++, m.Message, m)).ToArray());
                }

                return null;
            }
        }
    }
}
