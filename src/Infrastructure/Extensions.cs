using System;
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
            
#if USING_BLOBS
            services.AddTransient<IFileService, FileService>();
#endif
#if USING_QUEUES
            services.AddTransient<IQueueService, QueueService>();
#endif
            return services;
        }
    }
}
