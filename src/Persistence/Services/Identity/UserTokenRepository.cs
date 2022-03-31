using Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

namespace Persistence.Services.Identity
{
    public class UserTokenRepository : GenericRepository<UserToken>
    {
        public UserTokenRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }
}
