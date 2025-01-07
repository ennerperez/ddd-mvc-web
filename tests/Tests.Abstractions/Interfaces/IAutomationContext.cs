using System;
using System.Collections.Generic;
using Tests.Abstractions.Settings;
#if USING_SPECFLOW
using TechTalk.SpecFlow;
#endif

namespace Tests.Abstractions.Interfaces
{
    public interface IAutomationContext : IAttributeLibrary
#if USING_SPECFLOW
        , ISpecFlowContext
#endif
    {
        string AutomationType { get; set; }
        string PlatformTarget { get; set; }
        string EnvironmentTarget { get; set; }
        string ApplicationTarget { get; set; }
        string Priority { get; set; }

        IAutomationConfiguration AutomationConfiguration { get; }
        ScreenshotConfiguration ScreenshotConfiguration { get; }

        bool IsInitialized { get; set; }

        string TestSuiteTarget { get; set; }
        string TestPlanTarget { get; set; }
        string TestCaseTarget { get; set; }

        string CurrentPage { get; set; }
        Stack<string> NavigationStack { get; }

        void AddException(Exception e);
        bool HasExceptions();
        IEnumerable<Exception> GetExceptions();

#if USING_SPECFLOW
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }
#endif
    }
}
