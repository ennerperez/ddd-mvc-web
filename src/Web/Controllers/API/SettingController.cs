using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using Web.Models;

namespace Web.Controllers.API
{
    [ApiExplorerSettings(GroupName = "v1")]
    public class SettingController : ApiControllerBase<Setting>
    {
        private readonly ILogger _logger;

        public SettingController(ILoggerFactory loggerFactory, IGenericRepository<Setting> repository) : base(repository)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("List all elements")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var collection = await Repository.ReadAsync(s => s);

                if (collection == null || !collection.Any())
                    return new JsonResult(new { last_created = default(DateTime?), last_updated = default(DateTime?), items = new List<Setting>() });

                return new JsonResult(new { last_created = collection.Max(m => m.CreatedAt), last_updated = collection.Max(m => m.ModifiedAt), items = collection });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Get specific element by id")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id != 0)
            {
                try
                {
                    var items = await Repository.ReadAsync(s => s, p => p.Id == id);

                    return new JsonResult(items);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                    return Problem(e.Message);
                }
            }

            return new JsonResult(null);
        }
        
        [SwaggerOperation("List all using a paged list")]
        [HttpGet("Page/{page}")]
        public async Task<IActionResult> GetPage(int page, int size = 10)
        {
            try
            {
                var collection = await Mediator.GetPaginated((new Setting()).Select(s => s),
                    null, null, null, skip: ((page - 1) * size), take: size);
                return new JsonResult(collection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Create a new element")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Create(Setting model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = model;
                record.Id = default;

                await Repository.CreateAsync(record);

                return Created(Url.Content($"~/api/{nameof(Setting)}/{record.Id}"), record.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Setting model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = await Repository.FirstOrDefaultAsync(s => s, p => p.Id == id);

                if (record != null)
                {
                    record.Key = model.Key;
                    record.Type = model.Type;
                    record.Value = model.Value;

                    await Repository.UpdateAsync(record);
                }

                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Delete an existing element by id")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await Repository.DeleteAsync(id);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }

        #region Extras

        [SwaggerOperation("Get data in table format")]
        [HttpPost("Table")]
        public async Task<JsonResult> Table(TableRequestViewModel model)
        {
            var selector = (new Setting()).Select(t => new
            {
                t.Id, 
                t.Key, 
                t.Type, 
                t.Value
            });
            return await base.Table(model, selector);
        }

        #endregion
    }
}
