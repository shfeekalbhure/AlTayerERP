namespace AlTayerERP.API.DTOs.Lookups;

/// <summary>
/// نتيجة موحدة لأي قائمة كبيرة يتم تحميلها على صفحات.
/// </summary>
public sealed class PagedLookupResultDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int Page_Size { get; init; }

    public int Total_Count { get; init; }

    public int Total_Pages => Page_Size <= 0
        ? 0
        : (int)Math.Ceiling((double)Total_Count / Page_Size);
}

/// <summary>
/// عنصر مرجعي صغير يستخدم في أنواع السندات والحالات وطرق السداد.
/// </summary>
public sealed class ReferenceLookupItemDto
{
    public int ID { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name_AR { get; init; } = string.Empty;

    public string? Name_EN { get; init; }

    public bool Is_Active { get; init; }

    public int Sort_Order { get; init; }
}

/// <summary>
/// حساب حركي يظهر في شاشة الاختيار الذكي.
/// </summary>
public sealed class AccountSmartLookupDto
{
    public string Account_ID { get; init; } = string.Empty;

    public string Account_Code { get; init; } = string.Empty;

    public string Account_Name_AR { get; init; } = string.Empty;

    public string? Account_Name_EN { get; init; }

    public string Account_Type { get; init; } = string.Empty;

    public string? Normal_Balance { get; init; }

    public string? Currency_Code { get; init; }

    public bool Is_Postable { get; init; }

    public bool Is_Active { get; init; }

    public bool Requires_Cost_Center { get; init; }

    public bool Requires_Party { get; init; }

    public bool Requires_Project { get; init; }

    public bool Multi_Currency { get; init; }
}

/// <summary>
/// طرف مالي يظهر في شاشة الاختيار الذكي.
/// </summary>
public sealed class PartySmartLookupDto
{
    public string Party_ID { get; init; } = string.Empty;

    public string Party_Code { get; init; } = string.Empty;

    public string Party_Name_AR { get; init; } = string.Empty;

    public string? Party_Name_EN { get; init; }

    public string Party_Type { get; init; } = string.Empty;

    public string? Mobile_No { get; init; }

    public string? Account_ID { get; init; }

    public bool Is_Active { get; init; }
}

/// <summary>
/// مركز تكلفة يظهر في شاشة الاختيار الذكي.
/// </summary>
public sealed class CostCenterSmartLookupDto
{
    public string Cost_Center_ID { get; init; } = string.Empty;

    public string Cost_Center_Code { get; init; } = string.Empty;

    public string Cost_Center_Name_AR { get; init; } = string.Empty;

    public string? Cost_Center_Name_EN { get; init; }

    public int Center_Level { get; init; }

    public bool Is_Postable { get; init; }

    public bool Is_Active { get; init; }
}

/// <summary>
/// عملة متاحة للشركة.
/// </summary>
public sealed class CurrencySmartLookupDto
{
    public int Currency_ID { get; init; }

    public string Currency_Code { get; init; } = string.Empty;

    public string Currency_Name_AR { get; init; } = string.Empty;

    public string? Currency_Name_EN { get; init; }

    public string? Currency_Symbol { get; init; }

    public int Decimal_Places { get; init; }

    public decimal Exchange_Rate { get; init; }

    public bool Is_Local_Currency { get; init; }

    public bool Is_Active { get; init; }
}

/// <summary>
/// صندوق متاح داخل فرع محدد.
/// </summary>
public sealed class CashBoxSmartLookupDto
{
    public string Cash_Box_ID { get; init; } = string.Empty;

    public string Cash_Box_Code { get; init; } = string.Empty;

    public string Cash_Box_Name_AR { get; init; } = string.Empty;

    public string? Cash_Box_Name_EN { get; init; }

    public int Branch_ID { get; init; }

    public string Account_ID { get; init; } = string.Empty;

    public string Currency_Code { get; init; } = string.Empty;

    public bool Is_Active { get; init; }
}
