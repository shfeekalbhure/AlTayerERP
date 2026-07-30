using System.Diagnostics;
using System.Net.Sockets;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class ApiConnectionDiagnosticsService(HttpClient httpClient)
{
    public async Task<ApiDiagnosticResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        const string endpoint = "api/health";
        try
        {
            using var response = await httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return FromStatus(endpoint, (int)response.StatusCode);

            using var document = await System.Text.Json.JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = document.RootElement;
            var ready = root.TryGetProperty("api", out var api) && api.GetString() == "ready" &&
                        root.TryGetProperty("database", out var database) && database.GetString() == "ready";
            return Create(endpoint, true, (int)response.StatusCode, ready ? ApiErrorType.None : ApiErrorType.ServerError);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) { return FromException(endpoint, ex); }
    }

    public ApiDiagnosticResult FromStatus(string endpoint, int statusCode, StoredSessionDto? session = null,
        int accountsCount = 0, int costCentersCount = 0, int currenciesCount = 0, int paymentMethodsCount = 0, int openPeriodsCount = 0) =>
        Create(endpoint, true, statusCode, statusCode switch
        {
            401 => ApiErrorType.Unauthorized, 403 => ApiErrorType.Forbidden, 404 => ApiErrorType.NotFound,
            409 => ApiErrorType.Conflict, >= 500 => ApiErrorType.ServerError, _ => ApiErrorType.Unknown
        }, session, accountsCount, costCentersCount, currenciesCount, paymentMethodsCount, openPeriodsCount);

    public ApiDiagnosticResult FromException(string endpoint, Exception exception, StoredSessionDto? session = null)
    {
        var error = exception switch
        {
            TaskCanceledException => ApiErrorType.Timeout,
            HttpRequestException dnsRequest
                when dnsRequest.HttpRequestError == HttpRequestError.NameResolutionError
                => ApiErrorType.Dns,

            HttpRequestException connectionRequest
                when connectionRequest.HttpRequestError == HttpRequestError.ConnectionError
                => ApiErrorType.ConnectionRefused,

            HttpRequestException refusedRequest
                when FindSocketException(refusedRequest)?.SocketErrorCode == SocketError.ConnectionRefused
                => ApiErrorType.ConnectionRefused,

            HttpRequestException hostRequest
                when FindSocketException(hostRequest)?.SocketErrorCode is
                    SocketError.HostNotFound or
                    SocketError.NoData or
                    SocketError.TryAgain
                => ApiErrorType.Dns,
            System.Text.Json.JsonException or NotSupportedException => ApiErrorType.DeserializeFailure,
            _ => ApiErrorType.Unknown
        };
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

    private static SocketException? FindSocketException(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is SocketException socket) return socket;
        return null;
    }
}
