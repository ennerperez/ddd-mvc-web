using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
#if USING_BULK
using EFCore.BulkExtensions;
#endif
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable CanSimplifyIsAssignableFrom
// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable RedundantCast
// ReSharper disable AssignNullToNotNullAttribute

namespace Persistence.Services
{
    public abstract class GenericRepository<TEntity> : GenericRepository<TEntity, int>, IGenericRepository<TEntity> where TEntity : class, IEntity<int>
    {
        protected GenericRepository(DbContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }

    public abstract class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey> where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        private readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> DbSet;
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

#if USING_BULK
        public ushort MinRowsToBulk { get; set; }
#endif

#if USING_SPLIT
        public ushort MinRowsToSplit { get; set; }
#endif

        private static Expression<Func<TEntity, bool>> PreparePredicate(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                return predicate;
            }

            if (predicate == null)
            {
                predicate = m => (m as ISoftDelete).IsDeleted == false;
            }
            else
            {
                var prop = typeof(TEntity).GetProperty("IsDeleted");
                var type = prop?.PropertyType;
                var constant = Expression.Constant(false);
                var methodInfo = type?.GetMethod("Equals", [
                    type
                ]);
                var member = Expression.Property(predicate.Parameters[0], prop);
                var callExp = Expression.Call(member, methodInfo, constant);
                var body = Expression.AndAlso(callExp, predicate.Body);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(body, predicate.Parameters[0]);
                predicate = lambda;
            }

            return predicate;
        }

        protected GenericRepository(DbContext context, ILoggerFactory logger, IConfiguration configuration)
        {
            _dbContext = context;
            DbSet = _dbContext.Set<TEntity>();
            Logger = logger.CreateLogger(GetType());
#if USING_BULK
            MinRowsToBulk = ushort.Parse(configuration["RepositorySettings:MinRowsToBulk"] ?? "1000");
#endif
#if USING_SPLIT
            MinRowsToSplit = ushort.Parse(configuration["RepositorySettings:MinRowsToSplit"] ?? "100");
#endif
            Configuration = configuration;
        }

        public virtual Task<IQueryable<TResult>> ReadAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var stprop = DbSet.EntityType.GetProperties()
                .FirstOrDefault(m => m.ClrType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.ClrType) && !m.ClrType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip is <= 0)
            {
                skip = null;
            }

            if (take is <= 0)
            {
                take = null;
            }

            if (skip != null)
            {
                result = result.Skip(skip.Value);
            }

            if (take != null)
            {
                result = result.Take(take.Value);
            }

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
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var args = Array.Empty<MemberExpression>();

            Expression searchPredicate = null;
            if (selector is { Body: NewExpression expression1 })
            {
                args = expression1.Arguments.OfType<MemberExpression>().ToArray();
            }
            else if (selector is { Body: MemberInitExpression expression2 })
            {
                args = expression2.Bindings.Select(m => (m as MemberAssignment)?.Expression).OfType<MemberExpression>().ToArray();
            }

            static ParameterExpression NestedMember(MemberExpression me)
            {
                if (me.Expression is ParameterExpression expression2)
                {
                    return expression2;
                }
                else if (me.Expression is MemberExpression expression3)
                {
                    return NestedMember(expression3);
                }
                else
                {
                    return null;
                }
            }

            var parameter = NestedMember(args.First());

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

                    if (value == null || value == type.GetDefault())
                    {
                        continue;
                    }

                    ConstantExpression constant = Expression.Constant(value);
                    var methods = new[]
                    {
                        "Contains", "IndexOf", "Equals", "CompareTo"
                    };
                    foreach (var method in methods)
                    {
                        var methodInfo = type.GetMethod(method, [
                            type
                        ]);
                        if (methodInfo == null)
                        {
                            continue;
                        }

                        var member = item;
                        var callExp = Expression.Call(member, methodInfo, constant);
                        searchPredicate = searchPredicate == null ? (Expression)callExp : Expression.OrElse(searchPredicate, callExp);
                        break;
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
            {
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);
            }

            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var stprop = DbSet.EntityType.GetProperties()
                .FirstOrDefault(m => m.ClrType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.ClrType) && !m.ClrType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip is <= 0)
            {
                skip = null;
            }

            if (take is <= 0)
            {
                take = null;
            }

            if (skip != null)
            {
                result = result.Skip(skip.Value);
            }

            if (take != null)
            {
                result = result.Take(take.Value);
            }

            return await Task.FromResult(result);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return await DbSet.CountAsync(predicate, cancellationToken);
            }

            return await DbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return await DbSet.LongCountAsync(predicate, cancellationToken);
            }

            return await DbSet.LongCountAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null,
            CancellationToken cancellationToken = default)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return await DbSet.AnyAsync(predicate, cancellationToken);
            }

            return await DbSet.AnyAsync(cancellationToken);
        }

        public virtual async Task CreateAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if USING_SPLIT
                if (entities.Length < MinRowsToSplit || MinRowsToSplit == 0)
                {
                    await DbSet.AddRangeAsync(entities, cancellationToken);
                }
                else
                {
                    var size = (int)Math.Ceiling(Math.Sqrt(entities.Length));
                    var parts = entities.Split(size);
                    foreach (var item in parts)
                    {
                        await DbSet.AddRangeAsync(item, cancellationToken);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    return;
                }
#else
                await DbSet.AddRangeAsync(entities, cancellationToken);
#endif
            }
