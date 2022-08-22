using System.Collections.Generic;
using TechTalk.SpecFlow;

namespace Tests.Abstractions.Interfaces
{
    public interface IAutomationContext : ISpecFlowContext
    {
        
        string AutomationType { get; set; }
        string PlatformTarget { get; set; }
        string EnvironmentTarget { get; set; }
        string ApplicationTarget { get; set; }
        string Priority { get; set; }

        IAutomationConfiguration AutomationConfigurations { get; }
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }

        bool IsInitialized { get; set; }

        string TestSuiteTarget { get; set; }
        string TestPlanTarget { get; set; }
        string TestCaseTarget { get; set; }
        
        public Dictionary<string, object> AttributeLibrary { get; }
        
    }
}
