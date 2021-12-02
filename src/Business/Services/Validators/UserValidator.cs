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
        private readonly IGenericService<User> _userService;

        public UserValidator(IGenericService<User> userService, ILoggerFactory loggerFactory)
        {
            _userService = userService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task ValidateAsync(User model, [CallerMemberName] string callerMemberName = "")
        {
            if (callerMemberName.StartsWith("Delete"))
            {
                if (model.Id == 1)
                    throw new ArgumentException("Cannot delete the default user");

                if (await _userService.CountAsync() == 1)
                    throw new ArgumentException("Cannot delete the last user");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                    throw new ArgumentException("Email is required");

                if (await _userService.AnyAsync(x => x.NormalizedEmail == model.Email.ToUpper() && x.Id != model.Id))
                    throw new ArgumentException("Email already exists");
            }
        }
    }
}
