namespace AlTayerERP.API.DTOs;

public sealed record MobileReferenceLookupItemDto(
    string Id,
    string Code,
    string Name,
    string DisplayName);

public sealed record MobileReferenceLookupResponseDto(
    IReadOnlyList<MobileReferenceLookupItemDto> Items,
    int Limit,
    string Search,
    bool HasMore);
