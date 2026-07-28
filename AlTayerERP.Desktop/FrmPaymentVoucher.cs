namespace AlTayerERP.Desktop;

/// <summary>
/// شاشة سند الصرف الموحدة.
/// تستخدم كامل تصميم وبرمجة شاشة السند المالي المعتمدة،
/// مع تثبيت نوع السند على PAYMENT وعكس اتجاه القيد محاسبياً.
/// </summary>
public sealed class FrmPaymentVoucher : FrmReceiptVoucher
{
    public FrmPaymentVoucher()
        : base("PAYMENT", "سند الصرف")
    {
    }
}
