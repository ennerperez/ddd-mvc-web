// ReSharper disable RedundantUsingDirective
// #define SESSION_TEST

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Business;
using Domain;
#if USING_IDENTITY
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
#endif
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
#if USING_COMPRESSION
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Persistence;
using Persistence.Contexts;
using Serilog;
using Web.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

#if USING_QUESTPDF
using System.IO;
using Infrastructure.Services;
#endif
#if USING_APIKEY
using Microsoft.AspNetCore.Authentication.ApiKey;
#endif
#if USING_BEARER
using Microsoft.IdentityModel.Tokens;
#endif
#if USING_COOKIES || !USING_IDENTITY
using Microsoft.AspNetCore.Authentication.Cookies;
#endif
#if USING_OPENID
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
#endif
#if USING_AUTH0
#if USING_BEARER
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
using Auth0.AspNetCore.Authentication;
#endif
#if USING_TOKEN_VALIDATION
using System.Security.Claims;
using Microsoft.IdentityModel.Logging;
#endif
#if USING_NEWTONSOFT
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif
#if USING_SWAGGER
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
#if USING_LOCALIZATION
			SupportedCultures = configuration.GetSection("CultureInfo:SupportedCultures").Get<string[]>().Select(m => new CultureInfo(m)).ToArray();
			CurrencyCulture = new CultureInfo(configuration["CultureInfo:CurrencyCulture"] ?? "en-US");
#endif
        }

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

#if USING_COOKIES
            services.Configure<CookiePolicyOptions>(options =>
            {
#if USING_COOKIE_CONSENT
                options.CheckConsentNeeded = _ => true;
#else
                options.CheckConsentNeeded = _ => false;
#endif
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
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
#if USING_IDENTITY
            services.AddTransient<IUserAccessorService<User>, UserAccessorService>();
#else
            services.AddTransient<IUserAccessorService, UserAccessorService>();
#endif

#if USING_IDENTITY
            services.AddTransient<IEmailSender, SmtpSender>();
#endif

            services
                .AddDomain()
                .AddInfrastructure()
                .AddPersistence<DefaultContext>(options => options.UseDbEngine(Configuration))
                .AddPersistence<CacheContext>(options => options.UseDbEngine(Configuration), ServiceLifetime.Transient)
                .AddBusiness();

#if USING_IDENTITY
            services
                .AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<Role>()
                .AddUserManager<UserManager<User>>()
                .AddRoleManager<RoleManager<Role>>()
                .AddEntityFrameworkStores<DefaultContext>()
                .AddDefaultTokenProviders();

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
#endif

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

#if USING_COMPRESSION

            services.AddResponseCompression(config =>
            {
                config.EnableForHttps = true;
                config.Providers.Add<BrotliCompressionProvider>();
                config.Providers.Add<GzipCompressionProvider>();
                config.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml", "text/css", "text/javascript" });
            });

            services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.SmallestSize; });

#endif

#if USING_COOKIES
            var cookieOptions = new Action<CookieAuthenticationOptions>(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
#if DEBUG
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
#endif
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                options.Cookie = new CookieBuilder
                {
                    Name = $"{Program.Name.Normalize(false)}.{CookieAuthenticationDefaults.AuthenticationScheme}",
                    IsEssential = true// required for auth to work without explicit user consent; adjust to suit your privacy policy
                };
#if USING_SWAGGER
                options.Events = new CustomCookieAuthenticationEvents(Configuration["SwaggerSettings:RoutePrefix"]);
#endif
            });

            services.ConfigureApplicationCookie(cookieOptions);
#endif

#if USING_SESSION
            var sessionOptions = new Action<SessionOptions>((option) =>
            {
                option.Cookie.Name = $"{Program.Name.Normalize(false)}.Session".ToUpperInvariant();
                option.Cookie.HttpOnly = true;
                option.Cookie.IsEssential = true;
#if DEBUG && SESSION_TEST
		        //option.Cookie.Expiration = TimeSpan.FromMinutes(1);
		        option.IdleTimeout = TimeSpan.FromMinutes(1);
		        option.IOTimeout = TimeSpan.FromMinutes(1);
#else
                //option.Cookie.Expiration = TimeSpan.FromHours(8);
                option.IdleTimeout = TimeSpan.FromHours(8);
                option.IOTimeout = TimeSpan.FromHours(8);
#endif
            });

            services.AddSession(sessionOptions);
#endif

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
                {
                    c.SwaggerDoc(version, new OpenApiInfo { Title = $"{Program.Name}", Description = $"{Program.Name} API", Version = $"{version}" });
                }

                c.DocInclusionPredicate((_, _) => true);

#if USING_APIKEY
                var apiKeySecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = ApiKeyAuthenticationDefaults.AuthenticationScheme,
                    Name = ApiKeyAuthenticationDefaults.ApiKeyHeaderName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Put **_ONLY_** your API Key",
                    Reference = new OpenApiReference {Id = ApiKeyAuthenticationDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme}
                };

                c.AddSecurityDefinition(apiKeySecurityScheme.Reference.Id, apiKeySecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {{apiKeySecurityScheme, Array.Empty<string>()}});
