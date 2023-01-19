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

		public static IServiceCollection AddPersistence<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder = null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) where TContext : DbContext
		{
			services.AddDbContext<TContext>(optionsBuilder, serviceLifetime);
			switch (serviceLifetime)
			{
				case ServiceLifetime.Transient:
					services.AddTransient<DbContext, TContext>();
					break;
				case ServiceLifetime.Singleton:
					services.AddSingleton<DbContext, TContext>();
					break;
				default:
					services.AddScoped<DbContext, TContext>();
					break;
			}
			DbContext = () => services.BuildServiceProvider().GetRequiredService<TContext>();

			return services;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="services"></param>
		/// <param name="optionsBuilder"></param>
		/// <param name="dbProvider"></param>
		/// <param name="serviceLifetime"></param>
		/// <returns></returns>
		public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder = null, string dbProvider = "", ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
		{
			services.AddPersistence<DefaultContext>(optionsBuilder, serviceLifetime);

#if USING_SQLITE
			services.AddPersistence<DefaultContext.Sqlite>(optionsBuilder, serviceLifetime);
#endif
#if USING_MSSQL
			services.AddPersistence<DefaultContext.SqlServer>(optionsBuilder, serviceLifetime);
#endif
#if USING_MARIADB || USING_MYSQL
			services.AddPersistence<DefaultContext.MySql>(optionsBuilder, serviceLifetime);
#endif
#if USING_POSTGRESQL
			services.AddPersistence<DefaultContext.PostgreSql>(optionsBuilder, serviceLifetime);
#endif
#if USING_ORACLE
			services.AddPersistence<DefaultContext.Oracle>(optionsBuilder, serviceLifetime);
#endif

			if (!string.IsNullOrWhiteSpace(dbProvider))
			{
				DbContext = () =>
				{
					var provider = services.BuildServiceProvider();
					switch (dbProvider)
					{
#if USING_SQLITE
						case Providers.Sqlite:
							return provider.GetRequiredService<DefaultContext.Sqlite>();
#endif
#if USING_MSSQL
						case Providers.SqlServer:
							return provider.GetRequiredService<DefaultContext.SqlServer>();
#endif
#if USING_MARIADB || USING_MYSQL
						case Providers.MariaDb:
						case Providers.MySql:
							return provider.GetRequiredService<DefaultContext.MySql>();
#endif
#if USING_POSTGRESQL
						case Providers.PostgreSql:
							return provider.GetRequiredService<DefaultContext.PostgreSql>();
#endif
#if USING_ORACLE
						case Providers.Oracle:
							return provider.GetRequiredService<DefaultContext.Oracle>();
#endif
						default:
							return provider.GetRequiredService<DefaultContext>();
					}
				};
			}

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
