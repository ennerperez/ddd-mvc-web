using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Business.Contexts
{
    public class AutomationContext : IAutomationContext, IAttributeLibrary
    {
        public AutomationContext(IAutomationConfiguration automationConfigurations, FeatureContext featureContext, ScenarioContext scenarioContext)
        {
            FeatureContext = featureContext;
            ScenarioContext = scenarioContext;
            AutomationConfigurations = automationConfigurations;
        }

        #region Tags

        public string AutomationType { get; set; }
        public string PlatformTarget { get; set; }

        public string ApplicationTarget { get; set; }
        public string EnvironmentTarget { get; set; }

        public string TestSuiteTarget { get; set; }
        public string TestPlanTarget { get; set; }
        public string TestCaseTarget { get; set; }
        public string Priority { get; set; }

        public string Code { get; set; }

        #endregion

        public IAutomationConfiguration AutomationConfigurations { get; }
        public FeatureContext FeatureContext { get; }
        public ScenarioContext ScenarioContext { get; }
        public bool IsInitialized { get; set; }

        public Dictionary<string, object> AttributeLibrary { get; } = new();
        public object GetAttributeFromAttributeLibrary(string attributeKey, bool throwException = true)
        {
            if (AttributeLibrary.TryGetValue(attributeKey, out var attributeObject))
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

        public void SetAttributeInAttributeLibrary(string attributeKey, object attributeObject)
        {
            AttributeLibrary.Remove(attributeKey);
            AttributeLibrary.Add(attributeKey, attributeObject);
        }

        public Exception TestError { get; } = null;
    }
}
