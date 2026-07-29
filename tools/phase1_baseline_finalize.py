from __future__ import annotations

from pathlib import Path
import re
import shutil

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path: str, content: str) -> None:
    target = ROOT / path
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(content, encoding="utf-8", newline="\n")


# 1) توحيد حارس الترحيل بعد اعتماد Branch_ID الرقمي.
# AppDbContext يطبق Phase1BaselineModelConfiguration مباشرة، لذلك لا نسجل
# IModelCustomizer ولا نحتاج خدمة إضافية وقت تشغيل API.
program_path = "AlTayerERP.API/Program.cs"
program = read(program_path)
program = re.sub(
    r'''\s*if \(!int\.TryParse\(voucher\.Branch_ID, out int branchId\)\)\s*\{\s*await RejectAsync\(context, "معرف فرع السند غير صالح\."\);\s*return;\s*\}''',
    "\n        int branchId = voucher.Branch_ID;",
    program,
    flags=re.MULTILINE,
)
write(program_path, program)


# 2) توحيد عقود Branch_ID إلى int/int? في المشاريع الثلاثة.
for project in ("AlTayerERP.API", "AlTayerERP.Desktop", "AlTayerERP.Mobile.Office"):
    for path in (ROOT / project).rglob("*.cs"):
        text = path.read_text(encoding="utf-8-sig")
        original = text
        text = re.sub(
            r"public\s+string\s+Branch_ID\s*\{\s*get;\s*set;\s*\}\s*=\s*string\.Empty;",
            "public int Branch_ID { get; set; }",
            text,
        )
        text = re.sub(
            r"public\s+string\?\s+Branch_ID\s*\{\s*get;\s*set;\s*\}",
            "public int? Branch_ID { get; set; }",
            text,
        )
        text = text.replace("Branch_ID = session.Branch_ID.ToString()", "Branch_ID = session.Branch_ID")
        text = text.replace("Branch_ID = Session.Branch_ID.ToString()", "Branch_ID = Session.Branch_ID")
        text = text.replace("Branch_ID = branchId.ToString()", "Branch_ID = branchId")
        text = text.replace("x.Branch_ID == branchId.ToString()", "x.Branch_ID == branchId")
        text = text.replace("voucher.Branch_ID == branchId.ToString()", "voucher.Branch_ID == branchId")
        text = text.replace("Branch_ID = dto.Branch_ID.Trim()", "Branch_ID = dto.Branch_ID")
        if text != original:
            path.write_text(text, encoding="utf-8", newline="\n")

# تعديل DTOs التي تحتوي MaxLength على Branch_ID.
for dto_path in (
    "AlTayerERP.API/DTOs/Accounting/CreateFinancialVoucherDto.cs",
    "AlTayerERP.API/DTOs/Accounting/UpdateFinancialVoucherDto.cs",
):
    dto = read(dto_path)
    dto = re.sub(
        r'''\[Required\(ErrorMessage = "الفرع مطلوب\."\)\]\s*\[MaxLength\(50\)\]\s*public string Branch_ID \{ get; set; \} = string\.Empty;''',
        '[Required(ErrorMessage = "الفرع مطلوب.")]\n    [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]\n    public int Branch_ID { get; set; }',
        dto,
    )
    write(dto_path, dto)


# 3) تعطيل Runtime DDL الخاص بطلبات الصرف.
write(
    "AlTayerERP.API/Services/PaymentRequestSchemaInitializer.cs",
    '''namespace AlTayerERP.API.Services;

/// <summary>
/// مكوّن توافق قديم. أصبح مخطط طلبات الصرف جزءًا من EF Core Baseline،
/// ولذلك لا يسمح بتنفيذ CREATE TABLE أو ALTER TABLE وقت تشغيل API.
/// </summary>
[Obsolete("تم نقل مخطط طلبات الصرف إلى EF Core Migrations.")]
public sealed class PaymentRequestSchemaInitializer
{
    private readonly ILogger<PaymentRequestSchemaInitializer> _logger;

    public PaymentRequestSchemaInitializer(ILogger<PaymentRequestSchemaInitializer> logger)
    {
        _logger = logger;
    }

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("تم تجاوز مهيئ طلبات الصرف؛ EF Core Migrations هي المصدر الوحيد للمخطط.");
        return Task.CompletedTask;
    }
}
''',
)


# 4) إضافة حزمة Design اللازمة لتوليد Migration دون قاعدة فعلية.
csproj_path = "AlTayerERP.Infrastructure/AlTayerERP.Infrastructure.csproj"
csproj = read(csproj_path)
if "Microsoft.EntityFrameworkCore.Design" not in csproj:
    csproj = csproj.replace(
        '<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />',
        '''<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.12" />
        <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.12">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
        </PackageReference>''',
    )
write(csproj_path, csproj)


# 5) حفظ تاريخ Migrations القديم خارج مجلد EF الفعال، دون حذفه من فرع المصدر.
legacy_dir = ROOT / "Database/Legacy/Migrations_PreBaseline"
legacy_dir.mkdir(parents=True, exist_ok=True)
migrations_dir = ROOT / "AlTayerERP.Infrastructure/Migrations"
for name in (
    "20260716223222_AddJournalEntryTables.cs",
    "20260716223222_AddJournalEntryTables.Designer.cs",
    "AppDbContextModelSnapshot.cs",
):
    source = migrations_dir / name
    destination = legacy_dir / name
    if source.exists():
        if destination.exists():
            destination.unlink()
        shutil.move(str(source), str(destination))

migrations_dir.mkdir(parents=True, exist_ok=True)

print("تم تجهيز نموذج المرحلة الأولى لتوليد Baseline دون تنفيذ SQL أو إنشاء قاعدة.")
