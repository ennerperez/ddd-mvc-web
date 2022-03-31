using System;
using System.Linq;
using System.Linq.Expressions;
using Business.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace Business.Abstractions
{
    public class GenericRequest<TEntity, TResult> : GenericRequest<TEntity, int, TResult> where TEntity : class, IEntity<int>
    {
    }

    public class GenericRequest<TEntity, TKey, TResult> : IRepositoryRequest<TEntity, TKey, TResult> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
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
