using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Infrastructure.Interfaces
{
    public interface ITableService
    {
        string TableName { get; set; }
        bool CreateIfNotExists { get; set; }
        
        Task CreateAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task<long> CountAsync<T>(Expression<Func<T, bool>> predicate, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        IAsyncEnumerable<T> ReadAsync<T>(Expression<Func<T, bool>> predicate, IEnumerable<string> select, int maxPerPage = 100, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task<IEnumerable<T>> ReadAllAsync<T>(Expression<Func<T, bool>> predicate, IEnumerable<string> select, int maxPerPage = 100, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task UpdateAsync<T>(T model, string tableName = "", bool @override = true, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task DeleteAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task DeleteAsync(string partitionKey, string rowKey, string tableName = "", CancellationToken cancellationToken = default);
        
    }
}
