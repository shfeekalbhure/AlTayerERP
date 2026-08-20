using AlTayerERP.API.DTOs.Accounting;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class VoucherValidationServiceTests
{
    [Fact]
    public async Task RejectsVoucherWhenDebitAndCreditTotalsAreNotBalanced()
    {
        await using var context = CreateContext();
        var service = new VoucherValidationService(context);
        var voucher = CreateVoucher(
            debitAmount: 100m,
            creditAmount: 90m);

        var result = await service.ValidateAsync(voucher);

        Assert.False(result.IsValid);
        Assert.Contains("إجمالي المدين", result.ErrorMessage);
    }

    [Fact]
    public async Task RejectsLineWhenItContainsDebitAndCreditTogether()
    {
        await using var context = CreateContext();
        var service = new VoucherValidationService(context);
        var voucher = CreateVoucher(
            debitAmount: 100m,
            creditAmount: 100m,
            firstLineCreditAmount: 10m);

        var result = await service.ValidateAsync(voucher);

        Assert.False(result.IsValid);
        Assert.Contains("مدينًا ودائنًا معًا", result.ErrorMessage);
    }

    [Fact]
    public async Task RejectsVoucherWhenLineNumbersAreDuplicated()
    {
        await using var context = CreateContext();
        var service = new VoucherValidationService(context);
        var voucher = CreateVoucher(
            debitAmount: 100m,
            creditAmount: 100m,
            duplicateLineNumbers: true);

        var result = await service.ValidateAsync(voucher);

        Assert.False(result.IsValid);
        Assert.Contains("أرقام سطور السند", result.ErrorMessage);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AlTayerERP-tests-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static CreateFinancialVoucherDto CreateVoucher(
        decimal debitAmount,
        decimal creditAmount,
        decimal firstLineCreditAmount = 0m,
        bool duplicateLineNumbers = false)
    {
        return new CreateFinancialVoucherDto
        {
            Voucher_Type_ID = 1,
            Voucher_Status_ID = 1,
            Branch_ID = "1",
            Fiscal_Year_ID = 1,
            Cash_Account_ID = "CASH",
            Received_From_Name = "اختبار",
            Currency_ID = 1,
            Against_Text = "اختبار",
            Details =
            [
                new CreateFinancialVoucherDetailDto
                {
                    Line_No = 1,
                    Account_ID = "CASH",
                    Currency_ID = 1,
                    Local_Amount = debitAmount,
                    Debit_Amount = debitAmount,
                    Credit_Amount = firstLineCreditAmount,
                    Line_Type = 1
                },
                new CreateFinancialVoucherDetailDto
                {
                    Line_No = duplicateLineNumbers ? 1 : 2,
                    Account_ID = "REVENUE",
                    Currency_ID = 1,
                    Local_Amount = creditAmount,
                    Debit_Amount = 0m,
                    Credit_Amount = creditAmount,
                    Line_Type = 2
                }
            ]
        };
    }
}