#endif
#if USING_BEARER
				var jwtSecurityScheme = new OpenApiSecurityScheme
				{
					Scheme = JwtBearerDefaults.AuthenticationScheme,
					BearerFormat = "JWT",
					Name = "JWT Authentication",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Description = "Put **_ONLY_** your JWT Bearer token",
					Reference = new OpenApiReference {Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme}
				};

				c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
				c.AddSecurityRequirement(new OpenApiSecurityRequirement {{jwtSecurityScheme, Array.Empty<string>()}});
#endif
#if USING_AUTH0 && !USING_BEARER
                var auth0SecurityScheme = new OpenApiSecurityScheme
                {
                    Name = "Auth0 Authentication",
                    Scheme = Auth0Constants.AuthenticationScheme,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Put **_ONLY_** your JWT Bearer token",
                    Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
                };

                c.AddSecurityDefinition(auth0SecurityScheme.Reference.Id, auth0SecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { auth0SecurityScheme, Array.Empty<string>() } });
#endif
                c.ResolveConflictingActions(apiDescription => apiDescription.First());
                c.CustomSchemaIds(type => type.ToString());
                c.EnableAnnotations();
            });
#if USING_NEWTONSOFT
            services.AddSwaggerGenNewtonsoftSupport();
#endif
#endif

#if USING_OPENID
			var openIdConnectOptions = new Action<OpenIdConnectOptions>(options =>
			{
				Configuration.Bind("OpenIdSettings", options);

#if USING_TOKEN_VALIDATION
				options.Events.OnTokenValidated = OpenId_OnTokenValidated;
#endif
				options.MetadataAddress = $"{Configuration["OpenIdSettings:Authority"]}/.well-known/openid-configuration";
				options.TokenValidationParameters = new TokenValidationParameters
				{
					// This sets the value of User.Identity.Name to users AD username
					NameClaimType = System.Security.Claims.ClaimTypes.WindowsAccountName, RoleClaimType = System.Security.Claims.ClaimTypes.Role, AuthenticationType = "Cookies", ValidateIssuer = false
				};
				var scopes = new List<string>();
				Configuration.Bind("OpenIdSettings:Scopes", scopes);
				if (scopes.Any())
				{
					options.Scope.Clear();
					foreach (var item in scopes)
						options.Scope.Add(item);
				}
				options.Events.OnSignedOutCallbackRedirect = context =>
				{
					context.HttpContext.Response.Redirect(context.Options.SignedOutRedirectUri);
					context.HandleResponse();
					return Task.FromResult(true);
				};
			});
#endif

            var antiforgeryOptions = new Action<AntiforgeryOptions>(options =>
            {
                options.Cookie = new CookieBuilder() { Name = $"{Program.Name.Normalize(false)}.AntiforgeryCookie", Expiration = AntiforgeryExpiration };
            });
            services.AddAntiforgery(antiforgeryOptions);

#if USING_CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowCors", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });
#endif

            // ReSharper disable once RedundantAssignment
            var defaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
#if USING_SMARTSCHEMA
            defaultScheme = SmartScheme.DefaultScheme;
#endif
            services.AddAuthentication(defaultScheme)
#if !USING_AUTH0 && USING_COOKIES
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
#if DEBUG
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
#endif
                    options.LoginPath = "/Identity/Account/Login";
                    options.LogoutPath = "/Identity/Account/Logout";
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                    options.Cookie = new CookieBuilder
                    {
                        Name = $"{Program.Name.Normalize(false)}.{CookieAuthenticationDefaults.AuthenticationScheme}",
                        IsEssential = true// required for auth to work without explicit user consent; adjust to suit your privacy policy
                    };
#if USING_SWAGGER
                    options.Events = new CustomCookieAuthenticationEvents(Configuration["SwaggerSettings:RoutePrefix"]);
#endif
                })
#endif
#if USING_APIKEY
#if USING_APIKEY_TABLES
				.AddApiKey<TableServiceApiKeyProvider>()
#else
                .AddApiKey<AppSettingsApiKeyProvider>()
#endif
#endif
#if USING_BEARER
				.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
				{
#if USING_AUTH0
					options.Authority = Configuration["Auth0Settings:Authority"];
					options.TokenValidationParameters = new TokenValidationParameters() {ValidateAudience = false};
#else
#if DEBUG
					options.IncludeErrorDetails = true;
#endif
					var audiences = new List<string>();
					Configuration.Bind("AuthSettings:Audiences", audiences);
					options.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidIssuer = Configuration["AuthSettings:Issuer"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"])),
						ValidateLifetime = Configuration.GetValue<bool>("AuthSettings:Validate:Lifetime"),
						ValidateIssuer = Configuration.GetValue<bool>("AuthSettings:Validate:Issuer"),
						ValidateAudience = Configuration.GetValue<bool>("AuthSettings:Validate:Audience"),
						ValidAudiences = audiences,
						RequireAudience = Configuration.GetValue<bool>("AuthSettings:Require:Audience"),
						RequireExpirationTime = Configuration.GetValue<bool>("AuthSettings:Require:ExpirationTime"),
					};
					options.SaveToken = Configuration.GetValue<bool>("AuthSettings:SaveToken");
					options.RequireHttpsMetadata = Configuration.GetValue<bool>("AuthSettings:RequireHttpsMetadata");

#endif

				})
