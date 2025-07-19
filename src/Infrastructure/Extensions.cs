using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#if USING_VAULT
using Azure.Security.KeyVault.Secrets;
#endif
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure<T>(this IServiceCollection services, Action<T> configureOptions = null)
        {
            var options = Activator.CreateInstance<T>();
            configureOptions?.Invoke(options);
            services.AddInfrastructure();
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IEmailService, SmtpService>();

            var assemblies = new[] { Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() };

            services.ConnectImplementationsToTypesClosing(typeof(IUserContext), assemblies, false);
            services.ConnectImplementationsToTypesClosing(typeof(IUserContext<>), assemblies, false);

            services.AddHttpClient();

            services.AddTransient<IDocumentService, DocumentService>();

#if USING_BLOBS
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IDirectoryService, DirectoryService>();
#else
            services.AddSingleton<IFileService>(new FileSystemService() { ContainerName = "Data", CreateIfNotExists = true });
            services.AddSingleton<IDirectoryService>(new FileSystemService() { ContainerName = "Data", CreateIfNotExists = true });
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

#if USING_AUTH0
        public static IServiceCollection WithAuth0ApiManagement(this IServiceCollection services)
        {
            // services.AddSingleton<IAuth0ApiClientFactory, Auth0ApiClientFactory>();
            return services;
        }
#endif
    }
}
