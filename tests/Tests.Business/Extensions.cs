using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;
using Tests.Abstractions.Helpers;
using Tests.Abstractions.Interfaces;
using Tests.Abstractions.Services;
using Tests.Abstractions.Settings;
using Tests.Business.Contexts;
using Tests.Business.Interfaces;
using Tests.Business.Services;

namespace Tests.Business
{
	public static class Extensions
	{
		public static IServiceCollection AddTests(this IServiceCollection services)
		{
			services.AddScoped<IAutomationConfiguration, SpecFlowConfiguration>();
			services.AddScoped<IAutomationContext, AutomationContext>();
			services.AddSingleton<IStepHelper, StepsHelper>();
			services.AddSingleton<LoremIpsumService>();

			//services.AddFromAssembly(Assembly.GetExecutingAssembly());

			services.AddSingleton<ITestService, ClientService>();

			return services;
		}

		// #region FromAssembly
		//
		// private static void AddFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
		// {
		// 	if (!assemblies.Any())
		// 		throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
		//
		// 	var assembliesToScan = assemblies.Distinct().ToArray();
		// 	services.ConnectImplementationsToTypesClosing(typeof(ITestService), assembliesToScan, false);
		// }
		//
		// #endregion
	}
}
