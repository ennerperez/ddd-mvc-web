// #define SESSION_TEST

using System;
using System.Linq;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Business;
using Domain;
using Domain.Entities.Identity;
using Infrastructure;
using Infrastructure.Interfaces;
#if USING_QUESTPDF
using Infrastructure.Services;
#endif
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
#if ENABLE_APIKEY
using Microsoft.AspNetCore.Authentication.ApiKey;
#endif
#if ENABLE_BEARER
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
#if ENABLE_OPENID
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
#endif
#if ENABLE_AB2C
using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Persistence;
using Persistence.Contexts;
using Web.Services;

#if USING_NEWTONSOFT
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

#if USING_SWAGGER
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
#endif

#if USING_LOCALIZATION
using System.Globalization;
using Microsoft.AspNetCore.Localization;
#endif


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Web
{
	public class Startup
	{

#if DEBUG && SESSION_TEST
        // internal static TimeSpan SessionIdleTimeout = TimeSpan.FromMinutes(1);
        // internal static TimeSpan SessionIOTimeout = TimeSpan.FromMinutes(1);
        internal static TimeSpan AntiforgeryExpiration = TimeSpan.FromMinutes(10);
        internal static TimeSpan CookieExpireTimeSpan = TimeSpan.FromMinutes(10);
        internal static TimeSpan TokenExpiryDurationMinutes = TimeSpan.FromMinutes(10);
#else
		// internal static TimeSpan SessionIdleTimeout = TimeSpan.FromHours(4);
		// internal static TimeSpan SessionIOTimeout = TimeSpan.FromHours(4);
		internal static TimeSpan AntiforgeryExpiration = TimeSpan.FromHours(8);
		internal static TimeSpan CookieExpireTimeSpan = TimeSpan.FromDays(8);
		internal static TimeSpan TokenExpiryDurationMinutes = TimeSpan.FromDays(8);
#endif
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			Name = configuration["AppSettings:Name"];
#if USING_LOCALIZATION
			SupportedCultures = configuration.GetSection("CultureInfo:SupportedCultures").Get<string[]>().Select(m => new CultureInfo(m)).ToArray();
			CurrencyCulture = new CultureInfo(configuration["CultureInfo:CurrencyCulture"]);
#endif
		}

		internal static string Name { get; private set; }
		internal IConfiguration Configuration { get; private set; }

#if USING_LOCALIZATION
		internal CultureInfo[] SupportedCultures { get; private set; }

		internal static CultureInfo CurrencyCulture { get; private set; }

#endif

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
#if USING_SASS && DEBUG
			services.AddSassCompiler();
#endif
#if USING_INSIGHTS
			// The following line enables Application Insights telemetry collection.
			services.AddApplicationInsightsTelemetry(options =>
			{
				options.ConnectionString = Configuration["AzureSettings:ApplicationInsights:ConnectionString"];
			});
#endif

			services.AddDatabaseDeveloperPageExceptionFilter();

#if USING_LOCALIZATION
			services.Configure<RequestLocalizationOptions>(options =>
			{
				options.DefaultRequestCulture = new RequestCulture(SupportedCultures.First());
				options.SupportedCultures = SupportedCultures;
				options.SupportedUICultures = SupportedCultures;
			});
			services.AddLocalization(options => options.ResourcesPath = "Resources");
#endif

			services.AddHttpContextAccessor();

			services
				.AddDomain()
				.AddInfrastructure()
				.AddPersistence(options =>
				{
					DefaultContext.UseDbEngine(options, Configuration);
				})
				.AddBusiness();

			services.AddScoped<IIdentityService, IdentityService>();

			services
				.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
				.AddRoles<Role>()
				.AddUserManager<UserManager<User>>()
				.AddRoleManager<RoleManager<Role>>()
				.AddDefaultTokenProviders()
				.AddEntityFrameworkStores<DefaultContext>();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings.
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequiredLength = 3;
				options.Password.RequiredUniqueChars = 1;

				// Lockout settings.
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				// User settings.
				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
				options.User.RequireUniqueEmail = true;
			});

