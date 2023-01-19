using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		public class MySql : DefaultContext
		{
			public MySql()
			{
				ProviderName = Providers.MySql;
			}

			public MySql(DbContextOptions<DefaultContext> options) : base(options)
			{
				ProviderName = Providers.MySql;
			}
		}
	}
}
