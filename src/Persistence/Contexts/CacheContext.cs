using System;
using System.Linq;
using Domain.Entities.Cache;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Conventions;

namespace Persistence.Contexts
{
    public class CacheContext : DbContext
    {
        public CacheContext()
        {
        }

        public CacheContext(DbContextOptions<CacheContext> options) : base(options)
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
            modelBuilder.AddSynchronizableEntitiesConventions<ISyncronizable>(ProviderName);
        }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder?.EnableDetailedErrors();
            optionsBuilder?.EnableSensitiveDataLogging();
        }
#endif

        #region DbSet

        public DbSet<Country> Countries { get; set; }

        #endregion DbSet
    }
}
