namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// عقد اختياري للشاشات التي تحتوي تعديلات لم تحفظ بعد.
    /// يستدعيه مدير مساحة العمل قبل إغلاق تبويب الشاشة أو إغلاق النظام.
    /// </summary>
    public interface IWorkspaceDirtyAware
    {
        /// <summary>يعيد true عندما توجد بيانات عمل عدلت ولم تحفظ.</summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// التنفيذ الافتراضي يمنع الإغلاق غير المقصود عند وجود تعديلات غير محفوظة.
        /// يمكن لأي شاشة تجاوز هذا السلوك عند حاجتها إلى حفظ فعلي قبل الإغلاق.
        /// </summary>
        bool ConfirmWorkspaceClose()
        {
            if (!HasUnsavedChanges) return true;

            var result = MessageBox.Show(
                "توجد تعديلات غير محفوظة. هل تريد تجاهلها وإغلاق الشاشة؟",
                "تعديلات غير محفوظة",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            return result == DialogResult.Yes;
        }
    }
}
