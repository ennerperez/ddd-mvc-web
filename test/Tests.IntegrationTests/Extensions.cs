using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.IntegrationTests.Contexts;
using Tests.IntegrationTests.Interfaces;
using Tests.IntegrationTests.Services;
#if USING_REQNROLL
using Reqnroll;
using Tests.Abstractions.Helpers;
using Tests.Abstractions.Settings;
#endif

namespace Tests.IntegrationTests
{
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
#if USING_REQNROLL
            services.AddSingleton<IStepHelper, StepsHelper>();
#endif
            services.AddSingleton<LoremIpsumService>();

            // Test Services
            services.AddSingleton<ITestService<Client>, ClientTestService>();

            return services;
        }
    }
}
