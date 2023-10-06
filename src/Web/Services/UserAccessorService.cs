using System.Security.Principal;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
#if USING_IDENTITY
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
#endif

namespace Web.Services
{
#if USING_IDENTITY
    public class UserAccessorService : IUserAccessorService<User>
#else
    public class UserAccessorService : IUserAccessorService
#endif
    {
        private readonly IHttpContextAccessor _httpContext;

        public string Scheme
        {
            get
            {
                if (_httpContext.HttpContext != null)
                {
                    return _httpContext.HttpContext.Request.Scheme;
                }

                return string.Empty;
            }
        }

        public string Host
        {
            get
            {
                if (_httpContext.HttpContext != null)
                {
                    return _httpContext.HttpContext.Request.Host.ToString();
                }

                return string.Empty;
            }
        }

        public HttpContext Context => _httpContext.HttpContext;

        public UserAccessorService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

#if USING_IDENTITY

        private readonly UserManager<User> _userManager;
        public UserAccessorService(IHttpContextAccessor httpContext, UserManager<User> userManager)
        {
            _httpContext = httpContext;
            _userManager = userManager;
        }
#endif

        public IPrincipal GetActiveUser()
        {
            if (_httpContext.HttpContext != null)
            {
                return _httpContext.HttpContext.User;
            }

            return null;
        }

#if USING_IDENTITY
        public async Task<User> GetLocalActiveUserAsync()
        {
            if (_httpContext.HttpContext != null)
            {
                var email = _httpContext.HttpContext.User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    return user;
                }
            }
            return null;
        }
        public string FindFirstValue(string claimType)
        {
            if (_httpContext.HttpContext != null)
            {
                return _httpContext.HttpContext.User.FindFirstValue(claimType);
            }

            return string.Empty;
        }
        public string FindLastValue(string claimType)
        {
            if (_httpContext.HttpContext != null)
            {
                return _httpContext.HttpContext.User.FindAll(claimType).LastOrDefault()?.Value;
            }

            return string.Empty;
        }

        public string[] FindValues(string claimType)
        {
            if (_httpContext.HttpContext != null)
            {
                return _httpContext.HttpContext.User.FindAll(claimType).Select(m => m.Value).ToArray();
            }

            return Array.Empty<string>();
        }
#endif
    }
}
