using Microsoft.EntityFrameworkCore;
using Persistence.Conventions;
using System;
using System.Linq;
using Domain.Entities;
using Domain.Interfaces;

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

			ProviderName = Database.ProviderName?.Split('.').Last();

			// Configurations
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(DefaultContext).Assembly, m => m.GetCustomAttributes(typeof(DbContextAttribute), true).OfType<DbContextAttribute>().Any(a => a.ContextType == typeof(DefaultContext)));

			// Conventions
			// modelBuilder.RemovePluralizingTableNameConvention();
			modelBuilder.AddProviderTypeConventions(m =>
			{
				m.Provider = ProviderName;
				m.DecimalConfig.Add(6, new[] {"Lat", "Long"});
				m.Exclude = null;
				m.UseDateTime = false;
			});
			modelBuilder.AddAuditableEntitiesConventions<IAuditable>(ProviderName);
			modelBuilder.AddSynchronizableEntitiesConventions<ISyncronizable>(ProviderName);
		}

		#region DbSet

		public DbSet<Setting> Settings { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<Budget> Budgets { get; set; }

		#endregion DbSet

	}
}
