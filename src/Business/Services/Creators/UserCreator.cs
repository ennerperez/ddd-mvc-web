using System.Threading.Tasks;
using Business.Interfaces.Creators;
using Business.Interfaces.Validators;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

namespace Business.Services.Creators
{
    public class UserCreator : IUserCreator
    {
        private readonly ILogger _logger;
        private readonly DefaultContext _context;
        private readonly IUserValidator _userValidator;
        
        public UserCreator(DefaultContext context, IUserValidator userValidator, ILoggerFactory loggerFactory)
        {
            _context = context;
            _userValidator = userValidator;
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public async Task<User> CreateAsync(User account)
        {
            await _userValidator.ValidateAsync(account);
            await _context.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }
    }
}
