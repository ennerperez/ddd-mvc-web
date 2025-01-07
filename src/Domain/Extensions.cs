using System;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class Extensions
    {
        public static IServiceCollection ChainInterfaceImplementation<TInterface, TImplementor>(this IServiceCollection services) where TInterface : class where TImplementor : TInterface
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScoped<TInterface>(provider => provider.GetService<TImplementor>());
        }

        public static IServiceCollection AddDomain<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            var options = Activator.CreateInstance<T>();
            configureOptions?.Invoke(options);
            services.AddDomain();
            return services;
        }

        public static IServiceCollection AddDomain(this IServiceCollection services) => services;
    }
}
