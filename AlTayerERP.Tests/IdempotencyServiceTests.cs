using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class IdempotencyServiceTests
{
    [Fact]
    public async Task SameKeyAndPayload_ReturnsOriginalCompletedResult()
    {
        await using var context = CreateContext();
        var service = new IdempotencyService(context);
        var session = CreateSession();

        var first = await service.BeginAsync(session, "FINANCIAL_VOUCHER_CREATE", "save-001", "payload-a");
        Assert.Equal(IdempotencyBeginState.New, first.State);

        await service.CompleteAsync(first.Record, 91, "REC-000091");

        var retry = await service.BeginAsync(session, "FINANCIAL_VOUCHER_CREATE", "save-001", "payload-a");

        Assert.Equal(IdempotencyBeginState.Completed, retry.State);
        Assert.Equal(91, retry.Record.Resource_ID);
        Assert.Equal("REC-000091", retry.Record.Resource_No);
    }

    [Fact]
    public async Task SameKeyWithDifferentPayload_IsRejected()
    {
        await using var context = CreateContext();
        var service = new IdempotencyService(context);
        var session = CreateSession();

        _ = await service.BeginAsync(session, "FINANCIAL_VOUCHER_CREATE", "save-002", "payload-a");
        var retry = await service.BeginAsync(session, "FINANCIAL_VOUCHER_CREATE", "save-002", "payload-b");

        Assert.Equal(IdempotencyBeginState.PayloadMismatch, retry.State);
    }

    [Fact]
    public async Task SameKeyInAnotherUserScope_IsIndependent()
    {
        await using var context = CreateContext();
        var service = new IdempotencyService(context);

        var first = await service.BeginAsync(
            CreateSession(userId: 10), "FINANCIAL_VOUCHER_CREATE", "save-003", "payload-a");
        var otherUser = await service.BeginAsync(
            CreateSession(userId: 11), "FINANCIAL_VOUCHER_CREATE", "save-003", "payload-a");

        Assert.Equal(IdempotencyBeginState.New, first.State);
        Assert.Equal(IdempotencyBeginState.New, otherUser.State);
    }

    [Fact]
    public async Task RelationalUniqueIndex_RejectsDuplicateScopeAcrossContexts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setup = new AppDbContext(options))
        {
            await setup.Database.EnsureCreatedAsync();
        }

        var session = CreateSession();
        await using (var firstContext = new AppDbContext(options))
        {
            var service = new IdempotencyService(firstContext);
            var first = await service.BeginAsync(session, "FINANCIAL_VOUCHER_CREATE", "relational-001", "payload-a");
            Assert.Equal(IdempotencyBeginState.New, first.State);
        }

        await using var duplicateContext = new AppDbContext(options);
        duplicateContext.Idempotency_Records.Add(new IdempotencyRecord
        {
            Operation = "FINANCIAL_VOUCHER_CREATE",
            Idempotency_Key = "relational-001",
            Company_ID = session.Company_ID,
            Branch_ID = session.Branch_ID.ToString(),
            Fiscal_Year_ID = session.Year_ID,
            User_ID = session.User_ID.ToString(),
            Request_Fingerprint = "payload-a",
            Status = IdempotencyRecord.InProgress,
            Created_At = DateTime.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => duplicateContext.SaveChangesAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AlTayerERP-idempotency-tests-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static ServerSession CreateSession(int userId = 10) => new(
        Session_ID: Guid.NewGuid().ToString("N"),
        User_ID: userId,
        Role_ID: 1,
        Is_System_Admin: false,
        Company_ID: "ALTAYER",
        Branch_ID: 1,
        Year_ID: 2026,
        Device_ID: "test-device",
        Issued_At: DateTime.UtcNow,
        Expires_At: DateTime.UtcNow.AddHours(1));
}
