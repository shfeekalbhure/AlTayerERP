using Xunit;

namespace AlTayerERP.Tests;

public sealed class ReleaseCiContractTests
{
    [Fact]
    public void Ci_MustValidateReleaseBuildAndTests_ForTheActiveWorkBranch()
    {
        var root = FindSolutionRoot();
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "ci.yml"));

        Assert.Contains("agent/unified-phase1-screens", workflow, StringComparison.Ordinal);
        Assert.Contains("--configuration Release", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test", workflow, StringComparison.Ordinal);
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
