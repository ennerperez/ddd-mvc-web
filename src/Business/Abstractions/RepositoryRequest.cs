using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Interfaces;

namespace Business.Abstractions
{

    public class RepositoryRequest<TEntity, TResult> : RepositoryRequest<TEntity, int, TResult>
        where TEntity : class, IEntity<int>;

    public class RepositoryRequest<TEntity, TKey, TResult> : IRepositoryRequest<TEntity, TKey, TResult>
        where TEntity : class, IEntity<TKey>
        where TKey : struct, IComparable<TKey>, IEquatable<TKey>
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

    public abstract class RepositoryRequestHandler<TRequest, TEntity, TResult> : RepositoryRequestHandler<TRequest, TEntity, int, TResult>
        where TRequest : IRepositoryRequest<TEntity, int, TResult>
        where TEntity : class, IEntity<int>
    {
        protected RepositoryRequestHandler(IGenericRepository<TEntity, int> repository) : base(repository)
        {
        }
    }

    public abstract class RepositoryRequestHandler<TRequest, TEntity, TKey, TResult> : IRepositoryRequestHandler<TRequest, TEntity, TKey, TResult>
        where TRequest : IRepositoryRequest<TEntity, TKey, TResult>
        where TEntity : class, IEntity<TKey>
        where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {

        private readonly IGenericRepository<TEntity, TKey> _repository;

        protected RepositoryRequestHandler(IGenericRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }

        public async Task<TResult[]> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted, cancellationToken);
            var items = await entities.ToArrayAsync(cancellationToken);
            return items;
        }
    }
}
