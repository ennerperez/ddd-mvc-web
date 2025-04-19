using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Business.Models;
using Domain;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;

// ReSharper disable RedundantCast

namespace Web.Controllers
{
#if USING_SMARTSCHEMA
    [SmartAuthorize]
#else
    [Authorize]
#endif
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender _mediator;
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected bool IsAdmin => User.IsInRole(Roles.Admin);
        protected int UserId => User.GetUserId<int>();

        protected IGenericRepository<TE, TK> Repository<TE, TK>() where TE : class, IEntity<TK> where TK : struct, IComparable<TK>, IEquatable<TK>
            => HttpContext.RequestServices.GetRequiredService<IGenericRepository<TE, TK>>();

        protected IGenericRepository<TE> Repository<TE>() where TE : class, IEntity<int>
            => HttpContext.RequestServices.GetRequiredService<IGenericRepository<TE>>();
    }

#if USING_SMARTSCHEMA
    [SmartAuthorize]
#else
    [Authorize]
#endif
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase<TEntity, TKey> : ApiControllerBase where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        protected IGenericRepository<TEntity, TKey> Repository()
            => HttpContext.RequestServices.GetRequiredService<IGenericRepository<TEntity, TKey>>();

        protected async Task<JsonResult> Table<TResult>(TableInfo model,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
            bool includeDeleted = false)
        {
            object rows;

            var props = typeof(TResult).GetProperties().ToArray();
            var orderByKeys = new Dictionary<string, string>();
            if (model.Order != null)
            {
                foreach (var item in model.Order)
                {
                    var column = model.Columns[item.Column];
                    if (column.Data == null)
                    {
                        continue;
                    }

                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null)
                    {
                        orderByKeys.Add(prop.Name, item.Dir);
                    }
                }
            }

            var order = orderByKeys.Select(m => new[] { m.Key, m.Value }).ToArray();

            Expression predicateExpression = null;
            ParameterExpression parameter = null;

            static ParameterExpression NestedMember(MemberExpression me)
            {
                if (me.Expression is ParameterExpression expression1)
                {
                    return expression1;
                }

                if (me.Expression is MemberExpression expression2)
                {
                    return NestedMember(expression2);
                }

                return null;
            }

            if (model.Columns != null)
            {
                var filters = model.Columns.Where(m => m.Searchable && m.Search != null && !string.IsNullOrWhiteSpace(m.Search.Value))
                    .Select(column =>
                    {
                        var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                        if (prop != null)
                        {
                            return new { Property = prop, column.Name, column.Search.Value };
                        }

                        return null;
                    });

                var args = Array.Empty<MemberExpression>();
                if (selector.Body is NewExpression expression1)
                {
                    args = expression1.Arguments.OfType<MemberExpression>().ToArray();
                }
                else if (selector.Body is MemberInitExpression expression2)
                {
                    args = expression2.Bindings.Select(m => (m as MemberAssignment)?.Expression).OfType<MemberExpression>().ToArray();
                }

                parameter = NestedMember(args.First());

                ConstantExpression constant;
                foreach (var filter in filters)
                {
                    foreach (var item in args.Where(m => filter != null && m.Member.Name.ToLower() == filter.Name.ToLower()))
                    {
                        var type = filter.Property.PropertyType;
                        object value = null;
                        try
                        {
                            value = Convert.ChangeType(filter.Value, type);
                        }
                        catch (Exception)
                        {
                            // ignore
                        }

                        if (value == null || value == type.GetDefault())
                        {
                            continue;
                        }

                        constant = Expression.Constant(value);
                        var methods = new[] { "Contains", "IndexOf", "Equals", "CompareTo" };
                        foreach (var method in methods)
                        {
                            var methodInfo = type.GetMethod(method, [type]);
                            if (methodInfo == null)
                            {
                                continue;
                            }

                            var callExp = Expression.Call(item, methodInfo, constant);
                            predicateExpression = predicateExpression == null ? (Expression)callExp : Expression.AndAlso(predicateExpression, callExp);
                            break;
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (predicateExpression != null)
            {
                if (predicate != null)
                {
                    predicateExpression = Expression.AndAlso(predicate, predicateExpression);
                }

                expression = Expression.Lambda<Func<TEntity, bool>>(predicateExpression, parameter);
            }
            else
            {
                if (predicate != null)
                {
                    expression = predicate;
                }
            }

            if (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value))
            {
                rows = await Repository().SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order), include,
                    model.Start, model.Length, includeDeleted: includeDeleted);
            }
            else
            {
                rows = await Repository().ReadAsync(selector, expression, o => o.SortDynamically(order), include,
                    model.Start, model.Length, includeDeleted: includeDeleted);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await Repository().CountAsync();
            var isFiltered = model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value);
            var stotal = isFiltered ? data.Count : total;

            return new JsonResult(new TableResult { iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data });
        }
    }

#if USING_SMARTSCHEMA
    [SmartAuthorize]
#else
    [Authorize]
#endif
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase<TEntity> : ApiControllerBase<TEntity, int> where TEntity : class, IEntity<int>
    {
        protected new IGenericRepository<TEntity> Repository()
            => HttpContext.RequestServices.GetRequiredService<IGenericRepository<TEntity>>();
    }
}
