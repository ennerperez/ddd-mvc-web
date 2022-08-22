using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Business.Contexts
{
    public class AutomationContext : IAutomationContext
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

        public Exception TestError { get; } = null;
    }
}
