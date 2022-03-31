using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class RelationalEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>([NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder, string name, string schema, bool auto) where TEntity : class
        {
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
            if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("Sqlite"))
                context.Database.EnsureCreated();
            else
                context.Database.Migrate();
        }

        public static async Task TruncateAsync(this DbContext context, bool reseed = true, CancellationToken cancellationToken = default)
        {
            var database = context.Database;
            var entityTypes = context.Model.GetEntityTypes().ToArray();
            var query = string.Empty;
            if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("SqlServer"))
                query = string.Join(Environment.NewLine, entityTypes.Select(m =>
                {
                    var q1 = $"DBCC CHECKIDENT ('[{m.GetSchema()}].[{m.GetTableName()}]', RESEED, 0);";
                    if (!reseed) q1 = string.Empty;
                    var q0 = $"TRUNCATE TABLE [{m.GetSchema()}].[{m.GetTableName()}];";
                    return string.Join(Environment.NewLine, new[] { q0, q1 }.Where(q => !string.IsNullOrWhiteSpace(q)));
                }).ToArray());
            else if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("Sqlite"))
                query = string.Join(Environment.NewLine, entityTypes.Select(m =>
                {
                    var q1 = $"UPDATE sqlite_sequence SET seq=0 WHERE name='{m.GetTableName()}';";
                    if (!reseed) q1 = string.Empty;
                    var q0 = $"DELETE FROM {m.GetTableName()};";
                    return string.Join(Environment.NewLine, new[] { q0, q1 }.Where(q => !string.IsNullOrWhiteSpace(q)));
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
            if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("SqlServer"))
            {
                if (entityType != null)
                {
                    var q0 = $"DBCC CHECKIDENT ('[{entityType.GetSchema()}].[{entityType.GetTableName()}]', RESEED, 0);";
                    if (!reseed) q0 = string.Empty;
                    var q1 = $"TRUNCATE TABLE [{entityType.GetSchema()}].[{entityType.GetTableName()}];";
                    query = string.Join(Environment.NewLine, new[] { q0, q1 }.Where(q => !string.IsNullOrWhiteSpace(q)));
                }
            }
            else if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("Sqlite"))
            {
                if (entityType != null)
                {
                    var q0 = $"UPDATE sqlite_sequence SET seq=0 WHERE name='{entityType.GetTableName()}';";
                    if (!reseed) q0 = string.Empty;
                    var q1 = $"DELETE FROM {entityType.GetTableName()};";
                    query = string.Join(Environment.NewLine, new[] { q0, q1 }.Where(q => !string.IsNullOrWhiteSpace(q)));
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
            var query = string.Empty;
            if (context.Database.ProviderName != null && context.Database.ProviderName.EndsWith("SqlServer"))
                query = $"SET IDENTITY_INSERT [{entityType.GetSchema()}].[{entityType.GetTableName()}] {(enable ? "ON" : "OFF")};";

            if (!string.IsNullOrWhiteSpace(query))
                await database.ExecuteSqlRawAsync(query, cancellationToken);
        }
    }
}
