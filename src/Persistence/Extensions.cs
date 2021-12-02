using System;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Interfaces;
using Persistence.Services;

namespace Persistence
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static IServiceCollection AddPersistence(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
        {
            services.AddDbContext<DefaultContext>(optionsBuilder);
            services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder, Action<T> configureOptions = null)
        {
            services.AddDbContext<DefaultContext>(optionsBuilder);
		    services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence(configureOptions);
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddPersistence<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            services.AddDbContext<DefaultContext>();
            services.AddTransient<DbContext, DefaultContext>();
            services.AddPersistence();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            // services.AddTransient<IGenericRepository<T>, GenericRepository<T>>();
            
            services.AddTransient<IGenericRepository<Setting>, SettingRepository>();
            
            services.AddTransient<IGenericRepository<Role>, RoleRepository>();
            services.AddTransient<IGenericRepository<RoleClaim>, RoleClaimRepository>();
            services.AddTransient<IGenericRepository<User>, UserRepository>();
            services.AddTransient<IGenericRepository<UserClaim>, UserClaimRepository>();
            services.AddTransient<IGenericRepository<UserLogin>, UserLoginRepository>();
            services.AddTransient<IGenericRepository<UserRole>, UserRoleRepository>();
            services.AddTransient<IGenericRepository<UserToken>, UserTokenRepository>();

            services.AddTransient<ISettingRepository, SettingRepository>();

            services.AddTransient<IRoleRepository, RoleRepository>();
            services.AddTransient<IRoleClaimRepository, RoleClaimRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserClaimRepository, UserClaimRepository>();
            services.AddTransient<IUserLoginRepository, UserLoginRepository>();
            services.AddTransient<IUserRoleRepository, UserRoleRepository>();
            services.AddTransient<IUserTokenRepository, UserTokenRepository>();

            return services;
        }
    }
}
