// ReSharper disable RedundantUsingDirective
// #define SESSION_TEST

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Business;
using Domain;
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Persistence;
using Persistence.Contexts;
#if USING_SERILOG
using Serilog;
#endif
using Web.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
#if USING_IDENTITY
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
#endif
#if USING_COMPRESSION
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
#endif
#if USING_QUESTPDF
using System.IO;
using Infrastructure.Services;
using QuestPDF;
using QuestPDF.Infrastructure;
#endif
#if USING_APIKEY
using Microsoft.AspNetCore.Authentication.ApiKey;
#endif
#if USING_BEARER
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
#if USING_AUTH0 && !USING_BEARER
using Microsoft.AspNetCore.Authentication.JwtBearer;
#endif
#if USING_COOKIES || !USING_IDENTITY
using Microsoft.AspNetCore.Authentication.Cookies;
// ReSharper disable MemberCanBePrivate.Global
#endif
#if USING_OPENID
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
#endif
#if USING_AUTH0
using Auth0.AspNetCore.Authentication;
#endif
#if USING_TOKEN_VALIDATION
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
#if USING_SASS && USING_SASS_WATCH
using System.Diagnostics;
#endif
#if USING_APPCONFIGURATION
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
#if USING_FEATURE_FLAG
using Microsoft.FeatureManagement;
#endif
#endif

namespace Web
{
    public class Program
    {
        internal static string Name { get; private set; }
        internal static WebApplication Instance { get; private set; }
        internal static WebApplicationBuilder Builder { get; private set; }
        internal static IConfiguration Configuration => Builder.Configuration;

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

#if USING_LOCALIZATION
        internal CultureInfo[] SupportedCultures { get; private set; }

        internal static CultureInfo CurrencyCulture { get; private set; }
#endif

        public static void Main(string[] args)
        {
#if USING_SASS && USING_SASS_WATCH
            var darts = Process.GetProcessesByName("dart");
            foreach (var process in darts)
            {
                process.Kill();
            }
#endif

            Builder = WebApplication.CreateBuilder(args);
#if USING_SERILOG
            Builder.Host.UseSerilog();
#endif

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Builder.Configuration
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            Name = Builder.Configuration["AppSettings:Name"];

#if USING_APPCONFIGURATION
      Builder.Configuration.AddAzureAppConfiguration(options =>
      {
        var connectionString = Builder.Configuration["AzureSettings:AppConfiguration:ConnectionString"];
        options.Connect(connectionString)
          .Select($"{Name}:*", environmentName);
#if USING_FEATURE_FLAG
        options.UseFeatureFlags(featureFlagOptions =>
        {
          featureFlagOptions.Select($"{Name}:*", environmentName);
          featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(Builder.Configuration.GetValue<double>("AzureSettings:AppConfiguration:CacheExpirationInterval"));
        });
        Builder.Services.AddSingleton(options.GetRefresher());
#endif
      }, true);
#endif

#if USING_LOCALIZATION
      SupportedCultures = Builder.Configuration.GetSection("CultureInfo:SupportedCultures").Get<string[]>().Select(m => new CultureInfo(m)).ToArray();
      CurrencyCulture = new CultureInfo(Builder.Configuration["CultureInfo:CurrencyCulture"] ?? "en-US");
#endif

#if USING_DATADOG
            var tags = Array.Empty<string>();
            Builder.Configuration.Bind("Datadog:Tags", tags);
            var serviceName = Builder.Configuration["Datadog:Service"];
            serviceName = !string.IsNullOrWhiteSpace(serviceName) ? serviceName : Name;
#endif
#if USING_SERILOG
            // Initialize Logger
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(Builder.Configuration)
                .Enrich.WithProperty("ApplicationName", Name);
#if USING_DATADOG
            if (!string.IsNullOrWhiteSpace(Builder.Configuration["Datadog:ApiKey"]))
            {
                loggerConfiguration = loggerConfiguration.WriteTo.Async(a =>
                    a.DatadogLogs(Builder.Configuration["Datadog:ApiKey"],
                        service: serviceName,
                        host: Builder.Configuration["Datadog:Host"],
                        tags: tags
                    ));
            }
#endif
            var logger = Log.Logger = loggerConfiguration.CreateLogger();
#endif
            Builder.Logging
                .ClearProviders()
#if USING_SERILOG
                .AddSerilog(logger)
#endif
                .Close();

            // Seed Services
            Builder.Services.AddHostedService<SeedService>();

            ConfigureServices(Builder.Services);
            Instance = Builder.Build();
            Configure(Instance, Instance.Environment);

            try
            {
#if USING_SERILOG
                Log.Information("Application Starting");
#endif
                Instance.Run();
            }
            catch (Exception ex)
            {
#if USING_SERILOG
                Log.Fatal(ex, "The Application failed to start");
#else
                Console.WriteLine(ex.Message);
#endif
                throw;
            }
            finally
            {
#if USING_SERILOG
                Log.CloseAndFlush();
#endif
            }
        }

