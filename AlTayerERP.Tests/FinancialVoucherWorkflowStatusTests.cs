using AlTayerERP.API.Controllers;
using AlTayerERP.API.Services;
using AlTayerERP.API.Services.Accounting;
using AlTayerERP.API.Services.Accounting.VoucherWorkflow;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class FinancialVoucherWorkflowStatusTests
{
    [Fact]
    public async Task WorkflowStatus_ForPermittedVoucher_IsNotRejectedBeforeVoucherTypeIsResolved()
    {
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"altayer-voucher-workflow-{Guid.NewGuid():N}")
                .Options);

        context.Voucher_Types.Add(new VoucherType
        {
            Voucher_Type_ID = 1,
            Voucher_Type_Code = "RECEIPT",
            Voucher_Type_Name_AR = "سند قبض",
            Is_Active = true
        });
        context.User_Permissions.Add(new UserPermission
        {
            User_ID = 10,
            Permission_Category = "SCREEN",
            Permission_Name = "ReceiptVoucher",
            Can_View = true
        });
        context.Financial_Voucher_Headers.Add(new FinancialVoucherHeader
        {
            Voucher_ID = 7001,
            Voucher_No = "RV-7001",
            Voucher_Type_ID = 1,
            Voucher_Status_ID = 1,
            Branch_ID = "1",
            Fiscal_Year_ID = 2026,
            Voucher_Date = new DateTime(2026, 8, 20),
            Transaction_Date = new DateTime(2026, 8, 20),
            Cash_Account_ID = "CASH-001",
            Currency_ID = 1,
            Exchange_Rate = 1m,
            Amount = 100m,
            Foreign_Total = 100m,
            Local_Total = 100m,
            Requires_Approval = true,
            Is_Active = true
        });
        await context.SaveChangesAsync();

        var audit = new VoucherAuditService(context);
        var controller = new FinancialVoucherController(
            new FinancialVoucherService(context, null!),
            null!,
            new VoucherApprovalService(context, audit),
            new VoucherPostingService(context, audit),
            new ScreenAuthorizationService(context),
            null!,
            context,
            NullLogger<FinancialVoucherController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext()
            }
        };

        var result = await controller.GetWorkflowStatus(7001);

        Assert.IsNotType<ForbidResult>(result);
        Assert.IsType<OkObjectResult>(result);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Items["ServerSession"] = new ServerSession(
            Session_ID: "voucher-workflow-test",
            User_ID: 10,
            Role_ID: 1,
            Is_System_Admin: false,
            Company_ID: "ALTAYER",
            Branch_ID: 1,
            Year_ID: 2026,
            Device_ID: "test-device",
            Issued_At: DateTime.UtcNow,
            Expires_At: DateTime.UtcNow.AddHours(1));
        return context;
    }
}
