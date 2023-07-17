using System;
using System.Linq;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Conventions;
#if USING_IDENTITY
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif

namespace Persistence.Contexts
{
    public class DefaultContext :
#if USING_IDENTITY
        IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
#else
        DbContext
#endif
    {
        public DefaultContext()
        {
        }

        public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
        {
        }

        private static string ProviderName { get; set; }
        internal static bool HasSchema => DbContextExtensions.HasSchema(ProviderName);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            ProviderName = Database.ProviderName?.Split('.').Last();

            // Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly, m => m.GetCustomAttributes(typeof(DbContextAttribute), true).OfType<DbContextAttribute>().Any(a => a.ContextType == GetType()));

            // Conventions
            // modelBuilder.RemovePluralizingTableNameConvention();
            modelBuilder.AddProviderTypeConventions(m =>
            {
                m.Provider = ProviderName;
                m.DecimalConfig.Add(6, new[] { "Lat", "Long" });
                m.Exclude = null;
                m.UseDateTime = false;
            });
            modelBuilder.AddAuditableEntitiesConventions<IAuditable>(ProviderName);
            modelBuilder.AddSynchronizableEntitiesConventions<ISyncronizable>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder?.EnableDetailedErrors();
            optionsBuilder?.EnableSensitiveDataLogging();
#endif
        }

        #region DbSet

        public DbSet<Setting> Settings { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        #endregion DbSet
    }
}
