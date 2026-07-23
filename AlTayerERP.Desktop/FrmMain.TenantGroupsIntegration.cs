using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// يربط شاشة المجموعات التجارية بشجرة النظام دون إبقاء الشاشة القديمة Form1.
/// </summary>
internal static class TenantGroupsMenuIntegration
{
    private const string Marker = "TenantGroupsMenuIntegrated";

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            foreach (var main in Application.OpenForms.OfType<FrmMain>().ToArray())
                Integrate(main);
        };
    }

    private static void Integrate(FrmMain main)
    {
        if (main.Controls.Find(Marker, true).Length > 0)
            return;

        var marker = new Label { Name = Marker, Visible = false };
        main.Controls.Add(marker);

        var tree = FindTree(main.Controls);
        if (tree == null)
            return;

        var administration = tree.Nodes.Cast<TreeNode>()
            .FirstOrDefault(x => string.Equals(x.Text, "الإدارة العامة", StringComparison.Ordinal));
        if (administration == null)
            return;

        var organization = administration.Nodes.Cast<TreeNode>()
            .FirstOrDefault(x => string.Equals(x.Text, "الهيكل المؤسسي", StringComparison.Ordinal));
        if (organization == null)
        {
            organization = new TreeNode("الهيكل المؤسسي") { Name = "OrganizationStructure" };
            administration.Nodes.Insert(0, organization);
        }

        if (!organization.Nodes.ContainsKey("TenantGroups"))
            organization.Nodes.Add("TenantGroups", "المجموعات التجارية");

        organization.Expand();
        administration.Expand();

        tree.NodeMouseDoubleClick += (_, e) =>
        {
            if (string.Equals(e.Node.Name, "TenantGroups", StringComparison.OrdinalIgnoreCase))
                new FrmTenantGroups().ShowDialog(main);
        };
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
                if (nested != null)
                    return nested;
            }
        }
        return null;
    }
}