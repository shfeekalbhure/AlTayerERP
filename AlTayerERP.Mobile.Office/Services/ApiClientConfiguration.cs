using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office.Services;

/// <summary>إعداد اتصال محلي قابل للتبديل بين USB وشبكة Wi-Fi في بيئة التطوير.</summary>
public sealed class ApiClientConfiguration
{
    private const string UsbAddress = "http://127.0.0.1:5021/";
    private const string ConnectionModeKey = "api.connection.mode";
    private const string WifiAddressKey = "api.connection.wifi.address";
    private const string PortKey = "api.connection.port";
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private HttpClient? _applicationClient;
    private Uri? _sessionBaseAddress;
    private ApiConnectionKind _sessionConnectionKind;
    private string? _sessionEnvironment;
    private string? _sessionDatabaseName;
    private bool _recoveryAttempted;

    public ApiConnectionMode ConnectionMode { get; private set; } =
        (ApiConnectionMode)Preferences.Default.Get(ConnectionModeKey, (int)ApiConnectionMode.Auto);

    public string WifiBaseAddress { get; private set; } =
        Preferences.Default.Get(WifiAddressKey, string.Empty);

    public int Port { get; private set; } =
        Math.Clamp(Preferences.Default.Get(PortKey, 5021), 1, 65535);

    public Uri UsbBaseAddress => new(UsbAddress);
    public Uri? ActiveBaseAddress => _sessionBaseAddress;

    public void Attach(HttpClient client)
    {
        _applicationClient = client;
        client.BaseAddress = UsbBaseAddress;
    }

    public void SaveSettings(ApiConnectionMode mode, string? wifiAddress, int port)
    {
        ConnectionMode = mode;
        WifiBaseAddress = NormalizeWifiAddress(wifiAddress);
        Port = Math.Clamp(port, 1, 65535);

        Preferences.Default.Set(ConnectionModeKey, (int)ConnectionMode);
        Preferences.Default.Set(WifiAddressKey, WifiBaseAddress);
        Preferences.Default.Set(PortKey, Port);

        _sessionBaseAddress = null;
        _sessionConnectionKind = ApiConnectionKind.None;
        _sessionEnvironment = null;
        _sessionDatabaseName = null;
        _recoveryAttempted = false;
        if (_applicationClient != null)
            _applicationClient.BaseAddress = UsbBaseAddress;
    }

