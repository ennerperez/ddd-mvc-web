using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using MediatR;
using Persistence.Interfaces;

namespace Business.Abstractions
{

    public class GenericRequestHandler<TEntity, TResult> : GenericRequestHandler<TEntity, int, TResult> where TEntity : class, IEntity<int>
    {
        public GenericRequestHandler(IGenericRepository<TEntity, int> repository) : base(repository)
        {
        }
    }
    public class GenericRequestHandler<TEntity, TKey, TResult>: IRequestHandler<GenericRequest<TEntity,TKey, TResult>, IEnumerable<TResult>> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        private readonly IGenericRepository<TEntity, TKey> _repository;
        
        public GenericRequestHandler(IGenericRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }
        
        public async Task<IEnumerable<TResult>> Handle(GenericRequest<TEntity, TKey, TResult> request, CancellationToken cancellationToken)
        {
            var entities = await _repository.ReadAsync(request.Selector, request.Predicate, request.OrderBy, request.Include, request.Skip, request.Take, request.DisableTracking, request.IgnoreQueryFilters, request.IncludeDeleted);
            return entities;
        }
    }
}
