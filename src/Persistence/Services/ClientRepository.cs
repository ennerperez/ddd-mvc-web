using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Services
{
    public class ClientRepository : GenericRepository<Client>
    {
        public ClientRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
        }
        
    }
}
