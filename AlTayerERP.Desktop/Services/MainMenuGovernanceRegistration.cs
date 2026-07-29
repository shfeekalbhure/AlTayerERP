using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// يطبق حوكمة الشجرة بعد تحميل الصلاحيات. لا يربط NodeMouseClick ولا يفتح أي شاشة.
/// </summary>
internal static class MainMenuGovernanceRegistration
{
    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var main in Application.OpenForms.OfType<FrmMain>().ToArray())
            {
                var tree = FindTree(main.Controls);
                if (tree is null || tree.Nodes.Count == 0 || !NeedsGovernance(tree))
                    continue;

                MainMenuGovernanceService.Apply(tree);
            }
        };
    }

    private static bool NeedsGovernance(TreeView tree)
    {
        var leaves = EnumerateLeaves(tree.Nodes).ToList();
        var hasDuplicateCode = leaves
            .Where(node => !string.IsNullOrWhiteSpace(node.Name))
            .GroupBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (hasDuplicateCode)
            return true;

        var expectedSections = new[]
        {
            "الهيكل المؤسسي", "المستخدمون والصلاحيات", "الأمن والرقابة",
            "التقارير المالية", "التهيئة والإعدادات"
        };
        var actualSections = tree.Nodes.Cast<TreeNode>().Select(node => node.Text).ToArray();
        if (!actualSections.Take(expectedSections.Length).SequenceEqual(expectedSections, StringComparer.Ordinal))
            return true;

        var fiscalYears = leaves.FirstOrDefault(node => node.Name.Equals("FiscalYears", StringComparison.OrdinalIgnoreCase));
        return fiscalYears?.Parent?.Text != "التهيئة والإعدادات";
    }

    private static IEnumerable<TreeNode> EnumerateLeaves(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Nodes.Count == 0)
            {
                yield return node;
                continue;
            }

            foreach (var child in EnumerateLeaves(node.Nodes))
                yield return child;
        }
    }

    private static TreeView? FindTree(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is TreeView tree)
                return tree;

            if (control.HasChildren)
            {
                var nested = FindTree(control.Controls);
                if (nested is not null)
                    return nested;
            }
        }

        return null;
    }
}
