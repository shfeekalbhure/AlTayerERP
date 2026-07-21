using AlTayerERP.API.Controllers;
using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;




namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة السندات المالية:
    /// سند القبض، سند الصرف، والأنواع المالية الأخرى.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FinancialVoucherController : ControllerBase
    {
        #region الخدمات

        private readonly FinancialVoucherService _service;
        private readonly JournalEntryInquiryService _journalEntryInquiryService;
        private readonly VoucherApprovalService _approvalService;
        private readonly VoucherPostingService _postingService;
        private readonly AppDbContext _context;

        #endregion

        #region المشيد

        public FinancialVoucherController(
            FinancialVoucherService service,
            JournalEntryInquiryService journalEntryInquiryService,
            VoucherApprovalService approvalService,
            VoucherPostingService postingService,
            AppDbContext context)
        {
            _service = service;
            _journalEntryInquiryService = journalEntryInquiryService;
            _approvalService = approvalService;
            _postingService = postingService;
            _context = context;
        }

        #endregion
        #region نماذج طلبات الاعتماد والترحيل

        /// <summary>
        /// بيانات تنفيذ عملية على السند.
        /// تستخدم للاعتماد والترحيل.
        /// </summary>
        public sealed class VoucherActionRequest
        {
            /// <summary>
            /// معرف المستخدم الذي نفذ العملية.
            /// </summary>
            public string User_ID { get; set; } = string.Empty;

            /// <summary>
            /// قناة تنفيذ العملية:
            /// DESKTOP أو MOBILE أو ANDROID أو IOS أو WEB.
            /// </summary>
            public string Action_Channel { get; set; } = "DESKTOP";

            /// <summary>
            /// اسم الجهاز المنفذ للعملية.
            /// </summary>
            public string? Device_Name { get; set; }

            /// <summary>
            /// ملاحظات اختيارية.
            /// </summary>
            public string? Notes { get; set; }
        }

        /// <summary>
        /// بيانات تنفيذ عملية تتطلب سببًا إلزاميًا.
        /// تستخدم لإلغاء الاعتماد وإلغاء الترحيل.
        /// </summary>
        public sealed class VoucherReasonActionRequest
        {
            /// <summary>
            /// معرف المستخدم الذي نفذ العملية.
            /// </summary>
            public string User_ID { get; set; } = string.Empty;

            /// <summary>
            /// سبب تنفيذ العملية.
            /// </summary>
            public string Reason { get; set; } = string.Empty;

            /// <summary>
            /// قناة تنفيذ العملية.
            /// </summary>
            public string Action_Channel { get; set; } = "DESKTOP";

            /// <summary>
            /// اسم الجهاز.
            /// </summary>
            public string? Device_Name { get; set; }
        }

        #endregion
        /// <summary>
        /// إنشاء سند مالي جديد.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFinancialVoucherDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await CanExecuteReceiptActionAsync("ADD"))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية إنشاء سند قبض." });

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? currentBranchId = User.FindFirstValue("branch_id");
            string? currentYearId = User.FindFirstValue("year_id");

            if (string.IsNullOrWhiteSpace(currentUserId) ||
                string.IsNullOrWhiteSpace(currentBranchId) ||
                !int.TryParse(currentYearId, out int currentFiscalYearId))
            {
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي بيانات الجلسة المالية كاملة." });
            }

            // لا يقبل الخادم هوية منشئ أو فرع أو سنة من جسم الطلب؛ يعتمد سياق الجلسة الموثوق.
            dto.Created_By = currentUserId;
            dto.Branch_ID = currentBranchId;
            dto.Fiscal_Year_ID = currentFiscalYearId;

            if (dto.Voucher_Type_ID == 1)
            {
                bool? requiresApproval = await ResolveBooleanSettingAsync(
                    "ReceiptVoucher.RequiresApproval",
                    "ReceiptVoucher",
                    "ACCOUNTING");

                if (requiresApproval.HasValue)
                    dto.Requires_Approval = requiresApproval.Value;
            }

            var result = await _service.CreateAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                voucher_ID = result.VoucherId,
                voucher_No = result.VoucherNo
            });
        }

        /// <summary>
        /// استعلام عن القيد المحاسبي بواسطة رقم السند.
        /// </summary>
        [HttpGet("journal-entry/{voucherNo}")]
        public async Task<IActionResult> GetJournalEntryByVoucherNo(
            string voucherNo,
            [FromQuery] string? branchId = null,
            [FromQuery] int? voucherTypeId = null)
        {
            if (string.IsNullOrWhiteSpace(voucherNo))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "يجب إدخال رقم السند."
                });
            }

            // يستبدل الفرع القادم من الطلب بفرع الجلسة لمنع الاستعلام العابر للفروع.
            branchId = User.FindFirstValue("branch_id");
            if (string.IsNullOrWhiteSpace(branchId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي الفرع." });

            var result =
                await _journalEntryInquiryService.GetByVoucherNoAsync(
                    voucherNo,
                    branchId,
                    voucherTypeId);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "لم يتم العثور على قيد محاسبي لهذا السند."
                });
            }

            return Ok(new
            {
                success = true,
                message = "تم جلب القيد المحاسبي بنجاح.",
                data = result
            });
        }


        /// <summary>
        /// جلب سند مالي بواسطة المعرف.
        /// </summary>
        [HttpGet("{voucherId:long}")]
        public async Task<IActionResult> GetById(long voucherId)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (!await IsVoucherInCurrentSessionContextAsync(voucherId))
                return NotFound(new { success = false, message = "السند غير موجود في نطاق جلسة المستخدم." });

            var voucher = await _service.GetByIdAsync(voucherId);

            if (voucher == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "السند المالي غير موجود."
                });
            }

            return Ok(new
            {
                success = true,
                data = voucher
            });
        }

        /// <summary>
        /// تعديل سند مالي.
        /// </summary>
        [HttpPut("{voucherId:long}")]
        public async Task<IActionResult> Update(
            long voucherId,
            [FromBody] UpdateFinancialVoucherDto dto)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (voucherId != dto.Voucher_ID)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند في الرابط لا يطابق معرف السند المرسل."
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await CanExecuteReceiptActionAsync("EDIT", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية تعديل سند القبض." });

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? currentBranchId = User.FindFirstValue("branch_id");
            string? currentYearId = User.FindFirstValue("year_id");

            if (string.IsNullOrWhiteSpace(currentUserId) ||
                string.IsNullOrWhiteSpace(currentBranchId) ||
                !int.TryParse(currentYearId, out int currentFiscalYearId))
            {
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي بيانات الجلسة المالية كاملة." });
            }

            dto.Updated_By = currentUserId;
            dto.Branch_ID = currentBranchId;
            dto.Fiscal_Year_ID = currentFiscalYearId;

            if (dto.Voucher_Type_ID == 1)
            {
                bool? requiresApproval = await ResolveBooleanSettingAsync(
                    "ReceiptVoucher.RequiresApproval",
                    "ReceiptVoucher",
                    "ACCOUNTING");

                if (requiresApproval.HasValue)
                    dto.Requires_Approval = requiresApproval.Value;
            }

            var result = await _service.UpdateAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        /// <summary>
        /// حذف سند مالي غير مرحل.
        /// </summary>
        [HttpDelete("{voucherId:long}")]
        public async Task<IActionResult> Delete(long voucherId)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (!await CanExecuteReceiptActionAsync("DELETE", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية حذف سند القبض." });

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            var result = await _service.DeleteAsync(voucherId, currentUserId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }
        /// <summary>
        /// البحث بالرقم الكامل، أو بالرقم التسلسلي ضمن فرع وسنة الشاشة.
        /// </summary>
        [HttpGet("ByNumber")]
        public async Task<IActionResult> GetByNumber(
            [FromQuery] string voucherNumber,
            [FromQuery] string? branchId = null,
            [FromQuery] int? fiscalYearId = null,
            [FromQuery] int? voucherTypeId = null)
        {
            #region التحقق من بيانات البحث

            if (string.IsNullOrWhiteSpace(voucherNumber))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "رقم السند مطلوب."
                });
            }

            // لا يعتمد البحث على نطاق مرسل من الواجهة؛ الفرع والسنة من جلسة المستخدم.
            branchId = User.FindFirstValue("branch_id");
            if (!int.TryParse(User.FindFirstValue("year_id"), out int sessionYearId) ||
                string.IsNullOrWhiteSpace(branchId))
            {
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي نطاق البحث المالي." });
            }

            fiscalYearId = sessionYearId;

            bool sequenceSearch = int.TryParse(voucherNumber.Trim(), out int sequence) && sequence > 0;
            if (sequenceSearch &&
                (string.IsNullOrWhiteSpace(branchId) ||
                 !fiscalYearId.HasValue ||
                 fiscalYearId.Value <= 0))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "عند البحث بالرقم فقط يجب تحديد فرع وسنة الشاشة."
                });
            }

            #endregion

            var voucher =
                await _service.GetByVoucherNumberAsync(
                    voucherNumber,
                    branchId,
                    fiscalYearId,
                    voucherTypeId);

            if (voucher == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "لم يتم العثور على السند المطلوب."
                });
            }

            return Ok(new
            {
                success = true,
                data = voucher
            });



        }

        #region اعتماد السند

        #region مراجعة السند

        [HttpPost("{voucherId:long}/review")]
        public async Task<IActionResult> Review(
            long voucherId,
            [FromBody] VoucherActionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "بيانات تنفيذ المراجعة مطلوبة." });
            }

            if (!await CanExecuteReceiptActionAsync("REVIEW", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية مراجعة سند القبض." });

            if (!TryBindCurrentActor(request, out string currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            var result = await _service.MarkReviewedAsync(
                voucherId,
                currentUserId,
                request.Notes);

            return result.Success
                ? Ok(new { success = true, message = result.Message })
                : BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("{voucherId:long}/return-for-correction")]
        public async Task<IActionResult> ReturnForCorrection(
            long voucherId,
            [FromBody] VoucherReasonActionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { success = false, message = "سبب الإعادة للتصحيح مطلوب." });
            }

            if (!await CanExecuteReceiptActionAsync("RETURN_CORRECTION", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية إعادة السند للتصحيح." });

            if (!TryBindCurrentActor(request, out string currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            var result = await _service.ReturnForCorrectionAsync(
                voucherId,
                currentUserId,
                request.Reason);

            return result.Success
                ? Ok(new { success = true, message = result.Message })
                : BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("{voucherId:long}/record-print")]
        public async Task<IActionResult> RecordPrint(
            long voucherId,
            [FromBody] VoucherActionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "بيانات تسجيل الطباعة مطلوبة." });
            }

            if (!await CanExecuteReceiptActionAsync("PRINT", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية طباعة سند القبض." });

            if (!TryBindCurrentActor(request, out string currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            var result = await _service.RecordPrintAsync(voucherId, currentUserId);
            return result.Success
                ? Ok(new { success = true, message = result.Message })
                : BadRequest(new { success = false, message = result.Message });
        }

        #endregion

        /// <summary>
        /// اعتماد سند مالي.
        /// يعمل من الكمبيوتر أو تطبيق الهاتف من خلال نفس الـ API.
        /// </summary>
        [HttpPost("{voucherId:long}/approve")]
        public async Task<IActionResult> Approve(
            long voucherId,
            [FromBody] VoucherActionRequest request)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بيانات تنفيذ الاعتماد مطلوبة."
                });
            }

            if (!await CanExecuteReceiptActionAsync("APPROVE", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية اعتماد سند القبض." });

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });
            }

            // هوية منفذ الاعتماد تؤخذ من الرمز الموثوق ولا تقبل من جسم الطلب.
            request.User_ID = currentUserId;

            bool? preventCreatorApproval = await ResolveBooleanSettingAsync(
                "ReceiptVoucher.PreventCreatorApproval",
                "ReceiptVoucher",
                "ACCOUNTING");

            if (preventCreatorApproval == true)
            {
                string? createdBy = await _context.Financial_Voucher_Headers.AsNoTracking()
                    .Where(x => x.Voucher_ID == voucherId)
                    .Select(x => x.Created_By)
                    .FirstOrDefaultAsync();

                if (string.Equals(createdBy, currentUserId, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "لا يمكن لمنشئ سند القبض اعتماد السند حسب إعدادات الرقابة."
                    });
                }
            }

            var reviewValidation = await _service.ValidateReviewedAsync(voucherId);
            if (!reviewValidation.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = reviewValidation.Message
                });
            }

            string? ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            var result =
                await _approvalService.ApproveAsync(
                    voucherId: voucherId,
                    userId: currentUserId,
                    actionChannel: request.Action_Channel,
                    deviceName: request.Device_Name,
                    ipAddress: ipAddress,
                    notes: request.Notes);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        #endregion

        #region إلغاء اعتماد السند

        /// <summary>
        /// إلغاء اعتماد سند مالي.
        /// يتطلب سببًا وصلاحية خاصة لاحقًا.
        /// </summary>
        [HttpPost("{voucherId:long}/cancel-approval")]
        public async Task<IActionResult> CancelApproval(
            long voucherId,
            [FromBody] VoucherReasonActionRequest request)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بيانات تنفيذ إلغاء الاعتماد مطلوبة."
                });
            }

            if (!await CanExecuteReceiptActionAsync("UNAPPROVE", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية إلغاء اعتماد سند القبض." });

            if (!TryBindCurrentActor(request, out string currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "سبب إلغاء الاعتماد مطلوب."
                });
            }

            string? ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            var result =
                await _approvalService.CancelApprovalAsync(
                    voucherId: voucherId,
                    userId: currentUserId,
                    reason: request.Reason,
                    actionChannel: request.Action_Channel,
                    deviceName: request.Device_Name,
                    ipAddress: ipAddress);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        #endregion

        #region ترحيل السند

        /// <summary>
        /// ترحيل السند وإنشاء القيد المحاسبي العام.
        /// </summary>
        [HttpPost("{voucherId:long}/post")]
        public async Task<IActionResult> PostVoucher(
            long voucherId,
            [FromBody] VoucherActionRequest request)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بيانات تنفيذ الترحيل مطلوبة."
                });
            }

            if (!await CanExecuteReceiptActionAsync("POST", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية ترحيل سند القبض." });

            if (!TryBindCurrentActor(request, out string currentUserId))
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });

            var reviewValidation = await _service.ValidateReviewedAsync(voucherId);
            if (!reviewValidation.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = reviewValidation.Message
                });
            }

            string? ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            var result =
                await _postingService.PostAsync(
                    voucherId: voucherId,
                    userId: currentUserId,
                    actionChannel: request.Action_Channel,
                    deviceName: request.Device_Name,
                    ipAddress: ipAddress,
                    notes: request.Notes);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,

                data = new
                {
                    journalEntryId =
                        result.Journal_Entry_ID,

                    journalEntryNo =
                        result.Journal_Entry_No
                }
            });
        }

        #endregion

        #region إلغاء ترحيل السند

        /// <summary>
        /// إلغاء ترحيل السند وإلغاء القيد المرتبط به مع الاحتفاظ بسجل الرقابة.
        /// </summary>
        [HttpPost("{voucherId:long}/unpost")]
        public async Task<IActionResult> UnpostVoucher(
            long voucherId,
            [FromBody] VoucherReasonActionRequest request)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (request == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "بيانات تنفيذ فك الترحيل مطلوبة."
                });
            }

            if (!await CanExecuteReceiptActionAsync("UNPOST", voucherId))
                return StatusCode(403, new { success = false, message = "ليس لديك صلاحية فك ترحيل سند القبض." });

            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized(new { success = false, message = "رمز الدخول لا يحتوي معرف المستخدم." });
            }

            // يمنع انتحال منفذ فك الترحيل من خلال بيانات الطلب.
            request.User_ID = currentUserId;

            bool requiresUnpostReason = await ResolveBooleanSettingAsync(
                "ReceiptVoucher.RequireUnpostReason",
                "ReceiptVoucher",
                "ACCOUNTING") ?? true;

            if (requiresUnpostReason && string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "سبب إلغاء الترحيل مطلوب حسب إعدادات الرقابة."
                });
            }

            // يبقى سجل التدقيق واضحاً حتى عند تعطيل إلزام السبب من الإعداد.
            if (!requiresUnpostReason && string.IsNullOrWhiteSpace(request.Reason))
                request.Reason = "لم يطلب سبب فك الترحيل حسب الإعداد المعتمد.";

            string? ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            var result =
                await _postingService.UnpostAsync(
                    voucherId: voucherId,
                    userId: currentUserId,
                    reason: request.Reason,
                    actionChannel: request.Action_Channel,
                    deviceName: request.Device_Name,
                    ipAddress: ipAddress);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        #endregion

        #region حالة الاعتماد والترحيل

        /// <summary>
        /// جلب حالة اعتماد وترحيل السند.
        /// </summary>
        [HttpGet("{voucherId:long}/workflow-status")]
        public async Task<IActionResult> GetWorkflowStatus(
            long voucherId)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (!await IsVoucherInCurrentSessionContextAsync(voucherId))
            {
                return NotFound(new
                {
                    success = false,
                    message = "السند غير موجود في نطاق جلسة المستخدم."
                });
            }

            var approvalStatus =
                await _approvalService
                    .GetApprovalStatusAsync(voucherId);

            var postingStatus =
                await _postingService
                    .GetPostingStatusAsync(voucherId);

            if (approvalStatus == null &&
                postingStatus == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "السند المالي غير موجود."
                });
            }

            return Ok(new
            {
                success = true,

                data = new
                {
                    approval = approvalStatus,
                    posting = postingStatus
                }
            });
        }

        /// <summary>
        /// يتحقق من صلاحية الإجراء الحساسة على الخادم، ولا يعتمد على حالة الزر في الواجهة.
        /// </summary>
        private async Task<bool> CanExecuteReceiptActionAsync(
            string actionCode,
            long? voucherId = null)
        {
            if (voucherId.HasValue &&
                !await IsVoucherInCurrentSessionContextAsync(voucherId.Value))
            {
                return false;
            }

            if (User.IsInRole("SystemAdmin"))
                return true;

            if (!int.TryParse(User.FindFirstValue("role_id"), out int roleId))
                return false;

            int? screenId = await _context.SystemScreens.AsNoTracking()
                .Where(x => x.Screen_Code == "ReceiptVoucher" && x.Is_Active)
                .Select(x => (int?)x.Screen_ID)
                .FirstOrDefaultAsync();

            if (!screenId.HasValue)
                return false;

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return false;

            string resourceType = "ACTION:" + screenId.Value;
            string resourceCode = GetResourceActionCode(actionCode);
            var userPermission = await _context.User_Resource_Permissions.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.User_ID == userId &&
                    x.Resource_Type == resourceType &&
                    x.Resource_Code == resourceCode &&
                    x.Permission_Code == "EXECUTE" &&
                    x.Is_Active &&
                    (x.Effective_To == null || x.Effective_To >= DateTime.UtcNow));

            // الاستثناء الفردي يحسم كل الأزرار، بما فيها إضافة وتعديل وطباعة،
            // لذلك لا يمكن للدور الالتفاف على قرار منع خاص بالمستخدم.
            if (userPermission != null)
                return userPermission.Effect;

            var screenPermission = await _context.RolePermissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Role_ID == roleId && x.Screen_ID == screenId.Value);

            switch (actionCode)
            {
                case "ADD":
                    return screenPermission?.Can_Add == true;
                case "EDIT":
                    return screenPermission?.Can_Edit == true;
                case "DELETE":
                    return screenPermission?.Can_Delete == true;
                case "APPROVE":
                    return screenPermission?.Can_Approve == true;
                case "UNAPPROVE":
                    return screenPermission?.Can_UnApprove == true;
                case "PRINT":
                    return screenPermission?.Can_Print == true;
            }

            return await _context.Role_Resource_Permissions.AsNoTracking().AnyAsync(x =>
                x.Role_ID == roleId &&
                x.Resource_Type == resourceType &&
                x.Resource_Code == resourceCode &&
                x.Permission_Code == "EXECUTE" &&
                x.Effect &&
                x.Is_Active);
        }

        /// <summary>
        /// يوحد رموز أزرار الواجهة مع رموز العمليات الداخلية في محرك السند.
        /// </summary>
        private static string GetResourceActionCode(string actionCode) =>
            actionCode switch
            {
                "ADD" => "SAVE",
                "UNAPPROVE" => "CANCEL_APPROVAL",
                _ => actionCode
            };

        /// <summary>
        /// يتحقق من أن السند يتبع الفرع والسنة المختارين عند تسجيل الدخول.
        /// </summary>
        private async Task<bool> IsVoucherInCurrentSessionContextAsync(long voucherId)
        {
            string? branchId = User.FindFirstValue("branch_id");
            string? yearId = User.FindFirstValue("year_id");

            if (string.IsNullOrWhiteSpace(branchId) ||
                !int.TryParse(yearId, out int fiscalYearId))
            {
                return false;
            }

            return await _context.Financial_Voucher_Headers.AsNoTracking()
                .AnyAsync(x => x.Voucher_ID == voucherId &&
                               x.Is_Active &&
                               x.Branch_ID == branchId &&
                               x.Fiscal_Year_ID == fiscalYearId);
        }

        /// <summary>
        /// يثبت هوية منفذ العملية من الرمز الموثوق ويمنع انتحالها في جسم الطلب.
        /// </summary>
        private bool TryBindCurrentActor(VoucherActionRequest request, out string userId)
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            request.User_ID = userId;
            return true;
        }

        /// <summary>
        /// نسخة طلبات الإجراءات التي تحتوي سبباً إلزامياً أو اختيارياً.
        /// </summary>
        private bool TryBindCurrentActor(VoucherReasonActionRequest request, out string userId)
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            request.User_ID = userId;
            return true;
        }

        /// <summary>
        /// يحدد القيمة الفعالة لإعداد منطقي وفق نطاق جلسة المستخدم الحالية.
        /// </summary>
        private async Task<bool?> ResolveBooleanSettingAsync(
            string settingCode,
            string screenCode,
            string moduleName)
        {
            var setting = await _context.System_Settings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Setting_Code == settingCode && x.Is_Active);

            if (setting == null)
                return null;

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? roleId = User.FindFirstValue("role_id");
            string? companyId = User.FindFirstValue("company_id");
            string? branchId = User.FindFirstValue("branch_id");

            var scopes = new[]
            {
                new { Type = "USER", Id = userId },
                new { Type = "ROLE", Id = roleId },
                new { Type = "SCREEN", Id = screenCode },
                new { Type = "MODULE", Id = moduleName },
                new { Type = "BRANCH", Id = branchId },
                new { Type = "COMPANY", Id = companyId },
                new { Type = "GLOBAL", Id = "GLOBAL" }
            }
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToList();

            var values = await _context.Setting_Scope_Values.AsNoTracking()
                .Where(x => x.Setting_ID == setting.Setting_ID &&
                            x.Is_Active &&
                            (x.Effective_From == null || x.Effective_From <= DateTime.UtcNow) &&
                            (x.Effective_To == null || x.Effective_To >= DateTime.UtcNow))
                .OrderByDescending(x => x.Created_At)
                .ToListAsync();

            string? rawValue = setting.Default_Value;
            foreach (var scope in scopes)
            {
                var scopedValue = values.FirstOrDefault(x =>
                    string.Equals(x.Scope_Type, scope.Type, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Scope_ID, scope.Id, StringComparison.OrdinalIgnoreCase));

                if (scopedValue != null)
                {
                    rawValue = scopedValue.Value;
                    break;
                }
            }

            return bool.TryParse(rawValue, out bool value) ? value : null;
        }

        #endregion



    }
}
