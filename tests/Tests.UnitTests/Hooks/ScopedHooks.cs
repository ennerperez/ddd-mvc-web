#if USING_REQNROLL
using Reqnroll;
using Tests.Abstractions.Interfaces;

namespace Tests.UnitTests.Hooks
{
    [Binding]
    public sealed class ScopedHooks : Abstractions.Hooks.ScopedHooks
    {
        public ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration) : base(automationContext, automationConfiguration)
        {
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
        }
    }
}
#endif
