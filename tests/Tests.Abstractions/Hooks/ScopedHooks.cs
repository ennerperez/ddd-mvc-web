using System.Linq;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Hooks
{
    public abstract class ScopedHooks
    {
        protected readonly IAutomationContext _automationContext;
        protected readonly IAutomationConfiguration _automationConfiguration;

        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks
        public ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration)
        {
            _automationContext = automationContext;
            _automationConfiguration = automationConfiguration;
        }

        [BeforeScenario]
        public void FlagScenarioAsPending()
        {
            if (_automationContext.ScenarioContext.ScenarioInfo.Tags.Contains(Resources.Keywords.NotYetImplemented))
            {
                _automationContext.ScenarioContext.Pending();
            }
        }
    }
}
