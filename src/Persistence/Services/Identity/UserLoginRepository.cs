using Domain.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;

namespace Persistence.Services.Identity
{
    public class UserLoginRepository : GenericRepository<UserLogin>
    {
        public UserLoginRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
    }
}
