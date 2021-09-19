#define ASYNC
//#define NONASYNC
//#define BULK

#if ASYNC || NONASYNC
using Microsoft.EntityFrameworkCore.Query;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Linq.Dynamic.Core;
#endif
#if ASYNC
using System.Threading.Tasks;
#endif

using Microsoft.Extensions.Configuration;
using Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Domain.Interfaces;

#if BULK
using EFCore.BulkExtensions;
#endif

// ReSharper disable RedundantCast
// ReSharper disable AssignNullToNotNullAttribute

namespace Persistence.Services
{
    public abstract class GenericService<TEntity> : GenericService<TEntity, int>, IGenericService<TEntity> where TEntity : class, IEntity<int>
    {
        protected GenericService(DbContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }

    public abstract class GenericService<TEntity, TKey> : IGenericService<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        protected DbContext _dbContext;
        protected DbSet<TEntity> _dbSet;
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

#if BULK
        public ushort MinRowsToBulk { get; set; }
#endif
        public ushort MinRowsToSplit { get; set; }

        protected GenericService(DbContext context, ILoggerFactory logger, IConfiguration configuration)
        {
            _dbContext = context;
            _dbSet = _dbContext.Set<TEntity>();
            Logger = logger.CreateLogger(GetType());
#if BULK
            MinRowsToBulk = ushort.Parse(configuration["RepositorySettings:MinRowsToBulk"] ?? "1000");
#endif
            MinRowsToSplit = ushort.Parse(configuration["RepositorySettings:MinRowsToSplit"] ?? "100");
            Configuration = configuration;
        }

#if ASYNC
        public virtual Task<IQueryable<TResult>> ReadAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);

            var stprop = typeof(TResult).GetProperties().FirstOrDefault()?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return Task.FromResult(result);
        }

        public virtual async Task<IQueryable<TResult>> SearchAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var parameter = Expression.Parameter(typeof(TEntity));

            Expression searchPredicate = null;
            if (selector != null && selector.Body is NewExpression)
            {
                var args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();

                ParameterExpression NestedMember(MemberExpression me)
                {
                    if (me.Expression is ParameterExpression)
                        return (ParameterExpression)me.Expression;
                    else if (me.Expression is MemberExpression)
                        return NestedMember((MemberExpression)me.Expression);
                    else
                        return null;
                }

                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var search in searchs)
                {
                    foreach (var item in args)
                    {
                        var type = item.Type;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(search, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] { "Contains", "Equals", "CompareTo" };
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] { type });
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    searchPredicate = searchPredicate == null ? (Expression)callExp : Expression.OrElse(searchPredicate, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);

            IQueryable<TEntity> query = _dbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);
            if (expression != null) query = query.Where(expression);

            var stprop = typeof(TResult).GetProperties().FirstOrDefault()?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return await Task.FromResult(result);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return await _dbSet.CountAsync(predicate);
            return await _dbSet.CountAsync();
        }

        public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return await _dbSet.LongCountAsync(predicate);
            return await _dbSet.LongCountAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return await _dbSet.AnyAsync(predicate);
            return await _dbSet.AnyAsync();
        }

        public virtual async Task CreateAsync(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    await _dbSet.AddRangeAsync(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        await _dbSet.AddRangeAsync(item);
                        await _dbContext.SaveChangesAsync();
                    }

                    return;
                }
            }
#if BULK
            else
                await _dbContext.BulkInsertAsync(entities);
#endif

            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    _dbSet.UpdateRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        _dbSet.UpdateRange(item);
                        await _dbContext.SaveChangesAsync();
                    }

                    return;
                }
            }
#if BULK
            else
                await _dbContext.BulkUpdateAsync(entities);
#endif

            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(params TKey[] keys)
        {
            var list = new List<TEntity>();
            foreach (var item in keys)
            {
                var entity = await _dbSet.FindAsync(item);
                if (entity != null)
                    list.Add(entity);
            }
#if BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            _dbSet.RemoveRange(list);
#if BULK
            else
                await _dbContext.BulkDeleteAsync(list);
#endif

            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task CreateOrUpdateAsync(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            foreach (var item in entities)
            {
                if (item.Id.Equals(default))
                    await CreateAsync(item);
                else
                    await UpdateAsync(item);
            }
#if BULK
            else
            {
                await _dbContext.BulkUpdateAsync(entities.Where(m => !m.Id.Equals(default)).ToArray());
                await _dbContext.BulkInsertAsync(entities.Where(m => m.Id.Equals(default)).ToArray());
                await _dbContext.SaveChangesAsync();
            }
#endif
        }

        // * //

        public virtual async Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync();
        }

        public virtual async Task<TResult> LastOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync();
        }