#if USING_BULK
            else
            {
                await _dbContext.BulkInsertAsync(entities, cancellationToken: cancellationToken);
            }
#endif
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if USING_SPLIT
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
                        await _dbContext.SaveChangesAsync(cancellationToken);
                    }

                    return;
                }
#else
                DbSet.UpdateRange(entities);
#endif
            }
#if USING_BULK
            else
            {
                await _dbContext.BulkUpdateAsync(entities, cancellationToken: cancellationToken);
            }
#endif
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            DbSet.Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task CreateOrUpdateAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                foreach (var item in entities)
                {
                    if (item.Id.Equals(default))
                    {
                        await CreateAsync(item, cancellationToken);
                    }
                    else
                    {
                        await UpdateAsync(item, cancellationToken);
                    }
                }
            }
#if USING_BULK
            else
            {
                await _dbContext.BulkUpdateAsync(entities.Where(m => !m.Id.Equals(default)).ToArray(), cancellationToken: cancellationToken);
                await _dbContext.BulkInsertAsync(entities.Where(m => m.Id.Equals(default)).ToArray(), cancellationToken: cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
#endif
        }

        public virtual async Task CreateOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Equals(default))
            {
                await CreateAsync(entity, cancellationToken);
            }
            else
            {
                await UpdateAsync(entity, cancellationToken);
            }
        }

        public virtual async Task DeleteAsync(TEntity[] entities, CancellationToken cancellationToken = default)
        {
            var list = entities.ToList();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    if (entity == null || ((ISoftDelete)entity).IsDeleted)
                    {
                        continue;
                    }

                    ((ISoftDelete)entity).IsDeleted = true;
                    ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                }

                await UpdateAsync(list.ToArray(), cancellationToken);
                return;
            }

#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                DbSet.RemoveRange(list);
            }
#if USING_BULK
            else
            {
                await _dbContext.BulkDeleteAsync(list, cancellationToken: cancellationToken);
            }
#endif

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (entity != null)
            {
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    if (!((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }

                    await UpdateAsync(entity, cancellationToken);
                    return;
                }
            }

            DbSet.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync<T>(T[] keys, CancellationToken cancellationToken = default) where T : struct, IComparable<T>, IEquatable<T>
        {
            var list = new List<TEntity>();
            foreach (var item in keys)
            {
                var entity = await DbSet.FindAsync([
                    item
                ], cancellationToken: cancellationToken);
                if (entity != null)
                {
                    list.Add(entity);
                }
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    if (entity == null || ((ISoftDelete)entity).IsDeleted)
                    {
                        continue;
                    }

                    ((ISoftDelete)entity).IsDeleted = true;
                    ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                }

                await UpdateAsync(list.ToArray(), cancellationToken);
                return;
            }

#if USING_BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                DbSet.RemoveRange(list);
            }
