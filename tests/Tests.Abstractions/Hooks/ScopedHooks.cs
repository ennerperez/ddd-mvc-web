#if USING_REQNROLL
using System.Linq;
using Reqnroll;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Resources;

namespace Tests.Abstractions.Hooks
{
    public abstract class ScopedHooks
    {
        private readonly IAutomationConfiguration _automationConfiguration;
        private readonly IAutomationContext _automationContext;

        protected ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration)
        {
            _automationContext = automationContext;
            _automationConfiguration = automationConfiguration;
        }

        public IAutomationContext AutomationContext => _automationContext;
        public IAutomationConfiguration AutomationConfiguration => _automationConfiguration;

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
