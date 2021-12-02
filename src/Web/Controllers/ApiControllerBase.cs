using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Web.Models;

// ReSharper disable RedundantCast

namespace Web.Controllers
{
    
    public abstract class ApiControllerBase<TEntity> : ApiControllerBase<TEntity, int> where TEntity : class, IEntity<int>
    {
        public ApiControllerBase(IGenericRepository<TEntity, int> repository) : base(repository)
        {
        }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBaseWithDbContext<TContext> : ControllerBase where TContext : DbContext
    {
        protected readonly TContext DbContext;

        public ApiControllerBaseWithDbContext(TContext dbContext)
        {
            DbContext = dbContext;
        }
    }

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase<TEntity, TKey> : ControllerBase where TEntity : class, IEntity<TKey> where TKey : struct, IComparable<TKey>, IEquatable<TKey>
    {
        protected readonly IGenericRepository<TEntity, TKey> Repository;
        protected readonly IMediator<TEntity, TKey> Mediator;

        public ApiControllerBase(IGenericRepository<TEntity, TKey> repository, IMediator<TEntity, TKey> mediator)
        {
            Repository = repository;
            Mediator = mediator;
        }

        public async Task<JsonResult> Data<TResult>(AjaxViewModel model, Expression<Func<TEntity, TResult>> selector, Expression<Func<TEntity, bool>> predicate = null)
        {
            object rows;

            var props = typeof(TEntity).GetProperties().ToArray();
            var orderBy = new Dictionary<string, string>();
            if (model.Order != null)
            {
                foreach (var item in model.Order)
                {
                    var column = model.Columns[item.Column];
                    var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                    if (prop != null) orderBy.Add(prop.Name, item.Dir);
                }
            }

            var order = orderBy.Select(m => new[] { m.Key, m.Value }).ToArray();
            
            Expression predicateExpression = null;
            ParameterExpression parameter = null;

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

            if (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value))
            {
                rows = await Repository.SearchAsync(selector, expression, model.Search.Value, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await Repository.ReadAsync(selector, expression, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await Repository.CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new JsonResult(new { iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data });
        }
    }
}
