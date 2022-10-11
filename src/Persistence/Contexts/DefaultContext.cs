#define SQLITE

using Microsoft.EntityFrameworkCore;
using Persistence.Conventions;
using System;
using System.Data.Common;
using System.Linq;
using Domain.Entities;
using Domain.Entities.Identity;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using DbContextOptions=Microsoft.EntityFrameworkCore.DbContextOptions;

#if SQLITE && USING_SQLITE
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
#endif

// ReSharper disable UnusedVariable
#pragma warning disable 219
#pragma warning disable 168

namespace Persistence.Contexts
{
	public class DefaultContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
	{
		private readonly DbContextOptions _options;

		public DbContextOptions Options => _options;

		public DefaultContext()
		{
		}

		public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
		{
			_options = options;
		}

		internal static string ProviderName { get; set; }
		internal static bool HasSchema => !(new[] { "Sqlite", "MySql" }).Contains(ProviderName);

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

			ProviderName = Database.ProviderName?.Split('.').Last();

			// Configurations
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(DefaultContext).Assembly, m => m.GetCustomAttributes(typeof(DbContextAttribute), true).OfType<DbContextAttribute>().Any(a => a.ContextType == typeof(DefaultContext)));

			// Conventions
			// modelBuilder.RemovePluralizingTableNameConvention();
			modelBuilder.AddProviderTypeConventions(m =>
			{
				m.Provider = ProviderName;
				m.DecimalConfig.Add(6, new[] { "Lat", "Long" });
				m.Exclude = null;
				m.UseDateTime = false;
			});
			modelBuilder.AddAuditableEntitiesConventions<IAuditable>(ProviderName);
			modelBuilder.AddSincronizableEntitiesConventions<ISyncronizable>(ProviderName);
		}

#if DEBUG
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.EnableDetailedErrors();
			optionsBuilder.EnableSensitiveDataLogging();
#if HAS_DATABASE_PROVIDER
			if (_options == null)
			{
				var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				var config = new ConfigurationBuilder()
					.SetBasePath(Directory.GetCurrentDirectory())
					.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
					.AddJsonFile($"appsettings.{environmentName}.json", true, true)
					.AddEnvironmentVariables()
					.Build();

				UseDbEngine(optionsBuilder, config);
			}
#endif
		}
#endif

		#region DbSet

		public DbSet<Setting> Settings { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Budget> Budgets { get; set; }

		#endregion DbSet

		public static void UseDbEngine(DbContextOptionsBuilder optionsBuilder, IConfiguration config)
		{
#pragma warning disable 168
#pragma warning disable 219
#if USING_DATABASE_PROVIDER
			var migrationsHistoryTableName = "__EFMigrationsHistory";
			var connectionString = config.GetConnectionString(nameof(DefaultContext));
			DbConnectionStringBuilder csb;
#endif
#pragma warning restore 219
#pragma warning restore 168
#if SQLITE && USING_SQLITE
			csb = new SqliteConnectionStringBuilder() { ConnectionString = connectionString };
			var dbPath = Regex.Match(csb.ConnectionString.ToLower(), "(data source ?= ?)(.*)(;?)").Groups[2].Value;
			var dbPathExpanded = Environment.ExpandEnvironmentVariables(dbPath);
			csb.ConnectionString = csb.ConnectionString.Replace(dbPath, dbPathExpanded);
			optionsBuilder.UseSqlite(csb.ConnectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName));
#elif MSSQL && USING_MSSQL
            optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
#elif (MYSQL && USING_MYSQL) || (MARIADB && USING_MARIADB)
#if USING_MARIADB
			var serverVersion = ServerVersion.Parse("10.6");
#elif USING_MYSQL
			var serverVersion = ServerVersion.Parse("8.0");
#endif
			try
			{
				serverVersion = ServerVersion.AutoDetect(connectionString);
			}
			catch (Exception)
			{
				// ignore
			}
            optionsBuilder.UseMySql(connectionString, serverVersion, x => x.MigrationsHistoryTable(migrationsHistoryTableName));
#elif POSTGRESQL && USING_POSTGRESQL
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
#elif ORACLE && USING_ORACLE
            optionsBuilder.UseOracle(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
#endif
		}
	}
}
