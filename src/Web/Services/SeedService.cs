using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
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
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<DefaultContext>();
                context.Initialize();
                if (context == null || !context.Database.CanConnect()) return;

                try
                {
                    await FromLocal<Setting>(context, cancellationToken: cancellationToken);
                    await FromLocal<Role>(context, cancellationToken: cancellationToken);
                    await FromLocal<User>(context, cancellationToken: cancellationToken);
                    await FromLocal<UserRole>(context, cancellationToken: cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }

            await StopAsync(cancellationToken);
        }
        
        private async Task FromLocal<T>(DefaultContext context, string source = "Data", CancellationToken cancellationToken = default) where T : class
        {
            var dbSet = context.Set<T>();
            if (!await dbSet.AnyAsync())
            {
                try
                {
                    List<T> entities;
                    if (!Directory.Exists(source)) Directory.CreateDirectory(source);
                    var targetFile = Path.Combine(source, $"{typeof(T).Name}.json");
                    if (File.Exists(targetFile))
                    {
                        var responseBody = await File.ReadAllTextAsync(targetFile);
                        entities = System.Text.Json.JsonSerializer.Deserialize<List<T>>(responseBody);
                        if (entities != null && entities.Any())
                        {
                            dbSet.AddRange(entities);
                            await context.SaveChangesWithIdentityInsertAsync<T>(cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                finally
                {
                    context.Database.CloseConnection();
                }
            }
        }

        private async Task FromMockaroo<T>(DefaultContext context, string id, int count, string key, string source = "Data", CancellationToken cancellationToken = default) where T : class
        {
            var dbSet = context.Set<T>();
            if (!await dbSet.AnyAsync())
            {
                try
                {
                    List<T> entities;
                    if (!Directory.Exists(source)) Directory.CreateDirectory(source);
                    var targetFile = Path.Combine(source, $"{id}.json");
                    if (!File.Exists(targetFile))
                    {
                        using (var client = new HttpClient())
                        {
                            var url = $"https://api.mockaroo.com/api/{id}?count={count}&key={key}";
                            var response = await client.GetAsync(url);
                            response.EnsureSuccessStatusCode();
                            var responseBody = await response.Content.ReadAsStringAsync();
                            await File.WriteAllTextAsync(targetFile, responseBody);
                            entities = System.Text.Json.JsonSerializer.Deserialize<List<T>>(responseBody);
                        }
                    }
                    else
                    {
                        var responseBody = await File.ReadAllTextAsync(targetFile);
                        entities = System.Text.Json.JsonSerializer.Deserialize<List<T>>(responseBody);
                    }

                    if (entities != null && entities.Any())
                    {
                        dbSet.AddRange(entities);
                        await context.SaveChangesWithIdentityInsertAsync<T>(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                finally
                {
                    context.Database.CloseConnection();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
