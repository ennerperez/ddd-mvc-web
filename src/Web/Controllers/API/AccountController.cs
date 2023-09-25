#if (!USING_AUTH0 && !USING_OPENID) && USING_BEARER
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using RegisterModel=Web.Areas.Identity.Pages.Account.RegisterModel.InputModel;

namespace Web.Controllers.API
{

    [Route("api/[controller]/[action]")]
    public class AccountController : ApiControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger;

        public AccountController(IConfiguration configuration, SignInManager<User> signInManager, ILoggerFactory loggerFactory, UserManager<User> userManager, IUserStore<User> userStore, IEmailSender emailSender)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [SwaggerOperation("Create a new token")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Token(NetworkCredential credential)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var audiences = new List<string>();
            _configuration.Bind("AuthSettings:Audiences", audiences);
            if (audiences.Any(m => !string.IsNullOrWhiteSpace(m)) && !audiences.Contains(credential.Domain))
                return BadRequest();

            var result = await _signInManager.PasswordSignInAsync(credential.UserName, credential.Password, true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(credential.UserName);
                var issuer = _configuration["AuthSettings:Issuer"];
                var audience = credential.Domain;
                var key = Encoding.ASCII.GetBytes(_configuration["AppSettings:Secret"]);

                var authClaims = new List<Claim>();

                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var userRole in userRoles)
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                authClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                authClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, credential.UserName));
                authClaims.Add(new Claim(JwtRegisteredClaimNames.Email, credential.UserName));
                authClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(authClaims),
                    Expires = DateTime.UtcNow.AddMinutes(Startup.TokenExpiryDurationMinutes.TotalMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var stringToken = tokenHandler.WriteToken(token);
                return Ok(new {Token = stringToken, expiration = token.ValidTo});
            }
            else if (result.IsLockedOut)
                _logger.LogWarning("User account locked out");
            else
                _logger.LogError("Invalid login attempt");

            return Unauthorized();
        }

    }
}
#endif
