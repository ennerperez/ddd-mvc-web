using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Business.Models;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Interfaces;

// ReSharper disable RedundantCast

namespace Web.Controllers
{
    [Authorize(AuthenticationSchemes = SmartScheme.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase<TEntity, TKey> : ControllerBase where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        private ISender _mediator = null!;
        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected readonly IGenericRepository<TEntity, TKey> Repository;

        public ApiControllerBase(IGenericRepository<TEntity, TKey> repository)
        {
            Repository = repository;
        }
        
        public async Task<JsonResult> Table<TResult>(TableInfo model,
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            object rows;

            var props = typeof(TResult).GetProperties().ToArray();
            var orderByKeys = new Dictionary<string, string>();
            if (model.Order != null)
            {
                foreach (var item in model.Order)
                {
                    var column = model.Columns[item.Column];
                    if (column.Data == null) continue;
                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null) orderByKeys.Add(prop.Name, item.Dir);
                }
            }

            var order = orderByKeys.Select(m => new[] {m.Key, m.Value}).ToArray();

            Expression predicateExpression = null;
            ParameterExpression parameter = null;

            ParameterExpression NestedMember(MemberExpression me)
            {
                if (me.Expression is ParameterExpression)
                    return (ParameterExpression)me.Expression;
                else if (me.Expression is MemberExpression)
                    return NestedMember((MemberExpression)me.Expression);
                else
                    return null;
            }

            if (model.Columns != null)
            {
                var filters = model.Columns.Where(m => m.Searchable && m.Search != null && !string.IsNullOrWhiteSpace(m.Search.Value))
                    .Select(column =>
                    {
                        var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                        if (prop != null)
                            return new {Property = prop, column.Name, column.Search.Value};

                        return null;
                    });

                MemberExpression[] args = null;
                if (selector.Body is NewExpression)
                {
                    args = ((NewExpression)selector.Body).Arguments.OfType<MemberExpression>().ToArray();
                    parameter = NestedMember(args.First());
                }
                else if (selector.Body is MemberInitExpression)
                {
                    //TODO: Validate args
                    // args = ((MemberInitExpression)selector.Body).NewExpression.Arguments.OfType<MemberExpression>().ToArray();
                    // parameter = NestedMember(args.First());
                }

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

                        if (value != null && value != type.GetDefault())
                        {
                            constant = Expression.Constant(value);
                            var methods = new[] {"Contains", "Equals", "CompareTo"};
                            foreach (var method in methods)
                            {
                                var methodInfo = type.GetMethod(method, new[] {type});
                                if (methodInfo != null)
                                {
                                    var member = item;
                                    var callExp = Expression.Call(member, methodInfo, constant);
                                    predicateExpression = predicateExpression == null ? (Expression)callExp : Expression.AndAlso(predicateExpression, callExp);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            Expression<Func<TEntity, bool>> expression = null;
            if (predicateExpression != null)
            {
                if (predicate != null)
                    predicateExpression = Expression.AndAlso(predicate, predicateExpression);

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
                rows = await Repository.SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order), include: include,
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await Repository.ReadAsync(selector, expression, o => o.SortDynamically(order), include: include,
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await Repository.CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new JsonResult(new TableResult() {iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data});
        }
    }

    public abstract class ApiControllerBase<TEntity> : ApiControllerBase<TEntity, int> where TEntity : class, IEntity<int>
    {
        public ApiControllerBase(IGenericRepository<TEntity, int> repository) : base(repository)
        {
        }
    }
}
