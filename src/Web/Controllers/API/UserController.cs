using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Business.Abstractions;
using Business.Requests.Identity;
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
                //Mediator.Send2<User>(s=> new { s.Id, s.Email}, null);
                //var d1 = Mediator.Send2<User>(s => new {s.Id, s.Email}, p => true, CancellationToken.None);
                var collection = await Mediator.Get((new User()).Select(s => new {s.Id, s.Email, s.CreatedAt, s.ModifiedAt}), null, null, null);

                //var collection = await Mediator.Send(new ReadUserRequest(s=> new {s.Id, s.Email}));
                //var collection = await Repository.ReadAsync(s => s);

                if (collection == null || !collection.Any())
                    return new JsonResult(new {last_created = default(DateTime?), last_updated = default(DateTime?), items = new List<Setting>()});

                return new JsonResult(new {last_created = collection.Max(m => m.CreatedAt), last_updated = collection.Max(m => m.ModifiedAt), items = collection});
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
                    var collection = await Mediator.Get((new User()).Select(s => s), p => p.Id == id, null, null);

                    //var items = await Repository.ReadAsync(s => s, p => p.Id == id);

                    return new JsonResult(collection);
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
                var collection = await Mediator.GetPaginated((new User()).Select(s => s),
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
        public async Task<IActionResult> Create(CreateUserRequest model)
        {
            if (model == null) return BadRequest();

            try
            {
                var result = await Mediator.Send(model);

                // var record = model;
                // record.Id = default;
                // record.NormalizedEmail = model.Email.ToUpper();
                // record.NormalizedUserName = model.UserName.ToUpper();
                //
                // await _userMediator.CreateAsync(record);

                return Created(Url.Content($"~/api/{nameof(User)}/{result}"), result);
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
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest model)
        {
            if (model == null || id != model.Id) return BadRequest();

            try
            {
                // var record = await Repository.FirstOrDefaultAsync(s => s, p => p.Id == id);
                //
                // if (record != null)
                // {
                //     record.Email = model.Email;
                //     record.PhoneNumber = model.PhoneNumber;
                //     record.NormalizedEmail = model.Email?.ToUpper();
                //     record.EmailConfirmed = model.EmailConfirmed;
                //     record.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
                //     record.TwoFactorEnabled = model.TwoFactorEnabled;
                //     
                //     await _userMediator.UpdateAsync(record);
                // }

                await Mediator.Send(model);

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
                await Mediator.Send(new DeleteUserRequest() {Id = id});
                //await _userMediator.DeleteAsync(id);
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
            var selector = (new User()).Select(t => new
            {
                Id = t.Id,
                UserName = t.UserName,
                Email = t.Email,
                EmailConfirmed = t.EmailConfirmed,
                PhoneNumber = t.PhoneNumber,
                PhoneNumberConfirmed = t.PhoneNumberConfirmed,
                TwoFactorEnabled = t.TwoFactorEnabled,
                FirstName = t.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.GivenName).ClaimValue,
                LastName = t.UserClaims.FirstOrDefault(c => c.ClaimType == System.Security.Claims.ClaimTypes.Surname).ClaimValue,
                //LoginCounts = t.UserLogins.Count()
            });

            return await base.Table(model, selector);
        }

        #endregion
    }
}
