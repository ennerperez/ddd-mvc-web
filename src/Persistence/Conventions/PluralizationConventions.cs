using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedType.Global

namespace Persistence.Conventions
{
    internal static class PluralizationConventions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!entity.IsOwned())
                {
                    entity.SetTableName(entity.DisplayName());
                }
            }
        }
    }
}
