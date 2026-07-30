using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestReferenceService(
    HttpClient httpClient,
    SessionStorageService sessionStorage,
    ApiConnectionDiagnosticsService diagnostics)
{
    private const string Endpoint = "api/mobile/payment-request-references";

    public async Task<PaymentRequestReferenceResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync();
        if (session == null)
            return Failure(diagnostics.Create(Endpoint, false, null, ApiErrorType.Unauthorized));

        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var statusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
                return Failure(diagnostics.FromStatus(Endpoint, statusCode, session));

            PaymentRequestReferencesDto? references;
            try
            {
                references = await response.Content.ReadFromJsonAsync<PaymentRequestReferencesDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
            {
                return Failure(diagnostics.Create(Endpoint, true, statusCode, ApiErrorType.DeserializeFailure, session));
            }

            if (references == null)
                return Failure(diagnostics.Create(Endpoint, true, statusCode, ApiErrorType.DeserializeFailure, session));

            references.Accounts ??= [];
            references.CostCenters ??= [];
            references.Currencies ??= [];
            references.PaymentMethods ??= [];
            references.OpenPeriods ??= [];
            diagnostics.Create(Endpoint, true, statusCode, ApiErrorType.None, session,
                references.Accounts.Count, references.CostCenters.Count, references.Currencies.Count,
                references.PaymentMethods.Count, references.OpenPeriods.Count);
            return PaymentRequestReferenceResult.Success(references, statusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure(diagnostics.FromException(Endpoint, ex, session));
        }
    }

    private static PaymentRequestReferenceResult Failure(ApiDiagnosticResult diagnostic) => new()
    {
        HttpStatusCode = diagnostic.StatusCode,
        ServerReached = diagnostic.ServerReached,
        ErrorType = diagnostic.ErrorType,
        AccountsCount = diagnostic.AccountsCount,
        CostCentersCount = diagnostic.CostCentersCount,
        CurrenciesCount = diagnostic.CurrenciesCount,
        PaymentMethodsCount = diagnostic.PaymentMethodsCount,
        OpenPeriodsCount = diagnostic.OpenPeriodsCount
    };
}
