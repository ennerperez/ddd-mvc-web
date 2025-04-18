#if USING_SPECFLOW
using SolidToken.SpecFlow.DependencyInjection;
using TechTalk.SpecFlow;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using Tests.Abstractions.Resources;
using Tests.Web.Settings;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tests.Web
{
#if USING_SPECFLOW
    [Binding]
#endif
    internal class Program
    {
        // ReSharper disable once CollectionNeverQueried.Local
        private static Dictionary<string, string> s_arguments;

        public static bool IsInitialized { get; private set; }

        public static void Main(string[] args) => Initialize("", args);

        public static void Initialize(string browser = "", string[] args = null)
        {
            if (IsInitialized)
            {
                return;
            }

            var currentDirectory = string.Empty;
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(currentDirectory, "appsettings.json"), false, true)
                .AddJsonFile(Path.Combine(currentDirectory, "specflow.json"), false, true)
                .AddJsonFile(Path.Combine(currentDirectory, "specflow.Timeouts.json"), false, true)
#if DEBUG
                .AddJsonFile(Path.Combine(currentDirectory, "appsettings.Development.json"), true, true)
                .AddJsonFile(Path.Combine(currentDirectory, "specflow.Development.json"), true, true)
                .AddJsonFile(Path.Combine(currentDirectory, "specflow.Timeouts.Development.json"), false, true)
#endif
                .AddEnvironmentVariables()
                .Build();

            // Initialize Logger
            var logger = Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

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
                builder.AddSerilog(logger);
            }).AddOptions();

            Services.AddTests();

            Container = Services.BuildServiceProvider();
            var factory = Container.GetService<ILoggerFactory>();
            if (factory != null)
            {
                Logger = factory.CreateLogger(typeof(Program));
            }

            // Arguments
            s_arguments = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly().Location;

            if (args == null || !args.Any())
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

        internal static void LoadBrowserDriver(string browser = "")
        {
            if (string.IsNullOrWhiteSpace(browser) || browser == "Chrome")
            {
                var options = new ChromeOptions();
                //options.AddAdditionalCapability("useAutomationExtension", false);
                options.AddAdditionalOption("useAutomationExtension", false);

                var browserSettings = new BrowserConfiguration();
                Configuration.Bind("BrowserSettings", browserSettings);

                if (browserSettings.Hidden)
                {
                    options.AddArgument("--headless");
                }

                var driver = new ChromeDriver(options);

                if (browserSettings.Maximized)
                {
                    driver.Manage().Window.Maximize();
                }
                else if (browserSettings.Size.Any())
                {
                    driver.Manage().Window.Size = new Size(browserSettings.Size[0], browserSettings.Size[1]);
                }

                driver.Navigate().GoToUrl(Configuration["AppSettings:WebUrl"]);
                Driver = driver;
            }
        }

#if USING_SPECFLOW
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

        internal static string GetCurrentDirectory()
        {
            var currentDirectory = string.Empty;
            return currentDirectory;
        }

        public static void Dispose()
        {
            IsInitialized = false;
            Driver?.Close();
            Driver?.Dispose();
            Driver = null;
        }

        #region IOC

        public static IConfiguration Configuration { get; private set; }
        public static ILogger Logger { get; private set; }

        public static ServiceCollection Services { get; private set; }
        public static ServiceProvider Container { get; private set; }

        public static IWebDriver Driver { get; private set; }

        #endregion
    }
}
