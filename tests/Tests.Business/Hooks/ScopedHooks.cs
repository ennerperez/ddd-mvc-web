using System.Linq;
using System.Threading;
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
            {
                RestoreDatabase();
            }
        }

        [BeforeFeature]
        public static void FlagRestoreDatabaseInFeature(FeatureContext featureContext)
        {
            if (featureContext.FeatureInfo.Tags.Contains(Abstractions.Resources.Keywords.RestoreDatabase))
            {
                RestoreDatabase();
            }
        }

        private static bool _isRestoring;
        private static void RestoreDatabase()
        {
            while (_isRestoring)
            {
                Thread.Sleep(1000);
            }

            _isRestoring = true;
            var context = Persistence.Extensions.DbContext();
            if (context != null)
            {
                context.Database.EnsureDeleted();
                context.Initialize();
                _isRestoring = false;
            }
        }
    }
}
