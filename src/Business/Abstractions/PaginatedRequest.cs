using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Models;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;

namespace Business.Abstractions
{
    public static partial class ISenderExtensions
    {
        public static async Task<PaginatedList<TResult>> GetPaginated<TEntity, TResult>(this ISender @this,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false) where TEntity : class, IEntity<int>
        {
            var request = new GenericPaginatedRequest<TEntity, TResult>()
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
    }
    
    public class PaginatedRequest<TResult> : IRequest<PaginatedList<TResult>>
    {
    }
}
