using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using PersonalJournalDesktopApp.Data;
using PersonalJournalDesktopApp.Services;
using PersonalJournalDesktopApp.ViewModels;
using PersonalJournalDesktopApp.Views;

namespace PersonalJournalDesktopApp;

public static class MauiProgram
{
    public static IServiceProvider Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Database
        builder.Services.AddSingleton<DatabaseService>();

        // Register Services
        builder.Services.AddSingleton<JournalService>();
        builder.Services.AddSingleton<MoodService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<TagService>();
        builder.Services.AddSingleton<CategoryService>();
        builder.Services.AddSingleton<AnalyticsService>();
        builder.Services.AddSingleton<SearchService>();
        builder.Services.AddSingleton<ExportService>();
        builder.Services.AddSingleton<SecurityService>();

        // Register ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<EntryDetailViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<AnalyticsViewModel>();
        builder.Services.AddTransient<SecurityViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EntryDetailPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<AnalyticsPage>();
        builder.Services.AddTransient<SecurityPage>();
        builder.Services.AddTransient<LoginPage>();

        // Register Navigation Routes
        Routing.RegisterRoute("EntryDetailPage", typeof(EntryDetailPage));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        Services = app.Services;
        return app;
    }
}