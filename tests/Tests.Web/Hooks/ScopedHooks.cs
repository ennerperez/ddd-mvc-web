using System.Diagnostics;
using System.IO;
using TechTalk.SpecFlow;
using Tests.Abstractions;
using Tests.Abstractions.Interfaces;

namespace Tests.Web.Hooks
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

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            var browser = featureContext.FeatureInfo.Tags.GetTagValue<string>("Browser");
            Program.LoadBrowserDriver(browser);
        }
    }
}
