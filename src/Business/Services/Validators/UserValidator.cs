using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Business.Interfaces.Validators;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;

namespace Business.Services.Validators
{
    public class UserValidator : IUserValidator
    {
        private readonly ILogger _logger;
        private readonly IGenericRepository<User> _userRepository;

        public UserValidator(IGenericRepository<User> userRepository, ILoggerFactory loggerFactory)
        {
            _userRepository = userRepository;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task ValidateAsync(User model, [CallerMemberName] string callerMemberName = "")
        {
            if (callerMemberName.StartsWith("Delete"))
            {
                if (model.Id == 1)
                    throw new ArgumentException("Cannot delete the default user");

                if (await _userRepository.CountAsync() == 1)
                    throw new ArgumentException("Cannot delete the last user");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                    throw new ArgumentException("Email is required");

                if (await _userRepository.AnyAsync(x => x.NormalizedEmail == model.Email.ToUpper() && x.Id != model.Id))
                    throw new ArgumentException("Email already exists");
            }
        }
    }
}
