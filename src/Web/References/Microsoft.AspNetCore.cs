using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
#if USING_TABLES
using Azure;
using Azure.Data.Tables;
using Infrastructure.Interfaces;
#endif
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
#if USING_APIKEY
using Microsoft.AspNetCore.Authentication.ApiKey;
#endif
#if USING_COOKIES
using Microsoft.AspNetCore.Authentication.Cookies;
#endif
#if USING_BEARER
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
#if USING_OPENID
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
#endif
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

// ReSharper disable MemberCanBePrivate.Global

#if USING_AUTH0
using Auth0.AspNetCore.Authentication;
#endif

#pragma warning disable 618
// ReSharper disable CheckNamespace

#if USING_AUTH0
namespace Auth0.AspNetCore.Authentication
{
	public sealed class Auth0Defaults
	{
		//BUG: Auth0Constants.AuthenticationScheme it's not a constant
		public const string AuthenticationScheme = "Auth0";
	}
}
#endif

namespace Microsoft.AspNetCore
{
	namespace Authorization
	{
		public class SmartAuthorizeAttribute : AuthorizeAttribute
		{
			private readonly string[] _schemes = new string[]
			{
				"Identity.Application",
#if !USING_OPENID && USING_COOKIES
				CookieAuthenticationDefaults.AuthenticationScheme,
#endif
#if USING_APIKEY
				ApiKeyAuthenticationDefaults.AuthenticationScheme,
#endif
#if USING_BEARER
				JwtBearerDefaults.AuthenticationScheme,
#endif
#if USING_OPENID
				OpenIdConnectDefaults.AuthenticationScheme,
#endif
#if USING_AUTH0
				Auth0Defaults.AuthenticationScheme,
#endif
#if USING_SMARTSCHEMA
				Authentication.SmartScheme.DefaultScheme,
#endif
			};

			public SmartAuthorizeAttribute()
			{
				AuthenticationSchemes = string.Join(",", _schemes);
			}
		}

	}
	namespace Authentication
	{
		public sealed class SmartScheme
		{
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
				private readonly IApiKeyProvider _apiKeyProvider;

				public ApiKeyAuthenticationHandler(
					IOptionsMonitor<ApiKeyAuthenticationOptions> options,
					ILoggerFactory logger,
					UrlEncoder encoder,
					ISystemClock clock,
					IApiKeyProvider apiKeyProvider) : base(options, logger, encoder, clock)
				{
					_apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
				}

				protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
				{
					if (!Request.Headers.TryGetValue(ApiKeyAuthenticationDefaults.ApiKeyHeaderName, out var apiKeyHeaderValues)) return AuthenticateResult.NoResult();

					var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

					if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey)) return AuthenticateResult.NoResult();

					var existingApiKey = await _apiKeyProvider.Execute(providedApiKey);

					if (existingApiKey == null)
						return AuthenticateResult.Fail("Invalid API Key provided.");

					var claims = new List<Claim> {new Claim(ClaimTypes.Name, existingApiKey.Owner)};

					if (existingApiKey.Roles != null)
						claims.AddRange(existingApiKey.Roles.Split("|").Select(role => new Claim(ClaimTypes.Role, role)));

					var identity = new ClaimsIdentity(claims, Options.AuthenticationType);
					var identities = new List<ClaimsIdentity> {identity};
					var principal = new ClaimsPrincipal(identities);
					var ticket = new AuthenticationTicket(principal, Options.Scheme);

					return AuthenticateResult.Success(ticket);

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

			public interface IApiKeyProvider
			{
				Task<ApiKey> Execute(string providedApiKey);
			}

			public class ApiKey
#if USING_TABLES
				: ITableEntity
#endif
			{
				public ApiKey()
				{

				}

				public ApiKey(int id, string owner, string key, bool active, string roles) : this()
				{
					Id = id;
					Owner = owner ?? throw new ArgumentNullException(nameof(owner));
					Key = key ?? throw new ArgumentNullException(nameof(key));
					Active = active;
					Roles = roles ?? throw new ArgumentNullException(nameof(roles));
				}

				public int Id { get; set; }
				public string Owner { get; set; }
				public string Key { get; set; }
				public string Roles { get; set; }
				public bool Active { get; set; }

#if USING_TABLES
				#region ITableEntity

				public string PartitionKey { get; set; }
				public string RowKey { get; set; }
				public DateTimeOffset? Timestamp { get; set; }
				public ETag ETag { get; set; }

				#endregion
#endif
			}

			public class AppSettingsApiKeyProvider : IApiKeyProvider
			{
				private readonly IDictionary<string, ApiKey> _apiKeys;

				public AppSettingsApiKeyProvider(IConfiguration configuration)
				{
					var existingApiKeys = new List<ApiKey>();
					var apilocal = configuration["AppSettings:ApiKey"];
					var apikey = new ApiKey(1, configuration["AppSettings:Name"], apilocal, true, "General");
					existingApiKeys.Add(apikey);

					_apiKeys = existingApiKeys.ToDictionary(x => x.Key, x => x);
				}

