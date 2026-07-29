from __future__ import annotations

from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8-sig")


def write(path: str, content: str) -> None:
    (ROOT / path).write_text(content, encoding="utf-8", newline="\n")


def replace_exact(path: str, old: str, new: str, *, required: bool = True) -> None:
    """تعديل موضعي صريح في ملف محدد، دون مسح عام أو تحويل أكواد الأعمال."""
    content = read(path)
    if old not in content:
        if required:
            raise RuntimeError(f"لم يوجد المقطع المطلوب في {path}: {old[:120]}")
        return
    write(path, content.replace(old, new))


def replace_all_exact(path: str, old: str, new: str, *, minimum: int = 1) -> None:
    """استبدال جميع التكرارات المتطابقة مع إثبات الحد الأدنى المتوقع."""
    content = read(path)
    count = content.count(old)
    if count < minimum:
        raise RuntimeError(
            f"عدد المقاطع في {path} أقل من المتوقع: وجد {count} والمتوقع {minimum} للمقطع {old[:100]}"
        )
    write(path, content.replace(old, new))


# -----------------------------------------------------------------------------
# 1) عقود السند المالي: Branch_ID معرف داخلي رقمي وليس Branch_Code.
# -----------------------------------------------------------------------------
replace_exact(
    "AlTayerERP.API/DTOs/Accounting/CreateFinancialVoucherDto.cs",
    '''    [Required(ErrorMessage = "الفرع مطلوب.")]
    [MaxLength(50)]
    public string Branch_ID { get; set; } = string.Empty;''',
    '''    [Required(ErrorMessage = "الفرع مطلوب.")]
    [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]
    public int Branch_ID { get; set; }''',
    required=False,
)
replace_exact(
    "AlTayerERP.API/DTOs/Accounting/UpdateFinancialVoucherDto.cs",
    '''        [Required(ErrorMessage = "الفرع مطلوب.")]
        [MaxLength(50)]
        public string Branch_ID { get; set; } = string.Empty;''',
    '''        [Required(ErrorMessage = "الفرع مطلوب.")]
        [Range(1, int.MaxValue, ErrorMessage = "معرف الفرع غير صحيح.")]
        public int Branch_ID { get; set; }''',
    required=False,
)
replace_exact(
    "AlTayerERP.API/DTOs/Accounting/FinancialVoucherResponseDto.cs",
    "        public string Branch_ID { get; set; } = string.Empty;",
    "        public int Branch_ID { get; set; }",
)


# -----------------------------------------------------------------------------
# 2) JournalEntryInquiryService: الفلاتر والنتائج تستخدم int/int? فقط.
# -----------------------------------------------------------------------------
inquiry = "AlTayerERP.API/DTOs/Accounting/JournalEntryInquiryService.cs"
replace_all_exact(
    inquiry,
    "            public string Branch_ID { get; set; } = string.Empty;",
    "            public int Branch_ID { get; set; }",
    minimum=2,
)
replace_all_exact(
    inquiry,
    "            string? branchId = null,",
    "            int? branchId = null,",
    minimum=2,
)
replace_all_exact(
    inquiry,
    "            if (!string.IsNullOrWhiteSpace(branchId))",
    "            if (branchId.HasValue)",
    minimum=2,
)
replace_all_exact(
    inquiry,
    "                    x => x.Branch_ID == branchId);",
    "                    x => x.Branch_ID == branchId.Value);",
    minimum=2,
)


# -----------------------------------------------------------------------------
# 3) VoucherPostingService: التخزين رقمي، والتحويل للنص فقط عند تكوين رقم المستند.
# -----------------------------------------------------------------------------
posting = "AlTayerERP.API/Services/Accounting/VoucherWorkflow/VoucherPostingService.cs"
replace_exact(
    posting,
    '''                if (!int.TryParse(voucher.Branch_ID, out var branchId))
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("معرف فرع السند غير صالح.");
                }''',
    '''                int branchId = voucher.Branch_ID;
                if (branchId <= 0)
                {
                    await transaction.RollbackAsync();
                    return PostingResult.Fail("معرف فرع السند غير صالح.");
                }''',
)
replace_exact(
    posting,
    '''                parts.Add(
                    voucher.Branch_ID);''',
    '''                parts.Add(
                    voucher.Branch_ID.ToString());''',
)
replace_exact(
    posting,
    '''        private async Task<string> GetCurrentCompanyIdAsync(string branchId)
        {
            if (!int.TryParse(branchId, out var branchKey))
                throw new InvalidOperationException("معرف الفرع غير صالح لتوليد رقم القيد.");

            var companyId = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchKey && x.Is_Active)''',
    '''        private async Task<string> GetCurrentCompanyIdAsync(int branchId)
        {
            if (branchId <= 0)
                throw new InvalidOperationException("معرف الفرع غير صالح لتوليد رقم القيد.");

            var companyId = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchId && x.Is_Active)''',
)


