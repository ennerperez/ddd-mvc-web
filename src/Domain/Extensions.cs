using System;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementor"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection ChainInterfaceImplementation<TInterface, TImplementor>(this IServiceCollection services) where TInterface : class where TImplementor : TInterface
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            return services.AddScoped<TInterface>(provider => provider.GetService<TImplementor>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddDomain<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            services.AddDomain();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            return services;
        }
    }
}
