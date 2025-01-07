using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.Web.Contexts;
using Tests.Web.Services;
using Tests.Web.Settings;
#if USING_SPECFLOW
using TechTalk.SpecFlow;
using StepsHelper = Tests.Web.Helpers.StepsHelper;
#endif

namespace Tests.Web
{
    public static class Extensions
    {
        public static IServiceCollection AddTests(this IServiceCollection services)
        {
#if USING_SPECFLOW
            services.AddScoped<IAutomationConfiguration, SpecFlowConfiguration>();
            services.AddScoped<IFeatureContext, FeatureContext>();
            services.AddScoped<IScenarioContext, ScenarioContext>();
#endif
            services.AddScoped<IAutomationContext, AutomationContext>();

            services.AddScoped<IDefinitionConfiguration, DefinitionConfiguration>();
            services.AddScoped<IDefinitionService<IWebElement>, DefinitionService>();

#if USING_SPECFLOW
            services.AddSingleton<IStepHelper, StepsHelper>();
#endif
            services.AddSingleton<LoremIpsumService>();

            return services;
        }
    }
}
