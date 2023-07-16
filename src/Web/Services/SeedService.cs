using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Entities.Cache;
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

// ReSharper disable UnusedMember.Local

namespace Web.Services
{
    public class SeedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SeedService(ILoggerFactory logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger.CreateLogger(GetType());
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
#if USING_DATABASE_PROVIDER
            using (var scope = _scopeFactory.CreateScope())
            {
                DbContext defaultContext = scope.ServiceProvider.GetService<DefaultContext>();
                defaultContext.Initialize();
                if (defaultContext == null || !await defaultContext.Database.CanConnectAsync(cancellationToken))
                {
                    return;
                }

                var cacheContext = scope.ServiceProvider.GetService<CacheContext>();
                cacheContext.Initialize();
                if (cacheContext == null || !await cacheContext.Database.CanConnectAsync(cancellationToken))
                {
                    return;
                }

                try
                {
#if DEBUG
                    await FromLocal<Setting>(defaultContext, cancellationToken: cancellationToken);
                    await FromLocal<Country>(cacheContext, cancellationToken: cancellationToken);
#if USING_IDENTITY
                    await FromLocal<Role>(defaultContext, cancellationToken: cancellationToken);
                    await FromLocal<User>(defaultContext, cancellationToken: cancellationToken);
                    await FromLocal<UserRole>(defaultContext, cancellationToken: cancellationToken);
                    await FromLocal<UserClaim>(defaultContext, cancellationToken: cancellationToken);
#endif
                    await FromLocal<Client>(defaultContext, cancellationToken: cancellationToken);
#endif
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Message}", e.Message);
                }
            }
#endif
            await StopAsync(cancellationToken);
        }

        private async Task FromLocal<T>(DbContext context, string source = "Data", int minRows = 0, CancellationToken cancellationToken = default) where T : class
        {
            var dbSet = context.Set<T>();
            var hasRow = await dbSet.AnyAsync(cancellationToken: cancellationToken);
            if (minRows > 0)
            {
                hasRow = await dbSet.CountAsync(cancellationToken: cancellationToken) > minRows;
            }

            if (!hasRow)
            {
                try
                {
                    if (!Directory.Exists(source) && source != null)
                    {
                        Directory.CreateDirectory(source);
                    }

                    var targetFile = Path.Combine(source ?? string.Empty, $"{typeof(T).Name}.json");
                    if (File.Exists(targetFile))
                    {
                        var responseBody = await File.ReadAllTextAsync(targetFile, cancellationToken);
                        var entities = System.Text.Json.JsonSerializer.Deserialize<List<T>>(responseBody);
                        if (entities != null && entities.Any())
                        {
                            dbSet.AddRange(entities);
                            await context.SaveChangesWithIdentityInsertAsync<T>(cancellationToken);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Message}", e.Message);
                }
                finally
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
