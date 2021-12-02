using System;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces.Validators;
using Domain.Entities;
using Persistence.Contexts;

namespace Business.Services.Validators
{
    public class UserValidator : IUserValidator
    {
        private readonly DefaultContext _context;

        public UserValidator(DefaultContext context)
        {
            _context = context;
        }

        public Task ValidateAsync(User entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Email))
                throw new ArgumentException("Email is required");

            if (_context.Users.Any(x => x.NormalizedEmail == entity.Email.ToUpper() && x.Id != entity.Id))
                throw new ArgumentException("Email already exists");

            return Task.CompletedTask;
        }
    }
}
