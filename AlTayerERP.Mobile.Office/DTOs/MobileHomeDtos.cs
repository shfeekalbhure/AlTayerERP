namespace AlTayerERP.Mobile.Office.DTOs;

public sealed class MobileHomePermissionsResponseDto
{
    public int User_ID { get; set; }
    public string Company_ID { get; set; } = string.Empty;
    public int Branch_ID { get; set; }
    public int YearId { get; set; }
    public List<MobileScreenPermissionDto> Permissions { get; set; } = [];
}

public sealed class MobileScreenPermissionDto
{
    public string ScreenCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool CanView { get; set; }
}
