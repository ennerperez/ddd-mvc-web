using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Business
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsBuilder"></param>
        /// <returns></returns>
        public static IServiceCollection AddBusiness(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
        {
            services.AddBusiness();
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
        public static IServiceCollection AddBusiness<T>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder, Action<T> configureOptions = null)
        {
            services.AddBusiness(configureOptions);
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddBusiness<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            services.AddBusiness();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBusiness(this IServiceCollection services)
        {
            return services;
        }
    }
}
