using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Route("api/payment-requests")]
public sealed class PaymentRequestsController : ControllerBase
{
    private const string PaymentRequestTable = "payment_requests";
    private const string ReviewAction = "REVIEW";
    private const long StoragePrecisionTicks = TimeSpan.TicksPerSecond;

    private readonly AppDbContext _db;
    private readonly ScreenAuthorizationService _auth;
    private readonly AuditTrailService _audit;
    private readonly FinancialVoucherService _vouchers;
    private readonly NumberGeneratorService _numbers;

    public PaymentRequestsController(
        AppDbContext db,
        ScreenAuthorizationService auth,
        AuditTrailService audit,
        FinancialVoucherService vouchers,
        NumberGeneratorService numbers)
    {
        _db = db;
        _auth = auth;
        _audit = audit;
        _vouchers = vouchers;
        _numbers = numbers;
    }

    private ServerSession Session() =>
        HttpContext.Items["ServerSession"] as ServerSession
        ?? throw new InvalidOperationException("جلسة الخادم غير متاحة.");

    private async Task<IActionResult?> Allow(string screenCode, ScreenOperation operation) =>
        await _auth.IsExplicitlyAllowedAsync(Session(), screenCode, operation)
            ? null
            : StatusCode(StatusCodes.Status403Forbidden,
                new { message = "لا تملك الصلاحية المطلوبة لتنفيذ هذه العملية." });

    private Task<IActionResult?> Allow(ScreenOperation operation) => Allow("PaymentRequest", operation);

    private IQueryable<PaymentRequest> Scoped() =>
        _db.Payment_Requests
            .Include(x => x.Details)
            .Where(x => x.Company_ID == Session().Company_ID &&
                        x.Branch_ID == Session().Branch_ID &&
                        x.Fiscal_Year_ID == Session().Year_ID);

    private bool IsCreator(PaymentRequest row) =>
        string.Equals(row.Created_By?.Trim(), Session().User_ID.ToString(), StringComparison.Ordinal);

    private IActionResult CreatorSeparationConflict(string operation) =>
        Conflict(new { message = $"لا يجوز لمنشئ طلب الصرف {operation} طلبه بنفسه وفق فصل الواجبات." });

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

        var rows = await query.OrderByDescending(x => x.Created_At).Take(500).ToListAsync();
        rows.ForEach(NormalizeResponseTimestamps);
        return Ok(rows);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var denial = await Allow(ScreenOperation.View);
        if (denial != null) return denial;

        var request = await Scoped().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (request == null) return NotFound();

