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
				RestoreDatabase();
		}

		[BeforeFeature]
		public static void FlagRestoreDatabaseInFeature(FeatureContext featureContext)
		{
			if (featureContext.FeatureInfo.Tags.Contains(Abstractions.Resources.Keywords.RestoreDatabase))
				RestoreDatabase();
		}

		private static bool s_isRestoring;
		private static void RestoreDatabase()
		{
			while (s_isRestoring)
				Thread.Sleep(1000);
			s_isRestoring = true;
			var context = Persistence.Extensions.DbContext();
			if (context != null)
			{
				context.Database.EnsureDeleted();
				context.Initialize();
				s_isRestoring = false;
			}
		}
	}
}
