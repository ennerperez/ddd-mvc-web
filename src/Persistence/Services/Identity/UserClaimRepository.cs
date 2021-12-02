using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Persistence.Interfaces;
using Persistence.Contexts;

namespace Persistence.Services
{
    public class UserClaimRepository : GenericRepository<UserClaim>, IUserClaimRepository
    {
        public UserClaimRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }
}
