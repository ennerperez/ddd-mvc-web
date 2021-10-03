using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using Web.Models;

namespace Web.Controllers
{
    
    public abstract class ApiControllerBase<TEntity> : ApiControllerBase<TEntity, int> where TEntity : class, IEntity<int>
    {
        public ApiControllerBase(IGenericService<TEntity, int> service) : base(service)
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
        protected readonly IGenericService<TEntity, TKey> Service;

        public ApiControllerBase(IGenericService<TEntity, TKey> service)
        {
            Service = service;
        }

        public async Task<JsonResult> Data<TResult>(AjaxViewModel model, Expression<Func<TEntity, TResult>> selector)
        {
            object rows;

            var props = typeof(TEntity).GetProperties().ToArray();
            var orderBy = new Dictionary<string, string>();
            foreach (var item in model.Order)
            {
                var column = model.Columns[item.Column];
                var prop = props.FirstOrDefault(m => m.Name.ToLower() == column.Name.ToLower());
                if (prop != null) orderBy.Add(prop.Name, item.Dir);
            }

            var order = orderBy.Select(m => new[] { m.Key, m.Value }).ToArray();

            if (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value))
            {
                rows = await Service.SearchAsync(selector, null, model.Search.Value, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }
            else
            {
                rows = await Service.ReadAsync(selector, null, o => o.SortDynamically(order),
                    skip: model.Start, take: model.Length);
            }

            var data = await ((IQueryable<object>)rows).ToListAsync();

            var total = await Service.CountAsync();
            var isFiltered = (model.Search != null && !string.IsNullOrWhiteSpace(model.Search.Value));
            var stotal = isFiltered ? data.Count : total;

            return new JsonResult(new { iTotalRecords = total, iTotalDisplayRecords = stotal, aaData = data });
        }
    }
}
