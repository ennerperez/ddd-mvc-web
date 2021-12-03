﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Interfaces;
using Business.Interfaces.Mediators;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using Web.Models;

namespace Web.Controllers.API
{
    [ApiExplorerSettings(GroupName = "v1")]
    public class UserController : ApiControllerBase<User>
    {
        private readonly ILogger _logger;
        private readonly IUserMediator _userMediator;

        public UserController(ILoggerFactory loggerFactory, IUserMediator userMediator, IGenericRepository<User> repository, IMediator<User> mediator) : base(repository, mediator)
        {
            _userMediator = userMediator;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("List all elements")]
        [HttpGet]
        public async Task<IActionResult> Get()
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
        public async Task<IActionResult> Get(int id)
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

        [SwaggerOperation("Create a new element")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Post(User model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = model;
                record.Id = default;
                record.NormalizedEmail = model.Email.ToUpper();
                record.NormalizedUserName = model.UserName.ToUpper();

                await _userMediator.CreateAsync(record);

                return Created(Url.Content($"~/api/{nameof(User)}/{record.Id}"), record.Id);
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
        public async Task<IActionResult> Put(int id, [FromBody] User model)
        {
            if (model == null) return BadRequest();

            try
            {
                var record = await Repository.FirstOrDefaultAsync(s => s, p => p.Id == id);

                if (record != null)
                {
                    record.Email = model.Email;
                    record.PhoneNumber = model.PhoneNumber;
                    record.NormalizedEmail = model.Email?.ToUpper();
                    record.EmailConfirmed = model.EmailConfirmed;
                    record.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
                    record.TwoFactorEnabled = model.TwoFactorEnabled;
                    
                    await _userMediator.UpdateAsync(record);
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
                await _userMediator.DeleteAsync(id);
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
            var selector = (new User()).Select(t => new
            {
                t.Id,
                t.UserName,
                t.Email,
                t.EmailConfirmed,
                t.PhoneNumber,
                t.PhoneNumberConfirmed,
                t.TwoFactorEnabled
            });
            return await base.Data(model, selector);
        }

        #endregion
    }
}
