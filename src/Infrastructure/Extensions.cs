using System;
using System.Linq;
using System.Reflection;
#if USING_VAULT
using Azure.Security.KeyVault.Secrets;
#endif
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

			var assemblies = new[] {Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly()};
			var types = assemblies.Where(m => m != null).SelectMany(m => m.GetTypes()).ToArray();

			var userAccessorServiceType = types.FirstOrDefault(m => m.IsClass && typeof(IUserAccessorService).IsAssignableFrom(m));
			if (userAccessorServiceType != null)
				services.AddTransient(typeof(IUserAccessorService), userAccessorServiceType);

			services.AddHttpClient();

			services.AddTransient<IDocumentService, DocumentService>();

#if USING_BLOBS
			services.AddTransient<IFileService, FileService>();
			services.AddTransient<IDirectoryService, DirectoryService>();
#else
			services.AddSingleton<IFileService>(new FileSystemService() {ContainerName = "Data", CreateIfNotExists = true});
			services.AddSingleton<IDirectoryService>(new FileSystemService() {ContainerName = "Data", CreateIfNotExists = true});
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