    /// <summary>يفحص الطرق حسب الوضع المختار ويحفظ أول عنوان سليم لجلسة التطبيق.</summary>
    public async Task<ApiDiagnosticResult> EnsureConnectionAsync(bool forceRetry = false, CancellationToken cancellationToken = default)
    {
        if (!forceRetry && _sessionBaseAddress != null)
            return ReadyResult(_sessionBaseAddress, _sessionConnectionKind, _sessionDatabaseName, _sessionEnvironment);

        await _connectionGate.WaitAsync(cancellationToken);
        try
        {
            if (!forceRetry && _sessionBaseAddress != null)
                return ReadyResult(_sessionBaseAddress, _sessionConnectionKind, _sessionDatabaseName, _sessionEnvironment);

            ApiDiagnosticResult? lastFailure = null;
            foreach (var candidate in GetCandidates())
            {
                var result = await ProbeAsync(candidate.Address, candidate.Kind, cancellationToken);
                if (result.ErrorType == ApiErrorType.None)
                {
                    _sessionBaseAddress = candidate.Address;
                    _sessionConnectionKind = candidate.Kind;
                    _sessionEnvironment = result.Environment;
                    _sessionDatabaseName = result.DatabaseName;
                    if (_applicationClient != null)
                        _applicationClient.BaseAddress = candidate.Address;
                    return result;
                }

                lastFailure = result;
            }

            return lastFailure ?? FailureResult(ApiErrorType.ConnectionRefused, null, ApiConnectionKind.None);
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    /// <summary>يعيد اكتشاف العنوان مرة واحدة فقط بعد فشل اتصال فعلي في أثناء الجلسة.</summary>
    public async Task RecoverAfterConnectionFailureAsync()
    {
        if (_recoveryAttempted)
            return;

        _recoveryAttempted = true;
        _sessionBaseAddress = null;
        _sessionConnectionKind = ApiConnectionKind.None;
        await EnsureConnectionAsync(forceRetry: true, CancellationToken.None);
    }

    private IEnumerable<(Uri Address, ApiConnectionKind Kind)> GetCandidates()
    {
        var wifi = TryGetWifiUri();
        return ConnectionMode switch
        {
            ApiConnectionMode.USB => [(UsbBaseAddress, ApiConnectionKind.USB)],
            ApiConnectionMode.WiFi when wifi != null => [(wifi, ApiConnectionKind.WiFi)],
            ApiConnectionMode.WiFi => [],
            _ => wifi == null
                ? [(UsbBaseAddress, ApiConnectionKind.USB)]
                : [(UsbBaseAddress, ApiConnectionKind.USB), (wifi, ApiConnectionKind.WiFi)]
        };
    }

    private async Task<ApiDiagnosticResult> ProbeAsync(Uri baseAddress, ApiConnectionKind kind, CancellationToken cancellationToken)
    {
        const string endpoint = "api/health";
        try
        {
            using var client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromSeconds(8) };
            using var response = await client.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return FailureResult(response.StatusCode >= System.Net.HttpStatusCode.InternalServerError ? ApiErrorType.ServerError : ApiErrorType.Unknown, baseAddress, kind, (int)response.StatusCode, true);

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = document.RootElement;
            var isReady = root.TryGetProperty("api", out var api) && api.GetString() == "ready" &&
                          root.TryGetProperty("database", out var database) && database.GetString() == "ready";
            var environment = root.TryGetProperty("environment", out var environmentElement)
                ? environmentElement.GetString()
                : null;
            var databaseName = root.TryGetProperty("databaseName", out var name) ? name.GetString() : null;
            return isReady
                ? ReadyResult(baseAddress, kind, databaseName, environment)
                : FailureResult(ApiErrorType.ServerError, baseAddress, kind, (int)response.StatusCode, true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return FailureResult(Classify(ex), baseAddress, kind);
        }
    }

    private ApiDiagnosticResult ReadyResult(Uri baseAddress, ApiConnectionKind kind, string? databaseName = null, string? environment = null) => new()
    {
        Endpoint = "api/health",
        BaseAddress = baseAddress.ToString(),
        ServerReached = true,
        StatusCode = 200,
        ErrorType = ApiErrorType.None,
        ConnectionKind = kind,
        Environment = environment,
        DatabaseName = databaseName
    };

    private static ApiDiagnosticResult FailureResult(ApiErrorType errorType, Uri? baseAddress, ApiConnectionKind kind, int? statusCode = null, bool serverReached = false) => new()
    {
        Endpoint = "api/health",
        BaseAddress = baseAddress?.ToString() ?? string.Empty,
        ServerReached = serverReached,
        StatusCode = statusCode,
        ErrorType = errorType,
        ConnectionKind = kind
    };

    private Uri? TryGetWifiUri()
    {
        if (string.IsNullOrWhiteSpace(WifiBaseAddress))
            return null;

        return Uri.TryCreate($"http://{WifiBaseAddress}:{Port}/", UriKind.Absolute, out var address) ? address : null;
    }

    private static string NormalizeWifiAddress(string? value)
    {
        var address = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(address))
            return string.Empty;

        var candidate = address.Contains("://", StringComparison.Ordinal)
            ? address
            : $"http://{address}";

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
            return address.TrimEnd('/');

        // حقل المنفذ مستقل في الواجهة؛ نحتفظ بالمضيف فقط حتى لا ينتج عنوان مثل host:5021:5021.
        return uri.HostNameType == UriHostNameType.IPv6
            ? $"[{uri.Host}]"
            : uri.Host;
    }

    internal static ApiErrorType Classify(Exception exception) => exception switch
    {
        TaskCanceledException => ApiErrorType.Timeout,
        HttpRequestException request when request.HttpRequestError == HttpRequestError.NameResolutionError => ApiErrorType.Dns,
        HttpRequestException request when request.HttpRequestError == HttpRequestError.ConnectionError => ApiErrorType.ConnectionRefused,
        HttpRequestException request when FindSocketException(request)?.SocketErrorCode == SocketError.ConnectionRefused => ApiErrorType.ConnectionRefused,
        HttpRequestException request when FindSocketException(request)?.SocketErrorCode is SocketError.HostNotFound or SocketError.NoData or SocketError.TryAgain => ApiErrorType.Dns,
        JsonException or NotSupportedException => ApiErrorType.DeserializeFailure,
        _ => ApiErrorType.Unknown
    };

    private static SocketException? FindSocketException(Exception exception)
    {
        for (Exception? current = exception; current != null; current = current.InnerException)
            if (current is SocketException socket)
                return socket;
        return null;
    }
}

public enum ApiConnectionMode { Auto, USB, WiFi }
public enum ApiConnectionKind { None, USB, WiFi }
