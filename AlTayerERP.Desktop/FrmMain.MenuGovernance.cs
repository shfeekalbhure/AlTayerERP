using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// حوكمة عرض شجرة النظام فقط. لا تضيف أي حدث فتح ولا تنشئ أي نافذة؛
/// فتح الشاشات يبقى حصرياً عبر FrmMain.OpenScreen وMainWorkspaceManager.
/// </summary>
internal static class MainMenuGovernance
{
    private const string MarkerName = "MainMenuGovernanceApplied";

    private static readonly (string Section, string[] Codes)[] ApprovedSections =
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

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var main in Application.OpenForms.OfType<FrmMain>().ToArray())
                Apply(main);
        };
    }

    private static void Apply(FrmMain main)
    {
        var tree = FindTree(main.Controls);
        if (tree is null || tree.Nodes.Count == 0)
            return;

        // يعاد التطبيق بعد كل إعادة بناء للشجرة، لكن لا يعاد إذا كانت البنية الحالية محكومة.
        if (tree.Tag is string value && value == MarkerName)
            return;

        var leaves = CollectLeaves(tree.Nodes)
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        if (leaves.Count == 0)
            return;

        tree.BeginUpdate();
        try
        {
            tree.Nodes.Clear();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (section, codes) in ApprovedSections)
            {
                var root = CreateSectionNode(section);
                foreach (var code in codes)
                {
                    if (!leaves.TryGetValue(code, out var item) || !used.Add(code))
                        continue;

                    root.Nodes.Add(CreateScreenNode(item.Code, item.Caption));
                }

                if (root.Nodes.Count > 0)
                {
                    root.Expand();
                    tree.Nodes.Add(root);
                }
            }

            // لا نخفي أي شاشة أخرى مسموحة؛ تجمع في قسم مستقل بعد الأقسام المعتمدة.
            var remaining = leaves.Values
                .Where(x => used.Add(x.Code))
                .OrderBy(x => x.Caption, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            if (remaining.Count > 0)
            {
                var other = CreateSectionNode("شاشات أخرى");
                foreach (var item in remaining)
                    other.Nodes.Add(CreateScreenNode(item.Code, item.Caption));
                tree.Nodes.Add(other);
            }

            tree.ShowNodeToolTips = true;
            tree.ItemHeight = 31;
            tree.Indent = 18;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.Tag = MarkerName;

            // عرض أوسع يمنع قص المسميات الطويلة ويقلل ظهور التمرير الأفقي.
            if (tree.Parent is Control parent)
                parent.Width = Math.Max(parent.Width, 300);
        }
        finally
        {
            tree.EndUpdate();
        }
    }

    private static TreeNode CreateSectionNode(string caption) => new(caption)
    {
        Name = "Section_" + caption,
        ToolTipText = caption,
        NodeFont = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.White
    };

    private static TreeNode CreateScreenNode(string code, string caption) => new(caption)
    {
        Name = code,
        ToolTipText = caption,
        NodeFont = new Font("Segoe UI", 9.2F, FontStyle.Regular),
        ForeColor = Color.White
    };

    private static List<MenuLeaf> CollectLeaves(TreeNodeCollection nodes)
    {
        var result = new List<MenuLeaf>();
        foreach (TreeNode node in nodes)
        {
            if (node.Nodes.Count == 0)
            {
                result.Add(new MenuLeaf(node.Name, node.Text));
                continue;
            }

            result.AddRange(CollectLeaves(node.Nodes));
        }
        return result;
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

    private sealed record MenuLeaf(string Code, string Caption);
}
