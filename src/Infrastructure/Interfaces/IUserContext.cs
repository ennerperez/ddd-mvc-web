using System.Globalization;
using System.Security.Principal;
#if USING_IDENTITY
using System.Threading.Tasks;
#endif

namespace Infrastructure.Interfaces
{
    public interface IUserContext : IUserContext<object>;

    public interface IUserContext<TUser> where TUser : class
    {
        string Scheme { get; }
        string Host { get; }

        public CultureInfo Culture { get; }
        public IPrincipal User { get; }

        public string UserId { get; }

        public string IdentityToken { get; }
        public string AccessToken { get; }
        public string RefreshToken { get; }

#if USING_IDENTITY
    Task<TUser> GetIdentityUserAsync();
    TUser GetIdentityUser();
#endif
        string FindFirstValue(string claimType);
        string FindLastValue(string claimType);
        string[] FindValues(string claimType);
    }
}
