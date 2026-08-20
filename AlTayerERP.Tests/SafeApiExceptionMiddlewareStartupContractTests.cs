using Xunit;

namespace AlTayerERP.Tests;

public sealed class SafeApiExceptionMiddlewareStartupContractTests
{
    [Fact]
    public void Safe_exception_middleware_uses_a_resolvable_typed_logger()
    {
        var root = FindRepositoryRoot();
        var middlewarePath = Path.Combine(
            root,
            "AlTayerERP.API",
            "Middleware",
            "SafeApiExceptionMiddleware.cs");
        var middlewareSource = File.ReadAllText(middlewarePath);

        Assert.Contains("ILogger<SafeApiExceptionMiddleware>", middlewareSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ILogger logger", middlewareSource, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AlTayerERP.sln")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("تعذر تحديد جذر مستودع AlTayerERP لاختبار عقد بدء وسيط الاستثناءات.");
    }
}
