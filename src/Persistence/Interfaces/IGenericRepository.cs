using System;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Persistence.Interfaces
{
    public interface IGenericRepository<TEntity> : IGenericRepository<TEntity, int> where TEntity : class, IEntity<int>
    {
    }

    public interface IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {

        Task<IQueryable<TResult>> ReadAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);

        Task<IQueryable<TResult>> SearchAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default);
        Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default);
        Task CreateAsync(params TEntity[] entity);
        Task CreateOrUpdateAsync(params TEntity[] entity);
        Task UpdateAsync(params TEntity[] entity);
        Task DeleteAsync(params object[] key);
        
        Task CreateAsync(TEntity entity,
            CancellationToken cancellationToken = default);
        Task CreateOrUpdateAsync(TEntity entity,
            CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(object key,
            CancellationToken cancellationToken = default);

        // * //

        Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);


        Task<TResult> LastOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default);

#if ENABLE_NONASYNC
		IQueryable<TResult> Read<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			int? skip = 0, int? take = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false,
            bool includeDeleted = false);

		IQueryable<TResult> Search<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			string criteria = "",
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			int? skip = 0, int? take = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false,
            bool includeDeleted = false);

		int Count(Expression<Func<TEntity, bool>> predicate = null);
		long LongCount(Expression<Func<TEntity, bool>> predicate = null);
		bool Any(Expression<Func<TEntity, bool>> predicate = null);
		
        void Create(params TEntity[] entity);
		void CreateOrUpdate(params TEntity[] entity);
		void Update(params TEntity[] entity);
		void Delete(params object[] key);
        
        void Create(TEntity entity);
        void CreateOrUpdate(TEntity entity);
        void Update(TEntity entity);
        void Delete(object key);

		// * //

		TResult FirstOrDefault<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false,
            bool includeDeleted = false);

		TResult LastOrDefault<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false,
            bool includeDeleted = false);

#endif
    }
}
