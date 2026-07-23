using System.Runtime.CompilerServices;

namespace AlTayerERP.Desktop;

internal static class GeographicScreensRegistration
{
    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) =>
        {
            if (!CurrentSession.Is_System_Admin) return;

            foreach (Form form in Application.OpenForms)
            {
                if (form is not FrmMain) continue;
                var tree = form.Controls.Find("tvMainMenu", true).OfType<TreeView>().FirstOrDefault();
                if (tree == null || tree.Nodes.Find("GeographicReferences", true).Length > 0) continue;

                var admin = tree.Nodes.Cast<TreeNode>().FirstOrDefault(x => x.Text == "الإدارة العامة");
                if (admin == null) continue;

                var geo = new TreeNode("المراجع الجغرافية") { Name = "GeographicReferences" };
                geo.Nodes.Add("Countries", "الدول");
                geo.Nodes.Add("Governorates", "المحافظات");
                geo.Nodes.Add("Cities", "المدن");
                admin.Nodes.Add(geo);
                admin.Expand();

                tree.NodeMouseDoubleClick += (_, e) =>
                {
                    Form? target = e.Node.Name switch
                    {
                        "Countries" => new FrmCountries(),
                        "Governorates" => new FrmGovernorates(),
                        "Cities" => new FrmCities(),
                        _ => null
                    };
                    target?.ShowDialog(form);
                };
            }
        };
    }
}