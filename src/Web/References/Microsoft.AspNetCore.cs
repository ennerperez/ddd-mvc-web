using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
#if ENABLE_AB2C
using Microsoft.Identity.Web;
#endif

#pragma warning disable 618
// ReSharper disable CheckNamespace

namespace Microsoft.AspNetCore
{
    namespace Authentication
    {
        public sealed class SmartScheme
        {
            public const string AuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme
#if ENABLE_APIKEY
                                                       + "," + ApiKeyAuthenticationDefaults.AuthenticationScheme
#endif
#if ENABLE_BEARER
                                                       + "," + JwtBearerDefaults.AuthenticationScheme
#endif
#if ENABLE_OPENID
                                                       + "," + OpenIdConnectDefaults.AuthenticationScheme
#endif
                ;

            public const string DefaultScheme = "SmartScheme";
        }

        namespace ApiKey
        {
            public static class ApiKeyAuthenticationDefaults
            {
                public const string AuthenticationScheme = "ApiKey";

                public const string ProblemDetailsContentType = "application/problem+json";

                public const string ApiKeyHeaderName = "X-Api-Key";
            }

            public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
            {
                public string AuthenticationType => ApiKeyAuthenticationDefaults.AuthenticationScheme;
                public string Scheme => ApiKeyAuthenticationDefaults.AuthenticationScheme;
            }

            public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
            {
                private readonly IGetApiKeyQuery _getApiKeyQuery;

                public ApiKeyAuthenticationHandler(
                    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                    ILoggerFactory logger,
                    UrlEncoder encoder,
                    ISystemClock clock,
                    IGetApiKeyQuery getApiKeyQuery) : base(options, logger, encoder, clock)
                {
                    _getApiKeyQuery = getApiKeyQuery ?? throw new ArgumentNullException(nameof(getApiKeyQuery));
                }

                protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
                {
                    if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.ApiKeyHeaderName, out var apiKeyHeaderValues)) return AuthenticateResult.NoResult();

                    var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

                    if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey)) return AuthenticateResult.NoResult();

                    var existingApiKey = await _getApiKeyQuery.Execute(providedApiKey);

                    if (existingApiKey != null)
                    {
                        var claims = new List<Claim> { new Claim(ClaimTypes.Name, existingApiKey.Owner) };

                        claims.AddRange(existingApiKey.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                        var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                        var identities = new List<ClaimsIdentity> { identity };
                        var principal = new ClaimsPrincipal(identities);
                        var ticket = new AuthenticationTicket(principal, Options.Scheme);

                        return AuthenticateResult.Success(ticket);
                    }

                    return AuthenticateResult.Fail("Invalid API Key provided.");
                }

                protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
                {
                    Response.StatusCode = 401;
                    Response.ContentType = ApiKeyAuthenticationDefaults.ProblemDetailsContentType;
                    await base.HandleChallengeAsync(properties);
                }

                protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
                {
                    Response.StatusCode = 403;
                    Response.ContentType = ApiKeyAuthenticationDefaults.ProblemDetailsContentType;
                    await base.HandleForbiddenAsync(properties);
                }
            }

            public interface IGetApiKeyQuery
            {
                Task<ApiKey> Execute(string providedApiKey);
            }

            public class ApiKey
            {
                public ApiKey(int id, string owner, string key, DateTime created, IReadOnlyCollection<string> roles)
                {
                    Id = id;
                    Owner = owner ?? throw new ArgumentNullException(nameof(owner));
                    Key = key ?? throw new ArgumentNullException(nameof(key));
                    Created = created;
                    Roles = roles ?? throw new ArgumentNullException(nameof(roles));
                }

                public int Id { get; }
                public string Owner { get; }
                public string Key { get; }
                public DateTime Created { get; }
                public IReadOnlyCollection<string> Roles { get; }
            }

            public class StaticApiKeyQuery : IGetApiKeyQuery
            {
                private readonly IDictionary<string, ApiKey> _apiKeys;

                public StaticApiKeyQuery(IConfiguration configuration)
                {
                    var existingApiKeys = new List<ApiKey>();
                    var apilocal = configuration.GetSection("AppSettings")?["ApiKey"];
                    var apikey = new ApiKey(1, configuration["AppSettings:Name"], apilocal, DateTime.Now, new[] { "general" });
                    existingApiKeys.Add(apikey);

                    _apiKeys = existingApiKeys.ToDictionary(x => x.Key, x => x);
                }

                public Task<ApiKey> Execute(string providedApiKey)
                {
                    _apiKeys.TryGetValue(providedApiKey, out var key);
                    return Task.FromResult(key);
                }
            }
        }

        namespace Cookies
        {
            public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
            {
                private string _apiPrefix;

                public CustomCookieAuthenticationEvents(string apiPrefix)
                {
                    _apiPrefix = apiPrefix;
                }

                public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
                {
                    if (!string.IsNullOrWhiteSpace(_apiPrefix) && context.Request.Path.StartsWithSegments($"/{_apiPrefix}") &&
                        context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return base.RedirectToLogin(context);
                    }
                }

                public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
                {
                    if (!string.IsNullOrWhiteSpace(_apiPrefix) && context.Request.Path.StartsWithSegments($"/{_apiPrefix}") &&
                        context.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    else
                    {
                        return base.RedirectToAccessDenied(context);
                    }
                }
            }
        }

        public static class AuthenticationBuilderExtensions
        {
            
            public static AuthenticationBuilder Close(this AuthenticationBuilder authenticationBuilder)
            {
                return authenticationBuilder;
            }
#if ENABLE_AB2C
            public static MicrosoftIdentityWebAppAuthenticationBuilder Close(this MicrosoftIdentityWebAppAuthenticationBuilder authenticationBuilder)
            {
                return authenticationBuilder;
            }
#endif
            
            public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options = null)
            {
                authenticationBuilder.Services.AddTransient<IGetApiKeyQuery, StaticApiKeyQuery>();
                return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationDefaults.AuthenticationScheme, options);
            }

            public static AuthenticationBuilder AddSmartScheme(this AuthenticationBuilder authenticationBuilder)
            {
                return authenticationBuilder.AddPolicyScheme("SmartScheme", "Smart Scheme Selector", options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var hasJwtBearerhHeader = context.Request.Headers.ContainsKey("Authorization") && context.Request.Headers["Authorization"].Any(m => m.ToLower().StartsWith("bearer"));
                        var hasApiKeyHeader = context.Request.Headers.ContainsKey(ApiKeyAuthenticationDefaults.ApiKeyHeaderName) && !string.IsNullOrWhiteSpace(context.Request.Headers[ApiKeyAuthenticationDefaults.ApiKeyHeaderName]);
                        // ReSharper disable once UnusedVariable
                        var hasCookieHeader = context.Request.Headers.ContainsKey("cookie") && !string.IsNullOrWhiteSpace(context.Request.Headers["cookie"]);
                        var hasOpenId = false; //TODO: OpenId

                        if (hasJwtBearerhHeader)
                            return JwtBearerDefaults.AuthenticationScheme;
                        else if (hasApiKeyHeader)
                            return ApiKeyAuthenticationDefaults.AuthenticationScheme;
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        else if (hasOpenId)
                            return OpenIdConnectDefaults.AuthenticationScheme;

                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });
            }
        }
    }

    namespace Http
    {
        public static class SessionExtensions
        {
            public static void SetObjectAsJson(this ISession session, string key, object value)
            {
                session.SetString(key, JsonSerializer.Serialize(value));
            }

            public static T GetObjectFromJson<T>(this ISession session, string key)
            {
                var value = session.GetString(key);

                return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
            }

            public static T GetFromSession<T>(this Mvc.ControllerBase controller, string key = null) where T : class
            {
                if (string.IsNullOrEmpty(key))
                    key = typeof(T).Name;

                return controller.HttpContext.Session.GetObjectFromJson<T>(key);
            }

            // public static void Set<T>(this ISession session, string key, T value)
            // {
            //     using (var ms = new MemoryStream())
            //     {
            //         //TODO: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
            //         var bf = new BinaryFormatter();
            //         bf.Serialize(ms, value);
            //         session.Set(key, ms.ToArray());
            //     }
            // }
            //
            // public static T Get<T>(this ISession session, string key)
            // {
            //     var data = session.Get(key);
            //     using (var ms = new MemoryStream(data))
            //     {
            //         //TODO: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
            //         var bf = new BinaryFormatter();
            //         var result = (T)bf.Deserialize(ms);
            //         return result;
            //     }
            // }

            public static Guid? GetKey(this ISession session)
            {
                var prop = session.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == "_sessionKey");
                if (prop != null)
                {
                    var value = prop.GetValue(session)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return Guid.Parse(value);
                }

                return null;
            }
        }
    }

    namespace Identity
    {
        public static class Extensions
        {
            public static T GetUserId<T>(this ClaimsPrincipal principal) where T : struct
            {
                var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (value != null)
                    return (T)Convert.ChangeType(value, typeof(T));
                return default;
            }

            public static string GetUserId(this ClaimsPrincipal principal) => principal.FindFirstValue(ClaimTypes.NameIdentifier);

            public static string GetUserFullName(this ClaimsPrincipal principal)
            {
                var displayName = string.Join(' ', new[] { principal.FindFirstValue(ClaimTypes.GivenName), principal.FindFirstValue(ClaimTypes.Surname) });
                if (string.IsNullOrWhiteSpace(displayName)) displayName = principal.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrWhiteSpace(displayName)) displayName = "N/A";
                return displayName;
            }
        }
    }

    namespace Mvc
    {
        public static class HtmlRenderExtensions
        {
            private const string ScriptsKey = "scripts_";
            private const string StylesKey = "styles_";

            private static Dictionary<string, Queue<string>> PageList { get; set; } = new Dictionary<string, Queue<string>>();

            public static IDisposable BeginBlock(this IHtmlHelper helper, string key, Func<HttpContext, List<string>> getPageList = null)
            {
                return new HtmlBlock(helper.ViewContext, key, getPageList);
            }

            public static HtmlString PageBlocks(this IHtmlHelper helper, string key)
            {
                if (PageList != null && PageList.Any(m => m.Key.StartsWith(key)))
                {
                    var items = PageList.Where(m => m.Key.StartsWith(key)).Select(m => m.Value);
                    var result = new List<string>();
                    foreach (var qitem in items)
                    {
                        while (qitem.Any())
                        {
                            var value = qitem.Dequeue();
                            if (!string.IsNullOrWhiteSpace(value) && !result.Contains(value))
                                result.Add(value);
                        }
                    }

                    return new HtmlString(string.Join(Environment.NewLine, result.ToArray()));
                }

                return HtmlString.Empty;
            }

            private static List<string> GetPageBlocksList(HttpContext httpContext, string key)
            {
                var pageBlocks = (List<string>)httpContext.Items[key];
                if (pageBlocks == null)
                {
                    pageBlocks = new List<string>();
                    httpContext.Items[key] = pageBlocks;
                }

                return pageBlocks;
            }

            /** Scripts **/
            public static IDisposable BeginScripts(this IHtmlHelper helper, string key = ScriptsKey)
            {
                return BeginBlock(helper, key, m => GetPageScriptsList(m, key));
            }

            public static HtmlString PageScripts(this IHtmlHelper helper, string key = ScriptsKey)
            {
                return PageBlocks(helper, key);
            }

            private static List<string> GetPageScriptsList(HttpContext httpContext, string key = ScriptsKey)
            {
                return GetPageBlocksList(httpContext, key);
            }

            /** Styles **/
            public static IDisposable BeginStyles(this IHtmlHelper helper, string key = StylesKey)
            {
                return BeginBlock(helper, key, m => GetPageStylesList(m, key));
            }

            public static HtmlString PageStyles(this IHtmlHelper helper, string key = StylesKey)
            {
                return PageBlocks(helper, key);
            }

            private static List<string> GetPageStylesList(HttpContext httpContext, string key = StylesKey)
            {
                return GetPageBlocksList(httpContext, key);
            }

            private class HtmlBlock : IDisposable
            {
                private readonly StringWriter _blockWriter;
                private readonly TextWriter _originalWriter;

                private readonly ViewContext _viewContext;
                private string _key;

                public HtmlBlock(ViewContext viewContext, string key, Func<HttpContext, List<string>> getPageList = null)
                {
                    _key = key;
                    _viewContext = viewContext;
                    _originalWriter = _viewContext.Writer;
                    _viewContext.Writer = _blockWriter = new StringWriter();
                    GetPageList = getPageList;
                }

                public Func<HttpContext, List<string>> GetPageList { get; }

                public void Dispose()
                {
                    _viewContext.Writer = _originalWriter;
                    var pageList = GetPageList(_viewContext.HttpContext);
                    pageList.Add(_blockWriter.ToString());

                    if (!PageList.ContainsKey(_key)) PageList.Add(_key, new Queue<string>());
                    if (PageList.ContainsKey(_key))
                        foreach (var item in pageList)
                            PageList[_key].Enqueue(item);
                }
            }
        }

        public static class HtmlExtensions
        {
            public static bool IsDebug(this IHtmlHelper htmlHelper)
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }

            public static bool UsingSass(this IHtmlHelper htmlHelper)
            {
#if USING_SASS
                return true;
#else
                return false;
#endif
            }

            public static bool UsingSwagger(this IHtmlHelper htmlHelper)
            {
#if USING_SWAGGER
                return true;
#else
                return false;
#endif
            }

            public static bool UsingInsights(this IHtmlHelper htmlHelper)
            {
#if USING_INSIGHTS
                return true;
#else
                return false;
#endif
            }

            public static bool UsingNewtonsoft(this IHtmlHelper htmlHelper)
            {
#if USING_NEWTONSOFT
                return true;
#else
                return false;
#endif
            }

            public static bool UsingLocalization(this IHtmlHelper htmlHelper)
            {
#if USING_LOCALIZATION
                return true;
#else
                return false;
#endif
            }
        }

        namespace Rendering
        {
            public static class ViewContextExtensions
            {
                public static bool IsPost(this ViewContext viewContext)
                {
                    return viewContext.HttpContext.Request.Method == "POST";
                }

                public static SelectList ToSelectList<TKey>(this Dictionary<TKey, string> source, string dataValueField = "Key", string dataTextField = "Value")
                {
                    return new SelectList(source.OrderBy(m => m.Key), dataValueField, dataTextField);
                }
                
                public static async Task<string> ToHtmlAsync(this Controller @this, string viewToRender, ViewDataDictionary viewData )
                {
                    var engine = @this.HttpContext.RequestServices.GetService<ICompositeViewEngine>();
                    if (engine == null) return string.Empty;
                    
                    var result = engine.FindView(@this.ControllerContext, viewToRender, false);

                    StringWriter output;
                    using (output = new StringWriter())
                    {
                        if (result.View != null)
                        {
                            var viewContext = new ViewContext(@this.ControllerContext, result.View, viewData, @this.TempData, output, new HtmlHelperOptions());
                            await result.View.RenderAsync(viewContext);
                        }
                    }

                    return output.ToString();
                }
                
            }
        }
    }
}
