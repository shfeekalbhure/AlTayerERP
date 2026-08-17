from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def replace(path: str, old: str, new: str) -> None:
    target = ROOT / path
    text = target.read_text(encoding="utf-8-sig")
    if old not in text:
        raise RuntimeError(f"لم يوجد مقطع Branch_ID المطلوب في {path}")
    target.write_text(text.replace(old, new), encoding="utf-8", newline="\n")


# المسار الفيزيائي يحتاج تحويل المعرف الرقمي إلى نص عند تكوين اسم المجلد فقط.
replace(
    "AlTayerERP.API/Controllers/VoucherAttachmentsController.cs",
    "Session().Company_ID, Session().Branch_ID, Session().Year_ID.ToString(), voucherId.ToString()",
    "Session().Company_ID, Session().Branch_ID.ToString(), Session().Year_ID.ToString(), voucherId.ToString()",
)

# الاستعلام المحاسبي يقارن Branch_ID الرقمي مباشرة بمعرف الجلسة.
replace(
    "AlTayerERP.API/Controllers/FiscalPeriodsController.cs",
    "x.Branch_ID==Session.Branch_ID.ToString()",
    "x.Branch_ID==Session.Branch_ID",
)

print("تم إصلاح آخر خطأين Branch_ID دون تنفيذ SQL أو إنشاء Baseline.")
