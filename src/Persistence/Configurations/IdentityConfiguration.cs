using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Persistence.Contexts;

namespace Persistence.Configurations
{
    internal static class IdentityConfiguration
    {
        internal static string Schema => DefaultContext.HasSchema ? Schemas.Identity : string.Empty;
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> e)
        {
            e.ToTable("Roles", IdentityConfiguration.Schema, true);

            e.Property(m => m.Id).ValueGeneratedOnAdd();

            e.HasMany(m => m.RoleClaims)
                .WithOne(m => m.Role)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.UserRoles)
                .WithOne(m => m.Role)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
    {
        public void Configure(EntityTypeBuilder<RoleClaim> e)
        {
            e.ToTable("RolesClaims", IdentityConfiguration.Schema, true);
            e.Property(m => m.Id).ValueGeneratedOnAdd();
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> e)
        {
            e.ToTable("Users", IdentityConfiguration.Schema, true);

            e.Property(m => m.Id).ValueGeneratedOnAdd();

            e.HasMany(m => m.UserClaims)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.UserLogins)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.UserTokens)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(m => m.UserRoles)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
    {
        public void Configure(EntityTypeBuilder<UserClaim> e)
        {
            e.ToTable("UsersClaims", IdentityConfiguration.Schema, true);

            e.Property(m => m.Id).ValueGeneratedOnAdd();
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> e)
        {
            e.ToTable("UsersRoles", IdentityConfiguration.Schema, true);

            //e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Ignore(m => m.Id);
            e.HasKey(m => new { m.UserId, m.RoleId });
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
    {
        public void Configure(EntityTypeBuilder<UserLogin> e)
        {
            e.ToTable("UsersLogins", IdentityConfiguration.Schema, true);

            //e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Property(m => m.LoginProvider).HasMaxLength(Lengths.Memo);
            e.Property(m => m.ProviderKey).HasMaxLength(Lengths.Memo);

            e.Ignore(m => m.Id);
            e.HasKey(m => new { m.UserId, m.LoginProvider });
        }
    }

    [DbContext(typeof(DefaultContext))]
    public sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> e)
        {
            e.ToTable("UsersTokens", IdentityConfiguration.Schema, true);

            //e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.Property(m => m.Name).HasMaxLength(Lengths.Memo);
            e.Property(m => m.LoginProvider).HasMaxLength(Lengths.Memo);

            e.Ignore(m => m.Id);
            e.HasKey(m => new { m.UserId, m.LoginProvider, m.Name });
        }
    }
}
