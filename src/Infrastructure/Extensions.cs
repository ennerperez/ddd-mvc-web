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

			var assemblies = new[] { Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() };
			var types = assemblies.SelectMany(m => m.GetTypes()).ToArray();
			
			var identityServiceType = types.FirstOrDefault(m=> m.IsClass && typeof(IIdentityService).IsAssignableFrom(m));
			if (identityServiceType != null)
				services.AddTransient(typeof(IIdentityService), identityServiceType);
			
			var documentServiceType = types.FirstOrDefault(m=> m.IsClass && typeof(IDocumentService).IsAssignableFrom(m));
			if (documentServiceType != null)
				services.AddTransient(typeof(IDocumentService), documentServiceType);
			
			var documentServiceType2 = types.FirstOrDefault(m=> m.IsClass && typeof(IDocumentService<IDocument>).IsAssignableFrom(m));
			if (documentServiceType2 != null)
				services.AddTransient(typeof(IDocumentService<IDocument>), documentServiceType2);
			
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
#if USING_VAULT
            services.AddTransient<IVaultService<KeyVaultSecret>, VaultService>();
#endif
			return services;
		}
	}
}
