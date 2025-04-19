using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Tests.Abstractions.Interfaces;
using Web.Services;

namespace Tests.IntegrationTests.Factories
{
    public class ApplicationFactory : WebApplicationFactory<Web.Program>, IApplicationFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                var testConfig = config
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                    .AddEnvironmentVariables()
                    .Build();
                config.AddConfiguration(testConfig);
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(Substitute.For<SeedService>());
            });
        }

        public void Initialize()
        {
        }
    }
}
