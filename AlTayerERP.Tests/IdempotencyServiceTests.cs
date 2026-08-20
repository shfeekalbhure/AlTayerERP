using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
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
