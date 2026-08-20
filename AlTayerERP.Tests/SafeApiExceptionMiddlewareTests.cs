using AlTayerERP.API.Controllers;
using AlTayerERP.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class SafeApiExceptionMiddlewareTests
{
    [Fact]
    public async Task UnexpectedException_ReturnsSafeResponseWithCorrelationId()
    {
        var middlewareType = typeof(FinancialVoucherController).Assembly.GetType(
            "AlTayerERP.API.Middleware.SafeApiExceptionMiddleware");
        Assert.NotNull(middlewareType);

        var middleware = Activator.CreateInstance(
            middlewareType!,
            new RequestDelegate(_ => throw new InvalidOperationException("internal database detail")),
            NullLogger<SafeApiExceptionMiddleware>.Instance);
        Assert.NotNull(middleware);

        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-safe-error-001";
        context.Response.Body = new MemoryStream();

        var invoke = middlewareType!.GetMethod("InvokeAsync");
        Assert.NotNull(invoke);
        await (Task)invoke!.Invoke(middleware, [context])!;

        context.Response.Body.Position = 0;
        var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("trace-safe-error-001", response);
        Assert.DoesNotContain("internal database detail", response, StringComparison.Ordinal);
    }
}
