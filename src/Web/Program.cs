using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Web.Services;

namespace Web
{
	[DebuggerStepThrough]
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

			// Initialize Logger
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(config)
				.Enrich.WithProperty("Application", Name)
				.CreateLogger();

#if USING_SASS && ENABLE_SASS_WATCH
			var darts = Process.GetProcessesByName("dart");
			foreach (var process in darts)
				process.Kill();
#endif

			try
			{
				Log.Information("Application Starting");
				CreateHostBuilder(args).Build().Run();
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
				.UseSerilog()// Uses Serilog instead of default .NET Logger
				.ConfigureServices(service => service.AddHostedService<SeedService>())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
