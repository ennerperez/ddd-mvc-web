using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		public class PostgreSql : DefaultContext
		{
			public PostgreSql()
			{
				ProviderName = Providers.PostgreSql;
			}

			public PostgreSql(DbContextOptions<DefaultContext> options) : base(options)
			{
				ProviderName = Providers.PostgreSql;
			}
		}
	}
}
