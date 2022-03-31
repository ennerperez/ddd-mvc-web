using System;
using System.Linq;
using System.Linq.Expressions;
using Business.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace Business.Abstractions
{
    public class PaginatedRequest<TResult> : IPaginatedRequest<TResult>
    {
    }

    public class PaginatedRequest<TEntity, TResult> : PaginatedRequest<TEntity, int, TResult> where TEntity : class, IEntity<int>
    {
    }

    public class PaginatedRequest<TEntity, TKey, TResult> : IPaginatedRequest<TResult> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
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
        
        /* */
        
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
