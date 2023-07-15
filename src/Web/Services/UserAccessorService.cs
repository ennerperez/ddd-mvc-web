using System.Linq;
using System.Security.Principal;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
#if USING_IDENTITY
using System.Security.Claims;
#endif

namespace Web.Services
{
    public class UserAccessorService : IUserAccessorService
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

        public IPrincipal GetActiveUser()
        {
            if (_httpContext.HttpContext != null)
            {
                return _httpContext.HttpContext.User;
            }

            return null;
        }
#if USING_IDENTITY
		public string FindFirstValue(string claimType)
		{
			if (_httpContext.HttpContext != null) return _httpContext.HttpContext.User.FindFirstValue(claimType);
			return string.Empty;
		}
		public string FindLastValue(string claimType)
		{
			if (_httpContext.HttpContext != null) return _httpContext.HttpContext.User.FindAll(claimType).LastOrDefault()?.Value;
			return string.Empty;
		}
#endif
    }
}
