using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.DTOs;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/payment-requests")]
public sealed class PaymentRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _auth;
    private readonly AuditTrailService _audit;
    private readonly FinancialVoucherService _vouchers;
    private readonly NumberGeneratorService _numbers;
    private readonly IdempotencyService _idempotencyService;
    private const string PaymentRequestApprovalType = "PAYMENT_REQUEST";

    public PaymentRequestsController(
        AppDbContext db,
        ScreenAuthorizationService auth,
        AuditTrailService audit,
        FinancialVoucherService vouchers,
        NumberGeneratorService numbers,
        IdempotencyService idempotencyService)
    {
        _db = db;
        _auth = auth;
        _audit = audit;
        _vouchers = vouchers;
        _numbers = numbers;
        _idempotencyService = idempotencyService;
    }

    private ServerSession Session() =>
        HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private ApiErrorResponse Error(string code, string? message) =>
        new ApiErrorResponse(
            code,
            string.IsNullOrWhiteSpace(message) ? "تعذر إتمام العملية. راجع البيانات ثم أعد المحاولة." : message,
            HttpContext.TraceIdentifier);

    private async Task<IActionResult?> Allow(ScreenOperation operation) =>
        await _auth.IsExplicitlyAllowedAsync(Session(), "PaymentRequest", operation)
            ? null
            : Forbid();

    private IQueryable<PaymentRequest> Scoped() =>
        _db.Payment_Requests
            .Include(x => x.Details)
            .Where(x => x.Company_ID == Session().Company_ID &&
                        x.Branch_ID == Session().Branch_ID &&
                        x.Fiscal_Year_ID == Session().Year_ID);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? requestNo)
    {
        var denial = await Allow(ScreenOperation.View);
        if (denial != null) return denial;

        var query = Scoped().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status.Trim().ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(requestNo))
            query = query.Where(x => x.Request_No.Contains(requestNo.Trim()));

        return Ok(await query.OrderByDescending(x => x.Created_At).Take(500).ToListAsync());
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var denial = await Allow(ScreenOperation.View);
        if (denial != null) return denial;

        var request = await Scoped().AsNoTracking().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        return request == null
            ? NotFound(Error("PAYMENT_REQUEST_NOT_FOUND", "طلب الصرف غير موجود ضمن النطاق الحالي."))
            : Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentRequestDto dto, CancellationToken cancellationToken)
    {
        var denial = await Allow(ScreenOperation.Add);
        if (denial != null) return denial;

        var validation = await Validate(dto);
        if (validation != null) return BadRequest(Error("PAYMENT_REQUEST_VALIDATION_FAILED", validation));

        if (!TryGetIdempotencyKey(out var idempotencyKey, out var idempotencyError))
            return BadRequest(Error("IDEMPOTENCY_KEY_INVALID", idempotencyError));

        // يظل سطح المكتب والعملاء الأقدم متوافقين إن لم يرسلوا الرأس الاختياري.
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            try
            {
                return Ok(await CreateRowAsync(dto, cancellationToken));
            }
            catch (NumberingException ex)
            {
                return BadRequest(Error("PAYMENT_REQUEST_NUMBERING_FAILED", ex.Message));
            }
        }

        var session = Session();
        var fingerprint = ComputeRequestFingerprint(dto);
        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var begin = await _idempotencyService.BeginAsync(
                session,
                "PAYMENT_REQUEST_CREATE",
                idempotencyKey,
                fingerprint,
                cancellationToken);

            if (begin.State == IdempotencyBeginState.PayloadMismatch)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Conflict(Error("IDEMPOTENCY_KEY_PAYLOAD_MISMATCH", "تم استخدام مفتاح حفظ طلب الصرف نفسه مع بيانات مختلفة."));
            }

            if (begin.State is IdempotencyBeginState.InProgress or IdempotencyBeginState.Contended)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Conflict(Error("IDEMPOTENCY_IN_PROGRESS", "طلب الصرف ما زال قيد المعالجة. أعد المحاولة بالمفتاح نفسه بعد لحظات."));
            }

            if (begin.State == IdempotencyBeginState.Completed)
            {
                await transaction.RollbackAsync(cancellationToken);
                var existing = await Scoped().AsNoTracking().SingleOrDefaultAsync(
                    x => x.Payment_Request_ID == begin.Record.Resource_ID,
                    cancellationToken);
                return existing == null
                    ? Conflict(Error("IDEMPOTENCY_RESULT_NOT_FOUND", "تمت معالجة الطلب سابقاً، لكن لا يمكن استعادة نتيجته ضمن نطاق الجلسة الحالية."))
                    : Ok(existing);
            }

            var row = await CreateRowAsync(dto, cancellationToken);
            await _idempotencyService.CompleteAsync(
                begin.Record,
                row.Payment_Request_ID,
                row.Request_No,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(row);
        }
        catch (NumberingException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BadRequest(Error("PAYMENT_REQUEST_NUMBERING_FAILED", ex.Message));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Conflict(Error("PAYMENT_REQUEST_NUMBER_CONTENTION", "تعذر حجز رقم طلب الصرف بسبب عملية متزامنة. أعد المحاولة بالمفتاح نفسه."));
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return StatusCode(StatusCodes.Status500InternalServerError,
                Error("PAYMENT_REQUEST_CREATE_FAILED", "تعذر حفظ طلب الصرف حالياً. تحقق من حالة الطلب قبل إعادة المحاولة."));
        }
    }

    private async Task<PaymentRequest> CreateRowAsync(PaymentRequestDto dto, CancellationToken cancellationToken)
    {
        var session = Session();
        var reservation = await _numbers.ReserveNextNumberAsync(
            "PAYMENT_REQUEST", session.Company_ID, session.Branch_ID, session.Year_ID, cancellationToken);

        var row = new PaymentRequest
        {
            Company_ID = session.Company_ID,
            Branch_ID = session.Branch_ID,
            Fiscal_Year_ID = session.Year_ID,
            Request_No = reservation.Document_Number,
            Request_Date = dto.Request_Date.Date,
            Status = "DRAFT",
            Beneficiary_Name = dto.Beneficiary_Name.Trim(),
            Party_ID = Text(dto.Party_ID),
            Payment_Method_ID = dto.Payment_Method_ID,
            Header_Reference_No = Text(dto.Header_Reference_No),
            Description = Text(dto.Description),
            Created_By = session.User_ID.ToString(),
            Created_At = DateTime.UtcNow,
            Details = dto.Lines.Select((x, index) => Line(x, index + 1)).ToList()
        };

        _db.Payment_Requests.Add(row);
        _audit.Add(session, HttpContext, "numbering_counters", reservation.Counter_ID.ToString(),
            "NUMBER_RESERVED", null, new
            {
                reservation.Document_Number,
                reservation.Document_Type,
                reservation.Serial_Number,
                reservation.Company_ID,
                reservation.Branch_ID,
                reservation.Fiscal_Year_ID
            });
        _audit.Add(session, HttpContext, "payment_requests", "new", "CREATE", null,
            new { row.Request_No, row.Status, row.Beneficiary_Name, Lines = row.Details.Count });

        await _db.SaveChangesAsync(cancellationToken);
        return row;
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PaymentRequestDto dto)
    {
        var denial = await Allow(ScreenOperation.Edit);
        if (denial != null) return denial;

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound(Error("PAYMENT_REQUEST_NOT_FOUND", "طلب الصرف غير موجود ضمن النطاق الحالي."));
        if (row.Status is not ("DRAFT" or "RETURNED"))
            return Conflict(Error("PAYMENT_REQUEST_STATE_NOT_EDITABLE", "لا يعدل إلا طلب مسودة أو معاد."));

        var validation = await Validate(dto);
        if (validation != null) return BadRequest(Error("PAYMENT_REQUEST_VALIDATION_FAILED", validation));

        var before = new { row.Beneficiary_Name, row.Status, row.Approved_Local_Total };
        row.Beneficiary_Name = dto.Beneficiary_Name.Trim();
        row.Party_ID = Text(dto.Party_ID);
        row.Payment_Method_ID = dto.Payment_Method_ID;
        row.Header_Reference_No = Text(dto.Header_Reference_No);
        row.Description = Text(dto.Description);
        row.Request_Date = dto.Request_Date.Date;
        _db.Payment_Request_Lines.RemoveRange(row.Details);
        row.Details = dto.Lines.Select((x, index) => Line(x, index + 1)).ToList();
        row.Status = "DRAFT";
        row.Updated_By = Session().User_ID.ToString();
        row.Updated_At = DateTime.UtcNow;

        _audit.Add(Session(), HttpContext, "payment_requests", id.ToString(), "UPDATE", before,
            new { row.Status, row.Beneficiary_Name, Lines = row.Details.Count });
        await _db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpPost("{id:long}/submit")]
    public Task<IActionResult> Submit(long id) =>
        Transition(id, "DRAFT", "PENDING_REVIEW", ScreenOperation.Edit, null, "SUBMIT", false);

    [HttpPost("{id:long}/review")]
    public Task<IActionResult> Review(long id, [FromBody] ReasonDto dto) =>
        Transition(id, "PENDING_REVIEW", "PENDING_APPROVAL", ScreenOperation.Approve,
            dto?.Reason, "REVIEW", true);

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id, [FromBody] ReasonDto dto)
    {
        var denial = await Allow(ScreenOperation.Approve);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto?.Reason))
            return BadRequest(Error("APPROVAL_REASON_REQUIRED", "سبب الاعتماد إلزامي."));

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound(Error("PAYMENT_REQUEST_NOT_FOUND", "طلب الصرف غير موجود ضمن النطاق الحالي."));
        if (row.Status != "PENDING_APPROVAL")
            return Conflict(Error("PAYMENT_REQUEST_STATE_CONFLICT", "الحالة الحالية لا تسمح بالاعتماد."));
        if (IsRequester(row))
            return Conflict(Error("SEGREGATION_OF_DUTIES", "لا يمكن لمقدم الطلب تنفيذ قرار على طلبه. اختر مستخدماً مخولاً آخر."));

        var amount = row.Details.Sum(x => x.Local_Amount);
        var limit = await FindFinancialLimitAsync(row);
        if (limit != null && amount > limit.Limit_Amount - limit.Used_Amount)
            return Conflict(Error("FINANCIAL_LIMIT_EXCEEDED", "المبلغ يتجاوز السقف المالي المتاح."));

        row.Status = "APPROVED";
        row.Approved_Local_Total = amount;
        row.Approval_Reason = dto.Reason.Trim();
        row.Updated_By = Session().User_ID.ToString();
        row.Updated_At = DateTime.UtcNow;
        await SynchronizeApprovalRequestAsync(row, ApprovalStatus.Approved, dto.Reason);
        _audit.Add(Session(), HttpContext, "payment_requests", id.ToString(), "APPROVE", null,
            new { row.Status, row.Approved_Local_Total }, dto.Reason);
        await _db.SaveChangesAsync();
        return Ok(row);
    }

    [HttpPost("{id:long}/reject")]
    public Task<IActionResult> Reject(long id, [FromBody] ReasonDto dto) =>
        Close(id, "REJECTED", dto, "REJECT");

    [HttpPost("{id:long}/return")]
    public Task<IActionResult> Return(long id, [FromBody] ReasonDto dto) =>
        Close(id, "RETURNED", dto, "RETURN");

    [HttpPost("{id:long}/create-payment-voucher")]
    public async Task<IActionResult> CreatePaymentVoucher(long id, [FromBody] CreateVoucherDto dto)
    {
        var denial = await Allow(ScreenOperation.Add);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto.Cash_Account_ID))
            return BadRequest(Error("PAYMENT_VOUCHER_CASH_ACCOUNT_REQUIRED", "الصندوق/البنك الدائن مطلوب."));

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);
        try
        {
            var session = Session();
            var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
            if (row == null) return NotFound(Error("PAYMENT_REQUEST_NOT_FOUND", "طلب الصرف غير موجود ضمن النطاق الحالي."));
            if (row.Status != "APPROVED" || row.Payment_Voucher_ID.HasValue)
                return Conflict(Error("PAYMENT_VOUCHER_ALREADY_CREATED", "لا ينشأ سند الصرف إلا مرة واحدة من طلب معتمد."));
            if (!row.Payment_Method_ID.HasValue)
                return Conflict(Error("PAYMENT_METHOD_INVALID", "طلب الصرف لا يحتوي طريقة سداد صالحة."));

            var local = row.Details.Sum(x => x.Local_Amount);
            if (local <= 0 || local > row.Approved_Local_Total)
                return Conflict(Error("PAYMENT_VOUCHER_AMOUNT_INVALID", "مبلغ السند يجب أن يكون موجباً وألا يتجاوز المبلغ المعتمد."));

            var limit = await FindFinancialLimitAsync(row);
            if (limit != null && local > limit.Limit_Amount - limit.Used_Amount)
                return Conflict(Error("FINANCIAL_LIMIT_UNAVAILABLE", "السقف المالي لم يعد متاحاً لإنشاء سند الصرف."));

            var type = await _db.Voucher_Types
                .Where(x => x.Is_Active && x.Voucher_Type_Code == "PAYMENT")
                .Select(x => x.Voucher_Type_ID)
                .SingleAsync();
            var draft = await _db.Voucher_Statuses
                .Where(x => x.Is_Active && x.Voucher_Status_Code == "DRAFT")
                .Select(x => x.Voucher_Status_ID)
                .SingleAsync();
            var headerCurrency = await _db.Currencies
                .Where(x => x.Company_ID == session.Company_ID && x.Is_Active && x.Is_Local_Currency)
                .Select(x => x.Currency_ID)
                .SingleAsync();

            var now = DateTime.UtcNow;
            var request = new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDto
            {
                Voucher_Type_ID = type,
                Voucher_Status_ID = draft,
                Branch_ID = session.Branch_ID.ToString(),
                Fiscal_Year_ID = session.Year_ID,
                Voucher_Date = now,
                Transaction_Date = now,
                Cash_Account_ID = dto.Cash_Account_ID.Trim(),
                Party_ID = row.Party_ID,
                Received_From_Name = row.Beneficiary_Name,
                Payment_Method_ID = row.Payment_Method_ID,
                Currency_ID = headerCurrency,
                Exchange_Rate = 1m,
                Amount = local,
                Foreign_Total = 0m,
                Local_Total = local,
                Reference_No = row.Request_No,
                Against_Text = row.Description ?? $"طلب صرف {row.Request_No}",
                Description = row.Description,
                Source_Document_No = row.Request_No,
                Requires_Approval = true,
                Created_By = session.User_ID.ToString(),
                Updated_By = session.User_ID.ToString(),
                Details = row.Details.Select((x, index) =>
                    new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDetailDto
                    {
                        Line_No = index + 2,
                        Account_ID = x.Account_ID,
                        Cost_Center_ID = x.Cost_Center_ID,
                        Currency_ID = x.Currency_ID,
                        Exchange_Rate = x.Exchange_Rate,
                        Foreign_Amount = x.Foreign_Amount,
                        Local_Amount = x.Local_Amount,
                        Debit_Amount = x.Local_Amount,
                        Credit_Amount = 0m,
                        Reference_No = x.Reference_No,
                        Description = x.Description,
                        Line_Type = 2
                    })
                    .Prepend(new AlTayerERP.API.DTOs.Accounting.CreateFinancialVoucherDetailDto
                    {
                        Line_No = 1,
                        Account_ID = dto.Cash_Account_ID.Trim(),
                        Currency_ID = headerCurrency,
                        Exchange_Rate = 1m,
                        Foreign_Amount = 0m,
                        Local_Amount = local,
                        Debit_Amount = 0m,
                        Credit_Amount = local,
                        Description = row.Description,
                        Line_Type = 1
                    }).ToList()
            };

            var result = await _vouchers.CreateAsync(request);
            if (!result.Success)
            {
                await transaction.RollbackAsync();
                return BadRequest(Error("PAYMENT_VOUCHER_CREATE_FAILED", result.Message));
            }

            row.Payment_Voucher_ID = result.VoucherId;
            row.Updated_By = session.User_ID.ToString();
            row.Updated_At = now;

            if (limit != null)
            {
                limit.Used_Amount += local;
                limit.Updated_At = now;
                _db.Financial_Policy_Movements.Add(new FinancialPolicyMovement
                {
                    Limit_ID = limit.Limit_ID,
                    Company_ID = session.Company_ID,
                    Movement_Date = now,
                    Movement_Type = "PAYMENT_VOUCHER",
                    Reference_Type = "PAYMENT_REQUEST",
                    Reference_ID = row.Payment_Request_ID.ToString(),
                    Currency_Code = limit.Currency_Code,
                    Amount = local,
                    Balance_After = limit.Limit_Amount - limit.Used_Amount,
                    Notes = $"طلب الصرف {row.Request_No} / سند الصرف {result.VoucherNo}",
                    Created_By = session.User_ID.ToString(),
                    Created_At = now
                });
            }

            _audit.Add(session, HttpContext, "payment_requests", id.ToString(),
                "CREATE_PAYMENT_VOUCHER", null,
                new { result.VoucherId, result.VoucherNo, local, FinancialLimitId = limit?.Limit_ID });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { result.VoucherId, result.VoucherNo });
        }
        catch
        {
            await transaction.RollbackAsync();
            return StatusCode(StatusCodes.Status500InternalServerError,
                Error("PAYMENT_VOUCHER_CREATE_FAILED", "تعذر إنشاء سند الصرف؛ لم يتم تسجيل أي تعديل."));
        }
    }

    private async Task<IActionResult> Transition(
        long id,
        string from,
        string to,
        ScreenOperation operation,
        string? reason,
        string action,
        bool reasonRequired)
    {
        var denial = await Allow(operation);
        if (denial != null) return denial;
        if (reasonRequired && string.IsNullOrWhiteSpace(reason))
            return BadRequest(Error("ACTION_REASON_REQUIRED", "سبب الإجراء إلزامي."));

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound(Error("PAYMENT_REQUEST_NOT_FOUND", "طلب الصرف غير موجود ضمن النطاق الحالي."));
        if (row.Status != from)
            return Conflict(Error("STATE_TRANSITION_NOT_ALLOWED", "الحالة الحالية لا تسمح بهذه العملية."));
        if ((operation is ScreenOperation.Approve or ScreenOperation.Unapprove) && IsRequester(row))
            return Conflict(Error("SEGREGATION_OF_DUTIES", "لا يمكن لمقدم الطلب تنفيذ قرار على طلبه. اختر مستخدماً مخولاً آخر."));

        row.Status = to;
        row.Review_Reason = Text(reason);
        row.Updated_By = Session().User_ID.ToString();
        row.Updated_At = DateTime.UtcNow;
        if (string.Equals(action, "SUBMIT", StringComparison.Ordinal))
            await CreateApprovalRequestIfNeededAsync(row);
        else if (string.Equals(action, "REVIEW", StringComparison.Ordinal))
            await SynchronizeApprovalRequestAsync(row, ApprovalStatus.UnderReview, reason);
        else if (string.Equals(action, "REJECT", StringComparison.Ordinal))
            await SynchronizeApprovalRequestAsync(row, ApprovalStatus.Rejected, reason);
        else if (string.Equals(action, "RETURN", StringComparison.Ordinal))
            await SynchronizeApprovalRequestAsync(row, ApprovalStatus.Returned, reason);
        _audit.Add(Session(), HttpContext, "payment_requests", id.ToString(), action, null,
            new { row.Status }, reason);
        await _db.SaveChangesAsync();
        return Ok(row);
    }

    private bool IsRequester(PaymentRequest row) =>
        !string.IsNullOrWhiteSpace(row.Created_By) &&
        string.Equals(row.Created_By, Session().User_ID.ToString(), StringComparison.OrdinalIgnoreCase);

    private async Task CreateApprovalRequestIfNeededAsync(PaymentRequest row)
    {
        var referenceId = row.Payment_Request_ID.ToString();
        var hasOpenRequest = await _db.Approval_Requests.AnyAsync(x =>
            x.Company_ID == Session().Company_ID &&
            x.Reference_Type == PaymentRequestApprovalType &&
            x.Reference_ID == referenceId &&
            (x.Status == ApprovalStatus.Pending.ToString() || x.Status == ApprovalStatus.UnderReview.ToString()));
        if (hasOpenRequest) return;

        _db.Approval_Requests.Add(new ApprovalRequest
        {
            Company_ID = Session().Company_ID,
            Request_Type = "PaymentRequest",
            Reference_Type = PaymentRequestApprovalType,
            Reference_ID = referenceId,
            Entity_Type = PaymentRequestApprovalType,
            Entity_ID = referenceId,
            Amount = row.Details.Sum(x => x.Local_Amount),
            Reason = row.Description,
            Status = ApprovalStatus.Pending.ToString(),
            Requested_By = row.Created_By,
            Requested_At = DateTime.UtcNow
        });
    }

    private async Task SynchronizeApprovalRequestAsync(
        PaymentRequest row, ApprovalStatus target, string? reason)
    {
        var referenceId = row.Payment_Request_ID.ToString();
        var approval = await _db.Approval_Requests
            .Where(x => x.Company_ID == Session().Company_ID &&
                        x.Reference_Type == PaymentRequestApprovalType &&
                        x.Reference_ID == referenceId &&
                        (x.Status == ApprovalStatus.Pending.ToString() || x.Status == ApprovalStatus.UnderReview.ToString()))
            .OrderByDescending(x => x.Requested_At)
            .FirstOrDefaultAsync();
        if (approval is null) return;

        approval.Status = target.ToString();
        approval.Approval_Notes = Text(reason) ?? approval.Approval_Notes;
        if (target == ApprovalStatus.Approved)
        {
            approval.Approved_By = Session().User_ID.ToString();
            approval.Approved_At = DateTime.UtcNow;
        }
        else
        {
            approval.Approved_By = null;
            approval.Approved_At = null;
        }
    }

    private Task<IActionResult> Close(long id, string to, ReasonDto dto, string action) =>
        string.IsNullOrWhiteSpace(dto?.Reason)
            ? Task.FromResult<IActionResult>(BadRequest(Error("ACTION_REASON_REQUIRED", "السبب إلزامي.")))
            : Transition(id, "PENDING_APPROVAL", to, ScreenOperation.Unapprove,
                dto.Reason, action, true);

    private async Task<FinancialPolicy?> FindFinancialLimitAsync(PaymentRequest row) =>
        await _db.Financial_Policies
            .Where(x => x.Company_ID == Session().Company_ID &&
                        x.Is_Active &&
                        x.Limit_Type == "PAYMENT" &&
                        (x.Entity_ID == row.Party_ID || x.Entity_ID == Session().Branch_ID.ToString()))
            .OrderBy(x => x.Limit_Amount)
            .FirstOrDefaultAsync();

    private async Task<string?> Validate(PaymentRequestDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Beneficiary_Name) || dto.Lines.Count == 0)
            return "المستفيد والتفاصيل مطلوبان.";
        if (!dto.Payment_Method_ID.HasValue)
            return "طريقة السداد مطلوبة.";

        var session = Session();
        var methodValid = await _db.Payment_Methods.AsNoTracking().AnyAsync(x =>
            x.Payment_Method_ID == dto.Payment_Method_ID.Value && x.Is_Active);
        if (!methodValid)
            return "طريقة السداد غير موجودة أو غير فعالة.";

        var periodOpen = await _db.Fiscal_Periods.AsNoTracking().AnyAsync(period =>
            period.Branch_ID == session.Branch_ID &&
            period.Fiscal_Year_ID == session.Year_ID &&
            period.Is_Active && !period.Is_Closed &&
            period.Start_Date.Date <= dto.Request_Date.Date &&
            period.End_Date.Date >= dto.Request_Date.Date);
        if (!periodOpen)
            return "لا توجد فترة مالية مفتوحة لتاريخ طلب الصرف.";

        foreach (var line in dto.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.Account_ID) || line.Currency_ID <= 0 ||
                line.Exchange_Rate <= 0 || line.Local_Amount <= 0)
                return "الحساب والعملة وسعر الصرف والمبلغ المحلي مطلوبة لكل سطر.";

            var currency = await _db.Currencies.AsNoTracking().SingleOrDefaultAsync(x =>
                x.Currency_ID == line.Currency_ID &&
                x.Company_ID == session.Company_ID &&
                x.Is_Active);
            if (currency == null)
                return "توجد عملة موقوفة أو خارج نطاق الشركة.";
            if (currency.Is_Local_Currency &&
                (line.Foreign_Amount != 0m || line.Exchange_Rate != 1m))
                return "في العملة المحلية يجب أن يكون الأجنبي صفراً وسعر الصرف 1.";
            if (!currency.Is_Local_Currency &&
                decimal.Round(line.Foreign_Amount * line.Exchange_Rate, 4) !=
                decimal.Round(line.Local_Amount, 4))
                return "المبلغ المحلي للسطر الأجنبي يجب أن يساوي الأجنبي × سعر الصرف.";

            var accountValid = await _db.Chart_Of_Accounts.AsNoTracking().AnyAsync(x =>
                x.Account_ID == line.Account_ID.Trim() &&
                x.Company_ID == session.Company_ID &&
                x.Is_Active && x.Is_Postable);
            if (!accountValid)
                return "يوجد حساب غير نشط أو غير قابل للترحيل.";

            if (!string.IsNullOrWhiteSpace(line.Cost_Center_ID))
            {
                var centerValid = await _db.Cost_Centers.AsNoTracking().AnyAsync(x =>
                    x.Cost_Center_ID == line.Cost_Center_ID.Trim() &&
                    x.Company_ID == session.Company_ID &&
                    x.Is_Active && x.Is_Postable);
                if (!centerValid)
                    return "يوجد مركز تكلفة غير نشط أو غير قابل للترحيل.";
            }
        }

        return null;
    }

    private static PaymentRequestLine Line(PaymentRequestLineDto dto, int number) => new()
    {
        Line_No = number,
        Account_ID = dto.Account_ID.Trim(),
        Cost_Center_ID = Text(dto.Cost_Center_ID),
        Currency_ID = dto.Currency_ID,
        Exchange_Rate = dto.Exchange_Rate,
        Foreign_Amount = dto.Foreign_Amount,
        Local_Amount = dto.Local_Amount,
        Reference_No = Text(dto.Reference_No),
        Description = Text(dto.Description)
    };

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private bool TryGetIdempotencyKey(out string? key, out string? error)
    {
        key = HttpContext.Request.Headers["Idempotency-Key"].ToString().Trim();
        error = null;
        if (string.IsNullOrWhiteSpace(key))
        {
            key = null;
            return true;
        }

        if (key.Length > 100 || key.Any(character =>
                !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')))
        {
            error = "مفتاح منع تكرار الحفظ غير صالح.";
            return false;
        }

        return true;
    }

    private static string ComputeRequestFingerprint(PaymentRequestDto dto)
    {
        var payload = JsonSerializer.Serialize(dto);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    public sealed class PaymentRequestDto
    {
        public DateTime Request_Date { get; set; } = DateTime.UtcNow;
        public string Beneficiary_Name { get; set; } = string.Empty;
        public string? Party_ID { get; set; }
        public int? Payment_Method_ID { get; set; }
        public string? Header_Reference_No { get; set; }
        public string? Description { get; set; }
        public List<PaymentRequestLineDto> Lines { get; set; } = [];
    }

    public sealed class PaymentRequestLineDto
    {
        public string Account_ID { get; set; } = string.Empty;
        public string? Cost_Center_ID { get; set; }
        public int Currency_ID { get; set; }
        public decimal Exchange_Rate { get; set; } = 1m;
        public decimal Foreign_Amount { get; set; }
        public decimal Local_Amount { get; set; }
        public string? Reference_No { get; set; }
        public string? Description { get; set; }
    }

    public sealed class ReasonDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class CreateVoucherDto
    {
        public string Cash_Account_ID { get; set; } = string.Empty;
    }
}
