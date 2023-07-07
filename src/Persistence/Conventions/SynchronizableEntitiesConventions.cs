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
                if (!properties.Any())
                {
                    continue;
                }

                var key = properties.First(m => m.Name == "RowKey");
                var version = properties.First(m => m.Name == "RowVersion");

                var keyProperty = modelBuilder.Entity(key.DeclaringEntityType.ClrType).Property(key.Name);
                var versionProperty = modelBuilder.Entity(key.DeclaringEntityType.ClrType).Property(version.Name);

                keyProperty.ValueGeneratedOnAdd();
                versionProperty.IsRowVersion().ValueGeneratedOnAddOrUpdate();
            }
        }
    }
}
