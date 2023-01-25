using Microsoft.EntityFrameworkCore;
using Persistence.Conventions;
using System;
using System.Linq;
using Domain.Entities;
using Domain.Interfaces;
#if USING_IDENTITY
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif
using Microsoft.EntityFrameworkCore.Infrastructure;

#if DEBUG && HAS_DATABASE_PROVIDER
using System.IO;
#endif


namespace Persistence.Contexts
{
	public partial class DefaultContext :
#if USING_IDENTITY
		IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
#else
		DbContext
#endif
	{
		public DefaultContext()
		{
		}

		public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
		{
		}

		private static string ProviderName { get; set; }
		internal static bool HasSchema => DbContextExtensions.HasSchema(ProviderName);
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

			var providerName = Database.ProviderName?.Split('.').Last();

			// Configurations
			modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly, m => m.GetCustomAttributes(typeof(DbContextAttribute), true).OfType<DbContextAttribute>().Any(a => a.ContextType == GetType()));

			// Conventions
			// modelBuilder.RemovePluralizingTableNameConvention();
			modelBuilder.AddProviderTypeConventions(m =>
			{
				m.Provider = providerName;
				m.DecimalConfig.Add(6, new[] {"Lat", "Long"});
				m.Exclude = null;
				m.UseDateTime = false;
			});
			modelBuilder.AddAuditableEntitiesConventions<IAuditable>(providerName);
			modelBuilder.AddSynchronizableEntitiesConventions<ISyncronizable>(providerName);
		}

#if DEBUG
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder?.EnableDetailedErrors();
			optionsBuilder?.EnableSensitiveDataLogging();
// #if HAS_DATABASE_PROVIDER
// 			if (optionsBuilder == null)
// 			{
// 				var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// 				var config = new ConfigurationBuilder()
// 					.SetBasePath(Directory.GetCurrentDirectory())
// 					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
// 					.AddJsonFile($"appsettings.{environmentName}.json", true, true)
// 					.AddEnvironmentVariables()
// 					.Build();
//
// 				this.UseDbEngine(optionsBuilder, config);
// 			}
// #endif
		}
#endif

		#region DbSet

		public DbSet<Setting> Settings { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Budget> Budgets { get; set; }

		#endregion DbSet

	}
}
