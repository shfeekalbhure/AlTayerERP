using System;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// سند الصرف. يعيد استخدام محرك سندات المرحلة الأولى عبر API فقط،
    /// مع تثبيت نوع السند PAYMENT ومنع تغييره من الواجهة.
    /// </summary>
    public sealed class FrmPaymentVoucher : FrmReceiptVoucher
    {
        public FrmPaymentVoucher() : base("PAYMENT", "سند الصرف")
        {
        }
    }
}
