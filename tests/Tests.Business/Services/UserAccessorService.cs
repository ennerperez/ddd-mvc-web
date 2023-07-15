using System.Security.Claims;
using System.Security.Principal;
using Infrastructure.Interfaces;
#if USING_IDENTITY
using System.Linq;
#endif

namespace Tests.Business.Services
{
    public class UserAccessorService : IUserAccessorService
    {

        private readonly ClaimsPrincipal _testUser;

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Scheme { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Host { get; }

        public UserAccessorService()
        {
            _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser"), new Claim(ClaimTypes.NameIdentifier, 1.ToString()), }));
        }

        public IPrincipal GetActiveUser()
        {
            return _testUser;
        }

#if USING_IDENTITY
		public string FindFirstValue(string claimType)
		{
			return _testUser.FindFirstValue(claimType);
		}
		public string FindLastValue(string claimType)
		{
			return _testUser.FindAll(claimType).LastOrDefault()?.Value;
		}
#endif
    }
}
