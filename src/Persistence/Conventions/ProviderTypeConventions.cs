using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
#if USING_MSSQL
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
#endif

namespace Persistence.Conventions
{
    internal class ProviderTypeOptions
    {
        public Dictionary<int, string[]> DecimalConfig { get; set; } = new();
        public string Provider { get; set; }
        public string[] Exclude { get; set; } = ["Identity"];

        public bool UseDateTime { get; set; } = true;
        public string ClusteredColumn { get; set; } = "CreatedAt";
    }

    internal static class ProviderTypeConventions
    {
        public static void AddProviderTypeConventions(this ModelBuilder modelBuilder, Action<ProviderTypeOptions> optionsAction = null)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif

            var options = new ProviderTypeOptions();
            optionsAction?.Invoke(options);

            options.Exclude ??= [];

            // Fix datetime offset support for integration tests
            // See: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (new[] { DatabaseProviders.Sqlite }.Contains(options.Provider))
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Domain, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                var datetimeProperties = modelBuilder.Model.GetEntityTypes()
                    .Where(m => !m.IsOwned())
                    .SelectMany(m => m.GetProperties())
                    .Where(p => p.ClrType == typeof(DateTimeOffset) || p.ClrType == typeof(DateTimeOffset?))
                    .ToArray();
                foreach (var p in datetimeProperties)
                {
                    var property = modelBuilder.Entity(p.DeclaringType.ClrType).Property(p.Name);
                    property.HasConversion(new DateTimeOffsetToBinaryConverter());
                }

                var guidProperties = modelBuilder.Model.GetEntityTypes()
                    .Where(m => !m.IsOwned())
                    .SelectMany(m => m.GetProperties())
                    .Where(m => m.IsPrimaryKey() && m.ClrType == typeof(Guid) || m.ClrType == typeof(Guid?))
                    .ToArray();

                foreach (var p in guidProperties)
                {
                    var property = modelBuilder.Entity(p.DeclaringType.ClrType).Property(p.Name);
                    if (p.IsColumnNullable())
                    {
                        property.HasDefaultValue(null);
                    }
                }

                //TODO: Sqlite Temporal Patch
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties()
                        .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

                    foreach (var property in properties)
                    {
                        if (property.PropertyType == typeof(decimal))
                        {
                            modelBuilder
                                .Entity(entityType.Name)
                                .Property(property.Name)
                                .HasConversion<double>();
                        }
                        else if (property.PropertyType == typeof(decimal?))
                        {
                            modelBuilder
                                .Entity(entityType.Name)
                                .Property(property.Name)
                                .HasConversion<double?>();
                        }
                    }
                }
            }
            else if (new[] { DatabaseProviders.SqlServer }.Contains(options.Provider))
            {
                var entities = modelBuilder.Model.GetEntityTypes().Where(m => !m.IsOwned());

                foreach (var entity in entities)
                {
                    var pk = entity.FindPrimaryKey();
                    if (pk is not { Properties.Count: 1 } || !pk.Properties.Any(p => p.ClrType == typeof(Guid)))
                    {
                        continue;
                    }

#if USING_MSSQL
                    pk.SetAnnotation(SqlServerAnnotationNames.Clustered, false);
#endif
                    var ck = entity.FindProperty(options.ClusteredColumn);
                    if (ck == null || ck.ClrType != typeof(DateTime))
                    {
                        continue;
                    }

                    try
                    {
                        var ix = entity.FindIndex(ck);
                        if (ix != null)
                        {
#if USING_MSSQL
                            ix.SetAnnotation(SqlServerAnnotationNames.Clustered, true);
#endif
                        }
                        else
                        {
                            var name = $"IX_{entity.ClrType.Name}_{ck.Name}";
                            var clusteredPropertyIndex = ck;
#if USING_MSSQL
                            entity.AddIndex(clusteredPropertyIndex, name).SetAnnotation(SqlServerAnnotationNames.Clustered, true);
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            // Generic Fields
            var items1 = modelBuilder.Model.GetEntityTypes().Where(m => !options.Exclude.Contains(m.Name)).SelectMany(t => t.GetProperties()).ToArray();
            foreach (var p in items1)
            {
                if (p.DeclaringType.ClrType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.DeclaringType.ClrType))
                {
                    continue;
                }

                var entity = modelBuilder.Entity(p.DeclaringType.ClrType).Property(p.Name);
                var columnType = p.GetColumnType();
                if (columnType != null)
                {
                    continue;
                }

                if (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
                {
                    var precision = 2;
                    foreach (var (key, value) in options.DecimalConfig)
                    {
                        if (value.Contains(p.Name))
                        {
                            precision = key;
                        }
                    }

                    var dataType = $"decimal(18,{precision})";
                    if (new[] { DatabaseProviders.Sqlite }.Contains(options.Provider) && !options.DecimalConfig.SelectMany(m => m.Value).Contains(p.Name))
                    {
                        dataType = "double";
                    }

                    p.SetColumnType(dataType);

                    columnType = p.GetColumnType();
                    entity.HasColumnType(columnType);
                }
                else if (p.ClrType == typeof(float) || p.ClrType == typeof(float?))
                {
                    p.SetColumnType("float");
                    columnType = p.GetColumnType();
                    entity.HasColumnType(columnType);
                }
                else if ((p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
                {
                    if (options.UseDateTime || (new[] { DatabaseProviders.Sqlite }.Contains(options.Provider)))
                    {
                        p.SetColumnType("datetime");
                        columnType = p.GetColumnType();
                        entity.HasColumnType(columnType);
                    }
                }
                else if (p.ClrType == typeof(string))
                {
                    var maxValue = p.GetMaxLength();
                    var max = maxValue.HasValue ? maxValue.ToString() : "max";

                    if (new[] { DatabaseProviders.MySql, DatabaseProviders.MariaDb }.Contains(options.Provider) && (maxValue is > 500 || !maxValue.HasValue))
                    {
                        p.SetColumnType(max == "max" ? $"longtext" : $"text");
                    }
                    else if (new[] { DatabaseProviders.Sqlite }.Contains(options.Provider))
                    {
                        p.SetColumnType(max != "max" ? $"varchar({max})" : $"varchar(500)");
                    }
                    else if (new[] { DatabaseProviders.PostgreSql }.Contains(options.Provider))
                    {
                        p.SetColumnType(max != "max" ? $"varchar({max})" : $"text");
                    }
                    else
                    {
                        p.SetColumnType($"varchar({max})");
                    }

                    columnType = p.GetColumnType();
                    entity.HasColumnType(columnType);
                }

                // See: https://stackoverflow.com/questions/8746207/1071-specified-key-was-too-long-max-key-length-is-1000-bytes
                if (new[] { DatabaseProviders.MySql, DatabaseProviders.MariaDb }.Contains(options.Provider) && p.GetMaxLength() > 255)
                {
                    p.SetMaxLength(255);
                }
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine($"[INFO] - Elapsed time for provider type conventions: {sw.Elapsed}");
#endif
        }
    }
}
