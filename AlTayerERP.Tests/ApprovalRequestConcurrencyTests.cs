using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class ApprovalRequestConcurrencyTests
{
    [Fact]
    public async Task CompetingApprovalDecisions_SecondSaveIsRejectedByOptimisticConcurrency()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"altayer-approval-concurrency-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath};Default Timeout=30;Pooling=False")
            .Options;

        try
        {
            await using (var setup = new AppDbContext(options))
            {
                await setup.Database.EnsureCreatedAsync();
                setup.Approval_Requests.Add(new ApprovalRequest
                {
                    Company_ID = "ALTAYER",
                    Request_Type = "PAYMENT_REQUEST",
                    Reference_Type = "PAYMENT_REQUEST",
                    Reference_ID = "9001",
                    Status = ApprovalStatus.Pending.ToString(),
                    Requested_By = "100"
                });
                await setup.SaveChangesAsync();
            }

            await using var firstContext = new AppDbContext(options);
            await using var secondContext = new AppDbContext(options);
            var first = await firstContext.Approval_Requests.SingleAsync();
            var second = await secondContext.Approval_Requests.SingleAsync();

            first.Status = ApprovalStatus.Approved.ToString();
            await firstContext.SaveChangesAsync();

            second.Status = ApprovalStatus.Rejected.ToString();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());
        }
        finally
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }
}
