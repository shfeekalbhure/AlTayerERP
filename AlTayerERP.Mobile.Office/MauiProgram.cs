using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.Logging;

namespace AlTayerERP.Mobile.Office;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://10.0.2.2:5001/"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<SessionStorageService>();
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
