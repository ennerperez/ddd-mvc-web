#if USING_SPECFLOW
using System.Linq;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Resources;

namespace Tests.Abstractions.Hooks
{
    public abstract class ScopedHooks
    {
        protected readonly IAutomationConfiguration _automationConfiguration;
        protected readonly IAutomationContext _automationContext;

        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks
        protected ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration)
        {
            _automationContext = automationContext;
            _automationConfiguration = automationConfiguration;
        }

        [BeforeScenario]
        public void FlagScenarioAsPending()
        {
            if (_automationContext.ScenarioContext.ScenarioInfo.Tags.Contains(Keywords.NotYetImplemented))
            {
                _automationContext.ScenarioContext.Pending();
            }
        }
    }
}
#endif
