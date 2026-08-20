namespace AlTayerERP.Mobile.Office.Services;

/// <summary>
/// يولد معرفاً عشوائياً ثابتاً لكل تثبيت للتطبيق ويخزنه في المخزن الآمن.
/// لا يعتمد على الطراز وحده، حتى لا تتشارك أجهزة من الطراز نفسه هوية منطقية.
/// </summary>
public sealed class DeviceIdentityService
{
    private const string DeviceIdKey = "altayer_mobile_device_id";

    public async Task<string> GetAsync()
    {
        var stored = await SecureStorage.Default.GetAsync(DeviceIdKey);
        if (!string.IsNullOrWhiteSpace(stored))
            return stored;

        var deviceId = $"maui-{Guid.NewGuid():N}";
        await SecureStorage.Default.SetAsync(DeviceIdKey, deviceId);
        return deviceId;
    }
}
