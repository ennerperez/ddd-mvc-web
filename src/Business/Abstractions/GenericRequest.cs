using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;

namespace Business.Abstractions
{
    public static class ISenderExtensions
    {
        public static async Task<IQueryable<TResult>> Send<TEntity, TResult>(this ISender @this, 
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
            return (IQueryable<TResult>)result;
        }
    }

    public class GenericRequest<TEntity, TResult> : GenericRequest<TEntity, int, TResult> where TEntity : class, IEntity<int>
    {
    }

    public class GenericRequest<TEntity, TKey, TResult> : IRequest<IEnumerable<TResult>> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        public Expression<Func<TEntity, TResult>> Selector { get; set; }
        public Expression<Func<TEntity, bool>> Predicate { get; set; }

        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }
        public Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> Include { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool DisableTracking { get; set; }
        public bool IgnoreQueryFilters { get; set; }
        public bool IncludeDeleted { get; set; }
    }
}
