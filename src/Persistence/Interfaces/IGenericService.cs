﻿using System;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Persistence.Interfaces
{
    public interface IGenericService<TEntity> : IGenericService<TEntity, int> where TEntity : class, IEntity<int>
    {
    }

    public interface IGenericService<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {

        Task<IQueryable<TResult>> ReadAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false);

        Task<IQueryable<TResult>> SearchAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null);
        Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null);
        Task CreateAsync(params TEntity[] entity);
        Task CreateOrUpdateAsync(params TEntity[] entity);
        Task UpdateAsync(params TEntity[] entity);
        Task DeleteAsync(params TKey[] key);

        // * //

        Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false);


        Task<TResult> LastOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false);

#if NONASYNC
		IQueryable<TResult> Read<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			int? skip = 0, int? take = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false);

		IQueryable<TResult> Search<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			string criteria = "",
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			int? skip = 0, int? take = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false);

		int Count(Expression<Func<TEntity, bool>> predicate = null);
		long LongCount(Expression<Func<TEntity, bool>> predicate = null);
		bool Any(Expression<Func<TEntity, bool>> predicate = null);
		void Create(params TEntity[] entity);

		void CreateOrUpdate(params TEntity[] entity);
		void Update(params TEntity[] entity);
		void Delete(params TKey[] key);

		// * //

		TResult FirstOrDefault<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false);

		TResult LastOrDefault<TResult>(
			Expression<Func<TEntity, TResult>> selector = null,
			Expression<Func<TEntity, bool>> predicate = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
			bool disableTracking = false,
			bool ignoreQueryFilters = false);

#endif
    }
}
