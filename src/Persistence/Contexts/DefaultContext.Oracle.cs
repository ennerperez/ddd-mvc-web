using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts
{
	public partial class DefaultContext
	{
		public class Oracle : DefaultContext
		{
			public Oracle()
			{
				ProviderName = Providers.Oracle;
			}

			public Oracle(DbContextOptions<DefaultContext> options) : base(options)
			{
				ProviderName = Providers.Oracle;
			}
		}
	}
}
