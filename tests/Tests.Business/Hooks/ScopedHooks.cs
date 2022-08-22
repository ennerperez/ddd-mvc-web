using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        [BeforeScenario]
        public void FlagRestoreDatabaseInScenario()
        {
            if (_automationContext.ScenarioContext.ScenarioInfo.Tags.Contains(Abstractions.Resources.Keywords.RestoreDatabase))
                RestoreDatabase();
        }

        [BeforeFeature]
        public static void FlagRestoreDatabaseInFeature(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Contains(Abstractions.Resources.Keywords.RestoreDatabase))
                RestoreDatabase();
        }

        private static void RestoreDatabase()
        {
            var context = Persistence.Extensions.DbContext();
            if (context != null)
            {
                context.Database.EnsureDeleted();
                if (context.Database.ProviderName != null && !context.Database.ProviderName.EndsWith("Sqlite"))
                    context.Database.EnsureCreated();
                context.Database.Migrate();
            }
        }
    }
}
