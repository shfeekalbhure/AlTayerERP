using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// النموذج الأساسي الموحد لشاشات سطح المكتب.
/// يحتوي إعدادات العرض العامة فقط ولا ينفذ اتصالاً أو منطق أعمال.
/// </summary>
public class BaseForm : Form
{
    protected BaseForm()
    {
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = SystemColors.Control;
        AutoScaleMode = AutoScaleMode.Font;
    }

    /// <summary>
    /// يعيد تطبيق النمط العام عند الحاجة من النماذج المشتقة.
    /// </summary>
    protected void ApplyBaseFormStyle()
    {
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = SystemColors.Control;
        AutoScaleMode = AutoScaleMode.Font;
    }
}
