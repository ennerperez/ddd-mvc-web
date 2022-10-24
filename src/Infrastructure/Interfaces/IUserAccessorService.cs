using System.Security.Principal;

namespace Infrastructure.Interfaces
{
	public interface IUserAccessorService
	{
		string Scheme { get; }
		string Host { get; }

		IPrincipal GetActiveUser();

		string FindFirstValue(string claimType);
	}
}
