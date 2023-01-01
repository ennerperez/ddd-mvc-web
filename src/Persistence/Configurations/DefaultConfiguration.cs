using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Contexts;

namespace Persistence.Configurations
{
	internal static class DefaultConfiguration
	{
		internal static string Schema => DefaultContext.HasSchema ? Schemas.Default : string.Empty;
		internal static string Prefix => !DefaultContext.HasSchema ? Schemas.Default : string.Empty;
	}

	[DbContext(typeof(DefaultContext))]
	public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
	{
		public void Configure(EntityTypeBuilder<Setting> e)
		{
			e.ToTable("Settings", DefaultConfiguration.Schema, DefaultConfiguration.Prefix);

			e.Property(m => m.Id).ValueGeneratedOnAdd();
			e.Property(m => m.Key).HasMaxLength(Lengths.Code).IsRequired();
			e.Property(m => m.Type).HasDefaultValue(Data.Text).IsRequired();

			e.HasIndex(m => m.Key).IsUnique();
		}
	}

	[DbContext(typeof(DefaultContext))]
	public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
	{
		public void Configure(EntityTypeBuilder<Client> e)
		{
			e.ToTable("Clients", DefaultConfiguration.Schema, DefaultConfiguration.Prefix);

			e.Property(m => m.Id).ValueGeneratedOnAdd();
			e.Property(m => m.Identification).HasMaxLength(Lengths.Code).IsRequired();
			e.Property(m => m.FullName).HasDefaultValue(Data.Text).IsRequired();

			e.HasIndex(m => m.Identification).IsUnique();

			e.HasMany(m => m.Budgets)
				.WithOne(m => m.Client)
				.HasForeignKey(m => m.ClientId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}

	[DbContext(typeof(DefaultContext))]
	public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
	{
		public void Configure(EntityTypeBuilder<Budget> e)
		{
			e.ToTable("Budgets", DefaultConfiguration.Schema, DefaultConfiguration.Prefix);

			e.Property(m => m.Id).ValueGeneratedOnAdd();
			e.Property(m => m.Code).HasMaxLength(Lengths.Code).IsRequired();
			e.Property(m => m.Status).IsRequired();

			e.HasIndex(m => m.Code).IsUnique();
			e.HasIndex(m => m.Status);
			e.HasIndex(m => m.ExpireAt);
		}
	}
}
