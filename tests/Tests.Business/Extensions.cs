using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Business.Contexts;
using Tests.Business.Interfaces;
using Tests.Business.Services;
#if USING_SPECFLOW
using TechTalk.SpecFlow;
using Tests.Abstractions.Helpers;
using Tests.Abstractions.Settings;
#endif

namespace Tests.Business
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
#if USING_SPECFLOW
            services.AddSingleton<IStepHelper, StepsHelper>();
#endif
            services.AddSingleton<LoremIpsumService>();

            // Test Services
            services.AddSingleton<ITestService<Client>, ClientTestService>();

            return services;
        }
    }
}
