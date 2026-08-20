using AlTayerERP.API.DTOs;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class PaymentRequestErrorContractTests
{
    [Fact]
    public void ApiErrorResponse_PreservesLegacyMessageAndAddsMachineReadableFields()
    {
        var response = new ApiErrorResponse(
            "VALIDATION_ERROR",
            "بيانات طلب الصرف غير مكتملة.",
            "trace-payment-request-001");

        Assert.False(response.Success);
        Assert.Equal("بيانات طلب الصرف غير مكتملة.", response.Message);
        Assert.Equal("VALIDATION_ERROR", response.Code);
        Assert.Equal("trace-payment-request-001", response.CorrelationId);
    }

    [Fact]
    public void PaymentRequestBusinessErrors_UseCompatibleErrorEnvelope()
    {
        var sourcePath = Path.Combine(FindRepositoryRoot(), "AlTayerERP.API", "Controllers", "PaymentRequestsController.cs");
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("new ApiErrorResponse(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BadRequest(new { message", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Conflict(new { message", source, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عقد أخطاء طلبات الصرف.");
    }
}