#endif

#if NONASYNC
        public virtual IQueryable<TResult> Read<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);

            var stprop = typeof(TResult).GetProperties().FirstOrDefault()?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return result;
        }

        public virtual IQueryable<TResult> Search<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            string criteria = "",
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var parameter = Expression.Parameter(typeof(TEntity));

            Expression searchPredicate = null;
            if (selector != null && selector.Body is NewExpression)
            {
                var args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();

                ParameterExpression NestedMember(MemberExpression me)
                {
                    if (me.Expression is ParameterExpression)
                        return (ParameterExpression)me.Expression;
                    else if (me.Expression is MemberExpression)
                        return NestedMember((MemberExpression)me.Expression);
                    else
                        return null;
                }

                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var search in searchs)
                {
                    foreach (var item in args)
                    {
                        var type = item.Type;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(search, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] { "Contains", "Equals", "CompareTo" };
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] { type });
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    searchPredicate = searchPredicate == null ? callExp : Expression.OrElse(searchPredicate, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);

            IQueryable<TEntity> query = DbSet;
            if (disableTracking) query = query.AsNoTracking();
            if (include != null) query = include(query);
            if (ignoreQueryFilters) query = query.IgnoreQueryFilters();
            if (predicate != null) query = query.Where(predicate);
            if (expression != null) query = query.Where(expression);

            var stprop = typeof(TResult).GetProperties().FirstOrDefault()?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip != null && skip <= 0) skip = null;
            if (take != null && take <= 0) take = null;
            if (skip != null) result = result.Skip(skip.Value);
            if (take != null) result = result.Take(take.Value);

            return result;
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return DbSet.Count(predicate);
            return DbSet.Count();
        }

        public virtual long LongCount(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return DbSet.LongCount(predicate);
            return DbSet.LongCount();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate != null)
                return DbSet.Any(predicate);
            return DbSet.Any();
        }

        public virtual void Create(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    DbSet.AddRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        DbSet.AddRange(item);
                        DbContext.SaveChanges();
                    }

                    return;
                }
            }
#if BULK
            else
                DbContext.BulkInsert(entities);
#endif

            DbContext.SaveChanges();
        }

        public virtual void Update(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    DbSet.UpdateRange(entities);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        DbSet.UpdateRange(item);
                        DbContext.SaveChanges();
                    }

                    return;
                }
            }
#if BULK
            else
                DbContext.BulkUpdate(entities);
#endif

            DbContext.SaveChanges();
        }

        public virtual void Delete(params TKey[] keys)
        {
            var list = new List<TEntity>();
            foreach (var item in keys)
            {
                var entity = DbSet.Find(item);
                if (entity != null)
                    list.Add(entity);
            }
#if BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
                DbSet.RemoveRange(list);
#if BULK
            else
                DbContext.BulkDelete(list);
#endif

            DbContext.SaveChanges();
        }

        public virtual void CreateOrUpdate(params TEntity[] entities)
        {
#if BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
                foreach (var item in entities)
                {
                    if (item.Id.Equals(default))
                        Create(item);
                    else
                        Update(item);
                }
#if BULK
            else
            {
                DbContext.BulkUpdate(entities.Where(m => !m.Id.Equals(default)));
                DbContext.BulkInsert(entities.Where(m => m.Id.Equals(default)));
                DbContext.SaveChanges();
            }
#endif
        }

        
        // * //


        public virtual TResult FirstOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();
            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

        public virtual TResult LastOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false)
        {
            IQueryable<TEntity> query = DbSet;
            if (disableTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (predicate != null)
                query = query.Where(predicate);
            if (ignoreQueryFilters)
                query = query.IgnoreQueryFilters();

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

#endif
    }
}