# -----------------------------------------------------------------------------
# 4) FinancialVoucherService: int في البحث والمقارنة، ToString للترقيم فقط.
# -----------------------------------------------------------------------------
voucher_service = "AlTayerERP.API/Services/Accounting/FinancialVoucherService.cs"
replace_exact(
    voucher_service,
    "                numberParts.Add(dto.Branch_ID);",
    "                numberParts.Add(dto.Branch_ID.ToString());",
)
replace_exact(
    voucher_service,
    '''        private async Task<string> ResolveCompanyIdAsync(CreateFinancialVoucherDto dto)
        {
            if (!int.TryParse(dto.Branch_ID, out var branchId))
                throw new InvalidOperationException("معرف الفرع غير صالح لتوليد رقم المستند.");

            var companyId = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchId && x.Is_Active)''',
    '''        private async Task<string> ResolveCompanyIdAsync(CreateFinancialVoucherDto dto)
        {
            int branchId = dto.Branch_ID;
            if (branchId <= 0)
                throw new InvalidOperationException("معرف الفرع غير صالح لتوليد رقم المستند.");

            var companyId = await _context.Tenant_Branches.AsNoTracking()
                .Where(x => x.Branch_ID == branchId && x.Is_Active)''',
)
replace_exact(
    voucher_service,
    '''                string voucherNumber,
                string? branchId,
                int? fiscalYearId,''',
    '''                string voucherNumber,
                int? branchId,
                int? fiscalYearId,''',
)
replace_exact(
    voucher_service,
    '''            if (string.IsNullOrWhiteSpace(voucherNumber) ||
                string.IsNullOrWhiteSpace(branchId) ||
                !fiscalYearId.HasValue ||''',
    '''            if (string.IsNullOrWhiteSpace(voucherNumber) ||
                !branchId.HasValue ||
                branchId.Value <= 0 ||
                !fiscalYearId.HasValue ||''',
)
replace_exact(
    voucher_service,
    '''            string searchValue = voucherNumber.Trim();
            string currentBranch = branchId.Trim();
            int currentFiscalYearId = fiscalYearId.Value;''',
    '''            string searchValue = voucherNumber.Trim();
            int currentBranch = branchId.Value;
            int currentFiscalYearId = fiscalYearId.Value;''',
)


# -----------------------------------------------------------------------------
# 5) FinancialVoucherController: نطاق الجلسة يقارن معرفات رقمية مباشرة.
# -----------------------------------------------------------------------------
voucher_controller = "AlTayerERP.API/Controllers/FinancialVoucherController.cs"
replace_all_exact(
    voucher_controller,
    "                !string.Equals(voucher.Branch_ID, session.Branch_ID.ToString(), StringComparison.Ordinal) ||",
    "                voucher.Branch_ID != session.Branch_ID ||",
    minimum=2,
)
replace_all_exact(
    voucher_controller,
    "                    session.Branch_ID.ToString(),",
    "                    session.Branch_ID,",
    minimum=2,
)


# -----------------------------------------------------------------------------
# 6) تقارير الجوال: معرف الفرع من الجلسة int؛ لا يخلط مع Branch_Code.
# -----------------------------------------------------------------------------
replace_exact(
    "AlTayerERP.API/Controllers/MobileTrialBalanceController.cs",
    "        var branchId = session.Branch_ID.ToString();",
    "        int branchId = session.Branch_ID;",
)
replace_exact(
    "AlTayerERP.API/Controllers/MobileGeneralLedgerController.cs",
    "        var branchId = session.Branch_ID.ToString();",
    "        int branchId = session.Branch_ID;",
)


# -----------------------------------------------------------------------------
# 7) سجل تدقيق الجغرافيا: العلاقة الفعلية int?، بينما Record_ID يبقى نصيًا.
# -----------------------------------------------------------------------------
replace_exact(
    "AlTayerERP.API/Controllers/GeographicReferencesController.cs",
    "Branch_ID=session?.Branch_ID.ToString(),",
    "Branch_ID=session?.Branch_ID,",
)


# -----------------------------------------------------------------------------
# 8) خدمة التحقق: إلغاء Parse لأن العقد أصبح int فعليًا.
# -----------------------------------------------------------------------------
validation = "AlTayerERP.API/Services/Accounting/VoucherValidationService.cs"
replace_exact(
    validation,
    "        if (string.IsNullOrWhiteSpace(voucher.Branch_ID) ||",
    "        if (voucher.Branch_ID <= 0 ||",
    required=False,
)
replace_exact(
    validation,
    '''        if (!int.TryParse(voucher.Branch_ID, out int branchId))
            return (false, "معرف الفرع غير صالح.");''',
    '''        int branchId = voucher.Branch_ID;''',
    required=False,
)


# -----------------------------------------------------------------------------
# 9) تدقيق النظام: Branch_ID رقمي، User_ID التاريخي يبقى نصيًا حسب القرار.
# -----------------------------------------------------------------------------
audit_controller = "AlTayerERP.API/Controllers/AuditLogsController.cs"
replace_exact(
    audit_controller,
    "x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID.ToString()",
    "x.User_ID == session.User_ID.ToString() && x.Branch_ID == session.Branch_ID",
    required=False,
)
replace_exact(
    audit_controller,
    "x.Branch_ID == request.Branch_ID.Value.ToString()",
    "x.Branch_ID == request.Branch_ID.Value",
    required=False,
)
replace_exact(
    audit_controller,
    "var branchIds = logs.Select(x => x.Branch_ID).Where(x => int.TryParse(x, out _)).Select(x => int.Parse(x!)).Distinct().ToList();",
    "var branchIds = logs.Select(x => x.Branch_ID).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();",
    required=False,
)
replace_exact(
    audit_controller,
    'Branch_Name = int.TryParse(x.Branch_ID, out var branchId) && branches.TryGetValue(branchId, out var branchName) ? branchName : "غير متاح"',
    'Branch_Name = x.Branch_ID.HasValue && branches.TryGetValue(x.Branch_ID.Value, out var branchName) ? branchName : "غير متاح"',
    required=False,
)
replace_exact(
    "AlTayerERP.API/Services/AuditTrailService.cs",
    "Branch_ID = session.Branch_ID.ToString()",
    "Branch_ID = session.Branch_ID",
    required=False,
)

print("تم تطبيق إصلاحات Branch_ID الموجهة ملفًا ملفًا دون تنفيذ SQL أو تحويل Branch_Code.")
