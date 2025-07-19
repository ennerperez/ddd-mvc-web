using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Conventions
{
    internal static class SynchronizableEntitiesConventions
    {
        public static void AddSynchronizableEntitiesConventions<T>(this ModelBuilder modelBuilder, string provider)
        {
            var items = modelBuilder.Model.GetEntityTypes();
            foreach (var t in items)
            {
                var type = t.ClrType;
                if (!typeof(T).IsAssignableFrom(type))
                {
                    continue;
                }

                var properties = t.GetProperties().ToArray();
                if (properties.Length == 0)
                {
                    continue;
                }

                var key = properties.FirstOrDefault(m => m.Name == "RowKey");
                var version = properties.FirstOrDefault(m => m.Name == "RowVersion");
                if (key != null)
                {
                    var keyProperty = modelBuilder.Entity(key.DeclaringType.ClrType).Property(key.Name);
                    keyProperty.ValueGeneratedOnAdd();
                }

                if (version == null)
                {
                    continue;
                }

                var versionProperty = modelBuilder.Entity(version.DeclaringType.ClrType).Property(version.Name);
                versionProperty.IsRowVersion().ValueGeneratedOnAddOrUpdate();
            }
        }
    }
}
