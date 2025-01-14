using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Infrastructure.Interfaces;
#if USING_IDENTITY
using System.Threading.Tasks;
using Domain.Entities.Identity;
#endif

namespace Tests.Business.Services
{
#if USING_IDENTITY
    public class UserAccessorService : IUserAccessorService<User>
#else
    public class UserAccessorService : IUserAccessorService
#endif
    {
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Scheme { get; }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public string Host { get; }

        public UserAccessorService()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser"), new Claim(ClaimTypes.NameIdentifier, 1.ToString()) }));
        }

        public IPrincipal User { get; }

        public CultureInfo Culture => CultureInfo.CurrentCulture;
        public string UserId => FindLastValue("oid");
        public string IdentityToken => throw new NotImplementedException();
        public string AccessToken => throw new NotImplementedException();
        public string RefreshToken => throw new NotImplementedException();

#if USING_IDENTITY
        public Task<User> GetIdentityUserAsync() =>
            Task.FromResult(new User { Id = 1 });

        public User GetIdentityUser() =>
            new() { Id = 1 };
#endif
        public string FindFirstValue(string claimType) => ((ClaimsPrincipal)User).FindFirst(claimType)?.Value;

        public string FindLastValue(string claimType) => ((ClaimsPrincipal)User).FindAll(claimType).LastOrDefault()?.Value;

        public string[] FindValues(string claimType) => ((ClaimsPrincipal)User).FindAll(claimType).Select(m => m.Value).ToArray();
    }
}
