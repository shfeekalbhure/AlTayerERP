using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    // تم تغيير اسم الشاشة هنا إلى FrmPaymentVoucher (سند الصرف)
    public partial class FrmPaymentVoucher : Form
    {
        public FrmPaymentVoucher()
        {
            InitializeComponent();

            // تطبيق المظهر العربي الموحد دون تغيير منطق الشاشة.
            ArabicErpFormStyle.Apply(this);
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // تم تعديل اسم الدالة ليتوافق مع اسم الجدول المعتمد في الـ Designer الخاص بك dgvVoucherDetails
        private void dgvVoucherDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // تم تعديل اسم الدالة ليتوافق مع اسم اللوحة المعتمدة في الـ Designer الخاص بك pnlTotals
        private void pnlTotals_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}