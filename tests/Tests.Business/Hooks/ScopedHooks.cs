using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;

namespace Tests.Business.Hooks
{
    [Binding]
    public sealed class ScopedHooks : Abstractions.Hooks.ScopedHooks
    {
        public ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration) : base(automationContext, automationConfiguration)
        {
        }
    }
}
