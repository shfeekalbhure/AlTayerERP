namespace AlTayerERP.API.DTOs.SystemCatalog;

public sealed class SystemModuleDto
{
    public string Module_Code { get; init; } = string.Empty;
    public string Module_Name_AR { get; init; } = string.Empty;
    public int Screen_Count { get; init; }
}

public sealed class SystemScreenCatalogDto
{
    public int Screen_ID { get; init; }
    public string Screen_Code { get; init; } = string.Empty;
    public string Screen_Name { get; init; } = string.Empty;
    public string Module_Name { get; init; } = string.Empty;
    public bool Is_Active { get; init; }
    public int Sort_Order { get; init; }
}

public sealed class SystemOperationDto
{
    public string Operation_Code { get; init; } = string.Empty;
    public string Operation_Name_AR { get; init; } = string.Empty;
    public string Shortcut_Key { get; init; } = string.Empty;
}

public sealed class ReferenceDataGroupDto
{
    public string Group_Code { get; init; } = string.Empty;
    public string Group_Name_AR { get; init; } = string.Empty;
    public int Item_Count { get; init; }
}

public sealed class ReferenceDataItemDto
{
    public int ID { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name_AR { get; init; } = string.Empty;
    public string? Name_EN { get; init; }
    public bool Is_Active { get; init; }
    public int Sort_Order { get; init; }
    public string Group_Code { get; init; } = string.Empty;
}
