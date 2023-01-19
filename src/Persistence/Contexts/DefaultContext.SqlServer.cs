using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		public class SqlServer : DefaultContext
		{
			public SqlServer()
			{
				ProviderName = Providers.SqlServer;
			}

			public SqlServer(DbContextOptions<DefaultContext> options) : base(options)
			{
				ProviderName = Providers.SqlServer;
			}
		}
	}
}