#if USING_NEWTONSOFT
			JsonConvert.DefaultSettings = () => new JsonSerializerSettings
			{
#if DEBUG
				Formatting = Formatting.Indented,
#else
                Formatting = Formatting.None,
#endif
				// NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			};
			var jsonOptions = new Action<MvcNewtonsoftJsonOptions>((option) =>
			{
#if DEBUG
				option.SerializerSettings.Formatting = Formatting.Indented;
#else
                option.SerializerSettings.Formatting = Formatting.None;
#endif
				// option.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
				option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
				option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				option.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			});
#endif

#if USING_NEWTONSOFT
			services.AddControllersWithViews()
#if USING_LOCALIZATION
				.AddViewLocalization()
#endif
#if DEBUG
				.AddRazorRuntimeCompilation()
#endif
				.AddNewtonsoftJson(jsonOptions);
#else
#if DEBUG
            services.AddControllersWithViews()
#if USING_LOCALIZATION
                .AddViewLocalization()
#endif
                .AddRazorRuntimeCompilation();
#else
#if USING_LOCALIZATION
            services.AddControllersWithViews()
                .AddViewLocalization();
#else
            services.AddControllersWithViews();
#endif
#endif
#endif

			services.AddResponseCaching(options =>
			{
				options.MaximumBodySize = 1024;
				options.UseCaseSensitivePaths = true;
			});

			services.AddResponseCompression(config =>
			{
				config.EnableForHttps = Configuration.GetValue<bool>("AppSettings:UseHttpsRedirection");
				config.Providers.Add<BrotliCompressionProvider>();
				config.Providers.Add<GzipCompressionProvider>();
				config.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml", "text/css", "text/javascript" });
			});

			services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
			services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.SmallestSize; });

			var cookieOptions = new Action<CookieAuthenticationOptions>(options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromHours(1);
#if DEBUG
				options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
#endif
				options.LoginPath = "/Identity/Account/Login";
				options.LogoutPath = "/Identity/Account/Login";
				options.AccessDeniedPath = "/Identity/Account/AccessDenied";

				options.Cookie = new CookieBuilder
				{
					IsEssential = true// required for auth to work without explicit user consent; adjust to suit your privacy policy
				};
				options.Events = new CustomCookieAuthenticationEvents(Configuration["SwaggerSettings:RoutePrefix"]);
			});

			services.ConfigureApplicationCookie(cookieOptions);

#if USING_NEWTONSOFT
			services.AddRazorPages()
				.AddNewtonsoftJson(jsonOptions);
#else
            services.AddRazorPages();
#endif

#if USING_SWAGGER
			// Register the Swagger generator, defining 1 or more Swagger documents
			services.AddSwaggerGen(c =>
			{
				var versions = new List<string>();
				Configuration.Bind("SwaggerSettings:Versions", versions);
				foreach (var version in versions)
					c.SwaggerDoc(version, new OpenApiInfo { Title = $"{Name}", Description = $"{Name} API", Version = $"{version}" });
				c.DocInclusionPredicate((_, _) => true);

#if ENABLE_APIKEY
				var apiKeySecurityScheme = new OpenApiSecurityScheme
				{
					Scheme = ApiKeyAuthenticationDefaults.AuthenticationScheme,
					Name = ApiKeyAuthenticationDefaults.ApiKeyHeaderName,
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Description = "Put **_ONLY_** your API Key",
					Reference = new OpenApiReference { Id = ApiKeyAuthenticationDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
				};

				c.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);
				c.AddSecurityRequirement(new OpenApiSecurityRequirement { { apiKeySecurityScheme, Array.Empty<string>() } });
#endif
#if ENABLE_BEARER
				var jwtSecurityScheme = new OpenApiSecurityScheme
				{
					Scheme = JwtBearerDefaults.AuthenticationScheme,
					BearerFormat = "JWT",
					Name = "JWT Authentication",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Description = "Put **_ONLY_** your JWT Bearer token",
					Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
				};

				c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
				c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
#endif
				c.ResolveConflictingActions(apiDescription => apiDescription.First());
				c.CustomSchemaIds(type => type.ToString());
				c.EnableAnnotations();
			});
