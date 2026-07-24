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
        /// تطلب الشاشة من المستخدم حفظ/تجاهل/إلغاء الإغلاق.
        /// تعيد true فقط عند السماح بإغلاق التبويب.
        /// </summary>
        bool ConfirmWorkspaceClose();
    }
}