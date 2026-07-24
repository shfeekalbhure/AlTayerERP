using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop;

public sealed partial class FrmTenantGroups
{
    /// <summary>يمنع إغلاق شاشة المجموعات عند وجود تعديل غير محفوظ إلا بقرار صريح من المستخدم.</summary>
    public bool ConfirmWorkspaceClose()
    {
        if (!HasUnsavedChanges) return true;

        var result = MessageBox.Show(
            "توجد تعديلات غير محفوظة في شاشة المجموعات التجارية. هل تريد تجاهلها وإغلاق الشاشة؟",
            "تعديلات غير محفوظة",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        return result == DialogResult.Yes;
    }
}
