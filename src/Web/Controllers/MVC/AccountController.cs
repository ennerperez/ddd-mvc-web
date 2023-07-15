#if !USING_IDENTITY
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
#if USING_AUTH0
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Auth0.AspNetCore.Authentication;
#endif
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers.MVC
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public async Task Login(string returnUrl = "/")
        {
#if USING_AUTH0
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                // Indicate here where Auth0 should redirect the user after a login.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Callback URLs** settings for the app.
                .WithRedirectUri(returnUrl)
                .Build();

            await HttpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
#else
            await Task.Yield();
#endif
        }

        [Authorize]
        public async Task Logout(string returnUrl = "/")
        {
#if USING_AUTH0
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Logout URLs** settings for the app.
                .WithRedirectUri(returnUrl)
                .Build();

            // Logout from Auth0
            await HttpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            // Logout from the application
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
#else
            await Task.Yield();
#endif

            // Cookies Clear
            HttpContext.Session.Clear();
            foreach (var cookie in Request.Cookies.Keys)
                Response.Cookies.Delete(cookie);
        }


        [Authorize]
        public IActionResult Profile()
        {
            return View(new UserProfileViewModel() {Name = User.Identity?.Name, EmailAddress = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value, ProfileImage = User.Claims.FirstOrDefault(c => c.Type == "picture")?.Value});
        }

        [Authorize]
        public IActionResult Claims()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
#endif
