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
        
        public static IServiceCollection AddBusiness(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
        {
            var options = new DbContextOptionsBuilder();
            optionsBuilder?.Invoke(options);
            services.AddBusiness();
            return services;
        }

        public static IServiceCollection AddBusiness<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            var options = Activator.CreateInstance<T>();
            configureOptions?.Invoke(options);
            services.AddBusiness();
            return services;
        }

        public static IServiceCollection AddBusiness(this IServiceCollection services)
        {
#if USING_LOCALIZATION
            services.AddLocalization(options => options.ResourcesPath = "Resources");
#endif
            return services;
        }

        public static IServiceCollection WithMediatR(this IServiceCollection services)
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly(), Assembly.GetCallingAssembly() };
            services.AddValidatorsFromAssembly(assemblies[0]);
            services.AddValidatorsFromAssembly(assemblies[1]);
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            });
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
