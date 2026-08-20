using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services;

/// <summary>
/// ينسق حماية إعادة الإرسال للعمليات الحساسة. يعتمد على فهرس قاعدة بيانات فريد
/// وليس على الذاكرة، لذلك يظل صحيحاً عند تعدد مثيلات الخادم أو إعادة تشغيله.
/// </summary>
public sealed class IdempotencyService
{
    private readonly AppDbContext _context;

    public IdempotencyService(AppDbContext context) => _context = context;

    public async Task<IdempotencyBeginResult> BeginAsync(
        ServerSession session,
        string operation,
        string idempotencyKey,
        string requestFingerprint,
        CancellationToken cancellationToken = default)
    {
        var key = idempotencyKey.Trim();
        var existing = await FindAsync(session, operation, key, cancellationToken);
        if (existing != null)
            return Interpret(existing, requestFingerprint);

        var record = new IdempotencyRecord
        {
            Operation = operation,
            Idempotency_Key = key,
            Company_ID = session.Company_ID,
            Branch_ID = session.Branch_ID.ToString(),
            Fiscal_Year_ID = session.Year_ID,
            User_ID = session.User_ID.ToString(),
            Request_Fingerprint = requestFingerprint,
            Status = IdempotencyRecord.InProgress,
            Created_At = DateTime.UtcNow
        };

        _context.Idempotency_Records.Add(record);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return new(IdempotencyBeginState.New, record);
        }
        catch (DbUpdateException)
        {
            // إذا سبقنا طلب متزامن بالمفتاح نفسه، فلا ننشئ مستنداً ثانياً.
            // نقرأ سجل الفائز بعد أن تحسم قاعدة البيانات الفهرس الفريد.
            _context.ChangeTracker.Clear();
            existing = await FindAsync(session, operation, key, cancellationToken);
            if (existing != null)
                return Interpret(existing, requestFingerprint);

            throw;
        }
    }

    public async Task CompleteAsync(
        IdempotencyRecord record,
        long resourceId,
        string resourceNo,
        CancellationToken cancellationToken = default)
    {
        if (record.Status != IdempotencyRecord.InProgress)
            throw new InvalidOperationException("لا يمكن إكمال سجل عدم تكرار غير نشط.");

        record.Status = IdempotencyRecord.Completed;
        record.Resource_ID = resourceId;
        record.Resource_No = resourceNo;
        record.Completed_At = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private Task<IdempotencyRecord?> FindAsync(
        ServerSession session,
        string operation,
        string key,
        CancellationToken cancellationToken) =>
        _context.Idempotency_Records
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Operation == operation &&
                x.Idempotency_Key == key &&
                x.Company_ID == session.Company_ID &&
                x.Branch_ID == session.Branch_ID.ToString() &&
                x.Fiscal_Year_ID == session.Year_ID &&
                x.User_ID == session.User_ID.ToString(), cancellationToken);

    private static IdempotencyBeginResult Interpret(
        IdempotencyRecord existing,
        string requestFingerprint)
    {
        if (!string.Equals(existing.Request_Fingerprint, requestFingerprint, StringComparison.Ordinal))
            return new(IdempotencyBeginState.PayloadMismatch, existing);

        return existing.Status == IdempotencyRecord.Completed
            ? new(IdempotencyBeginState.Completed, existing)
            : new(IdempotencyBeginState.InProgress, existing);
    }
}

public enum IdempotencyBeginState
{
    New,
    Completed,
    InProgress,
    PayloadMismatch
}

public sealed record IdempotencyBeginResult(
    IdempotencyBeginState State,
    IdempotencyRecord Record);
