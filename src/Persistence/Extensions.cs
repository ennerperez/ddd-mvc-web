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
            // services.AddTransient<IGenericService<T>, GenericService<T>>();
            
            services.AddTransient<IGenericService<Setting>, SettingService>();
            
            services.AddTransient<IGenericService<Role>, RoleService>();
            services.AddTransient<IGenericService<RoleClaim>, RoleClaimService>();
            services.AddTransient<IGenericService<User>, UserService>();
            services.AddTransient<IGenericService<UserClaim>, UserClaimService>();
            services.AddTransient<IGenericService<UserLogin>, UserLoginService>();
            services.AddTransient<IGenericService<UserRole>, UserRoleService>();
            services.AddTransient<IGenericService<UserToken>, UserTokenService>();

            services.AddTransient<ISettingService, SettingService>();

            services.AddTransient<IRoleService, RoleService>();
            services.AddTransient<IRoleClaimService, RoleClaimService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserClaimService, UserClaimService>();
            services.AddTransient<IUserLoginService, UserLoginService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<IUserTokenService, UserTokenService>();

            return services;
        }
    }
}
