using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if USING_SQLITE
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
#endif
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{

	public static class DatabaseProviders
	{
		public const string Sqlite = "Sqlite";
		public const string SqlServer = "SqlServer";
		public const string MariaDb = "MariaDB";
		public const string MySql = "MySql";
		public const string PostgreSql = "PostgreSQL";
		public const string Oracle = "Oracle";
	}

	public static partial class Schemas
	{
		public const string Migration = "db_migrations";
		public const string Identity = "db_identity";
		public const string Default = "db_default";
	}

	public static class RelationalEntityTypeBuilderExtensions
	{
		public static EntityTypeBuilder<TEntity> ToTable<TEntity>([NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder, string name = "", string schema = "", bool auto = true) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(name)) name = nameof(TEntity);
			if (!auto) return entityTypeBuilder.ToTable(name, schema);
			return string.IsNullOrEmpty(schema) ? entityTypeBuilder.ToTable(name) : entityTypeBuilder.ToTable(name, schema);
		}

		public static EntityTypeBuilder<TEntity> ToTable<TEntity>([NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder, string name = "", string schema = "", string prefix = "", bool auto = true) where TEntity : class
		{
			if (string.IsNullOrWhiteSpace(name)) name = nameof(TEntity);
			if (!string.IsNullOrWhiteSpace(prefix)) name = $"{prefix}_{name}";
			if (!auto) return entityTypeBuilder.ToTable(name, schema);
			return string.IsNullOrEmpty(schema) ? entityTypeBuilder.ToTable(name) : entityTypeBuilder.ToTable(name, schema);
		}
	}

	public static class DbContextExtensions
	{
		public static void Rollback(this DbContext context)
		{
			var changedEntriesCopy = context.ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added ||
				            e.State == EntityState.Modified ||
				            e.State == EntityState.Deleted)
				.ToList();

			foreach (var entry in changedEntriesCopy)
				entry.State = EntityState.Detached;
		}

		public static void Initialize(this DbContext context)
		{
			context.Database.Migrate();
		}

		public static async Task TruncateAsync(this DbContext context, bool reseed = true, CancellationToken cancellationToken = default)
		{
			var database = context.Database;
			var entityTypes = context.Model.GetEntityTypes().ToArray();
			var query = string.Empty;
			if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith(DatabaseProviders.SqlServer))
				query = string.Join(Environment.NewLine, entityTypes.Select(m =>
				{
					var q1 = $"DBCC CHECKIDENT ('[{m.GetSchema()}].[{m.GetTableName()}]', RESEED, 0);";
					if (!reseed) q1 = string.Empty;
					var q0 = $"TRUNCATE TABLE [{m.GetSchema()}].[{m.GetTableName()}];";
					return string.Join(Environment.NewLine, new[] {q0, q1}.Where(q => !string.IsNullOrWhiteSpace(q)));
				}).ToArray());
			else if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith(DatabaseProviders.Sqlite))
				query = string.Join(Environment.NewLine, entityTypes.Select(m =>
				{
					var q1 = $"UPDATE sqlite_sequence SET seq=0 WHERE name='{m.GetTableName()}';";
					if (!reseed) q1 = string.Empty;
					var q0 = $"DELETE FROM {m.GetTableName()};";
					return string.Join(Environment.NewLine, new[] {q0, q1}.Where(q => !string.IsNullOrWhiteSpace(q)));
				}).ToArray());

			if (!string.IsNullOrWhiteSpace(query))
			{
				await using var transaction = await database.BeginTransactionAsync(cancellationToken);
				await database.ExecuteSqlRawAsync(query, cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
		}

		public static async Task TruncateAsync<TEntity>(this DbContext context, bool reseed = true, CancellationToken cancellationToken = default) where TEntity : class
		{
			var database = context.Database;
			var entityType = context.Model.FindEntityType(typeof(TEntity));
			var query = string.Empty;
			if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith(DatabaseProviders.SqlServer))
			{
				if (entityType != null)
				{
					var q0 = $"DBCC CHECKIDENT ('[{entityType.GetSchema()}].[{entityType.GetTableName()}]', RESEED, 0);";
					if (!reseed) q0 = string.Empty;
					var q1 = $"TRUNCATE TABLE [{entityType.GetSchema()}].[{entityType.GetTableName()}];";
					query = string.Join(Environment.NewLine, new[] {q0, q1}.Where(q => !string.IsNullOrWhiteSpace(q)));
				}
			}
			else if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith(DatabaseProviders.Sqlite))
			{
				if (entityType != null)
				{
					var q0 = $"UPDATE sqlite_sequence SET seq=0 WHERE name='{entityType.GetTableName()}';";
					if (!reseed) q0 = string.Empty;
					var q1 = $"DELETE FROM {entityType.GetTableName()};";
					query = string.Join(Environment.NewLine, new[] {q0, q1}.Where(q => !string.IsNullOrWhiteSpace(q)));
				}
			}

			if (!string.IsNullOrWhiteSpace(query))
			{
				await using var transaction = await database.BeginTransactionAsync(cancellationToken);
				await database.ExecuteSqlRawAsync(query, cancellationToken);
				await transaction.CommitAsync(cancellationToken);
			}
		}

		public static void Clear<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class
		{
			dbSet.RemoveRange(dbSet);
		}

		public static async Task<int> SaveChangesWithIdentityInsertAsync<TEntity>(this DbContext context, CancellationToken cancellationToken = default)
		{
			var database = context.Database;
			await using var transaction = await database.BeginTransactionAsync(cancellationToken);
			await SetIdentityInsertAsync<TEntity>(context, true, cancellationToken);
			var result = await context.SaveChangesAsync(cancellationToken);
			await SetIdentityInsertAsync<TEntity>(context, false, cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			return result;
		}

		private static async Task SetIdentityInsertAsync<TEntity>(this DbContext context, bool enable, CancellationToken cancellationToken)
		{
			var database = context.Database;
			var entityType = context.Model.FindEntityType(typeof(TEntity));

			if (entityType != null)
			{
				var keys = entityType.GetKeys().ToArray();
				var types = keys.Select(m => m.GetKeyType());
				var isValueGenerated = keys.SelectMany(s => s.Properties).Any(m => m.ValueGenerated == ValueGenerated.OnAdd && !types.Any(s => s == typeof(Guid)));
				if (isValueGenerated)
				{
					var query = string.Empty;
					if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith(DatabaseProviders.SqlServer))
						query = $"SET IDENTITY_INSERT [{entityType.GetSchema()}].[{entityType.GetTableName()}] {(enable ? "ON" : "OFF")};";

					if (!string.IsNullOrWhiteSpace(query))
						await database.ExecuteSqlRawAsync(query, cancellationToken);
				}
			}
		}

		public static bool HasSchema(this DbContext context)
		{
			var providerName = context.Database.ProviderName?.Split('.').Last();
			return HasSchema(providerName);
		}
		public static bool HasSchema(string providerName)
		{
			return !(new[] {DatabaseProviders.Sqlite, DatabaseProviders.MySql, DatabaseProviders.MariaDb}).Contains(providerName);
		}

		public static void UseDbEngine(this DbContextOptionsBuilder optionsBuilder, IConfiguration config, string contextName = "", string providerName = "")
		{
			if (string.IsNullOrWhiteSpace(contextName))
				contextName = optionsBuilder.Options.ContextType.Name.Split(".").First();

			if (string.IsNullOrWhiteSpace(providerName))
			{
				var connectionStrings = new Dictionary<string, string>();
				config.Bind("ConnectionStrings", connectionStrings);
				connectionStrings = connectionStrings
					.Where(m => m.Key.StartsWith(contextName))
					.ToDictionary(k => k.Key, v => v.Value);
				if (connectionStrings.Count == 0)
					throw new InvalidDataException($"Unable to found a connection string for {contextName}");
				if (connectionStrings.Count != 1)
					throw new InvalidDataException($"The context {contextName} has more than one connection string");

				providerName = connectionStrings.First().Key.Split("_").Last();
			}
			var connectionString = config.GetConnectionString($"{contextName}_{providerName}");

#pragma warning disable 168
#pragma warning disable 219
#if USING_DATABASE_PROVIDER
			const string MigrationsHistoryTableName = "__EFMigrationsHistory";
			if (string.IsNullOrWhiteSpace(connectionString))
				connectionString = config.GetConnectionString(contextName);
#endif
			switch (providerName)
			{
#if USING_SQLITE
				case DatabaseProviders.Sqlite:
#pragma warning restore 219
#pragma warning restore 168
					DbConnectionStringBuilder csb = new SqliteConnectionStringBuilder() {ConnectionString = connectionString};
					var dbPath = Regex.Match(csb.ConnectionString.ToLower(), "(data source ?= ?)(.*)(;?)").Groups[2].Value;
					var dbPathExpanded = Environment.ExpandEnvironmentVariables(dbPath);
					csb.ConnectionString = csb.ConnectionString.Replace(dbPath, dbPathExpanded);
					optionsBuilder.UseSqlite(csb.ConnectionString, x => x.MigrationsHistoryTable(MigrationsHistoryTableName));
					break;
#endif
#if USING_MSSQL
				case DatabaseProviders.SqlServer:
					optionsBuilder.UseSqlServer(connectionString, x => x.MigrationsHistoryTable(MigrationsHistoryTableName, Schemas.Migration));
					break;
#endif
#if (USING_MYSQL) || (USING_MARIADB)
				case DatabaseProviders.MariaDb:
				case DatabaseProviders.MySql:
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
					optionsBuilder.UseMySql(connectionString, serverVersion, x => x.MigrationsHistoryTable(MigrationsHistoryTableName));
					break;
#endif
#if USING_POSTGRESQL
				case DatabaseProviders.PostgreSql:
					optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable(MigrationsHistoryTableName, Schemas.Migration));
					break;
#endif
#if USING_ORACLE
				case DatabaseProviders.Oracle:
					optionsBuilder.UseOracle(connectionString, x => x.MigrationsHistoryTable(MigrationsHistoryTableName, Schemas.Migration));
					break;
#endif
				default:
					break;
			}
		}
	}
}
