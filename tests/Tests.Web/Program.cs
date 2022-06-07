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
using SolidToken.SpecFlow.DependencyInjection;
using Tests.Abstractions.Helpers;
using Tests.Abstractions.Interfaces;
using Tests.Web.Contexts;
using Tests.Web.Settings;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tests.Web
{
    public class Program
    {
        #region IOC

        public static IConfiguration Configuration { get; private set; }
        public static ILogger Logger { get; private set; }
        
        public static ServiceCollection Services { get; private set; }
        public static ServiceProvider Container { get; private set; }
        
        public static IWebDriver Driver { get; private set; }
        public static SpecFlowConfiguration SpecFlowConfiguration { get; private set; }

        #endregion
        
        public static void Main(string[] args)
        {
            Initialize("", args);
        }
        
        // ReSharper disable once CollectionNeverQueried.Local
        private static Dictionary<string, string> s_arguments;

        public static bool IsInitialized { get; private set; }

        public static void Initialize(string browser = "", string[] args = null)
        {
            if (IsInitialized) return;

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
            
            //Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
            
            // Language
            var language = Configuration.GetValue<string>("language:feature") ?? "en";
            CultureInfo.CurrentCulture = new CultureInfo(language);
            CultureInfo.CurrentUICulture = new CultureInfo(language);
            Abstractions.Resources.Keywords.Culture = new CultureInfo(language);

            SpecFlowConfiguration = new SpecFlowConfiguration(Configuration);
            Configuration.Bind("AppSettings", SpecFlowConfiguration);

            Services = new ServiceCollection();
            Services.AddSingleton(Configuration);
            Services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog();
            }).AddOptions();
            
            Services.AddScoped<IAutomationConfiguration, SpecFlowConfiguration>();
            Services.AddScoped<IAutomationContext, AutomationContext>();
            Services.AddSingleton<IStepHelper, StepsHelper>();

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
                args = Environment.GetCommandLineArgs();

            foreach (var item in args.Where(m => m != assembly))
            {
                var regex = Regex.Match(item, @"^(?:\/|-)(\w+):?(.+)?$", RegexOptions.Compiled);
                if (regex.Success)
                    s_arguments.Add(regex.Groups[1].Value, regex.Groups[2].Value);
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

                if (Program.SpecFlowConfiguration.Browser.Hidden)
                    options.AddArgument("--headless");
                var driver = new ChromeDriver(options);

                if (SpecFlowConfiguration.Browser.Maximized) driver.Manage().Window.Maximize();
                else if (SpecFlowConfiguration.Browser.Size.Any()) driver.Manage().Window.Size = new Size(SpecFlowConfiguration.Browser.Size[0], SpecFlowConfiguration.Browser.Size[1]);

                driver.Navigate().GoToUrl(SpecFlowConfiguration.WebUrl);
                Driver = driver;
            }
        }
        
        [ScenarioDependencies]
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

        public static void Dispose()
        {
            IsInitialized = false;
            Driver?.Close();
            Driver?.Dispose();
            Driver = null;
        }
    }
}
