using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AlTayerERP.Tests;

public sealed class VoucherPostingGuardMiddlewareTests
{
    [Fact]
    public async Task RejectsPostingWhenVoucherDoesNotExist()
    {
        await using var db = CreateContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var context = CreateContextForRequest(HttpMethods.Post, "/api/FinancialVoucher/999/post");
        var middleware = new VoucherPostingGuardMiddleware(next);

        await middleware.InvokeAsync(context, db);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.False(nextCalled);
        Assert.Contains("السند المالي غير موجود", await ReadResponseAsync(context));
    }

    [Theory]
    [InlineData("GET", "/api/FinancialVoucher/999/post")]
    [InlineData("POST", "/api/FinancialVoucher/999")]
    public async Task PassesThroughRequestsThatAreNotPostingRequests(string method, string path)
    {
        await using var db = CreateContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var context = CreateContextForRequest(method, path);
        var middleware = new VoucherPostingGuardMiddleware(next);

        await middleware.InvokeAsync(context, db);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AlTayerERP-middleware-tests-{Guid.NewGuid():N}")
            .Options;

        return new AppDbContext(options);
    }

    private static DefaultHttpContext CreateContextForRequest(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
