using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Models;
using Business.Requests;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Controllers.API
{
    [ApiExplorerSettings(GroupName = "v1")]
    public class BudgetsController : ApiControllerBase<Budget, Guid>
    {
        private readonly ILogger _logger;

        public BudgetsController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("List all elements")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var collection = await Mediator.SendWithRepositoryAsync<Budget, Guid, Budget>(new Budget().Select(s => s));
                if (collection == null || collection.Length == 0)
                {
                    return new JsonResult(new { lastCreated = default(DateTime?), lastUpdated = default(DateTime?), items = new List<Budget>() });
                }

                return new JsonResult(new { lastCreated = collection.Max(m => m.CreatedAt), lastUpdated = collection.Max(m => m.ModifiedAt), items = collection });
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Get specific element by id")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new JsonResult(null);
            }

            try
            {
                var collection = await this.Mediator.SendWithRepositoryAsync<Budget, Guid, Budget>(new Budget().Select(s => s), p => p.Id == id);

                return new JsonResult(collection);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("List all using a paged list")]
        [HttpGet("Page/{page:int}")]
        public async Task<IActionResult> GetPage(int page, int size = 10)
        {
            try
            {
                var collection = await Mediator.SendWithPageAsync<Budget, Guid, Budget>(new Budget().Select(s => s),
                    skip: (page - 1) * size, take: size);
                return new JsonResult(collection);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
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
        public async Task<IActionResult> Create(CreateBudgetRequest model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            try
            {
                var result = await Mediator.Send(model);
                return Created(Url.Content($"~/api/{nameof(Budget)}/{result}"), result);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetRequest model)
        {
            if (model == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Partial update an existing element by id")]
        [DisableRequestSizeLimit]
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartialUpdate(Guid id, [FromBody] PartialUpdateBudgetRequest model)
        {
            if (model == null || id != model.Id)
            {
                return BadRequest();
            }

            try
            {
                await Mediator.Send(model);
                return Ok(id);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{Message}", e.Message);
                return Problem(e.Message);
            }
        }

        [SwaggerOperation("Delete an existing element by id")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await Mediator.Send(new DeleteBudgetRequest { Id = id });
                return Ok(id);
            }
            catch (ValidationException v)
            {
                return Problem(string.Join(Environment.NewLine, v.Errors.Select(m => m.ErrorMessage)));
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
            var selector = new Budget().Select(t => new
            {
                t.Id,
                t.Code,
                //Client = new { t.ClientId, ClientFullName = t.Client.FullName, ClientIdentification = t.Client.Identification },
                t.ClientId,
                ClientFullName = t.Client.FullName,
                ClientIdentification = t.Client.Identification,
                t.Status,
                t.Subtotal,
                t.Taxes,
                t.Total,
                t.CreatedAt
            });

            return await base.Table(model, selector);
        }

        #endregion
    }
}
