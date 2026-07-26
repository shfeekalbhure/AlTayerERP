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

        // عنوان تطوير محلي للهاتف الحقيقي على نفس شبكة Wi-Fi.
        // في الإنتاج يستبدل بعنوان HTTPS ثابت من إعدادات البيئة.
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("http://172.16.4.250:5021/"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<SessionStorageService>();
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<MobileHomeService>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
