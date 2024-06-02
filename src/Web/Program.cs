using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Web.Services;
#if USING_SASS && USING_SASS_WATCH
using System.Diagnostics;
#endif

namespace Web
{
    public static class Program
    {
        internal static string Name { get; private set; }

        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Name = config["AppSettings:Name"];

#if USING_DATADOG
            var tags = Array.Empty<string>();
            config.Bind("Datadog:Tags", tags);
            var serviceName = config["Datadog:Service"];
            serviceName = !string.IsNullOrWhiteSpace(serviceName) ? serviceName : Name;
#endif
            // Initialize Logger
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithClientIp()
                .Enrich.WithClientAgent()
                .Enrich.WithProperty("ApplicationName", Name);
#if USING_DATADOG
            if (!string.IsNullOrWhiteSpace(config["Datadog:ApiKey"]))
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Async(a =>
                    a.DatadogLogs(config["Datadog:ApiKey"],
                        service: serviceName,
                        host: config["Datadog:Host"],
                        tags: tags
                    ));
            }
#endif
            var logger = Log.Logger = loggerConfiguration.CreateLogger();

#if USING_SASS && USING_SASS_WATCH
            var darts = Process.GetProcessesByName("dart");
            foreach (var process in darts)
            {
                process.Kill();
            }
#endif

            try
            {
                Log.Information("Application Starting");
                var host = CreateHostBuilder(args);
                host.ConfigureLogging(s =>
                {
                    s.ClearProviders();
                    s.AddSerilog(logger);
                });
                var build = host.Build();
                build.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The Application failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // Uses Serilog instead of default .NET Logger
                .ConfigureServices(service =>
                {
                    service.AddHostedService<SeedService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
