namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة القيد اليومي. تستخدم محرك السندات والترحيل نفسه، مع نوع JOURNAL
    /// وصلاحيات مستقلة وشجرة نظام مستقلة.
    /// </summary>
    public sealed class FrmJournalVoucher : FrmReceiptVoucher
    {
        public FrmJournalVoucher() : base("JOURNAL", "القيد اليومي")
        {
        }
    }
}
