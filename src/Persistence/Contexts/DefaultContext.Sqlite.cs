using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		public class Sqlite : DefaultContext
		{
			public Sqlite()
			{
				ProviderName = Providers.Sqlite;
			}

			public Sqlite(DbContextOptions<DefaultContext> options) : base(options)
			{
				ProviderName = Providers.Sqlite;
			}
		}
	}
}
