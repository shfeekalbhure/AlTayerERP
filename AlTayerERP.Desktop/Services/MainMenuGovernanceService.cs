using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// ينظم شجرة النظام اعتماداً على Screen_Code الموجود فعلياً، دون إنشاء شاشات
/// أو ربط أحداث فتح إضافية. هدفه منع التكرار وتوحيد ترتيب الأقسام بالعربية.
/// </summary>
internal static class MainMenuGovernanceService
{
    private const string AppliedMarker = "MainMenuGovernanceApplied";

    private static readonly (string Title, string[] Codes)[] Sections =
    {
        ("الهيكل المؤسسي", new[] { "TenantGroups", "Companies", "Branches", "Countries", "Governorates", "Cities" }),
        ("المستخدمون والصلاحيات", new[] { "Users", "Roles", "RolePermissions", "PasswordChange", "Sessions" }),
        ("الأمن والرقابة", new[] { "AuditLogs" }),
        ("التقارير المالية", new[] { "TrialBalance", "GeneralLedger" }),
        ("التهيئة والإعدادات", new[]
        {
            "GeneralSettings", "SystemScreens", "NumberingSettings", "FiscalYears", "FiscalPeriods",
            "ExchangeRates", "PaymentMethods", "VoucherTypes", "VoucherStatuses", "ApprovalPolicies", "FinancialLimits"
        })
    };

    /// <summary>
    /// يراقب اكتمال بناء الشجرة فقط، ولا يربط NodeMouseClick أو DoubleClick أو Show.
    /// عند إعادة بناء القائمة بسبب البحث يعاد تطبيق الحوكمة مرة واحدة على المحتوى الجديد.
    /// </summary>
    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var main in Application.OpenForms.OfType<FrmMain>().ToArray())
            {
                var tree = FindTree(main.Controls);
                if (tree is null || tree.Nodes.Count == 0)
                    continue;

                var signature = BuildSignature(tree.Nodes);
                if (string.Equals(tree.Tag?.ToString(), AppliedMarker + signature, StringComparison.Ordinal))
                    continue;

                Apply(tree);
                tree.Tag = AppliedMarker + BuildSignature(tree.Nodes);
            }
        };
    }

    /// <summary>يعيد بناء الأقسام المطلوبة من العقد المسموح بها مع إبقاء كل كود مرة واحدة.</summary>
    public static void Apply(TreeView tree)
    {
        if (tree.IsDisposed)
            return;

        var leaves = EnumerateLeaves(tree.Nodes)
            .Where(node => !string.IsNullOrWhiteSpace(node.Name))
            .GroupBy(node => node.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => CloneLeaf(group.First()), StringComparer.OrdinalIgnoreCase);

        tree.BeginUpdate();
        try
        {
            tree.Nodes.Clear();
            foreach (var section in Sections)
            {
                var root = CreateSectionNode(section.Title);
                foreach (var code in section.Codes)
                {
                    if (leaves.Remove(code, out var leaf))
                        root.Nodes.Add(leaf);
                }

                if (root.Nodes.Count > 0)
                {
                    tree.Nodes.Add(root);
                    root.Expand();
                }
            }

            // الشاشات الأخرى المصرح بها تبقى ضمن قسم واضح دون تكرار.
            if (leaves.Count > 0)
            {
                var other = CreateSectionNode("شاشات أخرى");
                foreach (var leaf in leaves.Values.OrderBy(node => node.Text, StringComparer.CurrentCulture))
                    other.Nodes.Add(leaf);
                tree.Nodes.Add(other);
                other.Expand();
            }

            ConfigureAppearance(tree);
        }
        finally
        {
            tree.EndUpdate();
        }
    }

    private static TreeNode CreateSectionNode(string title) => new(title)
    {
        Name = "Section_" + title,
        NodeFont = new Font("Segoe UI", 9.5F, FontStyle.Bold),
        ForeColor = Color.White,
        ToolTipText = title
    };

    private static TreeNode CloneLeaf(TreeNode source) => new(source.Text)
    {
        Name = source.Name,
        Tag = source.Tag,
        ToolTipText = string.IsNullOrWhiteSpace(source.ToolTipText) ? source.Text : source.ToolTipText,
        NodeFont = new Font("Segoe UI", 9F, FontStyle.Regular),
        ForeColor = Color.FromArgb(235, 242, 250)
    };

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

    private static string BuildSignature(TreeNodeCollection nodes) =>
        string.Join("|", EnumerateLeaves(nodes)
            .Where(node => !string.IsNullOrWhiteSpace(node.Name))
            .Select(node => node.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase));

    private static void ConfigureAppearance(TreeView tree)
    {
        tree.ShowNodeToolTips = true;
        tree.ItemHeight = 31;
        tree.Indent = 18;
        tree.FullRowSelect = true;
        tree.HideSelection = false;
        tree.HotTracking = true;
        tree.RightToLeft = RightToLeft.Yes;

        // توسيع الحاوية يمنع قص الأسماء الطويلة ويزيل الحاجة للتمرير الأفقي غالباً.
        if (tree.Parent is Panel sidePanel)
            sidePanel.Width = Math.Max(sidePanel.Width, 300);
    }

    private static TreeView? FindTree(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is TreeView tree)
                return tree;
            if (!control.HasChildren)
                continue;

            var nested = FindTree(control.Controls);
            if (nested is not null)
                return nested;
        }

        return null;
    }
}
