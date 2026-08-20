using System.Text.Json;
using AlTayerERP.API.Controllers;
using AlTayerERP.API.Services;
using AlTayerERP.Core.Entities;
using AlTayerERP.Core.Entities.Accounting;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class ApprovalDecisionLogTests
{
    [Fact]
    public async Task DecisionLog_ReturnsDecisionMakerTimestampAndNote()
    {
        await using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"AlTayerERP-approval-decision-log-{Guid.NewGuid():N}")
                .Options);
        context.Approval_Requests.Add(new ApprovalRequest
        {
            Approval_ID = 44,
            Company_ID = "ALTAYER",
            Request_Type = "PAYMENT_REQUEST",
            Status = ApprovalStatus.Approved.ToString()
        });
        context.Users.Add(new User
        {
            User_ID = 22,
            Company_ID = "ALTAYER",
            Branch_ID = 1,
            Role_ID = 1,
            User_Code = "AUD-22",
            Full_Name = "مراجع التدقيق",
            Login_Name = "auditor",
            Password_Hash = "test"
        });
        context.User_Permissions.Add(new UserPermission
        {
            User_ID = 22,
            Permission_Category = "SCREEN",
            Permission_Name = "ApprovalRequests",
            Can_View = true,
            Can_Approve = true
        });
        context.Audit_Logs.Add(new AuditLog
        {
            Table_Name = "approval_requests",
            Record_ID = "44",
            Action_Type = "APPROVE",
            User_ID = "22",
            Action_At = new DateTime(2026, 8, 20, 10, 30, 0, DateTimeKind.Utc),
            Action_Channel = "API",
            Notes = "اعتماد بعد التحقق | Correlation-ID: trace-44"
        });
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-44" };
        httpContext.Items["ServerSession"] = new ServerSession(
            "session-44", 22, 1, false, "ALTAYER", 1, 2026, "device-44",
            DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        var controller = new ApprovalRequestsController(
            context,
            new ScreenAuthorizationService(context),
            new AuditTrailService(context))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = await controller.DecisionLog(44);

        var ok = Assert.IsType<OkObjectResult>(result);
        using var payload = JsonDocument.Parse(JsonSerializer.Serialize(ok.Value));
        var entry = Assert.Single(payload.RootElement.EnumerateArray());

        Assert.Equal("APPROVE", entry.GetProperty("Decision").GetString());
        Assert.Equal("مراجع التدقيق", entry.GetProperty("Decision_By").GetString());
        Assert.Equal(new DateTime(2026, 8, 20, 10, 30, 0, DateTimeKind.Utc),
            entry.GetProperty("Action_At").GetDateTime());
        Assert.Contains("اعتماد بعد التحقق", entry.GetProperty("Decision_Note").GetString());
    }
}