				public Task<ApiKey> Execute(string providedApiKey)
				{
					_apiKeys.TryGetValue(providedApiKey, out var key);
					return Task.FromResult(key);
				}
			}

#if USING_TABLES
			public class TableServiceApiKeyProvider : IApiKeyProvider
			{
				private readonly ITableService _tableService;
				public TableServiceApiKeyProvider(ITableService tableService)
				{
					_tableService = tableService;
				}
				public async Task<ApiKey> Execute(string providedApiKey)
				{
					var apiKey = await _tableService.ReadAllAsync<ApiKey>(p => p.Active && p.Key == providedApiKey);
					return apiKey.FirstOrDefault();
				}
			}
#endif

		}

		namespace Cookies
		{
			public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
			{
				private readonly string _apiPrefix;

				public CustomCookieAuthenticationEvents(string apiPrefix)
				{
					_apiPrefix = apiPrefix;
				}

				public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
				{
					if (string.IsNullOrWhiteSpace(_apiPrefix) || !context.Request.Path.StartsWithSegments($"/{_apiPrefix}") || context.Response.StatusCode != StatusCodes.Status200OK)
						return base.RedirectToLogin(context);

					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return Task.CompletedTask;
				}

				public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
				{
					if (string.IsNullOrWhiteSpace(_apiPrefix) || !context.Request.Path.StartsWithSegments($"/{_apiPrefix}") || context.Response.StatusCode != StatusCodes.Status200OK)
						return base.RedirectToAccessDenied(context);
					context.Response.StatusCode = StatusCodes.Status403Forbidden;
					return Task.CompletedTask;
				}
			}
		}

		public static class AuthenticationBuilderExtensions
		{

			public static AuthenticationBuilder Close(this AuthenticationBuilder authenticationBuilder)
			{
				return authenticationBuilder;
			}
#if USING_AUTH0
			public static Auth0WebAppWithAccessTokenAuthenticationBuilder Close(this Auth0WebAppWithAccessTokenAuthenticationBuilder authenticationBuilder)
			{
				return authenticationBuilder;
			}
#endif

#if USING_APIKEY
			public static AuthenticationBuilder AddApiKey<T>(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options = null) where T : IApiKeyProvider
			{
				authenticationBuilder.Services.AddSingleton(typeof(IApiKeyProvider), typeof(T));
				return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationDefaults.AuthenticationScheme, options);
			}
#endif
#if USING_SMARTSCHEMA
			public static AuthenticationBuilder AddSmartScheme(this AuthenticationBuilder authenticationBuilder)
			{
				return authenticationBuilder.AddPolicyScheme(SmartScheme.DefaultScheme, "Smart Scheme Selector", options =>
				{
					options.ForwardDefaultSelector = context =>
					{
						var hasJwtBearerhHeader = context.Request.Headers.ContainsKey("Authorization") && context.Request.Headers["Authorization"].Any(m => m.ToLower().StartsWith("bearer"));
#if USING_APIKEY
						var hasApiKeyHeader = context.Request.Headers.ContainsKey(ApiKeyAuthenticationDefaults.ApiKeyHeaderName) && !string.IsNullOrWhiteSpace(context.Request.Headers[ApiKeyAuthenticationDefaults.ApiKeyHeaderName]);
#endif
						// ReSharper disable once UnusedVariable
						var hasCookieHeader = context.Request.Headers.Any(m=> m.Key.EndsWith(CookieAuthenticationDefaults.AuthenticationScheme) && !m.Key.Contains("Antiforgery"));
#if USING_OPENID
						//TODO: Better implementation
						var hasOpenId = true;
#endif
						if (hasJwtBearerhHeader)
							return JwtBearerDefaults.AuthenticationScheme;
#if USING_APIKEY
						else if (hasApiKeyHeader)
							return ApiKeyAuthenticationDefaults.AuthenticationScheme;
#endif
						// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#if USING_OPENID
						else if (hasOpenId)
							return OpenIdConnectDefaults.AuthenticationScheme;
#endif

						return CookieAuthenticationDefaults.AuthenticationScheme;
					};
				});
			}
#endif
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
			//         // TODO: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
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
			//         // TODO: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
			//         var bf = new BinaryFormatter();
			//         var result = (T)bf.Deserialize(ms);
			//         return result;
			//     }
			// }

			public static Guid? GetKey(this ISession session)
			{
				var prop = session.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == "_sessionKey");
				if (prop == null)
					return null;
				var value = prop.GetValue(session)?.ToString();
				if (!string.IsNullOrWhiteSpace(value))
					return Guid.Parse(value);

