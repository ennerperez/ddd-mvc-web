using TechTalk.SpecFlow;
using Tests.Abstractions;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Hooks
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
            var browser = featureContext.FeatureInfo.Tags.GetTagValue<string>("Browser");
            Program.LoadBrowserDriver(browser);
        }
    }
}
