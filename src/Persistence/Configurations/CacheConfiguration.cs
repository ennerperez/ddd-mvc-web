using Domain.Entities.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Contexts;

namespace Persistence.Configurations
{
    internal static class CacheConfiguration
    {
        internal static string Schema => CacheContext.HasSchema ? Schemas.Default : string.Empty;
        internal static string Prefix => !CacheContext.HasSchema ? Schemas.Default : string.Empty;
    }

    [DbContext(typeof(CacheContext))]
    public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> e)
        {
            e.ToTable("Countries", CacheConfiguration.Schema, CacheConfiguration.Prefix);

            e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Property(m => m.ISO3166).HasMaxLength(Lengths.ISO3166).IsRequired();
            e.Property(m => m.Name).HasDefaultValue(Lengths.Name).IsRequired();

            e.HasIndex(m => m.ISO3166).IsUnique();
            e.HasIndex(m => m.Name);
        }
    }
}
