from __future__ import annotations

from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
PROJECTS = ("AlTayerERP.API", "AlTayerERP.Desktop", "AlTayerERP.Mobile.Office")


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")


def write(path: Path, content: str) -> None:
    path.write_text(content, encoding="utf-8", newline="\n")


def normalize_source(path: Path) -> None:
    text = read(path)
    original = text

    # عقود Branch_ID الحديثة تستخدم INT أو INT?، ولا يبقى المعرف الرقمي كنص.
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

    # إزالة ToString عندما يكون الطرف الآخر هو Branch_ID الرقمي.
    text = re.sub(
        r"(\b[\w.]+\.Branch_ID\s*(?:==|!=)\s*)([\w.]+\.Branch_ID)\.ToString\(\)",
        r"\1\2",
        text,
    )
    text = re.sub(
        r"(\b[\w.]+\.Branch_ID\s*(?:==|!=)\s*)(\w+)\.ToString\(\)",
        r"\1\2",
        text,
    )
    text = re.sub(
        r"(\bBranch_ID\s*=\s*)([\w.]+\.Branch_ID)\.ToString\(\)",
        r"\1\2",
        text,
    )
    text = re.sub(
        r"(\bBranch_ID\s*=\s*)(\w+)\.ToString\(\)",
        r"\1\2",
        text,
    )

    # إزالة Parse/TryParse عن خاصية أصبحت INT فعليًا.
    text = re.sub(r"int\.Parse\(([^()]+\.Branch_ID)\)", r"\1", text)
    text = re.sub(r"Convert\.ToInt32\(([^()]+\.Branch_ID)\)", r"\1", text)
    text = re.sub(
        r"\s*if\s*\(!int\.TryParse\(([^,]+\.Branch_ID),\s*out\s+int\s+(\w+)\)\)\s*\{.*?\}\s*",
        lambda m: f"\n        int {m.group(2)} = {m.group(1).strip()};\n",
        text,
        flags=re.DOTALL,
    )
    text = re.sub(
        r"\s*if\s*\(!int\.TryParse\(([^,]+\.Branch_ID),\s*out\s+int\s+(\w+)\)\)\s*return\s+[^;]+;",
        lambda m: f"\n        int {m.group(2)} = {m.group(1).strip()};",
        text,
    )

    # المعاملات المحلية التي تمثل Branch_ID تصبح رقمية.
    text = re.sub(r"\bstring\s+branchId\b", "int branchId", text)
    text = re.sub(r"\bstring\?\s+branchId\b", "int? branchId", text)

    if text != original:
        write(path, text)


for project in PROJECTS:
    for source in (ROOT / project).rglob("*.cs"):
        normalize_source(source)

# تصحيحات سجل التدقيق: User_ID يبقى Snapshot نصيًا، وBranch_ID يصبح INT?.
audit_controller = ROOT / "AlTayerERP.API/Controllers/AuditLogsController.cs"
if audit_controller.exists():
    text = read(audit_controller)
    text = text.replace(
        "x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID.ToString()",
        "x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID",
    )
    text = text.replace(
        "x.Branch_ID == request.Branch_ID.Value.ToString()",
        "x.Branch_ID == request.Branch_ID.Value",
    )
    text = text.replace(
        "var branchIds = logs.Select(x => x.Branch_ID).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x!)).Distinct().ToList();",
        "var branchIds = logs.Select(x => x.Branch_ID).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();",
    )
    text = text.replace(
        "Branch_Name = int.TryParse(x.Branch_ID, out var branchId) && branches.TryGetValue(branchId, out var branchName) ? branchName : \"غير متاح\"",
        "Branch_Name = x.Branch_ID.HasValue && branches.TryGetValue(x.Branch_ID.Value, out var branchName) ? branchName : \"غير متاح\"",
    )
    write(audit_controller, text)

# خدمة التدقيق لا تحول معرف الفرع الرقمي إلى نص.
audit_service = ROOT / "AlTayerERP.API/Services/AuditTrailService.cs"
if audit_service.exists():
    text = read(audit_service).replace(
        "Branch_ID = session.Branch_ID.ToString()",
        "Branch_ID = session.Branch_ID",
    )
    write(audit_service, text)

# التحقق المالي يتعامل مباشرة مع Branch_ID الرقمي.
validation = ROOT / "AlTayerERP.API/Services/Accounting/VoucherValidationService.cs"
if validation.exists():
    text = read(validation)
    text = text.replace("string.IsNullOrWhiteSpace(voucher.Branch_ID)", "voucher.Branch_ID <= 0")
    text = re.sub(
        r"\s*if\s*\(!int\.TryParse\(voucher\.Branch_ID,\s*out\s+int\s+branchId\)\)\s*return\s*\(false,\s*\"معرف الفرع غير صالح\.\"\);",
        "\n        int branchId = voucher.Branch_ID;",
        text,
    )
    write(validation, text)

print("تم توحيد توافق Branch_ID في API وDesktop وMobile دون تنفيذ SQL.")
