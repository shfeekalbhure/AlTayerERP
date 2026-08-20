using AlTayerERP.API.Controllers;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class PaymentRequestIdempotencyControllerTests
{
    [Fact]
    public async Task Create_WithRepeatedIdempotencyKey_ReturnsOriginalRequestWithoutDuplicate()
    {
        await using var connection = await OpenDatabaseAsync();
        await using (var setup = CreateContext(connection))
        {
            await SeedAsync(setup);
        }

        var session = CreateSession();
        var dto = CreateDto();
        PaymentRequest firstRequest;

        await using (var firstContext = CreateContext(connection))
        {
            var firstResult = await CreateController(firstContext, session, "payment-request-save-001")
                .Create(dto, CancellationToken.None);

            firstRequest = Assert.IsType<PaymentRequest>(Assert.IsType<OkObjectResult>(firstResult).Value);
        }

        await using (var retryContext = CreateContext(connection))
        {
            var retryResult = await CreateController(retryContext, session, "payment-request-save-001")
                .Create(CreateDto(), CancellationToken.None);

            var retriedRequest = Assert.IsType<PaymentRequest>(Assert.IsType<OkObjectResult>(retryResult).Value);
            Assert.Equal(firstRequest.Payment_Request_ID, retriedRequest.Payment_Request_ID);
            Assert.Equal(firstRequest.Request_No, retriedRequest.Request_No);
            Assert.Equal(1, await retryContext.Payment_Requests.CountAsync());
        }
    }

    [Fact]
    public async Task Create_WithSameIdempotencyKeyAndDifferentPayload_ReturnsConflict()
    {
        await using var connection = await OpenDatabaseAsync();
        await using (var setup = CreateContext(connection))
        {
            await SeedAsync(setup);
        }

        var session = CreateSession();
        await using (var firstContext = CreateContext(connection))
        {
            var firstResult = await CreateController(firstContext, session, "payment-request-save-002")
                .Create(CreateDto(), CancellationToken.None);
            Assert.IsType<OkObjectResult>(firstResult);
        }

        await using (var retryContext = CreateContext(connection))
        {
            var changed = CreateDto();
            changed.Description = "تغيير غير مسموح مع المفتاح نفسه";

            var retryResult = await CreateController(retryContext, session, "payment-request-save-002")
                .Create(changed, CancellationToken.None);

            Assert.IsType<ConflictObjectResult>(retryResult);
            Assert.Equal(1, await retryContext.Payment_Requests.CountAsync());
        }
    }

    private static PaymentRequestsController CreateController(
        AppDbContext context,
        ServerSession session,
        string idempotencyKey)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items["ServerSession"] = session;
        httpContext.Request.Headers["Idempotency-Key"] = idempotencyKey;
        httpContext.TraceIdentifier = "payment-request-test";

        return new PaymentRequestsController(
            context,
            new ScreenAuthorizationService(context),
            new AuditTrailService(context),
            null!,
            new NumberGeneratorService(context),
            new IdempotencyService(context))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static PaymentRequestsController.PaymentRequestDto CreateDto() => new()
    {
        Request_Date = new DateTime(2026, 8, 20),
        Beneficiary_Name = "مورد اختبار",
        Payment_Method_ID = 1,
        Description = "طلب صرف لاختبار منع التكرار",
        Lines =
        [
            new PaymentRequestsController.PaymentRequestLineDto
            {
                Account_ID = "EXP-001",
                Currency_ID = 1,
                Exchange_Rate = 1m,
                Foreign_Amount = 0m,
                Local_Amount = 500m,
                Description = "مصروف اختبار"
            }
        ]
    };

    private static async Task SeedAsync(AppDbContext context)
    {
        context.Companies.Add(new Company
        {
            Company_ID = "ALTAYER",
            Group_ID = "ALTAYER-GROUP",
            Company_Name_AR = "شركة الطائر للاختبار",
            Company_Name_EN = "AlTayer Test Company",
            Company_Prefix = "AT",
            Is_Active = true
        });
        context.User_Permissions.Add(new UserPermission
        {
            User_ID = 10,
            Permission_Category = "SCREEN",
            Permission_Name = "PaymentRequest",
            Can_View = true,
            Can_Add = true
        });
        context.Payment_Methods.Add(new PaymentMethod
        {
            Payment_Method_ID = 1,
            Payment_Method_Code = "CASH",
            Payment_Method_Name_AR = "نقدي",
            Is_Active = true
        });
        context.Fiscal_Periods.Add(new FiscalPeriod
        {
            Branch_ID = 1,
            Fiscal_Year_ID = 2026,
            Start_Date = new DateTime(2026, 1, 1),
            End_Date = new DateTime(2026, 12, 31),
            Is_Active = true,
            Is_Closed = false
        });
        context.Currencies.Add(new Currency
        {
            Currency_ID = 1,
            Company_ID = "ALTAYER",
            Currency_Code = "YER",
            Currency_Name_AR = "ريال يمني",
            Is_Local_Currency = true,
            Is_Active = true
        });
        context.Chart_Of_Accounts.Add(new ChartOfAccount
        {
            Account_ID = "EXP-001",
            Company_ID = "ALTAYER",
            Account_Code = "6100",
            Account_Name_AR = "مصروف اختبار",
            Account_Type = "EXPENSE",
            Account_Level = 1,
            Is_Active = true,
            Is_Postable = true
        });
        context.Numbering_Settings.Add(new NumberingSetting
        {
            Document_Type = "PAYMENT_REQUEST",
            Prefix = "PR",
            Digits_Count = 4,
            Reset_Type = "NEVER",
            Use_Company = true,
            Use_Branch = true,
            Use_Year = true,
            Is_Active = true
        });

        await context.SaveChangesAsync();
    }

    private static async Task<SqliteConnection> OpenDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var setup = CreateContext(connection);
        await setup.Database.EnsureCreatedAsync();
        return connection;
    }

    private static AppDbContext CreateContext(SqliteConnection connection) => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options);

    private static ServerSession CreateSession() => new(
        Session_ID: "payment-request-test-session",
        User_ID: 10,
        Role_ID: 1,
        Is_System_Admin: false,
        Company_ID: "ALTAYER",
        Branch_ID: 1,
        Year_ID: 2026,
        Device_ID: "test-device",
        Issued_At: DateTime.UtcNow,
        Expires_At: DateTime.UtcNow.AddHours(1));
}
