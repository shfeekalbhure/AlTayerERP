from __future__ import annotations

from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path: str, content: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8", newline="\n")


def replace(path: str, old: str, new: str, *, required: bool = True) -> None:
    content = read(path)
    if old not in content:
        if required:
            raise RuntimeError(f"لم يوجد النص المطلوب في {path}: {old[:100]}")
        return
    write(path, content.replace(old, new))


def regex_replace(path: str, pattern: str, replacement: str, *, required: bool = True, flags: int = 0) -> None:
    content = read(path)
    updated, count = re.subn(pattern, replacement, content, flags=flags)
    if count == 0 and required:
        raise RuntimeError(f"لم ينجح الاستبدال في {path}: {pattern}")
    if count:
        write(path, updated)


# -----------------------------------------------------------------------------
# 1) توحيد Branch_ID في الكيانات وعقود API المالية إلى INT.
# -----------------------------------------------------------------------------
regex_replace(
    "AlTayerERP.Core/Entities/Accounting/FinancialVoucherHeader.cs",
    r"\s*\[Required\]\s*\[MaxLength\(50\)\]\s*\[Column\(\"Branch_ID\"\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;",
    '\n        [Required]\n        [Column("Branch_ID")]\n        public int Branch_ID { get; set; }',
    flags=re.MULTILINE,
)
regex_replace(
    "AlTayerERP.Core/Entities/Accounting/JournalEntryHeader.cs",
    r"\s*\[Required\]\s*\[MaxLength\(50\)\]\s*\[Column\(\"Branch_ID\"\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;",
    '\n        [Required]\n        [Column("Branch_ID")]\n        public int Branch_ID { get; set; }',
    flags=re.MULTILINE,
)
regex_replace(
    "AlTayerERP.API/DTOs/Accounting/CreateFinancialVoucherDto.cs",
    r"\s*\[Required\(ErrorMessage = \"الفرع مطلوب\.\"\)\]\s*\[MaxLength\(50\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;",
    '\n    [Required(ErrorMessage = "الفرع مطلوب.")]\n    [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]\n    public int Branch_ID { get; set; }',
    flags=re.MULTILINE,
)
regex_replace(
    "AlTayerERP.API/DTOs/Accounting/UpdateFinancialVoucherDto.cs",
    r"\s*\[Required\(ErrorMessage = \"الفرع مطلوب\.\"\)\]\s*\[MaxLength\(50\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;",
    '\n        [Required(ErrorMessage = "الفرع مطلوب.")]\n        [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]\n        public int Branch_ID { get; set; }',
    flags=re.MULTILINE,
)

# إضافة معرفات الجغرافيا المعتمدة إلى كيان الفرع إذا لم تكن موجودة.
branch_path = "AlTayerERP.Core/Entities/TenantBranch.cs"
branch_content = read(branch_path)
if "public int? Country_ID" not in branch_content:
    marker = "        [Column(\"Created_Date\")]"
    geography = '''        /// <summary>معرف الدولة المرجعية للفرع.</summary>\n        [Column("Country_ID")]\n        public int? Country_ID { get; set; }\n\n        /// <summary>معرف المحافظة المرجعية للفرع.</summary>\n        [Column("Governorate_ID")]\n        public int? Governorate_ID { get; set; }\n\n        /// <summary>معرف المدينة المرجعية للفرع.</summary>\n        [Column("City_ID")]\n        public int? City_ID { get; set; }\n\n'''
    if marker not in branch_content:
        raise RuntimeError("تعذر تحديد موضع إضافة جغرافيا الفرع.")
    write(branch_path, branch_content.replace(marker, geography + marker))

# تحديث حارس الترحيل في Program بعد تحول Branch_ID إلى int.
program_path = "AlTayerERP.API/Program.cs"
program = read(program_path)
program = program.replace(
    "using Microsoft.EntityFrameworkCore;",
    "using Microsoft.EntityFrameworkCore;\nusing Microsoft.EntityFrameworkCore.Infrastructure;",
)
program = program.replace(
'''builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    )
);''',
'''builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );

    // تطبيق نموذج Baseline الموحد دون تشغيل أي DDL وقت إقلاع API.
    options.ReplaceService<IModelCustomizer, Phase1ModelCustomizer>();
});''')
program = re.sub(
    r"\s*if \(!int\.TryParse\(voucher\.Branch_ID, out int branchId\)\)\s*\{\s*await RejectAsync\(context, \"معرف فرع السند غير صالح\.\"\);\s*return;\s*\}",
    "\n        int branchId = voucher.Branch_ID;",
    program,
    flags=re.MULTILINE,
)
write(program_path, program)

