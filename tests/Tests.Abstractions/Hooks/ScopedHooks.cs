using System.Linq;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Abstractions.Hooks
{
    [Binding]
    public sealed class ScopedHooks
    {
        private readonly IAutomationContext _automationContext;
        private readonly IAutomationConfiguration _automationConfiguration;

        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks
        public ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration)
        {
            _automationContext = automationContext;
            _automationConfiguration = automationConfiguration;
        }

        [BeforeStep]
        private void FlagScenarioAsPending()
        {
            if (_automationContext.ScenarioContext.ScenarioInfo.Tags.Contains(Resources.Keywords.NotYetImplemented))
            {
                _automationContext.ScenarioContext.Pending();
            }
        }
    }
}
