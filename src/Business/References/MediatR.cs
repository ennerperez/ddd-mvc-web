using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Models;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace MediatR
{
    public static class ISenderExtensions
    {
        public static async Task<TEntity[]> SendWithRepositoryAsync<TEntity>(this ISender @this,
            Expression<Func<TEntity, TEntity>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            selector ??= (s) => s;

            return await @this.SendWithRepositoryAsync<TEntity, TEntity>(selector, predicate, orderBy, include, skip, take, disableTracking, ignoreQueryFilters, includeDeleted);
        }

        public static async Task<TResult[]> SendWithRepositoryAsync<TEntity, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            var request = new RepositoryRequest<TEntity, TResult>()
            {
                Selector = selector,
                Predicate = predicate,
                OrderBy = orderBy,
                Include = include,
                Skip = skip,
                Take = take,
                DisableTracking = disableTracking,
                IgnoreQueryFilters = ignoreQueryFilters,
                IncludeDeleted = includeDeleted
            };

            var result = await @this.Send(request);
            if (result != null)
            {
                return result;
            }

            return null;
        }

        public static async Task<TEntity[]> SendWithRepositoryAsync<TEntity, TKey>(this ISender @this,
            Expression<Func<TEntity, TEntity>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
        {
            selector ??= (s) => s;

            return await @this.SendWithRepositoryAsync<TEntity, TKey, TEntity>(selector, predicate, orderBy, include, skip, take, disableTracking, ignoreQueryFilters, includeDeleted);
        }

        public static async Task<TResult[]> SendWithRepositoryAsync<TEntity, TKey, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
        {
            var request = new RepositoryRequest<TEntity, TKey, TResult>()
            {
                Selector = selector,
                Predicate = predicate,
                OrderBy = orderBy,
                Include = include,
                Skip = skip,
                Take = take,
                DisableTracking = disableTracking,
                IgnoreQueryFilters = ignoreQueryFilters,
                IncludeDeleted = includeDeleted
            };

            var result = await @this.Send(request);
            if (result != null)
            {
                return result;
            }

            return null;
        }

        public static Task<PaginatedList<TEntity>> SendWithPageAsync<TEntity>(this ISender @this,
            Expression<Func<TEntity, TEntity>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            selector ??= (s) => s;

            return @this.SendWithPageAsync<TEntity, TEntity>(selector, predicate, orderBy, include, skip, take, disableTracking, ignoreQueryFilters, includeDeleted);
        }

        public static async Task<PaginatedList<TResult>> SendWithPageAsync<TEntity, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            var request = new PaginatedRequest<TEntity, TResult>()
            {
                Selector = selector,
                Predicate = predicate,
                OrderBy = orderBy,
                Include = include,
                Skip = skip,
                Take = take,
                DisableTracking = disableTracking,
                IgnoreQueryFilters = ignoreQueryFilters,
                IncludeDeleted = includeDeleted
            };

            var result = await @this.Send(request);
            if (result != null)
            {
                return result;
            }

            return null;
        }

        public static async Task<PaginatedList<TEntity>> SendWithPageAsync<TEntity, TKey>(this ISender @this,
            Expression<Func<TEntity, TEntity>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
        {
            selector ??= (s) => s;

            return await @this.SendWithPageAsync<TEntity, TKey, TEntity>(selector, predicate, orderBy, include, skip, take, disableTracking, ignoreQueryFilters, includeDeleted);
        }

        public static async Task<PaginatedList<TResult>> SendWithPageAsync<TEntity, TKey, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
        {
            var request = new PaginatedRequest<TEntity, TKey, TResult>()
            {
                Selector = selector,
                Predicate = predicate,
                OrderBy = orderBy,
                Include = include,
                Skip = skip,
                Take = take,
                DisableTracking = disableTracking,
                IgnoreQueryFilters = ignoreQueryFilters,
                IncludeDeleted = includeDeleted
            };

            var result = await @this.Send(request);
            if (result != null)
            {
                return result;
            }

            return null;
        }
    }
}