				return null;
			}
		}
	}

	namespace Mvc
	{
		public static class HtmlRenderExtensions
		{
			private const string ScriptsKey = "scripts_";
			private const string StylesKey = "styles_";

			public static Dictionary<string, Queue<string>> PageList { get; } = new Dictionary<string, Queue<string>>();

			public static IDisposable BeginBlock(this IHtmlHelper helper, string key, Func<HttpContext, List<string>> getPageList = null)
			{
				return new HtmlBlock(helper.ViewContext, key, getPageList);
			}

			public static HtmlString PageBlocks(this IHtmlHelper helper, string key)
			{
				if (PageList == null || !PageList.Any(m => m.Key.StartsWith(key)))
					return HtmlString.Empty;

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

			public static List<string> GetPageBlocksList(HttpContext httpContext, string key)
			{
				var pageBlocks = (List<string>)httpContext.Items[key];
				if (pageBlocks != null)
					return pageBlocks;

				pageBlocks = new List<string>();
				httpContext.Items[key] = pageBlocks;

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

			public static List<string> GetPageScriptsList(HttpContext httpContext, string key = ScriptsKey)
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

			public static List<string> GetPageStylesList(HttpContext httpContext, string key = StylesKey)
			{
				return GetPageBlocksList(httpContext, key);
			}

			private class HtmlBlock : IDisposable
			{
				private readonly StringWriter _blockWriter;
				private readonly TextWriter _originalWriter;

				private readonly ViewContext _viewContext;
				private readonly string _key;

				public HtmlBlock(ViewContext viewContext, string key, Func<HttpContext, List<string>> getPageList = null)
				{
					_key = key;
					_viewContext = viewContext;
					_originalWriter = _viewContext.Writer;
					_viewContext.Writer = _blockWriter = new StringWriter();
					GetPageList = getPageList;
				}

				private Func<HttpContext, List<string>> GetPageList { get; }

				public void Dispose()
				{
					_viewContext.Writer = _originalWriter;
					var pageList = GetPageList(_viewContext.HttpContext);
					pageList.Add(_blockWriter.ToString());

					if (!PageList.ContainsKey(_key))
					{
						try
						{
							PageList.Add(_key, new Queue<string>());
						}
						catch (Exception e)
						{
							Debug.WriteLine(e);
						}
					}

					if (!PageList.TryGetValue(_key, out var value))
						return;

					foreach (var item in pageList)
					{
						try
						{
							value.Enqueue(item);
						}
						catch (Exception e)
						{
							Debug.WriteLine(e);
						}
					}
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

			public static bool UsingIdentity(this IHtmlHelper htmlHelper)
			{
#if USING_IDENTITY
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

				public static async Task<string> ToHtmlAsync(this Controller @this, string viewToRender, ViewDataDictionary viewData)
				{
					var engine = @this.HttpContext.RequestServices.GetService<ICompositeViewEngine>();
					if (engine == null) return string.Empty;

					var result = engine.FindView(@this.ControllerContext, viewToRender, false);

					StringWriter output;
					await using (output = new StringWriter())
					{
						if (result.View == null)
							return output.ToString();

						var viewContext = new ViewContext(@this.ControllerContext, result.View, viewData, @this.TempData, output, new HtmlHelperOptions());
						await result.View.RenderAsync(viewContext);
					}

					return output.ToString();
				}

			}
		}

		namespace TagHelpers
		{
			[HtmlTargetElement(Attributes = ActiveClassAttributeName)]
			public class ActiveClassTagHelper : AnchorTagHelper
			{

				private const string ActiveClassAttributeName = "asp-active-class";

				[HtmlAttributeName(ActiveClassAttributeName)]
				private string ActiveClass { get; set; }

				public ActiveClassTagHelper(IHtmlGenerator generator) : base(generator)
				{
				}

				public override void Process(TagHelperContext context, TagHelperOutput output)
				{
					var routeData = ViewContext.RouteData.Values;
					var currentArea = routeData["area"] as string;
					var currentController = routeData["controller"] as string;
					var currentAction = routeData["action"] as string;
					var result = false;

					if (!string.IsNullOrWhiteSpace(Area) && !string.IsNullOrWhiteSpace(Controller) && !string.IsNullOrWhiteSpace(Action))
					{
						result = string.Equals(Action, currentAction, StringComparison.OrdinalIgnoreCase) &&
						         string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase) &&
						         string.Equals(Area, currentArea, StringComparison.OrdinalIgnoreCase);
					}
					else if (!string.IsNullOrWhiteSpace(Controller) && !string.IsNullOrWhiteSpace(Action))
					{
						result = string.Equals(Action, currentAction, StringComparison.OrdinalIgnoreCase) &&
						         string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase);
					}
					else if (!string.IsNullOrWhiteSpace(Action))
					{
						result = string.Equals(Action, currentAction, StringComparison.OrdinalIgnoreCase);
					}
					else if (!string.IsNullOrWhiteSpace(Controller))
					{
						result = string.Equals(Controller, currentController, StringComparison.OrdinalIgnoreCase);
					}

					if (!result)
						return;

					var @cssClass = ActiveClass ?? "active";
					var existingClasses = output.Attributes["class"].Value.ToString();
					if (output.Attributes["class"] != null)
					{
						output.Attributes.Remove(output.Attributes["class"]);
					}

					output.Attributes.Add("class", $"{existingClasses} {cssClass}");
				}
			}
		}
	}
}
