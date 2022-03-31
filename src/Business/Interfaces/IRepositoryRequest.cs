using System;
using System.Linq;
using System.Linq.Expressions;
using Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;

namespace Business.Interfaces
{
    public interface IRepositoryRequest<TEntity, TResult> : IRepositoryRequest<TEntity, int, TResult> where TEntity : class, IEntity<int>
    {
    }

    public interface IRepositoryRequest<TEntity, TKey, TResult>: IRequest<TResult[]> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        Expression<Func<TEntity, TResult>> Selector { get; set; }
        Expression<Func<TEntity, bool>> Predicate { get; set; }
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> Include { get; set; }
        int? Skip { get; set; }
        int? Take { get; set; }
        bool DisableTracking { get; set; }
        bool IgnoreQueryFilters { get; set; }
        bool IncludeDeleted { get; set; }
    }
}
