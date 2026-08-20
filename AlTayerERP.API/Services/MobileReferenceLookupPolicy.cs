namespace AlTayerERP.API.Services;

public static class MobileReferenceLookupPolicy
{
    public const int DefaultLimit = 25;
    public const int MaximumLimit = 50;
    public const int MaximumSearchLength = 80;

    public static int NormalizeLimit(int requested) => Math.Clamp(requested, 1, MaximumLimit);

    public static string NormalizeSearch(string? search)
    {
        var normalized = search?.Trim() ?? string.Empty;
        return normalized.Length <= MaximumSearchLength
            ? normalized
            : normalized[..MaximumSearchLength];
    }
}
