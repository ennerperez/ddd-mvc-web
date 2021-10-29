using Microsoft.EntityFrameworkCore;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;

namespace Persistence.Conventions
{
    internal static class AuditableEntitiesConventions
    {
        public static void AddAuditableEntitiesConventions<T>(this ModelBuilder modelBuilder, string provider, string[] exclude = null)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            if (exclude == null || exclude.Length == 0)
                exclude = System.Array.Empty<string>();


            var defaultDateFunction = provider switch
            {
                "MySql" => "now()",
                "MariaDB" => "now()",
                "PostgreSQL" => "now()",
                "Sqlite" => "CURRENT_TIMESTAMP",
                _ => "getdate()"
            };

            var items = modelBuilder.Model.GetEntityTypes().Where(m => !exclude.Contains(m.Name));
            foreach (var t in items)
            {
                var type = t.ClrType;
                if (!typeof(T).IsAssignableFrom(type)) continue;
                var properties = t.GetProperties().ToArray();
                if (!properties.Any()) continue;

                var created = properties.First(m => m.Name == "CreatedAt");
                created.SetDefaultValueSql(defaultDateFunction);
                t.AddIndex(created);

                var modified = properties.First(m => m.Name == "ModifiedAt");
                t.AddIndex(modified);
            }

#if DEBUG
            sw.Stop();
            Debug.WriteLine($"[INFO] - Elapsed time for auditable entities conventions: {sw.Elapsed}");
#endif
        }
    }
}
