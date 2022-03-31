﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Models;
using Business.Requests;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

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
                var collection = await Mediator.SendWithRepository((new Setting()).Select(s => s), null, null, null);
                if (collection == null || !collection.Any())
                    return new JsonResult(new {lastCreated = default(DateTime?), lastUpdated = default(DateTime?), items = new List<Setting>()});

                return new JsonResult(new {lastCreated = collection.Max(m => m.CreatedAt), lastUpdated = collection.Max(m => m.ModifiedAt), items = collection});
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
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
                    var collection = await Mediator.SendWithRepository((new Setting()).Select(s => s), p => p.Id == id, null, null);

                    return new JsonResult(collection);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,"{Message}", e.Message);
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
                var collection = await Mediator.SendWithPage((new Setting()).Select(s => s),
                    null, null, null, skip: ((page - 1) * size), take: size);
                return new JsonResult(collection);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Create a new element")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Create(CreateSettingRequest model)
        {
            if (model == null) return BadRequest();

            try
            {
                var result = await Mediator.Send(model);
                return Created(Url.Content($"~/api/{nameof(Setting)}/{result}"), result);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSettingRequest model)
        {
            if (model == null || id != model.Id) return BadRequest();

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
                return Problem(e.Message);
            }
        }
        
        [SwaggerOperation("Partial update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] PartialUpdateSettingRequest model)
        {
            if (model == null || id != model.Id) return BadRequest();

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Delete an existing element by id")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await Mediator.Send(new DeleteSettingRequest() {Id = id});
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e,"{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        #region Extras

        [SwaggerOperation("Get data in table format")]
        [HttpPost("Table")]
        public async Task<JsonResult> Table(TableInfo model)
        {
            var selector = (new Setting()).Select(t => new
            {
                t.Id,
                t.Key,
                t.Value
            });

            return await base.Table(model, selector);
        }

        #endregion
    }
}