# تحديث أكثر التحويلات الشائعة في API بعد اعتماد Branch_ID الرقمي.
for path in (ROOT / "AlTayerERP.API").rglob("*.cs"):
    text = path.read_text(encoding="utf-8-sig")
    original = text
    text = text.replace("Branch_ID = session.Branch_ID.ToString()", "Branch_ID = session.Branch_ID")
    text = text.replace("Branch_ID = Session.Branch_ID.ToString()", "Branch_ID = Session.Branch_ID")
    text = text.replace("Branch_ID = branchId.ToString()", "Branch_ID = branchId")
    text = text.replace("x.Branch_ID == branchId.ToString()", "x.Branch_ID == branchId")
    text = text.replace("voucher.Branch_ID == branchId.ToString()", "voucher.Branch_ID == branchId")
    if text != original:
        path.write_text(text, encoding="utf-8", newline="\n")

# -----------------------------------------------------------------------------
# 2) كيانات المراجع التي كانت SQL-only.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.Core/Entities/Phase1ReferenceEntities.cs",
    '''using System.ComponentModel.DataAnnotations;\nusing System.ComponentModel.DataAnnotations.Schema;\n\nnamespace AlTayerERP.Core.Entities;\n\n/// <summary>الدولة المرجعية.</summary>\n[Table("countries")]\npublic sealed class Country\n{\n    [Key] public int Country_ID { get; set; }\n    [Required, MaxLength(10)] public string Country_Code { get; set; } = string.Empty;\n    [Required, MaxLength(150)] public string Country_Name_AR { get; set; } = string.Empty;\n    [MaxLength(150)] public string? Country_Name_EN { get; set; }\n    [MaxLength(2)] public string? ISO2 { get; set; }\n    [MaxLength(3)] public string? ISO3 { get; set; }\n    [MaxLength(10)] public string? Phone_Code { get; set; }\n    [MaxLength(10)] public string? Currency_Code { get; set; }\n    [MaxLength(150)] public string? Nationality_Name_AR { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n    [MaxLength(500)] public string? Notes { get; set; }\n    public DateTime Created_At { get; set; } = DateTime.UtcNow;\n    public DateTime? Updated_At { get; set; }\n}\n\n/// <summary>المحافظة التابعة لدولة.</summary>\n[Table("governorates")]\npublic sealed class Governorate\n{\n    [Key] public int Governorate_ID { get; set; }\n    public int Country_ID { get; set; }\n    [Required, MaxLength(20)] public string Governorate_Code { get; set; } = string.Empty;\n    [Required, MaxLength(150)] public string Governorate_Name_AR { get; set; } = string.Empty;\n    [MaxLength(150)] public string? Governorate_Name_EN { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n    [MaxLength(500)] public string? Notes { get; set; }\n    public DateTime Created_At { get; set; } = DateTime.UtcNow;\n    public DateTime? Updated_At { get; set; }\n}\n\n/// <summary>المدينة التابعة لمحافظة ودولة.</summary>\n[Table("cities")]\npublic sealed class City\n{\n    [Key] public int City_ID { get; set; }\n    public int Country_ID { get; set; }\n    public int Governorate_ID { get; set; }\n    [Required, MaxLength(20)] public string City_Code { get; set; } = string.Empty;\n    [Required, MaxLength(150)] public string City_Name_AR { get; set; } = string.Empty;\n    [MaxLength(150)] public string? City_Name_EN { get; set; }\n    [MaxLength(20)] public string? Postal_Code { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n    [MaxLength(500)] public string? Notes { get; set; }\n    public DateTime Created_At { get; set; } = DateTime.UtcNow;\n    public DateTime? Updated_At { get; set; }\n}\n\n/// <summary>نوع الفرع المرجعي.</summary>\n[Table("branch_types")]\npublic sealed class BranchType\n{\n    [Key] public int Branch_Type_ID { get; set; }\n    [Required, MaxLength(30)] public string Branch_Type_Code { get; set; } = string.Empty;\n    [Required, MaxLength(100)] public string Branch_Type_Name_AR { get; set; } = string.Empty;\n    [MaxLength(100)] public string? Branch_Type_Name_EN { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n    public DateTime Created_At { get; set; } = DateTime.UtcNow;\n    public DateTime? Updated_At { get; set; }\n}\n\n/// <summary>حالة اعتماد مرجعية ثابتة.</summary>\n[Table("approval_statuses")]\npublic sealed class ApprovalStatusReference\n{\n    [Key] public int Approval_Status_ID { get; set; }\n    [Required, MaxLength(30)] public string Approval_Status_Code { get; set; } = string.Empty;\n    [Required, MaxLength(100)] public string Approval_Status_Name_AR { get; set; } = string.Empty;\n    [MaxLength(100)] public string? Approval_Status_Name_EN { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n}\n\n/// <summary>نوع مستند يستخدمه محرك الترقيم.</summary>\n[Table("document_types")]\npublic sealed class DocumentTypeReference\n{\n    [Key] public int Document_Type_ID { get; set; }\n    [Required, MaxLength(50)] public string Document_Type_Code { get; set; } = string.Empty;\n    [Required, MaxLength(150)] public string Document_Type_Name_AR { get; set; } = string.Empty;\n    [MaxLength(150)] public string? Document_Type_Name_EN { get; set; }\n    public int Sort_Order { get; set; }\n    public bool Is_Active { get; set; } = true;\n}\n''')

