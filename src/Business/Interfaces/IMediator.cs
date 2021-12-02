using System;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Business.Interfaces
{
    public interface IMediator<TEntity> : IMediator<TEntity, int> where TEntity : class, IEntity<int>
    {
    }

    public interface IMediator<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        public Task<TEntity> CreateAsync(TEntity model);
        public Task<TEntity> ReadAsync(TKey key);
        public Task<TEntity> UpdateAsync(TEntity model);
        public Task<bool> DeleteAsync(TKey key);
    }
}
