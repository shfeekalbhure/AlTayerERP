using System.Diagnostics;
using System.Net.Sockets;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class ApiConnectionDiagnosticsService(HttpClient httpClient, ApiClientConfiguration configuration)
{
    public Task<ApiDiagnosticResult> CheckHealthAsync(bool forceRetry = false, CancellationToken cancellationToken = default) =>
        configuration.EnsureConnectionAsync(forceRetry, cancellationToken);

    public ApiDiagnosticResult FromStatus(string endpoint, int statusCode, StoredSessionDto? session = null,
        int accountsCount = 0, int costCentersCount = 0, int currenciesCount = 0, int paymentMethodsCount = 0, int openPeriodsCount = 0) =>
        Create(endpoint, true, statusCode, statusCode switch
        {
            401 => ApiErrorType.Unauthorized, 403 => ApiErrorType.Forbidden, 404 => ApiErrorType.NotFound,
            409 => ApiErrorType.Conflict, >= 500 => ApiErrorType.ServerError, _ => ApiErrorType.Unknown
        }, session, accountsCount, costCentersCount, currenciesCount, paymentMethodsCount, openPeriodsCount);

    public ApiDiagnosticResult FromException(string endpoint, Exception exception, StoredSessionDto? session = null)
    {
        var error = ApiClientConfiguration.Classify(exception);
      //  return Create(endpoint, false, exception is HttpRequestException request ? (int?)request.StatusCode : null, error, session);
        return Create(
    endpoint,
    false,
    exception is HttpRequestException httpRequest
        ? (int?)httpRequest.StatusCode
        : null,
    error,
    session);
    }

    public ApiDiagnosticResult Create(string endpoint, bool serverReached, int? statusCode, ApiErrorType errorType,
        StoredSessionDto? session = null, int accountsCount = 0, int costCentersCount = 0, int currenciesCount = 0,
        int paymentMethodsCount = 0, int openPeriodsCount = 0)
    {
        var result = new ApiDiagnosticResult
        {
            Endpoint = endpoint, BaseAddress = httpClient.BaseAddress?.ToString() ?? string.Empty,
            ServerReached = serverReached, StatusCode = statusCode, ErrorType = errorType,
            CompanyId = session?.CompanyId, BranchId = session?.BranchId, FiscalYearId = session?.YearId,
            AccountsCount = accountsCount, CostCentersCount = costCentersCount, CurrenciesCount = currenciesCount,
            PaymentMethodsCount = paymentMethodsCount, OpenPeriodsCount = openPeriodsCount
        };
        WriteDevelopment(result);
        return result;
    }

    [Conditional("DEBUG")]
    private static void WriteDevelopment(ApiDiagnosticResult result) => Debug.WriteLine(
        $"[ApiDiagnostic]\nApp={result.App}\nEndpoint={result.Endpoint}\nBaseAddress={result.BaseAddress}\n" +
        $"ServerReached={result.ServerReached}\nStatusCode={result.StatusCode?.ToString() ?? "none"}\nErrorType={result.ErrorType}\n" +
        $"CompanyId={result.CompanyId ?? "none"}\nBranchId={result.BranchId?.ToString() ?? "none"}\nFiscalYearId={result.FiscalYearId?.ToString() ?? "none"}\n" +
        $"AccountsCount={result.AccountsCount}\nCostCentersCount={result.CostCentersCount}\nCurrenciesCount={result.CurrenciesCount}\nPaymentMethodsCount={result.PaymentMethodsCount}\nOpenPeriodsCount={result.OpenPeriodsCount}");

}
