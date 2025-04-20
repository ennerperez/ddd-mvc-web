#if USING_REQNROLL
using Reqnroll.Microsoft.Extensions.DependencyInjection;
using Reqnroll;
#endif
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Business;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.Contexts;
#if USING_SERILOG
using Serilog;
#endif
using Tests.Abstractions.Resources;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.IntegrationTests
{
#if USING_REQNROLL
    [Binding]
#endif
    internal class Program
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private static Dictionary<string, string> s_arguments;

        public static bool IsInitialized { get; private set; }

        public static void Main(string[] args) => Initialize(args);

        private static void Initialize(string[] args = null)
        {
            if (IsInitialized)
            {
                return;
            }

            var currentDirectory = string.Empty;
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(currentDirectory, "appsettings.json"), false, true)
                .AddJsonFile(Path.Combine(currentDirectory, "reqnroll.json"), false, true)
#if DEBUG
                .AddJsonFile(Path.Combine(currentDirectory, "appsettings.Development.json"), true, true)
                .AddJsonFile(Path.Combine(currentDirectory, "reqnroll.Development.json"), true, true)
#endif
                .AddEnvironmentVariables()
                .Build();

            // Initialize Logger
#if USING_SERILOG
            var logger = Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
#endif
            // Language
            var language = Configuration.GetValue<string>("language:feature") ?? "en";
            CultureInfo.CurrentCulture = new CultureInfo(language);
            CultureInfo.CurrentUICulture = new CultureInfo(language);
            Keywords.Culture = new CultureInfo(language);

            Services = new ServiceCollection();
            Services.AddSingleton(Configuration);
            Services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
#if USING_SERILOG
                builder.AddSerilog(logger);
#endif
            }).AddOptions();

            Services
                .AddDomain()
                .AddInfrastructure()
                .AddPersistence<CacheContext>(options => options.UseDbEngine(Configuration), ServiceLifetime.Transient)
                .AddPersistence<DefaultContext>(options => options.UseDbEngine(Configuration))
                .AddBusiness().WithRepositories().WithMediatR()
                .AddTests();

            Container = Services.BuildServiceProvider();
            var factory = Container.GetService<ILoggerFactory>();
            if (factory != null)
            {
                Logger = factory.CreateLogger(typeof(Program));
            }

            Container.GetService<CacheContext>().Initialize();
            Container.GetService<DefaultContext>().Initialize();

            // Arguments
            s_arguments = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly().Location;

            if (args == null || args.Length == 0)
            {
                args = Environment.GetCommandLineArgs();
            }

            foreach (var item in args.Where(m => m != assembly))
            {
                var regex = Regex.Match(item, @"^(?:\/|-)(\w+):?(.+)?$", RegexOptions.Compiled);
                if (regex.Success)
                {
                    s_arguments.Add(regex.Groups[1].Value, regex.Groups[2].Value);
                }
            }

            IsInitialized = true;
        }

#if USING_REQNROLL
        [ScenarioDependencies]
#endif
        public static IServiceCollection CreateServices()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Initialize();

            return Services;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            if (Logger != null)
            {
                Logger.LogError(ex, "{Message}", ex.Message);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        public static void Dispose() => IsInitialized = false;

        #region IOC

        public static IConfiguration Configuration { get; private set; }
        public static ILogger Logger { get; private set; }

        public static IServiceCollection Services { get; private set; }
        public static IServiceProvider Container { get; private set; }

        #endregion
    }
}
