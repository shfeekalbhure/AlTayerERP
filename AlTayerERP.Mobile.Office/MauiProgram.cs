using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.Logging;
#if ANDROID
using AlTayerERP.Mobile.Office.Platforms.Android;
#endif

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
            BaseAddress = new Uri("http://172.16.4.250:5021/"),
            Timeout = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<SessionStorageService>();
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<MobileHomeService>();
        builder.Services.AddSingleton<PaymentRequestService>();
        builder.Services.AddSingleton<PaymentRequestReferenceService>();
        builder.Services.AddSingleton<PaymentRequestAttachmentService>();
        builder.Services.AddSingleton<PaymentVoucherService>();
        builder.Services.AddSingleton<ReceiptVoucherService>();
        builder.Services.AddSingleton<JournalVoucherService>();
        builder.Services.AddSingleton<DocumentSearchService>();
        builder.Services.AddSingleton<TrialBalanceService>();
        builder.Services.AddSingleton<GeneralLedgerService>();
        builder.Services.AddSingleton<VoucherEntryService>();
        builder.Services.AddSingleton<VoucherWorkflowService>();
        builder.Services.AddSingleton<VoucherJournalService>();
        builder.Services.AddSingleton<VoucherAttachmentService>();
#if ANDROID
        builder.Services.AddSingleton<IReceiptVoucherPrintService, ReceiptVoucherPrintService>();
#endif
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
