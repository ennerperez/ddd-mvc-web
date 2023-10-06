using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Storage;
#if USING_AUTH0
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using Domain.Entities.Identity;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;
using IBrowser = IdentityModel.OidcClient.Browser.IBrowser;
#endif

namespace Microsoft.Maui.Authentication
{
#if USING_AUTH0
    namespace Auth0
    {


        public class Auth0AuthenticationStateProvider
        {
            public static ClaimsPrincipal CurrentUser = new(new ClaimsIdentity());
            private readonly Auth0Client _auth0Client;
            private readonly IGenericRepository<User> _userRepository;

            public User IdentityUser { get; private set; }

            public Auth0AuthenticationStateProvider(
                Auth0Client client,
                IGenericRepository<User> userRepository)
            {
                _auth0Client = client;
                _userRepository = userRepository;
            }

            public async Task LogInAsync()
            {
                var user = await LoginWithAuth0Async();
                CurrentUser = user;
            }

            public async Task<bool> ValidateTokenAsync()
            {
                var token = await GetAccessToken();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtSecurityToken = handler.ReadJwtToken(token);
                        var value = jwtSecurityToken.Claims.FirstOrDefault(a => a.Type == "exp")?.Value;

                        if (long.TryParse(value, out var exp) && exp > 0)
                        {
                            if ((new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(exp)).ToLocalTime() > DateTime.Now)
                            {
                                var user = new ClaimsIdentity(jwtSecurityToken.Claims);

                                CurrentUser = await createUserAsync(new ClaimsPrincipal(user));
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return false;
            }

            private async Task<ClaimsPrincipal> LoginWithAuth0Async()
            {
                var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
                var loginResult = await _auth0Client.LoginAsync();

                if (!loginResult.IsError)
                {
                    await SecureStorage.Default.SetAsync("access_token", loginResult.AccessToken);
                    await SecureStorage.Default.SetAsync("id_token", loginResult.IdentityToken);

                    authenticatedUser = await createUserAsync(loginResult.User);
                }
                return authenticatedUser;
            }

            private async Task<ClaimsPrincipal> createUserAsync(ClaimsPrincipal identity)
            {
                User user;
                var existing = await _userRepository.AnyAsync(a => a.Email == identity.FindFirstValue("email"));
                if (existing)
                {
                    user = await _userRepository.FirstOrDefaultAsync(s => s, a => a.Email == identity.FindFirstValue("email"),
                        include: y => y.Include(v => v.UserClaims));
                }
                else
                {
                    user = new User()
                    {
                        Email = identity.FindFirstValue("email"),
                        EmailConfirmed = true,
                        UserName = identity.FindFirstValue("email"),
                        NormalizedEmail = identity.FindFirstValue("email").ToUpper(),
                        NormalizedUserName = identity.FindFirstValue("email").ToUpper(),
                    };

                    user.UserClaims = identity.Claims.Select(a => new UserClaim(a.Type, a.Value)).ToList();

                    await _userRepository.CreateAsync(user);

                    user.UserClaims.Add(new UserClaim(ClaimTypes.NameIdentifier, user.Id.ToString()));

                    await _userRepository.UpdateAsync(user);
                }

                return fillUser(user, identity);
            }

            private ClaimsPrincipal fillUser(User u, ClaimsPrincipal user)
            {
                var identity = new ClaimsIdentity(user.Identity);

                identity.ReplaceClaim(ClaimTypes.NameIdentifier, u.Id.ToString());
                identity.ReplaceClaim(ClaimTypes.GivenName, user.FindFirstValue("given_name"));
                identity.ReplaceClaim(ClaimTypes.Surname, user.FindFirstValue("family_name"));

                IdentityUser = u;

                return new ClaimsPrincipal(identity);
            }

            public async Task LogOut()
            {
                await _auth0Client.LogoutAsync();
                CurrentUser = new ClaimsPrincipal(new ClaimsIdentity());

                await SecureStorage.Default.SetAsync("access_token", string.Empty);
                await SecureStorage.Default.SetAsync("id_token", string.Empty);
            }

            public static Task<string> GetAccessToken()
            {
                return SecureStorage.Default.GetAsync("id_token");
            }

            public User GetUser()
            {
                return IdentityUser;
            }

            public async Task UpdateUser()
            {
                var u = await _userRepository.FirstOrDefaultAsync(s => s, a => a.Email == CurrentUser.FindFirstValue("email"),
                    include: y => y.Include(v => v.UserClaims));
                CurrentUser = fillUser(u, CurrentUser);
            }
        }

        public class Auth0Client
        {
            private readonly OidcClient _oidcClient;

            public Auth0Client(Auth0ClientOptions options)
            {
                _oidcClient = new OidcClient(new OidcClientOptions
                {
                    Authority = $"https://{options.Domain}",
                    ClientId = options.ClientId,
                    Scope = options.Scope,
                    RedirectUri = options.RedirectUri,
                    Browser = options.Browser
                });
            }

            public IBrowser Browser
            {
                get => _oidcClient.Options.Browser;
                set => _oidcClient.Options.Browser = value;
            }

            public async Task<LoginResult> LoginAsync()
            {
                return await _oidcClient.LoginAsync();
            }

            public async Task<BrowserResult> LogoutAsync()
            {
                var logoutParameters = new Dictionary<string, string>
                {
                    {
                        "client_id", _oidcClient.Options.ClientId
                    },
                    {
                        "returnTo", _oidcClient.Options.RedirectUri
                    }
                };

                var logoutRequest = new LogoutRequest();
                var endSessionUrl = new RequestUrl($"{_oidcClient.Options.Authority}/v2/logout")
                    .Create(new Parameters(logoutParameters));
                var browserOptions = new BrowserOptions(endSessionUrl, _oidcClient.Options.RedirectUri)
                {
                    Timeout = TimeSpan.FromSeconds(logoutRequest.BrowserTimeout), DisplayMode = logoutRequest.BrowserDisplayMode
                };

                var browserResult = await _oidcClient.Options.Browser.InvokeAsync(browserOptions);

                return browserResult;
            }
        }

        public class Auth0ClientOptions
        {
            public Auth0ClientOptions()
            {
                Browser = new WebBrowserAuthenticator();
            }

            public string Domain { get; set; }

            public string ClientId { get; set; }

            public string RedirectUri { get; set; }

            public string Scope { get; set; }

            public IBrowser Browser { get; set; }
        }

        public class WebBrowserAuthenticator : IBrowser
        {
            public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
            {
                try
                {
                    var result = await WebAuthenticator.Default.AuthenticateAsync(
                        new Uri(options.StartUrl),
                        new Uri(options.EndUrl));

                    var url = new RequestUrl(options.EndUrl)
                        .Create(new Parameters(result.Properties));

                    return new BrowserResult
                    {
                        Response = url, ResultType = BrowserResultType.Success
                    };
                }
                catch (TaskCanceledException)
                {
                    return new BrowserResult
                    {
                        ResultType = BrowserResultType.UserCancel, ErrorDescription = "Login canceled by the user."
                    };
                }
            }
        }
    }
#endif
}
