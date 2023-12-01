#if USING_LOCALIZATION
using System.Globalization;
using Microsoft.Maui.Storage;
#endif
using System.Collections.Generic;
using System.Reflection;
using App.Services;
using Business;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Domain;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
#if IOS || ANDROID
using Microsoft.Maui.Controls.Platform;
#endif
using Microsoft.Maui.Hosting;
#if IOS
using Microsoft.Maui.Platform;
#endif
using Persistence;
using Persistence.Contexts;
using Serilog;
#if USING_AUTH0
using Microsoft.Maui.Authentication.Auth0;
#endif

namespace App
{
    public static class MauiProgram
    {
        private static string Name => Assembly.GetAssembly(typeof(MauiProgram)).Product();
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
#if USING_COMMUNITY_TOOLKIT
                .UseMauiCommunityToolkit()
#endif
                .RegisterViewModels()
                .RegisterViews()
                .RegisterStyles()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .Close();

            builder.Configuration
                .AddJsonFile(new EmbeddedFileProvider(typeof(MauiProgram).Assembly, typeof(MauiProgram).Namespace), "appsettings.json", false, true)
#if DEBUG
                .AddJsonFile(new EmbeddedFileProvider(typeof(MauiProgram).Assembly, typeof(MauiProgram).Namespace), "appsettings.Development.json", true, true)
#endif
                .AddEnvironmentVariables();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            // Initialize Logger
            var logger = Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.WithProperty("ApplicationName", Name ?? "N/A")
                .CreateLogger();

            builder.Logging
                .ClearProviders()
                .AddSerilog(logger);

#if USING_AUTH0
            var scopes = new List<string>();
            builder.Configuration.Bind("Auth0Settings:Scopes", scopes);
            builder.Services.AddSingleton(new Auth0Client(new()
            {
                Domain = builder.Configuration["Auth0Settings:Domain"],
                ClientId = builder.Configuration["Auth0Settings:ClientId"],
                Scope = string.Join(" ", scopes),
                RedirectUri = $"{Name.ToLower()}://callback"
            }));
            builder.Services.AddSingleton(typeof(Auth0AuthenticationStateProvider));
#endif

            builder.Services.AddSingleton<IHttpContextAccessor, MauiHttpContextAccessor>();

#if USING_LOCALIZATION
            builder.Services.AddLocalization();
            var cc = Preferences.Default.Get("language", string.Empty);
            if (!string.IsNullOrEmpty(cc))
            {
                CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(cc);
            }
#endif

            builder.Services
                .AddDomain()
                .AddInfrastructure()
                .AddPersistence<DefaultContext>(options => options.UseDbEngine(builder.Configuration))
                .AddPersistence<CacheContext>(options => options.UseDbEngine(builder.Configuration), ServiceLifetime.Transient)
                .AddBusiness();

            return builder.Build();
        }
        private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
        {
            return mauiAppBuilder;
        }
        private static MauiAppBuilder RegisterViews(this MauiAppBuilder mauiAppBuilder)
        {
            return mauiAppBuilder;
        }
        private static MauiAppBuilder RegisterStyles(this MauiAppBuilder builder)
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Entry", (handler, view) =>
            {
#if IOS
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                handler.PlatformView.BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent.ToPlatform();
#elif ANDROID
                handler.PlatformView.SetBackground(null);
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Picker", (handler, view) =>
            {
#if IOS
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                handler.PlatformView.BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent.ToPlatform();
#elif ANDROID
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("DatePicker", (handler, view) =>
            {
#if IOS
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                handler.PlatformView.BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent.ToPlatform();
#elif ANDROID
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#endif
            });
            return builder;
        }
    }
}
