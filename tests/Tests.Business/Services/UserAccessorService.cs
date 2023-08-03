using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Infrastructure.Interfaces;
#if USING_IDENTITY
using Domain.Entities.Identity;
using System.Linq;
#endif

namespace Tests.Business.Services
{
#if USING_IDENTITY
    public class UserAccessorService : IUserAccessorService<User>
#else
    public class UserAccessorService : IUserAccessorService
#endif
    {
        private readonly ClaimsPrincipal _testUser;

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Scheme { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Host { get; }

        public UserAccessorService()
        {
            _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, "TestUser"), new Claim(ClaimTypes.NameIdentifier, 1.ToString()),}));
        }

        public IPrincipal GetActiveUser()
        {
            return _testUser;
        }

#if USING_IDENTITY
        public Task<User> GetLocalActiveUserAsync()
        {
            return Task.FromResult(new User()
            {
                Id = 1
            });
        }
        public string FindFirstValue(string claimType)
        {
            return _testUser.FindFirstValue(claimType);
        }

        public string FindLastValue(string claimType)
        {
            return _testUser.FindAll(claimType).LastOrDefault()?.Value;
        }

        public string[] FindValues(string claimType)
        {
            return _testUser.FindAll(claimType).Select(m => m.Value).ToArray();
        }
#endif
    }
}
