using System.Security.Principal;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{

    public interface IUserAccessorService : IUserAccessorService<object>
    {

    }
    public interface IUserAccessorService<TUser> where TUser : class
    {
        string Scheme { get; }
        string Host { get; }

        IPrincipal GetActiveUser();

#if USING_IDENTITY
        Task<TUser> GetLocalActiveUserAsync();
        string FindFirstValue(string claimType);
        string FindLastValue(string claimType);
        string[] FindValues(string claimType);
#endif
    }
}
