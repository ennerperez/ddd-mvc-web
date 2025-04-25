using System;
using System.Linq;
using Domain.Entities.Cache;
using Domain.Interfaces;
#if USING_MULTITENANCY
using Infrastructure.Interfaces;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Persistence.Conventions;

namespace Persistence.Contexts
{
    public class CacheContext : DbContext
    {

#if USING_MULTITENANCY
        private readonly ITenantService _tenantService;
#endif
        private readonly IConfiguration _configuration;

        public CacheContext()
        {
        }

        public CacheContext(DbContextOptions<CacheContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

#if USING_MULTITENANCY
        public CacheContext(DbContextOptions<CacheContext> options, IConfiguration configuration, ITenantService tenantService) : base(options)
        {
            _configuration = configuration;
            _tenantService = tenantService;
        }
#endif

        private static string ProviderName { get; set; }
        internal static bool HasSchema => DbContextExtensions.HasSchema(ProviderName);

        #region DbSet

        public DbSet<Country> Countries { get; set; }

        #endregion DbSet

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
                m.DecimalConfig.Add(6, ["Lat", "Long"]);
                m.Exclude = null;
                m.UseDateTime = false;
            });
            modelBuilder.AddAuditableEntitiesConventions<IAuditable>(ProviderName);
            modelBuilder.AddSynchronizableEntitiesConventions<ISynchronizable>(ProviderName);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder?.EnableDetailedErrors();
            optionsBuilder?.EnableSensitiveDataLogging();
#endif

            if (optionsBuilder.IsConfigured)
            {
                return;
            }
#if USING_MULTITENANCY
            if (_tenantService != null)
            {
                var tenant = _tenantService.Tenant ?? _tenantService.GetTenants(nameof(CacheContext)).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(tenant))
                {
                    optionsBuilder.UseMultiTenantDbEngine(_configuration, tenant);
                }
            }
#endif
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseDbEngine(_configuration);
            }

        }
    }
}
