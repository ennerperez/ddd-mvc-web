using Microsoft.EntityFrameworkCore;

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
