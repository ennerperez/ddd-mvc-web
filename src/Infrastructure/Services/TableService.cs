#if USING_TABLES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;

namespace Infrastructure.Services
{
    public class TableService : ITableService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly List<TableClient> _clients;

        public TableService(IConfiguration configuration, ILoggerFactory logger)
        {
            _configuration = configuration;
            _logger = logger.CreateLogger(GetType());
            _clients = new List<TableClient>();
        }

        public string TableName { get; set; }
        public bool CreateIfNotExists { get; set; }

        private async Task<TableClient> GetClientAsync(string tableName = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tableName)) tableName = TableName;
            var client = _clients.FirstOrDefault(c => c.Name == tableName);
            if (client != null)
                return client;

            try
            {
                _logger.LogInformation("Initializing [{TableName}] table client", tableName);
                var connectionString = _configuration["AzureSettings:Storage:ConnectionString"];
                client = new TableClient(connectionString, tableName);
                if (CreateIfNotExists)
                {
                    await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
                    _clients.Add(client);
                }
                else
                    client = null;

                return client;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new TypeInitializationException(GetType().FullName, new Exception($"Could not initialize the [{tableName}] table client.", e));
            }
        }

        public async Task CreateAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return;

            try
            {
                var result = await client.AddEntityAsync(model, cancellationToken);
                if (result.IsError)
                    throw new Exception();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not insert into the [{tableName}] table.", e);
            }
        }

        public async Task<long> CountAsync<T>(Expression<Func<T, bool>> predicate, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            long result = 0;
            
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return result;
            
            try
            {
                var data = client.QueryAsync(predicate, select: new[] { "RowKey" }, cancellationToken: cancellationToken);
                await foreach (var item in data.AsPages())
                {
                    result += item.Values.Count;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not count the [{tableName}] table.", e);
            }

            return result;
        }

        public async IAsyncEnumerable<T> ReadAsync<T>(Expression<Func<T, bool>> predicate, IEnumerable<string> select, int maxPerPage = 100, string tableName = "", [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                yield break;

            var data = client.QueryAsync(predicate, maxPerPage, select, cancellationToken);
            await foreach (var item in data.AsPages())
            {
                foreach (var subitem in item.Values)
                {
                    yield return subitem;
                }
            }
        }

        public async Task<IEnumerable<T>> ReadAllAsync<T>(Expression<Func<T, bool>> predicate, IEnumerable<string> select, int maxPerPage = 100, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return null;

            var result = new List<T>();

            try
            {
                var data = client.QueryAsync(predicate, maxPerPage, select, cancellationToken);
                await foreach (var item in data.AsPages())
                {
                    result.AddRange(item.Values);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not read from the [{tableName}] table.", e);
            }

            return result;
        }
        public async Task UpdateAsync<T>(T model, string tableName = "", bool @override = true, CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return;

            try
            {
                var result = await client.UpdateEntityAsync(model, ETag.All, @override ? TableUpdateMode.Replace : TableUpdateMode.Merge, cancellationToken);
                if (result.IsError)
                    throw new Exception();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not update from the [{tableName}] table.", e);
            }
        }

        public async Task DeleteAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new()
        {
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return;

            try
            {
                var result = await client.DeleteEntityAsync(model.PartitionKey, model.RowKey, ETag.All, cancellationToken);
                if (result.IsError)
                    throw new Exception();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not delete from the [{tableName}] table.", e);
            }
        }

        public async Task DeleteAsync(string partitionKey, string rowKey, string tableName = "", CancellationToken cancellationToken = default)
        {
            var client = await GetClientAsync(tableName, cancellationToken);
            if (client == null)
                return;

            try
            {
                var result = await client.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: cancellationToken);
                if (result.IsError)
                    throw new Exception();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                throw new Exception($"Could not delete from the [{tableName}] table.", e);
            }
        }
    }
}
#endif
