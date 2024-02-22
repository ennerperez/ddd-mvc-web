using System;
using System.Reflection;
using Business.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;
using Persistence.Services;

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
            var options = new DbContextOptionsBuilder();
            optionsBuilder?.Invoke(options);
            services.AddBusiness();
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
            var options = Activator.CreateInstance<T>();
            configureOptions?.Invoke(options);
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
            var assemblies = new[]
            {
                Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly()
            };
            services.AddValidatorsFromAssembly(assemblies[0]);
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            });

#if USING_LOCALIZATION
            services.AddLocalization(options => options.ResourcesPath = "Resources");
#endif
            return services;
        }

        public static IServiceCollection WithRepositories(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient<ISettingRepository, SettingRepository>();
                    break;
                case ServiceLifetime.Singleton:
                    services.AddSingleton<ISettingRepository, SettingRepository>();
                    break;
                default:
                    services.AddScoped<ISettingRepository, SettingRepository>();
                    break;
            }
            return services;
        }
    }
}
