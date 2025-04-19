using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.UITests.Contexts;
using Tests.UITests.Services;
using Tests.UITests.Settings;
#if USING_REQNROLL
using Reqnroll;
using StepsHelper = Tests.UITests.Helpers.StepsHelper;
#endif

namespace Tests.UITests
{
    using StepsHelper = Helpers.StepsHelper;

    public static class Extensions
    {
        public static IServiceCollection AddTests(this IServiceCollection services)
        {
#if USING_REQNROLL
            services.AddScoped<IAutomationConfiguration, ReqnrollConfiguration>();
            services.AddScoped<IFeatureContext, FeatureContext>();
            services.AddScoped<IScenarioContext, ScenarioContext>();
#endif
            services.AddScoped<IAutomationContext, AutomationContext>();

            services.AddScoped<IDefinitionConfiguration, DefinitionConfiguration>();
            services.AddScoped<IDefinitionService<IWebElement>, DefinitionService>();

#if USING_REQNROLL
            services.AddSingleton<IStepHelper, StepsHelper>();
#endif
            services.AddSingleton<LoremIpsumService>();

            return services;
        }
    }
}
