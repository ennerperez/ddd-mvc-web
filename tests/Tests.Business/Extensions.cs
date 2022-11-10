using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Tests.Abstractions.Helpers;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.Business.Contexts;
using Tests.Business.Interfaces;
using Tests.Business.Services;

namespace Tests.Business
{
	public static class Extensions
	{
		public static IServiceCollection AddTests(this IServiceCollection services)
		{
			services.AddScoped<IAutomationConfiguration, SpecFlowConfiguration>();
			services.AddScoped<IFeatureContext, FeatureContext>();
			services.AddScoped<IScenarioContext, ScenarioContext>();
			services.AddScoped<IAutomationContext, AutomationContext>();

			services.AddSingleton<IStepHelper, StepsHelper>();
			services.AddSingleton<LoremIpsumService>();

			services.AddSingleton<ITestService, ClientService>();

			return services;
		}
	}
}
