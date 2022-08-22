using System;
using System.Linq;
using System.Reflection;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class Extensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructure<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            services.AddInfrastructure();
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, SmtpService>();
            services.AddFromAssembly(Assembly.GetExecutingAssembly());
            
            services.AddHttpClient();

#if USING_BLOBS
            services.AddTransient<IFileService, FileService>();
#endif
#if USING_QUEUES
            services.AddTransient<IQueueService, QueueService>();
#endif
#if USING_TABLES
            services.AddTransient<ITableService, TableService>();
#endif
            return services;
        }

        #region FromAssembly

        private static void AddFromAssembly(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
            }

            var assembliesToScan = assemblies.Distinct().ToArray();
            services.ConnectImplementationsToTypesClosing(typeof(IDocumentService<>), assembliesToScan, false);
        }

        #endregion
    }
}
