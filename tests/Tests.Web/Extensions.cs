using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.Web.Contexts;
using Tests.Web.Interfaces;
using Tests.Web.Services;
using Tests.Web.Settings;
using StepsHelper = Tests.Web.Helpers.StepsHelper;

namespace Tests.Web
{
  public static class Extensions
  {
    public static IServiceCollection AddTests(this IServiceCollection services)
    {
      services.AddScoped<IAutomationConfiguration, Abstractions.Settings.SpecFlowConfiguration>();
      services.AddScoped<IFeatureContext, FeatureContext>();
      services.AddScoped<IScenarioContext, ScenarioContext>();
      services.AddScoped<IAutomationContext, AutomationContext>();

      services.AddScoped<IDefinitionConfiguration, DefinitionConfiguration>();
      services.AddScoped<IDefinitionService, DefinitionService>();

      services.AddSingleton<IStepHelper, StepsHelper>();
      services.AddSingleton<LoremIpsumService>();

      return services;
    }

  }
}