        NormalizeResponseTimestamps(request);
        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentRequestDto dto)
    {
        var denial = await Allow(ScreenOperation.Add);
        if (denial != null) return denial;

        var validation = await Validate(dto);
        if (validation != null) return BadRequest(new { message = validation });

        var session = Session();
        NumberReservation reservation;
        try
        {
            reservation = await _numbers.ReserveNextNumberAsync(
                "PAYMENT_REQUEST", session.Company_ID, session.Branch_ID, session.Year_ID);
        }
        catch (NumberingException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var now = UtcNowAtStoragePrecision();
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
            Created_At = now,
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
        _audit.Add(session, HttpContext, PaymentRequestTable, "new", "CREATE", null,
            new { row.Request_No, row.Status, row.Beneficiary_Name, Lines = row.Details.Count });

        await _db.SaveChangesAsync();
        return Ok(await ReloadForResponse(row.Payment_Request_ID));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PaymentRequestDto dto)
    {
        var denial = await Allow(ScreenOperation.Edit);
        if (denial != null) return denial;

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status is not ("DRAFT" or "RETURNED"))
            return Conflict(new { message = "لا يعدل إلا طلب صرف مسودة أو معاد." });
        if (!IsCreator(row))
            return Conflict(new { message = "لا يسمح بتعديل طلب الصرف إلا لمنشئه بعد إعادته أو أثناء المسودة." });

        var currentModifiedAt = NormalizeUtcAtStoragePrecision(row.Updated_At ?? row.Created_At);
        var expectedModifiedAt = dto.Expected_Last_Modified_At.HasValue
            ? NormalizeUtcAtStoragePrecision(dto.Expected_Last_Modified_At.Value)
            : (DateTime?)null;

        if (!expectedModifiedAt.HasValue || currentModifiedAt != expectedModifiedAt.Value)
        {
            return Conflict(new
            {
                message = "تم تعديل طلب الصرف بواسطة مستخدم آخر. حدّث البيانات ثم أعد المحاولة."
            });
        }

        var validation = await Validate(dto);
        if (validation != null) return BadRequest(new { message = validation });

        var before = new
        {
            row.Beneficiary_Name,
            row.Status,
            row.Approved_Local_Total,
            LastModifiedAt = currentModifiedAt
        };
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
        row.Updated_At = UtcNowAtStoragePrecision();

        _audit.Add(Session(), HttpContext, PaymentRequestTable, id.ToString(), "UPDATE", before,
            new { row.Status, row.Beneficiary_Name, Lines = row.Details.Count, row.Updated_At });
        await _db.SaveChangesAsync();
        return Ok(await ReloadForResponse(id));
    }

    [HttpPost("{id:long}/submit")]
    public async Task<IActionResult> Submit(long id)
    {
        var denial = await Allow(ScreenOperation.Edit);
        if (denial != null) return denial;

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status is not ("DRAFT" or "RETURNED"))
            return Conflict(new { message = "الحالة الحالية لا تسمح بإرسال طلب الصرف للمراجعة." });
        if (!IsCreator(row))
            return Conflict(new { message = "لا يسمح بإرسال طلب الصرف إلا لمنشئه." });

        return await ApplyTransition(row, "PENDING_REVIEW", "SUBMIT", null);
    }

    [HttpPost("{id:long}/review")]
    public async Task<IActionResult> Review(long id, [FromBody] ReasonDto dto)
    {
        var denial = await Allow(ScreenOperation.Approve);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto?.Reason))
            return BadRequest(new { message = "سبب المراجعة إلزامي." });

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status != "PENDING_REVIEW")
            return Conflict(new { message = "الحالة الحالية لا تسمح بمراجعة طلب الصرف." });
        if (IsCreator(row)) return CreatorSeparationConflict("مراجعة");

        row.Review_Reason = dto.Reason.Trim();
        return await ApplyTransition(row, "PENDING_APPROVAL", ReviewAction, dto.Reason);
    }

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> Approve(long id, [FromBody] ReasonDto dto)
    {
        var denial = await Allow(ScreenOperation.Approve);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto?.Reason))
            return BadRequest(new { message = "سبب الاعتماد إلزامي." });

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status != "PENDING_APPROVAL")
            return Conflict(new { message = "الحالة الحالية لا تسمح باعتماد طلب الصرف." });
        if (IsCreator(row)) return CreatorSeparationConflict("اعتماد");

        var latestReviewerId = await LatestReviewerId(id);
        if (string.IsNullOrWhiteSpace(latestReviewerId))
        {
            _audit.Add(Session(), HttpContext, PaymentRequestTable, id.ToString(),
                "APPROVE_BLOCKED_NO_REVIEW", null,
                new { CurrentUserId = Session().User_ID, row.Status },
                "لا يوجد سجل مراجعة ناجح للدورة الحالية.");
            await _db.SaveChangesAsync();
            return Conflict(new
            {
                message = "لا يمكن اعتماد طلب الصرف لعدم وجود مراجعة ناجحة موثقة للدورة الحالية."
            });
        }

        if (string.Equals(latestReviewerId.Trim(), Session().User_ID.ToString(), StringComparison.Ordinal))
        {
            _audit.Add(Session(), HttpContext, PaymentRequestTable, id.ToString(),
                "APPROVE_BLOCKED_SAME_REVIEWER", null,
                new { ReviewerUserId = latestReviewerId, CurrentUserId = Session().User_ID, row.Status },
                "محاولة اعتماد بواسطة المستخدم الذي نفذ أحدث مراجعة ناجحة.");
            await _db.SaveChangesAsync();
            return Conflict(new
            {
                message = "لا يسمح للمستخدم الذي راجع طلب الصرف باعتماده. يجب أن ينفذ الاعتماد مستخدم آخر مخول."
            });
        }

        var amount = row.Details.Sum(x => x.Local_Amount);
        var limit = await FindFinancialLimitAsync(row);
        if (limit != null && amount > limit.Limit_Amount - limit.Used_Amount)
            return Conflict(new { message = "المبلغ يتجاوز السقف المالي المتاح." });

        row.Approved_Local_Total = amount;
        row.Approval_Reason = dto.Reason.Trim();
        return await ApplyTransition(row, "APPROVED", "APPROVE", dto.Reason,
            new { ApprovedLocalTotal = amount, FinancialLimitId = limit?.Limit_ID, ReviewerUserId = latestReviewerId });
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> Reject(long id, [FromBody] ReasonDto dto)
    {
        var denial = await Allow(ScreenOperation.Unapprove);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto?.Reason))
            return BadRequest(new { message = "سبب الرفض إلزامي." });

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status != "PENDING_APPROVAL")
            return Conflict(new { message = "لا يرفض طلب الصرف إلا من مرحلة انتظار الاعتماد." });
        if (IsCreator(row)) return CreatorSeparationConflict("رفض");

        return await ApplyTransition(row, "REJECTED", "REJECT", dto.Reason);
    }

    [HttpPost("{id:long}/return")]
    public async Task<IActionResult> Return(long id, [FromBody] ReasonDto dto)
    {
        var denial = await Allow(ScreenOperation.Unapprove);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto?.Reason))
            return BadRequest(new { message = "سبب الإرجاع إلزامي." });

        var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
        if (row == null) return NotFound();
        if (row.Status is not ("PENDING_REVIEW" or "PENDING_APPROVAL"))
            return Conflict(new { message = "الحالة الحالية لا تسمح بإرجاع طلب الصرف." });
        if (IsCreator(row)) return CreatorSeparationConflict("إرجاع");

        var action = row.Status == "PENDING_REVIEW" ? "RETURN_FROM_REVIEW" : "RETURN_FROM_APPROVAL";
        return await ApplyTransition(row, "RETURNED", action, dto.Reason,
            new { PreviousStage = row.Status });
    }

    [HttpPost("{id:long}/create-payment-voucher")]
    public async Task<IActionResult> CreatePaymentVoucher(long id, [FromBody] CreateVoucherDto dto)
    {
        var denial = await Allow("PaymentVoucher", ScreenOperation.Add);
        if (denial != null) return denial;
        if (string.IsNullOrWhiteSpace(dto.Cash_Account_ID))
            return BadRequest(new { message = "الصندوق/البنك الدائن مطلوب." });

        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable);
        try
        {
            var session = Session();
            var row = await Scoped().SingleOrDefaultAsync(x => x.Payment_Request_ID == id);
            if (row == null) return NotFound();
            if (IsCreator(row))
                return CreatorSeparationConflict("إنشاء سند صرف من");
            if (row.Status != "APPROVED")
                return Conflict(new { message = "لا ينشأ سند الصرف إلا من طلب معتمد." });
            if (row.Payment_Voucher_ID.HasValue)
                return Conflict(new { message = "تم إنشاء سند صرف لهذا الطلب مسبقاً، ولا يسمح بإنشاء سند ثانٍ." });
            if (!row.Payment_Method_ID.HasValue)
                return Conflict(new { message = "طلب الصرف لا يحتوي طريقة سداد صالحة." });

            var local = row.Details.Sum(x => x.Local_Amount);
            if (local <= 0 || local > row.Approved_Local_Total)
                return Conflict(new { message = "مبلغ السند يجب أن يكون موجباً وألا يتجاوز المبلغ المعتمد." });

            var limit = await FindFinancialLimitAsync(row);
            if (limit != null && local > limit.Limit_Amount - limit.Used_Amount)
                return Conflict(new { message = "السقف المالي لم يعد متاحاً لإنشاء سند الصرف." });

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

            var now = UtcNowAtStoragePrecision();
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
                return BadRequest(new { message = result.Message });
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

            _audit.Add(session, HttpContext, PaymentRequestTable, id.ToString(),
                "CREATE_PAYMENT_VOUCHER", null,
                new { result.VoucherId, result.VoucherNo, local, FinancialLimitId = limit?.Limit_ID });
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { result.VoucherId, result.VoucherNo });
        }
        catch
        {
            await transaction.RollbackAsync();
            return Problem("تعذر إنشاء سند الصرف؛ لم يتم تسجيل أي تعديل.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<IActionResult> ApplyTransition(
        PaymentRequest row,
        string to,
        string action,
        string? reason,
        object? extra = null)
    {
        var before = new { row.Status, row.Updated_By, row.Updated_At };
        var from = row.Status;
        row.Status = to;
        row.Updated_By = Session().User_ID.ToString();
        row.Updated_At = UtcNowAtStoragePrecision();

        _audit.Add(Session(), HttpContext, PaymentRequestTable, row.Payment_Request_ID.ToString(), action,
            before,
            new { From = from, To = to, UserId = Session().User_ID, At = row.Updated_At, Extra = extra },
            reason);
        await _db.SaveChangesAsync();
        return Ok(await ReloadForResponse(row.Payment_Request_ID));
    }

    private async Task<string?> LatestReviewerId(long id) =>
        await _db.Audit_Logs.AsNoTracking()
            .Where(x => x.Table_Name == PaymentRequestTable &&
                        x.Record_ID == id.ToString() &&
                        x.Action_Type == ReviewAction)
            .OrderByDescending(x => x.Action_At)
            .ThenByDescending(x => x.Audit_ID)
            .Select(x => x.User_ID)
            .FirstOrDefaultAsync();

    private async Task<PaymentRequest> ReloadForResponse(long id)
    {
        _db.ChangeTracker.Clear();
        var stored = await Scoped().AsNoTracking()
            .SingleAsync(x => x.Payment_Request_ID == id);
        NormalizeResponseTimestamps(stored);
        return stored;
    }

    private static void NormalizeResponseTimestamps(PaymentRequest row)
    {
        row.Created_At = NormalizeUtcAtStoragePrecision(row.Created_At);
        if (row.Updated_At.HasValue)
            row.Updated_At = NormalizeUtcAtStoragePrecision(row.Updated_At.Value);
    }

    private static DateTime UtcNowAtStoragePrecision() =>
        NormalizeUtcAtStoragePrecision(DateTime.UtcNow);

    private static DateTime NormalizeUtcAtStoragePrecision(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTime(
            utc.Ticks - (utc.Ticks % StoragePrecisionTicks),
            DateTimeKind.Utc);
    }

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

    public sealed class PaymentRequestDto
    {
        public DateTime Request_Date { get; set; } = DateTime.UtcNow;
        public string Beneficiary_Name { get; set; } = string.Empty;
        public string? Party_ID { get; set; }
        public int? Payment_Method_ID { get; set; }
        public string? Header_Reference_No { get; set; }
        public string? Description { get; set; }
        public DateTime? Expected_Last_Modified_At { get; set; }
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
