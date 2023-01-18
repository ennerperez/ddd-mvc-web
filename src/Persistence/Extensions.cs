using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Interfaces;

namespace Persistence
{
	public static class Extensions
	{
		public static Func<DbContext> DbContext { get; set; }

		/// <summary>
		///
		/// </summary>
		/// <param name="services"></param>
		/// <param name="optionsBuilder"></param>
		/// <param name="serviceLifetime"></param>
		/// <returns></returns>
		public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			services.AddDbContext<DefaultContext>(optionsBuilder,serviceLifetime);
			switch (serviceLifetime)
			{
			  case ServiceLifetime.Transient:
			    services.AddTransient<DbContext, DefaultContext>();
			    break;
			  case ServiceLifetime.Singleton:
			    services.AddSingleton<DbContext, DefaultContext>();
			    break;
			  default:
			    services.AddScoped<DbContext, DefaultContext>();
			    break;
			}
			DbContext = () => services.BuildServiceProvider().GetRequiredService<DefaultContext>();

			services.AddFromAssembly(Assembly.GetExecutingAssembly());

			return services;
		}

		#region FromAssembly

		private static void AddFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
		{
			if (!assemblies.Any())
				throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");

			var assembliesToScan = assemblies.Distinct().ToArray();
			services.ConnectImplementationsToTypesClosing(typeof(IGenericRepository<,>), assembliesToScan, false);
			services.ConnectImplementationsToTypesClosing(typeof(IGenericRepository<>), assembliesToScan, false);
		}

		#endregion

	}
}
