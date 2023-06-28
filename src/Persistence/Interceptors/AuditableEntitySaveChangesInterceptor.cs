using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors
{

    //TODO: Interceptors
    public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public static void UpdateEntities(DbContext context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Entity.ModifiedAt = DateTime.Now;
                }
            }
        }
    }

    public class ExtendedAuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IUserAccessorService _userAccessorService;

        public ExtendedAuditableEntitySaveChangesInterceptor(IUserAccessorService userAccessorService)
        {
            _userAccessorService = userAccessorService;
        }
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<IExtendedAuditable>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedById = int.Parse((_userAccessorService.GetActiveUser() as ClaimsPrincipal).GetUserId());
                }
                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Entity.ModifiedById = int.Parse((_userAccessorService.GetActiveUser() as ClaimsPrincipal).GetUserId());
                }
            }
        }
    }

}
