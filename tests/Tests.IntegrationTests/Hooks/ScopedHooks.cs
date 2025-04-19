using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Resources;

namespace Tests.IntegrationTests.Hooks
{
    [Binding]
    public sealed class ScopedHooks : Abstractions.Hooks.ScopedHooks
    {
        private static bool s_isRestoring;

        public ScopedHooks(IAutomationContext automationContext, IAutomationConfiguration automationConfiguration) : base(automationContext, automationConfiguration)
        {
        }

        [BeforeScenario]
        public void FlagRestoreDatabaseInScenario()
        {
            AutomationContext.ApplicationFactory.Initialize();
            if (AutomationContext.ScenarioContext.ScenarioInfo.Tags.Contains(Keywords.RestoreDatabase))
            {
                RestoreDatabase();
            }
        }

        [BeforeFeature]
        public static void FlagRestoreDatabaseInFeature(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Contains(Keywords.RestoreDatabase))
            {
                RestoreDatabase();
            }
        }

        private static void RestoreDatabase()
        {
            while (s_isRestoring)
            {
                Thread.Sleep(1000);
            }

            s_isRestoring = true;
            var context = Persistence.Extensions.DbContext();
            if (context == null)
            {
                return;
            }

            context.Database.EnsureDeleted();
            context.Initialize();
            s_isRestoring = false;
        }
    }
}
