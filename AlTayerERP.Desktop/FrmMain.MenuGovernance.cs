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
    private static readonly IReadOnlyList<MenuSection> ApprovedSections = new[]
    {
        new MenuSection("الهيكل المؤسسي", new[]
        {
            new MenuItem("TenantGroups", "المجموعات التجارية"),
            new MenuItem("Companies", "الشركات"),
            new MenuItem("Branches", "الفروع"),
            new MenuItem("Countries", "الدول"),
            new MenuItem("Governorates", "المحافظات"),
            new MenuItem("Cities", "المدن")
        }),
        new MenuSection("المستخدمون والصلاحيات", new[]
        {
            new MenuItem("Users", "المستخدمون"),
            new MenuItem("Roles", "الأدوار"),
            new MenuItem("RolePermissions", "صلاحيات الأدوار"),
            new MenuItem("PasswordChange", "تغيير كلمة المرور"),
            new MenuItem("Sessions", "الجلسات النشطة")
        }),
        new MenuSection("الأمن والرقابة", new[]
        {
            new MenuItem("AuditLogs", "سجل التدقيق والرقابة")
        }),
        new MenuSection("التقارير المالية", new[]
        {
            new MenuItem("TrialBalance", "ميزان المراجعة"),
            new MenuItem("GeneralLedger", "الأستاذ العام")
        }),
        new MenuSection("التهيئة والإعدادات", new[]
        {
            new MenuItem("GeneralSettings", "الإعدادات العامة والمالية"),
            new MenuItem("SystemScreens", "كتالوج شاشات النظام"),
            new MenuItem("NumberingSettings", "إعدادات الترقيم"),
            new MenuItem("FiscalYears", "السنوات المالية"),
            new MenuItem("FiscalPeriods", "الفترات المالية"),
            new MenuItem("ExchangeRates", "أسعار الصرف"),
            new MenuItem("PaymentMethods", "طرق السداد"),
            new MenuItem("VoucherTypes", "أنواع السندات"),
            new MenuItem("VoucherStatuses", "حالات السندات"),
            new MenuItem("ApprovalPolicies", "سياسات الاعتماد والسقوف"),
            new MenuItem("FinancialLimits", "السقوف المالية")
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

        var currentSignature = BuildSignature(tree);
        if (tree.Tag is string appliedSignature && appliedSignature == currentSignature)
            return;

        var leaves = CollectLeaves(tree.Nodes)
            .Where(item => !string.IsNullOrWhiteSpace(item.Code))
            .GroupBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        if (leaves.Count == 0)
            return;

        tree.BeginUpdate();
        try
        {
            tree.Nodes.Clear();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var section in ApprovedSections)
            {
                var root = CreateSectionNode(section.Caption);
                foreach (var item in section.Items)
                {
                    if (!leaves.ContainsKey(item.Code) || !used.Add(item.Code))
                        continue;

                    root.Nodes.Add(CreateScreenNode(item.Code, item.Caption));
                }

                if (root.Nodes.Count > 0)
                {
                    root.Expand();
                    tree.Nodes.Add(root);
                }
            }

            // نحافظ على أي شاشة أخرى مسموحة ولا نخفيها، مع منع تكرار Screen_Code.
            var remaining = leaves.Values
                .Where(item => used.Add(item.Code))
                .OrderBy(item => item.Caption, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            if (remaining.Count > 0)
            {
                var other = CreateSectionNode("شاشات أخرى");
                foreach (var item in remaining)
                    other.Nodes.Add(CreateScreenNode(item.Code, item.Caption));
                tree.Nodes.Add(other);
            }

            tree.ShowNodeToolTips = true;
            tree.ItemHeight = 32;
            tree.Indent = 14;
            tree.FullRowSelect = true;
            tree.HideSelection = false;
            tree.HotTracking = true;

            if (tree.Parent is Panel sidePanel)
            {
                sidePanel.Width = Math.Max(sidePanel.Width, 300);
                sidePanel.AutoScroll = false;
            }

            tree.Tag = BuildSignature(tree);
        }
        finally
        {
            tree.EndUpdate();
        }
    }

    private static string BuildSignature(TreeView tree) =>
        string.Join("|", CollectLeaves(tree.Nodes).Select(item => $"{item.Code}:{item.Caption}"));

    private static TreeNode CreateSectionNode(string caption) => new(caption)
    {
        Name = "Section_" + caption,
        ToolTipText = caption,
        NodeFont = new Font("Segoe UI", 10F, FontStyle.Bold),
        ForeColor = Color.FromArgb(210, 228, 245)
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

    private sealed record MenuSection(string Caption, IReadOnlyList<MenuItem> Items);
    private sealed record MenuItem(string Code, string Caption);
    private sealed record MenuLeaf(string Code, string Caption);
}
