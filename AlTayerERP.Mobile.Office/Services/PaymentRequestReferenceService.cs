using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

public sealed class PaymentRequestReferenceService(HttpClient httpClient, SessionStorageService sessionStorage)
{
    private const string Endpoint = "api/mobile/payment-request-references";

    public async Task<PaymentRequestReferenceResult> GetAsync(CancellationToken cancellationToken = default)
    {
        var session = await sessionStorage.GetAsync();
        if (session == null)
        {
            var failed = new PaymentRequestReferenceResult
            {
                ErrorType = PaymentRequestReferenceErrorType.Unauthorized
            };
            WriteDiagnostic(null, failed);
            return failed;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var statusCode = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                var failed = new PaymentRequestReferenceResult
                {
                    HttpStatusCode = statusCode,
                    ServerReached = true,
                    ErrorType = MapStatusCode(statusCode)
                };
                WriteDiagnostic(session, failed);
                return failed;
            }

            PaymentRequestReferencesDto? references;
            try
            {
                references = await response.Content.ReadFromJsonAsync<PaymentRequestReferencesDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                var failed = new PaymentRequestReferenceResult
                {
                    HttpStatusCode = statusCode,
                    ServerReached = true,
                    ErrorType = PaymentRequestReferenceErrorType.DeserializeFailure
                };
                WriteDiagnostic(session, failed);
                return failed;
            }

            if (references == null)
            {
                var failed = new PaymentRequestReferenceResult
                {
                    HttpStatusCode = statusCode,
                    ServerReached = true,
                    ErrorType = PaymentRequestReferenceErrorType.DeserializeFailure
                };
                WriteDiagnostic(session, failed);
                return failed;
            }

            references.Accounts ??= [];
            references.CostCenters ??= [];
            references.Currencies ??= [];
            references.PaymentMethods ??= [];
            references.OpenPeriods ??= [];

            var success = PaymentRequestReferenceResult.Success(references, statusCode);
            WriteDiagnostic(session, success);
            return success;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            var failed = new PaymentRequestReferenceResult { ErrorType = PaymentRequestReferenceErrorType.Timeout };
            WriteDiagnostic(session, failed);
            return failed;
        }
        catch (HttpRequestException ex)
        {
            var failed = new PaymentRequestReferenceResult
            {
                HttpStatusCode = ex.StatusCode is null ? null : (int)ex.StatusCode.Value,
                ErrorType = DetectNetworkError(ex)
            };
            WriteDiagnostic(session, failed);
            return failed;
        }
        catch (Exception)
        {
            var failed = new PaymentRequestReferenceResult { ErrorType = PaymentRequestReferenceErrorType.Unknown };
            WriteDiagnostic(session, failed);
            return failed;
        }
    }

    private static PaymentRequestReferenceErrorType MapStatusCode(int statusCode) => statusCode switch
    {
        401 => PaymentRequestReferenceErrorType.Unauthorized,
        403 => PaymentRequestReferenceErrorType.Forbidden,
        404 => PaymentRequestReferenceErrorType.NotFound,
        >= 500 => PaymentRequestReferenceErrorType.ServerError,
        _ => PaymentRequestReferenceErrorType.Unknown
    };

    private static PaymentRequestReferenceErrorType DetectNetworkError(HttpRequestException exception)
    {
        if (exception.HttpRequestError == HttpRequestError.NameResolutionError)
            return PaymentRequestReferenceErrorType.Dns;

        if (exception.HttpRequestError == HttpRequestError.ConnectionError)
            return PaymentRequestReferenceErrorType.ConnectionRefused;

        var socket = FindSocketException(exception);
        return socket?.SocketErrorCode switch
        {
            SocketError.ConnectionRefused => PaymentRequestReferenceErrorType.ConnectionRefused,
            SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain => PaymentRequestReferenceErrorType.Dns,
            _ => PaymentRequestReferenceErrorType.Unknown
        };
    }

    private static SocketException? FindSocketException(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is SocketException socket)
                return socket;

        return null;
    }

    [Conditional("DEBUG")]
    private void WriteDiagnostic(StoredSessionDto? session, PaymentRequestReferenceResult result)
    {
        Debug.WriteLine(
            "PaymentRequestReferences " +
            $"Endpoint={Endpoint}; BaseAddress={httpClient.BaseAddress}; " +
            $"Company_ID={session?.CompanyId ?? "none"}; Branch_ID={session?.BranchId.ToString() ?? "none"}; Fiscal_Year_ID={session?.YearId.ToString() ?? "none"}; " +
            $"StatusCode={result.HttpStatusCode?.ToString() ?? "none"}; ServerReached={result.ServerReached}; " +
            $"Accounts={result.AccountsCount}; CostCenters={result.CostCentersCount}; Currencies={result.CurrenciesCount}; " +
            $"PaymentMethods={result.PaymentMethodsCount}; OpenPeriods={result.OpenPeriodsCount}; ErrorType={result.ErrorType}");
    }
}
