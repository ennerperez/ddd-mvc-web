using System.Linq;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;

namespace Persistence.Conventions
{
    internal static class DomainEventsEntitiesConventions
    {
        public static void IgnoreDomainEventsEntitiesConventions(this ModelBuilder modelBuilder)
        {
            var items = modelBuilder.Model.GetEntityTypes().Where(m=> typeof(IDomainEvent).IsAssignableFrom(m.ClrType)).ToArray();
            foreach (var t in items)
            {
                var type = t.ClrType;
                if (typeof(IDomainEvent).IsAssignableFrom(type))
                    modelBuilder.Ignore(type);
            }
        }
    }
}
