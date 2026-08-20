using Xunit;

namespace AlTayerERP.Tests;

public sealed class SecureTransportClientContractTests
{
    [Fact]
    public void Production_Clients_MustDefaultToHttps_AndLimitHttpToDebugBuilds()
    {
        var root = FindSolutionRoot();
        var mobile = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Mobile.Office", "Services", "ApiClientConfiguration.cs"));
        var desktop = File.ReadAllText(Path.Combine(root,
            "AlTayerERP.Desktop", "Services.cs"));

        Assert.Contains("ApiScheme = \"https\"", mobile, StringComparison.Ordinal);
        Assert.Contains("#if DEBUG", mobile, StringComparison.Ordinal);
        Assert.Contains("https://", desktop, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("#if DEBUG", desktop, StringComparison.Ordinal);
    }

    private static string FindSolutionRoot()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current != null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "AlTayerERP.sln")))
                return current.FullName;
        }

        throw new DirectoryNotFoundException("تعذر العثور على جذر حل AlTayerERP.");
    }
}
