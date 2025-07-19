using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Conventions
{
    internal static class SoftDeleteEntitiesConventions
    {
        public static void AddSoftDeleteEntitiesConventions<T>(this ModelBuilder modelBuilder, string provider, string[] exclude = null)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            if (exclude == null || exclude.Length == 0)
            {
                exclude = [];
            }

            var items = modelBuilder.Model.GetEntityTypes().Where(m => !exclude.Contains(m.Name));
            foreach (var t in items)
            {
                var type = t.ClrType;
                if (!typeof(T).IsAssignableFrom(type))
                {
                    continue;
                }

                var parameter = Expression.Parameter(type, "p");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false)
                    ), parameter);
                t.SetQueryFilter(filter);
            }
#if DEBUG
            sw.Stop();
            Debug.WriteLine($"[INFO] - Elapsed time for soft delete entities conventions: {sw.Elapsed}");
#endif
        }
    }
}
