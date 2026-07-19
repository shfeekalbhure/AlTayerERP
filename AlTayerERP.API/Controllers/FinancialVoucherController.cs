using AlTayerERP.API.Controllers;
using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using Microsoft.AspNetCore.Mvc;




namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// إدارة السندات المالية:
    /// سند القبض، سند الصرف، والأنواع المالية الأخرى.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialVoucherController : ControllerBase
    {
        #region الخدمات

        private readonly FinancialVoucherService _service;
        private readonly JournalEntryInquiryService _journalEntryInquiryService;
        private readonly VoucherApprovalService _approvalService;
        private readonly VoucherPostingService _postingService;

        #endregion

        #region المشيد

        public FinancialVoucherController(
            FinancialVoucherService service,
            JournalEntryInquiryService journalEntryInquiryService,
            VoucherApprovalService approvalService,
            VoucherPostingService postingService)
        {
            _service = service;
            _journalEntryInquiryService = journalEntryInquiryService;
            _approvalService = approvalService;
            _postingService = postingService;
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
        public async Task<IActionResult> Delete(
            long voucherId,
            [FromQuery] string deletedBy)
        {
            if (voucherId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف السند غير صحيح."
                });
            }

            if (string.IsNullOrWhiteSpace(deletedBy))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف المستخدم الذي نفذ الحذف مطلوب."
                });
            }

            var result = await _service.DeleteAsync(voucherId, deletedBy);

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
            if (request == null || string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new { success = false, message = "معرف المستخدم مطلوب للمراجعة." });
            }

            var result = await _service.MarkReviewedAsync(
                voucherId,
                request.User_ID,
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
            if (request == null ||
                string.IsNullOrWhiteSpace(request.User_ID) ||
                string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { success = false, message = "معرف المستخدم وسبب الإعادة مطلوبان." });
            }

            var result = await _service.ReturnForCorrectionAsync(
                voucherId,
                request.User_ID,
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
            if (request == null || string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new { success = false, message = "معرف المستخدم مطلوب لتسجيل الطباعة." });
            }

            var result = await _service.RecordPrintAsync(voucherId, request.User_ID);
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

            if (request == null ||
                string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف المستخدم مطلوب لاعتماد السند."
                });
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
                    userId: request.User_ID,
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

            if (request == null ||
                string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف المستخدم مطلوب لإلغاء الاعتماد."
                });
            }

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
                    userId: request.User_ID,
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

            if (request == null ||
                string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف المستخدم مطلوب لترحيل السند."
                });
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
                await _postingService.PostAsync(
                    voucherId: voucherId,
                    userId: request.User_ID,
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

            if (request == null ||
                string.IsNullOrWhiteSpace(request.User_ID))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "معرف المستخدم مطلوب لإلغاء الترحيل."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "سبب إلغاء الترحيل مطلوب."
                });
            }

            string? ipAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString();

            var result =
                await _postingService.UnpostAsync(
                    voucherId: voucherId,
                    userId: request.User_ID,
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

        #endregion



    }
}
