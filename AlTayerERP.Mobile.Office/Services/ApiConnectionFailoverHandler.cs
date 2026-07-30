using System.Diagnostics;

namespace AlTayerERP.Mobile.Office.Services;

/// <summary>بعد فشل اتصال شبكي يعيد اختيار العنوان مرة واحدة، من دون إعادة إرسال طلب قد يغيّر بيانات.</summary>
public sealed class ApiConnectionFailoverHandler(ApiClientConfiguration configuration) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && IsNetworkFailure(ex))
        {
            await configuration.RecoverAfterConnectionFailureAsync();
            WriteDevelopment(configuration.ActiveBaseAddress, ApiClientConfiguration.Classify(ex));
            throw;
        }
    }

    private static bool IsNetworkFailure(Exception exception) => ApiClientConfiguration.Classify(exception) is
        ApiErrorType.ConnectionRefused or ApiErrorType.Timeout or ApiErrorType.Dns;

    [Conditional("DEBUG")]
    private static void WriteDevelopment(Uri? selectedAddress, DTOs.ApiErrorType errorType) =>
        Debug.WriteLine($"[ApiConnection] RecoveryAttempted=true; SelectedBaseAddress={selectedAddress}; ErrorType={errorType}");
}