        private static void ConfigureServices(IServiceCollection services)
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
                .AddBusiness().WithRepositories().WithMediatR();

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
            var jsonOptions = new Action<MvcNewtonsoftJsonOptions>(option =>
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

#if USING_HEALTHCHECK
            services.AddHealthChecks();
#endif

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
                    Name = $"{Program.Name.Normalize(false)}.{CookieAuthenticationDefaults.AuthenticationScheme}", IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
                };
#if USING_SWAGGER
                options.Events = new CustomCookieAuthenticationEvents(Configuration["SwaggerSettings:RoutePrefix"]);
#endif
            });

            services.ConfigureApplicationCookie(cookieOptions);
#endif

#if USING_SESSION
            var sessionOptions = new Action<SessionOptions>(option =>
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

#if USING_APPCONFIGURATION
            services.AddAzureAppConfiguration();
#if USING_FEATURE_FLAG
            services.AddFeatureManagement();
#endif
#endif

#if USING_SWAGGER
            if (Configuration.GetValue<bool>("SwaggerSettings:EnableUI"))
            {
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
            }
#endif

#if USING_OPENID
            var openIdConnectOptions = new Action<OpenIdConnectOptions>(options =>
            {
                Configuration.Bind("OpenIdSettings", options);

#if USING_TOKEN_VALIDATION && USING_IDENTITY
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
                options.Cookie = new CookieBuilder { Name = $"{Program.Name.Normalize(false)}.AntiforgeryCookie", Expiration = AntiforgeryExpiration };
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
                        Name = $"{Program.Name.Normalize(false)}.{CookieAuthenticationDefaults.AuthenticationScheme}", IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
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
#endif
                    options.TokenValidationParameters = new TokenValidationParameters() {ValidateAudience = false};
#if DEBUG
                    options.IncludeErrorDetails = true;
#endif
                    var audiences = new List<string>();
                    Configuration.Bind("AuthSettings:Audiences", audiences);
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = Configuration.GetValue<bool>("AuthSettings:Validate:IssuerSigningKey"),
                        ValidIssuer = Configuration["AuthSettings:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"] ?? string.Empty)),
                        ValidateLifetime = Configuration.GetValue<bool>("AuthSettings:Validate:Lifetime"),
                        ValidateIssuer = Configuration.GetValue<bool>("AuthSettings:Validate:Issuer"),
                        ValidateAudience = Configuration.GetValue<bool>("AuthSettings:Validate:Audience"),
                        ValidAudiences = audiences,
                        RequireAudience = Configuration.GetValue<bool>("AuthSettings:Require:Audience"),
                        RequireExpirationTime = Configuration.GetValue<bool>("AuthSettings:Require:ExpirationTime"),
                    };
                    options.SaveToken = Configuration.GetValue<bool>("AuthSettings:SaveToken");
                    options.RequireHttpsMetadata = Configuration.GetValue<bool>("AuthSettings:RequireHttpsMetadata");
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
#if USING_TOKEN_VALIDATION && USING_IDENTITY
                    options.OpenIdConnectEvents = new OpenIdConnectEvents();
                    options.OpenIdConnectEvents.OnTokenValidated = OpenId_OnTokenValidated ;
#endif
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

#if (USING_OPENID || USING_AUTH0) && USING_TOKEN_VALIDATION && USING_IDENTITY
        private async Task OpenId_OnTokenValidated(Microsoft.AspNetCore.Authentication.OpenIdConnect.TokenValidatedContext context)
        {
            if (context.Principal != null && context.Principal.Identity != null)
            {
                var cllist = context.Principal.Claims.GroupBy(m => m.Type).Select(m => m.FirstOrDefault()).ToList();
                var claims = cllist.ToDictionary(k => k.Type, v => v.Value);
                var emails = claims["name"];

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

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                app.Use(async (ctx, next) =>
                {
                    await next();
                    if (ctx.Response.StatusCode >= 400 && ctx.Response is { StatusCode: < 500, HasStarted: false })
                    {
                        //Re-execute the request so the user gets the error page
                        ctx.Request.Path = $"/Error/{ctx.Response.StatusCode}";
                        await next();
                    }
                });
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
#if USING_SERILOG
            app.UseSerilogRequestLogging();
#endif
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

#if USING_APPCONFIGURATION && USING_FEATURE_FLAG
            app.Use(async (context, next) =>
            {
                var refresher = (IConfigurationRefresher)context.RequestServices.GetService(typeof(IConfigurationRefresher));
                if (refresher != null)
                {
                    await refresher.TryRefreshAsync();
                }

                await next();
            });
#endif

#if USING_APPCONFIGURATION && USING_FEATURE_FLAG
            app.UseMiddleware<FeatureFlagRefresherMiddlerware>();
#endif

#if USING_COMPRESSION
            app.UseResponseCompression();
#endif

#if USING_HEALTHCHECK
            app.UseHealthChecks(Configuration["AppSettings:HealthCheckPath"]);
#endif
            app.UseRouting();

#if USING_SWAGGER

            if (Configuration.GetValue<bool>("SwaggerSettings:EnableUI"))
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger(c => { c.RouteTemplate = $"api/{{documentName}}/{Program.Name.Normalize(false, false, true)}.json"; });

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
                        c.InjectJavascript($"../lib/jquery/jquery.slim{(env.IsDevelopment() ? "" : ".min")}.js");
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
            Settings.License = LicenseType.Community;
            DocumentService.RegisterFonts(Path.Combine("wwwroot", "fonts"));
#endif

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "areas",
                    "{area:exists}/{controller=Default}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Default}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
