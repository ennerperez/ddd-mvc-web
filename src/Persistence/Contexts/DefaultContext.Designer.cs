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

#if DEBUG && HAS_DATABASE_PROVIDER
using System.IO;
#endif

#if USING_SQLITE
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
#endif

// ReSharper disable UnusedVariable
#pragma warning disable 219
#pragma warning disable 168

namespace Persistence.Contexts
{
	public partial class DefaultContext
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

#if DEBUG
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder?.EnableDetailedErrors();
			optionsBuilder?.EnableSensitiveDataLogging();
#if HAS_DATABASE_PROVIDER
			if (optionsBuilder == null)
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
		public static void UseDbEngine(DbContextOptionsBuilder optionsBuilder, IConfiguration config)
		{
			if (string.IsNullOrWhiteSpace(ProviderName)) ProviderName = config["AppSettings:DbProvider"];
			var connectionString = config.GetConnectionString($"{nameof(DefaultContext)}.{ProviderName}");

#pragma warning disable 168
#pragma warning disable 219
#if USING_DATABASE_PROVIDER
			var migrationsHistoryTableName = "__EFMigrationsHistory";
			if (string.IsNullOrWhiteSpace(connectionString))
				connectionString = config.GetConnectionString(nameof(DefaultContext));
			DbConnectionStringBuilder csb;
#endif
			switch (ProviderName)
			{
#if USING_SQLITE
				case Providers.Sqlite:
#pragma warning restore 219
#pragma warning restore 168
					csb = new SqliteConnectionStringBuilder() { ConnectionString = connectionString };
					var dbPath = Regex.Match(csb.ConnectionString.ToLower(), "(data source ?= ?)(.*)(;?)").Groups[2].Value;
					var dbPathExpanded = Environment.ExpandEnvironmentVariables(dbPath);
					csb.ConnectionString = csb.ConnectionString.Replace(dbPath, dbPathExpanded);
					optionsBuilder.UseSqlite(csb.ConnectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName));
					break;
#endif
#if USING_MSSQL
				case Providers.SqlServer:
					optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
					break;
#endif
#if (USING_MYSQL) || (USING_MARIADB)
				case Providers.MariaDb:
				case Providers.MySql:
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
					break;
#endif
#if USING_POSTGRESQL
				case Providers.PostgreSql:
					optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
					break;
#endif
#if USING_ORACLE
				case Providers.Oracle:
					optionsBuilder.UseOracle(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
					break;
#endif
			}
		}
	}
}
