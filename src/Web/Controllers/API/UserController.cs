using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Models;
using Business.Requests.Identity;
using Domain.Entities;
using Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers.API
{
    [ApiExplorerSettings(GroupName = "v1")]
    public class UserController : ApiControllerBase<User>
    {
        private readonly ILogger _logger;

        public UserController(ILoggerFactory loggerFactory, IGenericRepository<User> repository) : base(repository)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("List all elements")]
        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var collection = await Mediator.SendWithRepository((new User()).Select(s => s), null, null, null);
                if (collection == null || !collection.Any())
                    return new JsonResult(new { lastCreated = default(DateTime?), lastUpdated = default(DateTime?), items = new List<Setting>() });

                return new JsonResult(new { lastCreated = collection.Max(m => m.CreatedAt), lastUpdated = collection.Max(m => m.ModifiedAt), items = collection });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
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
                    var collection = await Mediator.SendWithRepository((new User()).Select(s => s), p => p.Id == id, null, null);

                    return new JsonResult(collection);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{Message}", e.Message);
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
                var collection = await Mediator.SendWithPage((new User()).Select(s => s),
                    null, null, null, skip: ((page - 1) * size), take: size);
                return new JsonResult(collection);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Create a new element")]
        [DisableRequestSizeLimit]
        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest model)
        {
            if (model == null) return BadRequest();

            try
            {
                var result = await Mediator.Send(model);
                return Created(Url.Content($"~/api/{nameof(User)}/{result}"), result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest model)
        {
            if (model == null || id != model.Id) return BadRequest();

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Partial update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] PartialUpdateUserRequest model)
        {
            if (model == null || id != model.Id) return BadRequest();

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Delete an existing element by id")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await Mediator.Send(new DeleteUserRequest() { Id = id });
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        #region Extras

        [SwaggerOperation("Get data in table format")]
        [HttpPost("Table")]
        public async Task<JsonResult> Table(TableInfo model)
        {
            var selector = (new User()).Select(t => new
            {
                t.Id,
                t.UserName,
                t.Email,
                t.EmailConfirmed,
                t.PhoneNumber,
                t.PhoneNumberConfirmed,
                t.TwoFactorEnabled,
                GivenName = t.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.GivenName).ClaimValue,
                Surname = t.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.Surname).ClaimValue,
            });

            return await base.Table(model, selector, include: i => i.Include(m => m.UserClaims));
        }

        #endregion
    }
}
