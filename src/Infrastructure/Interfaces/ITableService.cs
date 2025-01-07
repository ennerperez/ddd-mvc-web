using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
#if USING_TABLES
using Azure.Data.Tables;
#endif

namespace Infrastructure.Interfaces
{
#if !USING_TABLES
    public interface ITableEntity;
#endif
    public interface ITableService
    {
        string TableName { get; set; }
        bool CreateIfNotExists { get; set; }

        Task CreateAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task<long> CountAsync<T>(Expression<Func<T, bool>> predicate = null, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        IAsyncEnumerable<T> ReadAsync<T>(Expression<Func<T, bool>> predicate = null, IEnumerable<string> select = null, int maxPerPage = 100, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task<IEnumerable<T>> ReadAllAsync<T>(Expression<Func<T, bool>> predicate = null, IEnumerable<string> select = null, int maxPerPage = 100, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task UpdateAsync<T>(T model, string tableName = "", bool @override = true, CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task DeleteAsync<T>(T model, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
        Task DeleteAsync(string partitionKey, string rowKey, string tableName = "", CancellationToken cancellationToken = default);

        Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> predicate = null, IEnumerable<string> select = null, string tableName = "", CancellationToken cancellationToken = default) where T : class, ITableEntity, new();
    }
}