# -----------------------------------------------------------------------------
# 3) تعطيل DDL وقت التشغيل لطلبات الصرف مع إبقاء واجهة توافقية آمنة.
# -----------------------------------------------------------------------------
write(
    "AlTayerERP.API/Services/PaymentRequestSchemaInitializer.cs",
    '''namespace AlTayerERP.API.Services;\n\n/// <summary>\n/// واجهة توافقية قديمة. أصبح مخطط طلبات الصرف جزءًا من EF Core Baseline،\n/// ولذلك يمنع هذا المكوّن تنفيذ CREATE TABLE أو ALTER TABLE وقت التشغيل.\n/// </summary>\n[Obsolete("تم نقل مخطط طلبات الصرف إلى EF Core Migrations. لا تستخدم DDL وقت التشغيل.")]\npublic sealed class PaymentRequestSchemaInitializer\n{\n    private readonly ILogger<PaymentRequestSchemaInitializer> _logger;\n\n    public PaymentRequestSchemaInitializer(ILogger<PaymentRequestSchemaInitializer> logger)\n    {\n        _logger = logger;\n    }\n\n    /// <summary>لا ينفذ أي SQL؛ موجود مؤقتًا لمنع كسر استدعاء قديم إن ظهر.</summary>\n    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)\n    {\n        cancellationToken.ThrowIfCancellationRequested();\n        _logger.LogInformation("تم تجاوز مهيئ مخطط طلبات الصرف؛ EF Core Migrations هي المصدر الوحيد للمخطط.");\n        return Task.CompletedTask;\n    }\n}\n''')

# -----------------------------------------------------------------------------
# 4) إضافة أدوات EF Design اللازمة لتوليد Baseline دون اتصال بقاعدة فعلية.
# -----------------------------------------------------------------------------
csproj_path = "AlTayerERP.Infrastructure/AlTayerERP.Infrastructure.csproj"
csproj = read(csproj_path)
if "Microsoft.EntityFrameworkCore.Design" not in csproj:
    csproj = csproj.replace(
        '<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />',
        '<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />\n\t\t<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.12">\n\t\t\t<PrivateAssets>all</PrivateAssets>\n\t\t\t<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>\n\t\t</PackageReference>',
    )
    write(csproj_path, csproj)

# نقل تاريخ Migration القديم إلى مرجع Legacy داخل فرع التنفيذ فقط.
legacy = ROOT / "Database/Legacy/Migrations"
legacy.mkdir(parents=True, exist_ok=True)
for source in (ROOT / "AlTayerERP.Infrastructure/Migrations").glob("*.cs"):
    target = legacy / source.name
    target.write_text(source.read_text(encoding="utf-8-sig"), encoding="utf-8", newline="\n")
    source.unlink()

print("تم إعداد نموذج المرحلة الأولى دون إنشاء قاعدة أو تنفيذ SQL.")
