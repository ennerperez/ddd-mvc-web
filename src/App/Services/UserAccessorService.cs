using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
#if USING_IDENTITY
using System.Threading.Tasks;
#endif
using Domain.Entities.Identity;
using Infrastructure.Interfaces;
#if USING_AUTH0
using Microsoft.Maui.Authentication.Auth0;
#endif
namespace App.Services
{
    public class UserAccessorService : IUserAccessorService<User>
    {
        public UserAccessorService()
        {
        }

        public string Scheme => string.Empty;

        public string Host => string.Empty;
        public IPrincipal GetActiveUser()
        {
#if USING_AUTH0
            return Auth0AuthenticationStateProvider.CurrentUser;
#else
            return new ClaimsPrincipal();
#endif
        }

#if USING_IDENTITY
        public Task<User> GetLocalActiveUserAsync()
        {
            throw new NotImplementedException();
        }
        public string FindFirstValue(string claimType)
        {
            return ((ClaimsPrincipal)GetActiveUser())?.FindFirstValue(claimType) ?? string.Empty;
        }
        public string FindLastValue(string claimType)
        {
            return ((ClaimsPrincipal)GetActiveUser())?.FindFirstValue(claimType) ?? string.Empty;
        }
        public string[] FindValues(string claimType)
        {
            return ((ClaimsPrincipal)GetActiveUser())?.Claims.Where(m => m.Type == claimType).Select(m => m.Value.ToString()).ToArray();
        }

#endif
    }
}
