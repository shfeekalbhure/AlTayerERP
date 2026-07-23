using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// تسجيل شاشة المجموعات التجارية في شجرة النظام دون انتظار إعادة بناء FrmMain بالكامل.
/// </summary>
internal static class TenantGroupsScreenRegistration
{
    private static FrmMain? _registeredMain;

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            var main = Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
            if (main == null || ReferenceEquals(main, _registeredMain)) return;

            var tree = FindTree(main.Controls);
            if (tree == null) return;

            var admin = tree.Nodes.Cast<TreeNode>().FirstOrDefault(x => x.Text == "الإدارة العامة");
            if (admin == null)
            {
                admin = new TreeNode("الإدارة العامة");
                tree.Nodes.Insert(0, admin);
            }

            if (!admin.Nodes.ContainsKey("TenantGroups"))
                admin.Nodes.Insert(0, new TreeNode("المجموعات التجارية") { Name = "TenantGroups" });

            tree.NodeMouseDoubleClick += (_, e) =>
            {
                if (e.Node.Name == "TenantGroups")
                    new FrmTenantGroups().ShowDialog(main);
            };

            admin.Expand();
            _registeredMain = main;
        };
    }

    private static TreeView? FindTree(Control.ControlCollection controls)
    {
        foreach (Control control in controls)
        {
            if (control is TreeView tree) return tree;
            if (control.HasChildren)
            {
                var found = FindTree(control.Controls);
                if (found != null) return found;
            }
        }
        return null;
    }
}