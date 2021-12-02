using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Persistence.Interfaces;
using Persistence.Contexts;

namespace Persistence.Services
{
    public class RoleClaimRepository : GenericRepository<RoleClaim>, IRoleClaimRepository
    {
        public RoleClaimRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }
}