#if USING_NEWTONSOFT
			services.AddSwaggerGenNewtonsoftSupport();
#endif
#endif

#if ENABLE_OPENID
			var openIdConnectOptions = new Action<OpenIdConnectOptions>(options =>
			{
				options.ClientId = Configuration["OpenIdSettings:ClientId"];
				options.ClientSecret = Configuration["OpenIdSettings:ClientSecret"];
				options.Authority = Configuration["OpenIdSettings:Authority"];
				options.MetadataAddress = $"{Configuration["OpenIdSettings:Authority"]}/.well-known/openid-configuration";
				options.RequireHttpsMetadata = Configuration.GetValue<bool>("OpenIdSettings:RequireHttps");
				options.GetClaimsFromUserInfoEndpoint = true;
				//options.AuthenticationScheme = "oidc";
				options.SignInScheme = "Cookies";
				options.ResponseType = OpenIdConnectResponseType.IdToken;
				options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
				{
					// This sets the value of User.Identity.Name to users AD username
					NameClaimType = System.Security.Claims.ClaimTypes.WindowsAccountName, RoleClaimType = System.Security.Claims.ClaimTypes.Role, AuthenticationType = "Cookies", ValidateIssuer = false
				};
				var scopes = new List<string>();
				Configuration.Bind("OpenIdSettings:Scopes", scopes);
				if (scopes.Any())
					foreach (var item in scopes)
						options.Scope.Add(item);
			});

#endif

			services.AddAuthentication()
#if !ENABLE_AB2C
				.AddCookie()
#endif
#if ENABLE_APIKEY
				.AddApiKey()
#endif
#if ENABLE_BEARER
				.AddJwtBearer()
#endif
#if ENABLE_OPENID
				.AddOpenIdConnect(openIdConnectOptions)
#endif
#if USING_SMARTSCHEMA
				.AddSmartScheme()
#endif
#if ENABLE_AB2C
                .AddMicrosoftIdentityWebApp(options =>
                {
	                Configuration.Bind("AzureSettings:AdB2C", options);
#if ENABLE_LOCAL_AB2C
	                options.Events.OnTokenValidated = OnTokenValidated;
#endif
	                options.Events.OnSignedOutCallbackRedirect = context =>
	                {
		                context.HttpContext.Response.Redirect(context.Options.SignedOutRedirectUri);
		                context.HandleResponse();
		                return Task.FromResult(true);
	                };
                })
#endif
				.Close();

			services.AddAuthorization();

		}

#if ENABLE_AB2C && ENABLE_LOCAL_AB2C
        private async Task OnTokenValidated(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context)
        {
            if (context.Principal != null && context.Principal.Identity != null)
            {
                var cllist = context.Principal.Claims.GroupBy(m => m.Type).Select(m => m.FirstOrDefault()).ToList();
                var claims = cllist.ToDictionary(k => k.Type, v => v.Value);
                var emails = claims["emails"];

                //var userManager = Program.Host.Services.GetService<UserManager<Buyer>>();
                var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>();
                if (userManager != null)
                {
                    var user = await userManager.Users.FirstOrDefaultAsync(m => m.Email == emails);
                    if (user == null)
                    {
                        var ph = new PasswordHasher<User>();
                        user = new User() { NormalizedUserName = claims["emails"].ToUpper(), UserName = claims["emails"], Email = claims["emails"], EmailConfirmed = true };
#if DEBUG
                        user.PasswordHash = ph.HashPassword(user, $"Admin{DateTime.Now.Year}**");
#endif
                        await userManager.CreateAsync(user);
                        await userManager.AddClaimsAsync(user, claims.Select(m => new Claim(m.Key, m.Value)));
                    }
                    //TODO: What if user exists?, update metadata from B2C
                }
            }
        }
