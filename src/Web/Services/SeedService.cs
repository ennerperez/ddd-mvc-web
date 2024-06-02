using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;
#if USING_DATABASE_PROVIDER
using Domain.Entities;
using Domain.Entities.Cache;
#endif
#if USING_IDENTITY
using Domain.Entities.Identity;
#endif

// ReSharper disable UnusedMember.Local

namespace Web.Services
{
    public class SeedService : IHostedService
    {
        private readonly ILogger _logger;

        // ReSharper disable once NotAccessedField.Local
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

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task FromLocal<T>(DbContext context, string source = "Data", int minRows = 0, CancellationToken cancellationToken = default) where T : class
        {
            var dbSet = context.Set<T>();
            var hasRow = await dbSet.AnyAsync(cancellationToken);
            if (minRows > 0)
            {
                hasRow = await dbSet.CountAsync(cancellationToken) > minRows;
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
                        var entities = JsonSerializer.Deserialize<List<T>>(responseBody);
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

        private async Task FromMockaroo<T>(DefaultContext context, string id, int count, string key, string source = "Data", CancellationToken cancellationToken = default) where T : class
        {
            var dbSet = context.Set<T>();
            if (!await dbSet.AnyAsync(cancellationToken))
            {
                try
                {
                    List<T> entities;
                    if (!Directory.Exists(source) && source != null)
                    {
                        Directory.CreateDirectory(source);
                    }

                    var targetFile = Path.Combine(source ?? string.Empty, $"{id}.json");
                    if (!File.Exists(targetFile))
                    {
                        using var client = new HttpClient();
                        var url = $"https://api.mockaroo.com/api/{id}?count={count}&key={key}";
                        var response = await client.GetAsync(url, cancellationToken);
                        response.EnsureSuccessStatusCode();
                        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        await File.WriteAllTextAsync(targetFile, responseBody, cancellationToken);
                        entities = JsonSerializer.Deserialize<List<T>>(responseBody);
                    }
                    else
                    {
                        var responseBody = await File.ReadAllTextAsync(targetFile, cancellationToken);
                        entities = JsonSerializer.Deserialize<List<T>>(responseBody);
                    }

                    if (entities != null && entities.Any())
                    {
                        dbSet.AddRange(entities);
                        await context.SaveChangesWithIdentityInsertAsync<T>(cancellationToken);
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
    }
}
