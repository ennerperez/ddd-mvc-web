using System.Threading.Tasks;
using Business.Interfaces.Mediators;
using Business.Interfaces.Validators;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Business.Services.Mediators
{
    public class UserMediator : IUserMediator
    {
        private readonly ILogger _logger;
        private readonly IUserValidator _userValidator;
        private readonly IGenericRepository<User> _userRepository;

        public UserMediator(IUserValidator userValidator, IGenericRepository<User> userRepository,  ILoggerFactory loggerFactory)
        {
            _userValidator = userValidator;
            _userRepository = userRepository;
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public async Task<User> CreateAsync(User model)
        {
            await _userValidator.ValidateAsync(model);
            await _userRepository.CreateAsync(model);
            return model;
        }

        public async Task<User> ReadAsync(int key)
        {
            var result = await _userRepository.FirstOrDefaultAsync(m=> m, p=> p.Id == key);
            return result;
        }

        public async Task<User> UpdateAsync(User model)
        {
            await _userValidator.ValidateAsync(model);
            await _userRepository.UpdateAsync(model);
            return model;
        }

        public async Task<bool> DeleteAsync(int key)
        {
            var model = await _userRepository.FirstOrDefaultAsync(m=> m, p=> p.Id == key);
            await _userValidator.ValidateAsync(model);
            await _userRepository.DeleteAsync(key);
            return true;
        }
    }
}