#endif

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (!string.IsNullOrWhiteSpace(Configuration["AppSettings:PathBase"]))
			{
				app.UsePathBase($"/{Configuration["AppSettings:PathBase"]}");
				app.Use((context, next) =>
				{
					context.Request.PathBase = $"/{Configuration["AppSettings:PathBase"]}";
					return next();
				});
			}

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseMigrationsEndPoint();
#if ENABLE_AB2C
                IdentityModelEventSource.ShowPII = true;
#endif
			}
			else
			{
				app.UseExceptionHandler("/Default/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			if (Configuration.GetValue<bool>("AppSettings:UseHttpsRedirection"))
			{
				app.UseHttpsRedirection();
			}

#if USING_LOCALIZATION
			app.UseRequestLocalization(SupportedCultures.Select(m => m.Name).ToArray());
#endif

			var cacheControlInSeconds = Configuration.GetValue<int>("AppSettings:CacheControl");

			app.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = ctx =>
				{
					ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + cacheControlInSeconds.ToString("0");
				}
			});

			app.UseResponseCaching();

			app.Use(async (context, next) =>
			{
				string path = context.Request.Path;

				if (path.EndsWith(".css") || path.EndsWith(".js"))
				{
					context.Response.Headers.Append("Cache-Control", $"max-age={cacheControlInSeconds.ToString("0")}");
				}
				else if (path.EndsWith(".gif") || path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".webp") || path.EndsWith(".svg"))
				{
					context.Response.Headers.Append("Cache-Control", $"max-age={cacheControlInSeconds.ToString("0")}");
				}
				else
				{
					// Request for views fall here.
					context.Response.Headers.Append("Cache-Control", "no-cache");
					context.Response.Headers.Append("Cache-Control", "private, no-store");
				}

				await next();
			});

			app.UseResponseCompression();

			app.UseRouting();

#if USING_SWAGGER

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger(c => { c.RouteTemplate = $"api/{{documentName}}/{Name.ToLower()}.json"; });

			if (Configuration.GetValue<bool>("SwaggerSettings:EnableUI"))
			{
				var uiEngine = Configuration["SwaggerSettings:UIEngine"];
				var swaggerRoutePrefix = Configuration["SwaggerSettings:RoutePrefix"];
				var swaggerDocumentTitle = Configuration["SwaggerSettings:DocumentTitle"];
				var versions = new List<string>();
				Configuration.Bind("SwaggerSettings:Versions", versions);

				if (uiEngine == "ReDoc")
				{
					// Enable middleware to serve ReDoc (HTML, JS, CSS, etc.),
					// specifying the Swagger JSON endpoint.
					app.UseReDoc(c =>
					{
						c.DocumentTitle = swaggerDocumentTitle;
						c.RoutePrefix = swaggerRoutePrefix;
						var version = versions.LastOrDefault();
						c.SpecUrl($"{version}/{Name.ToLower()}.json");

						c.InjectStylesheet($"../css/swagger{(env.IsDevelopment() ? "" : ".min")}.css");
					});
				}
				else
				{
					// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
					// specifying the Swagger JSON endpoint.
					app.UseSwaggerUI(c =>
					{
						foreach (var version in versions)
							c.SwaggerEndpoint($"{version}/{Name.ToLower()}.json", $"{swaggerDocumentTitle} {version}");

						c.DocumentTitle = swaggerDocumentTitle;
						c.RoutePrefix = swaggerRoutePrefix;

						c.DefaultModelsExpandDepth(-1);

						c.InjectStylesheet($"../css/swagger{(env.IsDevelopment() ? "" : ".min")}.css");
						c.InjectJavascript($"../lib/jquery/jquery{(env.IsDevelopment() ? "" : ".min")}.js");
						c.InjectJavascript($"../js/swagger{(env.IsDevelopment() ? "" : ".min")}.js");
					});
				}
			}

#endif

			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
			}

			app.UseAuthentication();
			app.UseAuthorization();

#if USING_QUESTPDF
			DocumentService.RegisterFonts("wwwroot/fonts");
#endif

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					"areas",
					"{area:exists}/{controller=Default}/{action=Index}/{id?}");
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Default}/{action=Index}/{id?}");
				endpoints.MapRazorPages();
			});
		}
	}
}
