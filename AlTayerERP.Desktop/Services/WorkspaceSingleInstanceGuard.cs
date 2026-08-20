using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services;

/// <summary>
/// حارس دفاعي يمنع ظهور شاشة الشركات أو المجموعات التجارية كنافذة مستقلة.
/// المسار المركزي المعتمد يبقى FrmMain.OpenWorkspaceScreen ثم MainWorkspaceManager.
/// </summary>
internal static class WorkspaceSingleInstanceGuard
{
    private static bool _handling;

    [ModuleInitializer]
    internal static void Register()
    {
        Application.Idle += (_, _) => Enforce();
    }

    private static void Enforce()
    {
        if (_handling)
            return;

        var main = Application.OpenForms.OfType<FrmMain>().FirstOrDefault();
        if (main is null || main.IsDisposed)
            return;

        var standalone = Application.OpenForms.Cast<Form>()
            .Where(form => form != main && form.TopLevel)
            .Where(form => form is CompanyForm or FrmTenantGroups)
            .ToList();

        if (standalone.Count == 0)
            return;

        _handling = true;
        try
        {
            foreach (var form in standalone)
            {
                var screenCode = form is CompanyForm ? "Companies" : "TenantGroups";
                form.Hide();
                form.Close();
                main.OpenWorkspaceScreen(screenCode);
            }
        }
        finally
        {
            _handling = false;
        }
    }
}
