using Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

namespace Persistence.Services.Identity
{
    public class RoleClaimRepository : GenericRepository<RoleClaim>
    {
        public RoleClaimRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }
}
