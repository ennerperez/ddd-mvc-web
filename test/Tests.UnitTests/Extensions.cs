using Microsoft.Extensions.DependencyInjection;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.UnitTests.Contexts;
#if USING_REQNROLL
using Reqnroll;
#endif

namespace Tests.UnitTests
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

            services.AddSingleton<LoremIpsumService>();

            return services;
        }
    }
}
