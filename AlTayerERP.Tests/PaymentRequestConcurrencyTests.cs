using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class PaymentRequestConcurrencyTests
{
    [Fact]
    public async Task PaymentRequest_ConcurrentEdits_RejectTheStaleWriter()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var setup = CreateContext(connection))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Payment_Requests.Add(new PaymentRequest
            {
                Company_ID = "ALTAYER",
                Branch_ID = 1,
                Fiscal_Year_ID = 2026,
                Request_No = "PR-0001",
                Request_Date = new DateTime(2026, 8, 20),
                Beneficiary_Name = "مورد الاختبار",
                Status = "DRAFT",
                Created_By = "10"
            });
            await setup.SaveChangesAsync();
        }

        await using var firstWriter = CreateContext(connection);
        await using var staleWriter = CreateContext(connection);
        var current = await firstWriter.Payment_Requests.SingleAsync();
        var stale = await staleWriter.Payment_Requests.SingleAsync();

        current.Description = "التعديل الصحيح";
        await firstWriter.SaveChangesAsync();

        stale.Description = "تعديل متأخر لا يجوز أن يطغى على التعديل الصحيح";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => staleWriter.SaveChangesAsync());
    }

    private static AppDbContext CreateContext(SqliteConnection connection) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);
}
