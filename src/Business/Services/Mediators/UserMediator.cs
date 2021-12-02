using System.Threading.Tasks;
using Business.Interfaces.Creators;
using Business.Interfaces.Validators;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Business.Services.Creators
{
    public class UserMediator : IUserMediator
    {
        private readonly ILogger _logger;
        private readonly IUserValidator _userValidator;
        private readonly IGenericService<User> _userService;

        public UserMediator(IUserValidator userValidator, IGenericService<User> userService,  ILoggerFactory loggerFactory)
        {
            _userValidator = userValidator;
            _userService = userService;
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public async Task<User> CreateAsync(User model)
        {
            await _userValidator.ValidateAsync(model);
            await _userService.CreateAsync(model);
            return model;
        }

        public async Task<User> ReadAsync(int key)
        {
            var result = await _userService.FirstOrDefaultAsync(m=> m, p=> p.Id == key);
            return result;
        }

        public async Task<User> UpdateAsync(User model)
        {
            await _userValidator.ValidateAsync(model);
            await _userService.UpdateAsync(model);
            return model;
        }

        public async Task<bool> DeleteAsync(int key)
        {
            var model = await _userService.FirstOrDefaultAsync(m=> m, p=> p.Id == key);
            await _userValidator.ValidateAsync(model);
            await _userService.DeleteAsync(key);
            return true;
        }
    }
}
