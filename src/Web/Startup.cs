using System;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Business;
using Domain;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
#if ENABLE_APIKEY || ENABLE_BEARER
using Microsoft.AspNetCore.Authentication;
#endif
#if ENABLE_APIKEY
using Microsoft.AspNetCore.Authentication.ApiKey;
#endif
#if ENABLE_BEARER
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Persistence;
using Persistence.Contexts;

#if USING_NEWTONSOFT
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

#if USING_SWAGGER
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
#endif

#if USING_LOCALIZATION
using System.Globalization;
using Microsoft.AspNetCore.Localization;
#endif

#if DEBUG && USING_SASS
using AspNetCore.SassCompiler;
#endif

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Name = configuration["AppSettings:Name"];
#if USING_LOCALIZATION
            SupportedCultures = configuration.GetSection("CultureInfo:SupportedCultures").Get<string[]>().Select(m => new CultureInfo(m)).ToArray();
            CurrencyCulture = new CultureInfo(configuration["CultureInfo:CurrencyCulture"]);
#endif
        }

        public static string Name { get; private set; }
        internal IConfiguration Configuration { get; private set; }

#if USING_LOCALIZATION
        public CultureInfo[] SupportedCultures { get; private set; }

        public static CultureInfo CurrencyCulture { get; private set; }

#endif

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if USING_SASS && DEBUG
            services.AddSassCompiler();
#endif
#if USING_INSIGHTS
            // The following line enables Application Insights telemetry collection.
            services.AddApplicationInsightsTelemetry(Configuration["AzureSettings:ApplicationInsights:InstrumentationKey"]);
#endif

            services.AddDatabaseDeveloperPageExceptionFilter();

#if USING_LOCALIZATION
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(SupportedCultures.First());
                options.SupportedCultures = SupportedCultures;
                options.SupportedUICultures = SupportedCultures;
            });
            services.AddLocalization(options=> options.ResourcesPath = "Resources");
#endif

            services
                .AddDomain()
                .AddInfrastructure()
                .AddPersistence(options =>
                {
                    DefaultContext.UseDbEngine(options, Configuration);
                })
                .AddBusiness();

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
                //NullValueHandling = NullValueHandling.Ignore,
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
                //option.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
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

            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
            services.AddResponseCompression(config =>
            {
                config.EnableForHttps = Configuration.GetValue<bool>("AppSettings:UseHttpsRedirection");
                config.Providers.Add<GzipCompressionProvider>();
            });

            services.AddHttpContextAccessor();

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
                    HttpOnly = Configuration.GetValue<bool>("AppSettings:UseHttpsRedirection"), 
                    IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
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
                c.ResolveConflictingActions(apiDescription => apiDescription.First());
                c.CustomSchemaIds(type => type.ToString());
                c.EnableAnnotations();
            });

#if USING_NEWTONSOFT
            services.AddSwaggerGenNewtonsoftSupport();
#endif
#endif

            services.AddAuthentication()
                .AddCookie()
#if ENABLE_BEARER
                .AddJwtBearer()
#endif
#if ENABLE_APIKEY
                .AddApiKey()
#endif
                ;

            services.AddAuthorization();
        }

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

            app.UseStaticFiles();

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
