using System;
using System.Linq;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Contexts;
using Persistence.Interfaces;

namespace Persistence.Services
{
    public class ClientRepository : GenericRepository<Client>, IDefaultRepository<Client>
    {
        public ClientRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
        {
            DefaultInclude = r => r.Include(s => s.Budgets);
            DefaultOrderBy = r => r.OrderBy(s => s.Id);
        }

        public Func<IQueryable<Client>, IIncludableQueryable<Client, object>> DefaultInclude { get; }
        public Func<IQueryable<Client>, IOrderedQueryable<Client>> DefaultOrderBy { get; }
    }
}
