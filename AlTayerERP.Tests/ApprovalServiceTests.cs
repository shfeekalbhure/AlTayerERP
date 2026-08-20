using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class ApprovalServiceTests
{
    [Fact]
    public async Task CreateApprovalRequestAsync_PersistsPendingRequestAndBusinessContext()
    {
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AlTayerERP-approval-tests-{Guid.NewGuid():N}")
                .Options);
        var service = new ApprovalService(context);

        var created = await service.CreateApprovalRequestAsync(
            companyId: "ALTAYER",
            requestType: "PAYMENT_REQUEST",
            referenceType: "PAYMENT_REQUEST",
            referenceId: "42",
            entityType: "PAYMENT_REQUEST",
            entityId: "42",
            currencyCode: "YER",
            amount: 12500m,
            reason: "اختبار سير الاعتماد",
            requestedBy: "10");

        var stored = await context.Approval_Requests.SingleAsync();
        Assert.Equal(ApprovalStatus.Pending.ToString(), stored.Status);
        Assert.Equal(created.Approval_ID, stored.Approval_ID);
        Assert.Equal("PAYMENT_REQUEST", stored.Entity_Type);
        Assert.Equal("42", stored.Entity_ID);
        Assert.Equal(12500m, stored.Amount);
        Assert.Equal("10", stored.Requested_By);
    }
}
