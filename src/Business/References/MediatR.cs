using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Models;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

// ReSharper disable once CheckNamespace
namespace MediatR
{
    public static class ISenderExtensions
    {
        public static async Task<TResult[]> SendWithRepository<TEntity, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            var request = new GenericRequest<TEntity, TResult>()
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
            if (result != null) return result;
            return null;
        }
        
        public static async Task<PaginatedList<TResult>> SendWithPage<TEntity, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include,
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
            if (result != null) return result;
            return null;
        }

        public static async Task<PaginatedList<TResult>> SendWithPage<TResult>(this ISender @this)
        {
            var request = new PaginatedRequest<TResult>();

            var result = await @this.Send(request);
            if (result != null) return result;
            return null;
        }
        
    }
}
