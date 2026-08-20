using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class FinancialVoucherConcurrencyTests
{
    [Fact]
    public async Task FinancialVoucher_ConcurrentEdits_RejectTheStaleWriter()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using (var setup = CreateContext(connection))
        {
            await setup.Database.EnsureCreatedAsync();
            setup.Financial_Voucher_Headers.Add(new FinancialVoucherHeader
            {
                Voucher_No = "RV-0001",
                Voucher_Type_ID = 1,
                Voucher_Status_ID = 1,
                Branch_ID = "MAIN",
                Fiscal_Year_ID = 2026,
                Voucher_Date = new DateTime(2026, 8, 20),
                Transaction_Date = new DateTime(2026, 8, 20),
                Cash_Account_ID = "CASH-001",
                Currency_ID = 1,
                Requires_Approval = false,
                Approval_Status = 0,
                Created_By = "10"
            });
            await setup.SaveChangesAsync();
        }

        await using var firstWriter = CreateContext(connection);
        await using var staleWriter = CreateContext(connection);
        var current = await firstWriter.Financial_Voucher_Headers.SingleAsync();
        var stale = await staleWriter.Financial_Voucher_Headers.SingleAsync();

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
