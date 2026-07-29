from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def update(path: str, replacements: list[tuple[str, str]]) -> None:
    """تطبيق استبدالات Branch_ID الموجهة فقط داخل ملف محدد."""
    target = ROOT / path
    text = target.read_text(encoding="utf-8-sig")
    original = text
    for old, new in replacements:
        text = text.replace(old, new)
    if text != original:
        target.write_text(text, encoding="utf-8", newline="\n")


# إلغاء Parse بعد أن أصبح Branch_ID من نوع int فعليًا.
parse_block = '''        if (!int.TryParse(voucher.Branch_ID, out int branchId))
        {
            await RejectAsync(context, "معرف فرع السند غير صالح.");
            return;
        }'''
int_block = '''        int branchId = voucher.Branch_ID;
        if (branchId <= 0)
        {
            await RejectAsync(context, "معرف فرع السند غير صالح.");
            return;
        }'''
update("AlTayerERP.API/Program.cs", [(parse_block, int_block)])
update("AlTayerERP.API/Middleware/VoucherPostingGuardMiddleware.cs", [(parse_block, int_block)])

# فرض نطاق الجلسة رقميًا في إنشاء وتعديل السند.
update(
    "AlTayerERP.API/Controllers/FinancialVoucherController.cs",
    [("dto.Branch_ID = session.Branch_ID.ToString();", "dto.Branch_ID = session.Branch_ID;")],
)

# تقارير المحاسبة تقارن معرفات رقمية مباشرة.
update(
    "AlTayerERP.API/Controllers/AccountingReportsController.cs",
    [("h.Branch_ID==s.Branch_ID.ToString()", "h.Branch_ID==s.Branch_ID")],
)

# سجل التدقيق: Branch_ID رقمي nullable، بينما User_ID يبقى نصيًا تاريخيًا.
update(
    "AlTayerERP.API/Controllers/AuditLogsController.cs",
    [
        (
            "var branchIds = logs.Select(x => x.Branch_ID).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x!)).Distinct().ToList();",
            "var branchIds = logs.Select(x => x.Branch_ID).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();",
        ),
        ("public string? Branch_ID { get; set; }", "public int? Branch_ID { get; set; }"),
        (
            "Branch_ID = x.Branch_ID, Branch_Name = int.TryParse(x.Branch_ID, out var branchId) && branches.TryGetValue(branchId, out var branchName) ? branchName : \"غير متاح\"",
            "Branch_ID = x.Branch_ID, Branch_Name = x.Branch_ID.HasValue && branches.TryGetValue(x.Branch_ID.Value, out var branchName) ? branchName : \"غير متاح\"",
        ),
    ],
)

# جميع إنشاءات AuditLog تستخدم معرف الجلسة الرقمي مباشرة.
for relative in [
    "AlTayerERP.API/Controllers/BranchGeographyController.cs",
    "AlTayerERP.API/Controllers/BranchesController.cs",
    "AlTayerERP.API/Controllers/TenantGroupsController.cs",
    "AlTayerERP.API/Controllers/CompaniesController.cs",
]:
    update(relative, [("Branch_ID = session.Branch_ID.ToString(),", "Branch_ID = session.Branch_ID,")])

# إنشاء سند الصرف المرتبط بطلب الصرف.
update(
    "AlTayerERP.API/Controllers/PaymentRequestsController.cs",
    [("Branch_ID = session.Branch_ID.ToString(),", "Branch_ID = session.Branch_ID,")],
)

# مرفقات السندات وتقارير الجوال والبحث تستخدم int بدل النص.
for relative in [
    "AlTayerERP.API/Controllers/VoucherAttachmentsController.cs",
    "AlTayerERP.API/Controllers/MobileVoucherJournalController.cs",
    "AlTayerERP.API/Controllers/MobileReceiptVouchersController.cs",
    "AlTayerERP.API/Controllers/MobilePaymentVouchersController.cs",
    "AlTayerERP.API/Controllers/MobileJournalVouchersController.cs",
    "AlTayerERP.API/Controllers/MobileDocumentSearchController.cs",
    "AlTayerERP.API/Controllers/FiscalPeriodsController.cs",
]:
    update(
        relative,
        [
            ("session.Branch_ID.ToString()", "session.Branch_ID"),
            ("Session().Branch_ID.ToString()", "Session().Branch_ID"),
            ("s.Branch_ID.ToString()", "s.Branch_ID"),
            ("branchId.ToString()", "branchId"),
        ],
    )

# بدائل عامة آمنة داخل API لمواضع Branch_ID فقط.
for target in (ROOT / "AlTayerERP.API").rglob("*.cs"):
    text = target.read_text(encoding="utf-8-sig")
    original = text
    replacements = [
        ("int.Parse(voucher.Branch_ID)", "voucher.Branch_ID"),
        ("Convert.ToInt32(voucher.Branch_ID)", "voucher.Branch_ID"),
        ("voucher.Branch_ID == session.Branch_ID.ToString()", "voucher.Branch_ID == session.Branch_ID"),
        ("voucher.Branch_ID != session.Branch_ID.ToString()", "voucher.Branch_ID != session.Branch_ID"),
        ("x.Branch_ID == session.Branch_ID.ToString()", "x.Branch_ID == session.Branch_ID"),
        ("x.Branch_ID != session.Branch_ID.ToString()", "x.Branch_ID != session.Branch_ID"),
        ("x.Branch_ID == branchId.ToString()", "x.Branch_ID == branchId"),
        ("voucher.Branch_ID == branchId.ToString()", "voucher.Branch_ID == branchId"),
        ("Branch_ID=session.Branch_ID.ToString(),", "Branch_ID=session.Branch_ID,"),
        ("Branch_ID = session?.Branch_ID.ToString(),", "Branch_ID = session?.Branch_ID,"),
        ("Branch_ID=session?.Branch_ID.ToString(),", "Branch_ID=session?.Branch_ID,"),
    ]
    for old, new in replacements:
        text = text.replace(old, new)
    if text != original:
        target.write_text(text, encoding="utf-8", newline="\n")

print("تم إصلاح مجموعة أخطاء Branch_ID الحالية فقط دون إنشاء Baseline أو تنفيذ SQL.")
