using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;

namespace Persistence.Interfaces
{
    public interface IDefaultRepository<TEntity>
    {
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> DefaultInclude { get; }
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> DefaultOrderBy { get; }
    }
}
