using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// الجزء المسؤول عن اللمسات النهائية وتجربة الاستخدام الموحدة لشاشة الشركات.
    /// تم فصله عن منطق الاتصال بالـ API حتى تبقى الشاشة سهلة الصيانة.
    /// </summary>
    public partial class CompanyForm
    {
        private TextBox? _txtQuickSearch;
        private bool _unifiedUiInitialized;
        private int _localPrintCount;

        /// <summary>
        /// تهيئة الشاشة بعد اكتمال إنشاء عناصرها وإظهارها للمستخدم.
        /// </summary>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_unifiedUiInitialized)
                return;

            _unifiedUiInitialized = true;
            KeyPreview = true;

            InitializeQuickSearch();
            InitializeGridAppearance();
            InitializeActionStates();
            ResetAuditView();

            dgvCompanies.SelectionChanged += DgvCompanies_SelectionChanged;
            dgvCompanies.CellDoubleClick += DgvCompanies_CellDoubleClick;
            KeyDown += CompanyForm_KeyDown;
        }

        /// <summary>
        /// إضافة مربع البحث الفوري أعلى قائمة الشركات.
        /// </summary>
        private void InitializeQuickSearch()
        {
            lblListTitle.Dock = DockStyle.Top;
            lblListTitle.TextAlign = ContentAlignment.MiddleRight;

            _txtQuickSearch = new TextBox
            {
                Name = "txtQuickSearch",
                Dock = DockStyle.Bottom,
                Height = 29,
                Font = new Font("Segoe UI", 9.5F),
                PlaceholderText = "ابحث بالاسم أو الرمز أو الهاتف...",
                RightToLeft = RightToLeft.Yes,
                Margin = new Padding(0)
            };

            _txtQuickSearch.TextChanged += TxtQuickSearch_TextChanged;
            pnlListHeader.Height = 82;
            pnlListHeader.Controls.Add(_txtQuickSearch);
            _txtQuickSearch.BringToFront();
        }

        /// <summary>
        /// تنسيق جدول الشركات ليطابق الهوية البصرية الموحدة للنظام.
        /// </summary>
        private void InitializeGridAppearance()
        {
            dgvCompanies.EnableHeadersVisualStyles = false;
            dgvCompanies.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(31, 78, 121);
            dgvCompanies.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCompanies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCompanies.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvCompanies.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvCompanies.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 228, 242);
            dgvCompanies.DefaultCellStyle.SelectionForeColor = Color.FromArgb(25, 45, 65);
            dgvCompanies.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvCompanies.GridColor = Color.FromArgb(220, 226, 232);
            dgvCompanies.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            if (dgvCompanies.Columns.Contains("Company_ID"))
                dgvCompanies.Columns["Company_ID"].Visible = false;

            if (dgvCompanies.Columns.Contains("Company_Name_AR"))
                dgvCompanies.Columns["Company_Name_AR"].FillWeight = 150;

            if (dgvCompanies.Columns.Contains("Company_Name_EN"))
                dgvCompanies.Columns["Company_Name_EN"].FillWeight = 120;

            if (dgvCompanies.Columns.Contains("Address"))
                dgvCompanies.Columns["Address"].Visible = false;

            if (dgvCompanies.Columns.Contains("Email"))
                dgvCompanies.Columns["Email"].Visible = false;
        }

        /// <summary>
        /// ضبط حالة الأزرار عند فتح الشاشة دون اختيار شركة.
        /// </summary>
        private void InitializeActionStates()
        {
            btnSaveCompany.Enabled = true;
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnApprove.Enabled = false;
            btnPreview.Enabled = false;
            btnPrint.Enabled = true;
        }

        /// <summary>
        /// البحث الفوري داخل القائمة المحملة دون تنفيذ طلب جديد على الخادم.
        /// </summary>
        private void TxtQuickSearch_TextChanged(object? sender, EventArgs e)
        {
            string keyword = _txtQuickSearch?.Text.Trim() ?? string.Empty;

            if (keyword.Length == 0)
            {
                PopulateGrid(_originalCompaniesList);
                return;
            }

            var filtered = _originalCompaniesList
                .Where(company =>
                    ContainsText(company.Company_Name_AR, keyword) ||
                    ContainsText(company.Company_Name_EN, keyword) ||
                    ContainsText(company.Company_Prefix, keyword) ||
                    ContainsText(company.Phone, keyword) ||
                    ContainsText(company.Email, keyword))
                .ToList();

            PopulateGrid(filtered);
        }

        private static bool ContainsText(string? source, string keyword)
        {
            return !string.IsNullOrWhiteSpace(source) &&
                   source.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// تفعيل الإجراءات المرتبطة بالسجل عند تحديد صف من الجدول.
        /// </summary>
        private void DgvCompanies_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = dgvCompanies.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnApprove.Enabled = hasSelection;
            btnPreview.Enabled = hasSelection;
        }

        /// <summary>
        /// النقر المزدوج ينقل التركيز مباشرة إلى بيانات الشركة للتعديل السريع.
        /// </summary>
        private void DgvCompanies_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            txtCompanyNameAr.Focus();
            txtCompanyNameAr.SelectAll();
        }

        /// <summary>
        /// اختصارات موحدة لجميع العمليات الأساسية في الشاشة.
        /// </summary>
        private void CompanyForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                btnNew.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                btnSaveCompany.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.F)
            {
                _txtQuickSearch?.Focus();
                _txtQuickSearch?.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.P)
            {
                _localPrintCount++;
                lblPrintCount.Text = _localPrintCount.ToString();
                btnPrint.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                btnRefresh.PerformClick();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnClose.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// إعادة حقول التدقيق إلى حالتها الافتراضية عند إنشاء سجل جديد.
        /// القيم الحقيقية ستعرض عند توفيرها من استجابة الـ API.
        /// </summary>
        private void ResetAuditView()
        {
            lblCreatedBy.Text = "—";
            lblCreatedAt.Text = "—";
            lblModifiedBy.Text = "—";
            lblModifiedAt.Text = "—";
            lblEditCount.Text = "0";
            lblPrintCount.Text = _localPrintCount.ToString();
        }
    }
}
