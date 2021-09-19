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
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> e)
        {
            e.ToTable("Settings", DefaultConfiguration.Schema, true);

            e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Property(m => m.Key).HasMaxLength(Lengths.Code).IsRequired();
            e.Property(m => m.Type).HasDefaultValue(Data.Text).IsRequired();

            e.HasIndex(m => m.Key).IsUnique();
        }
    }
}
