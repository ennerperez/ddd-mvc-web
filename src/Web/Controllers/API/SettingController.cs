using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using Web.Models;

namespace Web.Controllers.API
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ApiControllerBase<Setting>
    {
        private readonly ILogger _logger;

        public SettingController(ILoggerFactory loggerFactory, IGenericService<Setting> service) : base(service)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("List all elements")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var collection = await Service.ReadAsync(s => s);

                if (collection == null || !collection.Any())
                    return new JsonResult(new { last_created = default(DateTime?), last_updated = default(DateTime?), items = new List<Setting>() });

                return new JsonResult(new { last_created = collection.Max(m => m.Created), last_updated = collection.Max(m => m.Modified), items = collection });
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Get specific element by id")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id != 0)
            {
                try
                {
                    var items = await Service.ReadAsync(s => s, p => p.Id == id);

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

        [SwaggerOperation("Create a new element")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Post(Setting model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = model;
                record.Id = default;

                await Service.CreateAsync(record);

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
        public async Task<IActionResult> Put(int id, [FromBody] Setting model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = await Service.FirstOrDefaultAsync(s => s, p => p.Id == id);

                if (record != null)
                {
                    //record = model;
                    await Service.UpdateAsync(record);
                }

                return Ok();
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
                await Service.DeleteAsync(id);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Problem(e.Message);
            }
        }

        #region Extras

        [SwaggerOperation("Get a data for ajax operations")]
        [HttpPost("data")] //INFO: Used to fill Datatable
        public async Task<JsonResult> Data(AjaxViewModel model)
        {
            var selector = (new Setting()).Select(t => new { t.Id, t.Key, t.Type, t.Value });
            return await base.Data(model, selector);
        }

        #endregion
    }
}
