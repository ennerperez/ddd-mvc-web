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
using DbContextOptions = Microsoft.EntityFrameworkCore.DbContextOptions;

#if DEBUG
using System.IO;
#endif

#if SQLITE && USING_SQLITE
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
#endif

// ReSharper disable UnusedVariable
#pragma warning disable 219
#pragma warning disable 168

namespace Persistence.Contexts
{
    public sealed class DefaultContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
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
            if (_options == null)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                    .AddEnvironmentVariables()
                    .Build();

                UseDbEngine(optionsBuilder, config);
            }
        }
#endif

        #region DbSet

        public DbSet<Setting> Settings { get; set; }
        public DbSet<Client> Clients { get; set; }

        #endregion DbSet

        public static void UseDbEngine(DbContextOptionsBuilder optionsBuilder, IConfiguration config)
        {
#pragma warning disable 168
#pragma warning disable 219
            var migrationsHistoryTableName = "__EFMigrationsHistory";
            var connectionString = config.GetConnectionString(nameof(DefaultContext));
            DbConnectionStringBuilder csb;
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
#elif (MYSQL || MARIADB) && (USING_MYSQL && USING_MARIADB)
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            optionsBuilder.UseMySql(connectionString, serverVersion, x => x.MigrationsHistoryTable(migrationsHistoryTableName));
#elif POSTGRESQL && USING_POSTGRESQL
            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
#elif ORACLE && USING_ORACLE
            optionsBuilder.UseOracle(connectionString, x => x.MigrationsHistoryTable(migrationsHistoryTableName, Schemas.Migration));
#endif
        }
    }
}
