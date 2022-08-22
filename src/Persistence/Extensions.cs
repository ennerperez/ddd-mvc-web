using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Interfaces;

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
            DbContext = () => services.BuildServiceProvider().GetRequiredService<DefaultContext>();
            
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
            DbContext = () => services.BuildServiceProvider().GetRequiredService<DefaultContext>();
            
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
            DbContext = () => services.BuildServiceProvider().GetRequiredService<DefaultContext>();
            
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
            //TODO: Make this possible
            //services.AddTransient(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
            //services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }

        #region FromAssembly

        private static void AddFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");

            var assembliesToScan = assemblies.Distinct().ToArray();
            services.ConnectImplementationsToTypesClosing(typeof(IGenericRepository<,>), assembliesToScan, false);
            services.ConnectImplementationsToTypesClosing(typeof(IGenericRepository<>), assembliesToScan, false);
        }

        #endregion

        public static Func<DbContext> DbContext { get; set; }
    }
}
