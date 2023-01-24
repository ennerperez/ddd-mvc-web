using System.Security.Principal;

namespace Infrastructure.Interfaces
{
	public interface IUserAccessorService
	{
		string Scheme { get; }
		string Host { get; }

		IPrincipal GetActiveUser();

#if USING_IDENTITY
		string FindFirstValue(string claimType);
#endif
	}
}
