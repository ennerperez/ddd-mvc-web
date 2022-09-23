using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Services
{
	public class BudgetRepository : GenericRepository<Budget, Guid>
	{
		public BudgetRepository(DefaultContext context, ILoggerFactory logger, IConfiguration configuration) : base(context, logger, configuration)
		{
		}

	}
}