#endif
#if USING_SMARTSCHEMA
                .AddSmartScheme()
#endif
#if USING_OPENID
				.AddOpenIdConnect(openIdConnectOptions)
#endif
#if USING_AUTH0
                .AddAuth0WebAppAuthentication(options =>
                {
                    Configuration.Bind("Auth0Settings", options);
                    var scopes = new List<string>();
                    Configuration.Bind("Auth0Settings:Scopes", scopes);
                    if (scopes.Any())
                    {
                        options.Scope = string.Join(" ", scopes);
                    }
                })
                .WithAccessToken(options =>
                {
                    options.Audience = Configuration["Auth0Settings:Audience"];
                    options.UseRefreshTokens = Configuration.GetValue<bool>("Auth0Settings:UseRefreshTokens");
                })
#endif
                .Close();

            services.AddAuthorization();
        }

#if (USING_OPENID) && USING_TOKEN_VALIDATION
		private async Task OpenId_OnTokenValidated(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context)
		{
			if (context.Principal != null && context.Principal.Identity != null)
			{
				var cllist = context.Principal.Claims.GroupBy(m => m.Type).Select(m => m.FirstOrDefault()).ToList();
				var claims = cllist.ToDictionary(k => k.Type, v => v.Value);
				var emails = claims["name"];

				//var userManager = Program.Host.Services.GetService<UserManager<Buyer>>();
				var userManager = context.HttpContext.RequestServices.GetService<UserManager<User>>();
				if (userManager != null)
				{
					var user = await userManager.Users.FirstOrDefaultAsync(m => m.Email == emails);
					if (user == null)
					{
						var ph = new PasswordHasher<User>();
						user = new User() {NormalizedUserName = claims["name"].ToUpper(), UserName = claims["nickname"], Email = claims["name"], EmailConfirmed = claims["email_verified"] == "true"};
#if DEBUG
						user.PasswordHash = ph.HashPassword(user, $"Admin{DateTime.Now.Year}**");
#endif
						await userManager.CreateAsync(user);
						await userManager.AddClaimsAsync(user, claims.Select(m => new Claim(m.Key, m.Value)));
					}
					//TODO: What if user exists?, update metadata from OpenID
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
#if USING_TOKEN_VALIDATION
				IdentityModelEventSource.ShowPII = true;
#endif
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();
#if USING_SESSION
            app.UseSession();
#endif
#if !DEBUG
			app.UseStatusCodePagesWithReExecute("/Error/{0}");
#endif

            if (Configuration.GetValue<bool>("AppSettings:UseHttpsRedirection"))
            {
                app.UseHttpsRedirection();
            }

#if USING_COOKIES
            app.UseCookiePolicy();
#endif

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
                    context.Response.Headers.Append("Cache-Control", $"max-age={cacheControlInSeconds:0}");
                }
                else if (path.EndsWith(".gif") || path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".webp") || path.EndsWith(".svg"))
                {
                    context.Response.Headers.Append("Cache-Control", $"max-age={cacheControlInSeconds:0}");
                }
                else
                {
                    // Request for views fall here.
                    context.Response.Headers.Append("Cache-Control", "no-cache");
                    context.Response.Headers.Append("Cache-Control", "private, no-store");
                }

                await next();
            });

#if USING_COMPRESSION
            app.UseResponseCompression();
#endif

            app.UseRouting();

#if USING_SWAGGER

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c => { c.RouteTemplate = $"api/{{documentName}}/{Program.Name.Normalize(false, false, true)}.json"; });

            if (Configuration.GetValue<bool>("SwaggerSettings:EnableUI"))
            {
                var uiEngine = Configuration["SwaggerSettings:UIEngine"];
                var swaggerRoutePrefix = Configuration["SwaggerSettings:RoutePrefix"];
                var swaggerDocumentTitle = Configuration["SwaggerSettings:DocumentTitle"] ?? Program.Name;
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
                        c.SpecUrl($"{version}/{Program.Name.Normalize(false, false, true)}.json");

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
                        {
                            c.SwaggerEndpoint($"{version}/{Program.Name.Normalize(false, false, true)}.json", $"{swaggerDocumentTitle} {version}");
                        }

                        c.DocumentTitle = swaggerDocumentTitle;
                        c.RoutePrefix = swaggerRoutePrefix;

#if USING_AUTH0
                        c.OAuthClientId(Configuration["Auth0Settings:ClientId"]);
                        c.OAuthClientSecret(Configuration["Auth0Settings:ClientSecret"]);
#endif

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

#if USING_CORS
            app.UseCors();
#endif

            app.UseAuthentication();
            app.UseAuthorization();

#if USING_QUESTPDF
            DocumentService.RegisterFonts(Path.Combine("wwwroot", "fonts"));
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
