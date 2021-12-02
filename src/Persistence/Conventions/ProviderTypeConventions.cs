using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;

namespace Persistence.Conventions
{
    internal class ProviderTypeOptions
    {
        public ProviderTypeOptions()
        {
            DecimalConfig = new Dictionary<int, string[]>();
            Exclude = new[] { "Identity" };
            UseDateTime = true;
        }

        public Dictionary<int, string[]> DecimalConfig { get; set; }
        public string Provider { get; set; }
        public string[] Exclude { get; set; }

        public bool UseDateTime { get; set; }
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

            options.Exclude ??= new string[0];

            // Fix datetime offset support for integration tests
            // See: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (new[] { "Sqlite" }.Contains(options.Provider))
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
                    var property = modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name);
                    property.HasConversion(new DateTimeOffsetToBinaryConverter());
                }

                var guidProperties = modelBuilder.Model.GetEntityTypes()
                    .Where(m => !m.IsOwned())
                    .SelectMany(m => m.GetProperties())
                    .Where(m => m.IsPrimaryKey() && m.ClrType == typeof(Guid) || m.ClrType == typeof(Guid?))
                    .ToArray();

                foreach (var p in guidProperties)
                {
                    var property = modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name);

                    if (p.IsColumnNullable())
                        property.HasDefaultValue(null);
                    //TODO: GUID from DB
                    // else
                    //     property.HasDefaultValue(Guid.NewGuid());
                }
            }

            // Generic Fields
            var items1 = modelBuilder.Model.GetEntityTypes().Where(m => !options.Exclude.Contains(m.Name)).SelectMany(t => t.GetProperties()).ToArray();
            foreach (var p in items1)
            {
                if (p.DeclaringEntityType.ClrType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.DeclaringEntityType.ClrType)) continue;
                var entity = modelBuilder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name);
                var columnType = p.GetColumnType();
                if (columnType != null) continue;
                if (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?))
                {
                    var precision = 2;
                    foreach (var (key, value) in options.DecimalConfig)
                        if (value.Contains(p.Name))
                            precision = key;

                    p.SetColumnType($"decimal(18,{precision})");
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
                    if (options.UseDateTime || (new[] { "Sqlite" }.Contains(options.Provider)))
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

                    if (new[] { "MySql", "MariaDB" }.Contains(options.Provider) && ((maxValue.HasValue && maxValue.Value > 500) || !maxValue.HasValue))
                        p.SetColumnType(max == "max" ? $"longtext" : $"text");
                    else if (new[] { "Sqlite" }.Contains(options.Provider))
                        p.SetColumnType(max != "max" ? $"varchar({max})" : $"varchar(500)");
                    else if (new[] { "PostgreSQL" }.Contains(options.Provider))
                        p.SetColumnType(max != "max" ? $"varchar({max})" : $"text");
                    else
                        p.SetColumnType($"varchar({max})");

                    columnType = p.GetColumnType();
                    entity.HasColumnType(columnType);
                }
                
                // See: https://stackoverflow.com/questions/8746207/1071-specified-key-was-too-long-max-key-length-is-1000-bytes
                if (new[] { "MySql", "MariaDB" }.Contains(options.Provider) && p.GetMaxLength() > 255)
                    p.SetMaxLength(255);
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine($"[INFO] - Elapsed time for provider type conventions: {sw.Elapsed}");
#endif
        }
    }
}