#if USING_BULK
            else
            {
                await _dbContext.BulkDeleteAsync(list, cancellationToken: cancellationToken);
            }
#endif

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAsync<T>(T key, CancellationToken cancellationToken = default) where T : struct, IComparable<T>, IEquatable<T>
        {
            var entity = await DbSet.FindAsync([
                key
            ], cancellationToken: cancellationToken);
            if (entity != null)
            {
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    if (!((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }

                    await UpdateAsync(entity, cancellationToken);
                    return;
                }
            }

            DbSet.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // * //

        public virtual async Task<TResult> FirstOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<TResult> LastOrDefaultAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false,
            CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return await result.FirstOrDefaultAsync(cancellationToken);
        }

#if USING_NONASYNC
        public virtual IQueryable<TResult> Read<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            int? skip = 0, int? take = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var stprop = DbSet.EntityType.GetProperties()
                .FirstOrDefault(m => m.ClrType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.ClrType) && !m.ClrType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip is <= 0)
            {
                skip = null;
            }

            if (take is <= 0)
            {
                take = null;
            }

            if (skip != null)
            {
                result = result.Skip(skip.Value);
            }

            if (take != null)
            {
                result = result.Take(take.Value);
            }

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
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            var searchs = criteria.Split(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            var args = Array.Empty<MemberExpression>();

            Expression searchPredicate = null;
            if (selector is { Body: NewExpression newExpression })
            {
                args = newExpression.Arguments.OfType<MemberExpression>().ToArray();
            }
            else if (selector is { Body: MemberInitExpression initExpression })
            {
                args = initExpression.Bindings.Select(m => (m as MemberAssignment)?.Expression).OfType<MemberExpression>().ToArray();
            }

            ParameterExpression NestedMember(MemberExpression me)
            {
                if (me.Expression is ParameterExpression meExpression)
                {
                    return meExpression;
                }
                else if (me.Expression is MemberExpression memberExpression)
                {
                    return NestedMember(memberExpression);
                }
                else
                {
                    return null;
                }
            }

            var parameter = NestedMember(args.First());

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
                        ConstantExpression constant = Expression.Constant(value);
                        var methods = new[] { "Contains", "IndexOf", "Equals", "CompareTo" };
                        foreach (var method in methods)
                        {
                            var methodInfo = type.GetMethod(method, [type]);
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

            Expression<Func<TEntity, bool>> expression = null;
            if (searchPredicate != null)
            {
                expression = Expression.Lambda<Func<TEntity, bool>>(searchPredicate, parameter);
            }

            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (expression != null)
            {
                query = query.Where(expression);
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var stprop = DbSet.EntityType.GetProperties()
                .FirstOrDefault(m => m.ClrType == typeof(string) || (!typeof(IEnumerable).IsAssignableFrom(m.ClrType) && !m.ClrType.IsClass))?.Name;
            var result = orderBy != null ? orderBy(query).Select(selector) : !string.IsNullOrWhiteSpace(stprop) ? query.OrderBy(stprop).Select(selector) : query.Select(selector);

            if (skip is <= 0)
            {
                skip = null;
            }

            if (take is <= 0)
            {
                take = null;
            }

            if (skip != null)
            {
                result = result.Skip(skip.Value);
            }

            if (take != null)
            {
                result = result.Take(take.Value);
            }

            return result;
        }

        public virtual int Count(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return DbSet.Count(predicate);
            }

            return DbSet.Count();
        }

        public virtual long LongCount(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return DbSet.LongCount(predicate);
            }

            return DbSet.LongCount();
        }

        public virtual bool Any(Expression<Func<TEntity, bool>> predicate = null)
        {
            predicate = PreparePredicate(predicate);
            if (predicate != null)
            {
                return DbSet.Any(predicate);
            }

            return DbSet.Any();
        }

        public virtual void Create(params TEntity[] entities)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if USING_SPLIT
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
                        _dbContext.SaveChanges();
                    }

                    return;
                }
#else
                DbSet.AddRange(entities);
