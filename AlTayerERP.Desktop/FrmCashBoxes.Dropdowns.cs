using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    public partial class FrmCashBoxes
    {
        /// <summary>
        /// يضبط سلوك منسدلات شاشة الصناديق.
        /// تستدعى من نقطة التهيئة الموحدة في FrmCashBoxes.Completion.cs.
        /// </summary>
        private void ConfigureCashBoxDropdowns()
        {
            // الفرع مرتبط بفرع الجلسة، لكنه يبقى ظاهرًا وقابلاً لفتح القائمة
            // حتى لا يبدو للمستخدم أن المنسدلة متوقفة.
            cmbBranch.Enabled = true;
            cmbBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccount.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbBranch.IntegralHeight = false;
            cmbCurrency.IntegralHeight = false;
            cmbAccount.IntegralHeight = false;

            cmbBranch.DropDownHeight = 180;
            cmbCurrency.DropDownHeight = 220;
            cmbAccount.DropDownHeight = 260;

            cmbBranch.MaxDropDownItems = 8;
            cmbCurrency.MaxDropDownItems = 10;
            cmbAccount.MaxDropDownItems = 12;
        }
    }
}