#endif
            }
#if USING_BULK
            else
            {
                _dbContext.BulkInsert(entities);
            }
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Create(TEntity entity)
        {
            DbSet.Add(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Update(params TEntity[] entities)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
#if USING_SPLIT
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
                        _dbContext.SaveChanges();
                    }

                    return;
                }
#else
                DbSet.UpdateRange(entities);
#endif
            }
#if USING_BULK
            else
            {
                _dbContext.BulkUpdate(entities);
            }
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
            _dbContext.SaveChanges();
        }

        public virtual void CreateOrUpdate(params TEntity[] entities)
        {
#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                foreach (var item in entities)
                {
                    if (item.Id.Equals(default))
                    {
                        Create(item);
                    }
                    else
                    {
                        Update(item);
                    }
                }
            }
#if USING_BULK
            else
            {
                _dbContext.BulkUpdate(entities.Where(m => !m.Id.Equals(default)).ToArray());
                _dbContext.BulkInsert(entities.Where(m => m.Id.Equals(default)).ToArray());
                _dbContext.SaveChanges();
            }
#endif
        }

        public virtual void CreateOrUpdate(TEntity entity)
        {
            if (entity.Id.Equals(default))
            {
                Create(entity);
            }
            else
            {
                Update(entity);
            }
        }

        public virtual void Delete(TEntity[] entities)
        {
            var list = entities.ToList();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    if (entity != null && !((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }
                }

                Update(list.ToArray());
                return;
            }

#if USING_BULK
            if (entities.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                DbSet.RemoveRange(list);
            }
#if USING_BULK
            else
            {
                _dbContext.BulkDelete(list);
            }
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity != null)
            {
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    if (!((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }

                    Update(entity);
                    return;
                }
            }

            _dbContext.Remove(entity);
            _dbContext.SaveChanges();
        }

        public virtual void Delete<T>(T[] keys) where T : struct, IComparable<T>, IEquatable<T>
        {
            var list = Enumerable.Select(keys, item => DbSet.Find(item)).Where(entity => entity != null).ToList();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                foreach (var entity in list.Cast<ISoftDelete>())
                {
                    if (entity != null && !((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }
                }

                Update(list.ToArray());
                return;
            }

#if USING_BULK
            if (keys.Length < MinRowsToBulk || MinRowsToBulk == 0)
#endif
            {
                DbSet.RemoveRange(list);
            }
#if USING_BULK
            else
            {
                _dbContext.BulkDelete(list);
            }
#endif

            _dbContext.SaveChanges();
        }

        public virtual void Delete<T>(T key) where T : struct, IComparable<T>, IEquatable<T>
        {
            var entity = DbSet.Find(key);
            if (entity != null)
            {
                if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                {
                    if (!((ISoftDelete)entity).IsDeleted)
                    {
                        ((ISoftDelete)entity).IsDeleted = true;
                        ((ISoftDelete)entity).DeletedAt = DateTime.Now;
                    }

                    Update(entity);
                    return;
                }
            }

            _dbContext.Remove(entity);
            _dbContext.SaveChanges();
        }

        // * //

        public virtual TResult FirstOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

        public virtual TResult LastOrDefault<TResult>(
            Expression<Func<TEntity, TResult>> selector = null,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool disableTracking = false,
            bool ignoreQueryFilters = false,
            bool includeDeleted = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (include == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                include = (this as IDefaultRepository<TEntity>)?.DefaultInclude;
            }

            if (orderBy == null && typeof(IDefaultRepository<TEntity>).IsAssignableFrom(GetType()))
            {
                orderBy = (this as IDefaultRepository<TEntity>)?.DefaultOrderBy;
            }

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (ignoreQueryFilters)
            {
                query = query.IgnoreQueryFilters();
            }

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)) && !includeDeleted)
            {
                query = query.Where(m => (m as ISoftDelete).IsDeleted == false);
            }

            query = query.OrderByDescending(m => m.Id);

            var result = orderBy != null ? orderBy(query).Select(selector) : query.Select(selector);
            return result.FirstOrDefault();
        }

#endif
    }
}
