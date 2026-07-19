
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;


namespace AlTayerERP.Desktop.Common
{
    /// <summary>
    /// شاشة البحث عن الحسابات.
    ///
    /// تستخدم في:
    /// - سند القبض.
    /// - سند الصرف.
    /// - القيود اليومية.
    /// - أي شاشة تحتاج اختيار حساب.
    /// </summary>
   
    
    
    public partial class FrmAccountLookup : Form
    {
        #region المتغيرات العامة

        /// <summary>
        /// كائن الاتصال المركزي بالـ API.
        /// </summary>
        private readonly HttpClient _client =
            ApiService.Client;
    
        /// <summary>
        /// النص القادم من الشاشة السابقة.
        /// يستخدم للبحث مباشرة.
        /// </summary>
        private readonly string _searchText;

        /// <summary>
        /// جميع الحسابات المحملة.
        /// </summary>
        private List<AccountLookupRow> _accounts =
            new();

        /// <summary>
        /// يمنع تكرار عمليات التحميل.
        /// </summary>
        private bool _isLoading;
        private bool _hasLoadedAccounts;
        #endregion


        #region بيانات الحساب المختار

        /// <summary>
        /// معرف الحساب الحقيقي.
        /// </summary>
        public string SelectedAccountId
        {
            get;
            private set;
        } = string.Empty;

        /// <summary>
        /// رقم الحساب.
        /// </summary>
        public string SelectedAccountCode
        {
            get;
            private set;
        } = string.Empty;

        /// <summary>
        /// اسم الحساب.
        /// </summary>
        public string SelectedAccountName
        {
            get;
            private set;
        } = string.Empty;

        /// <summary>
        /// مجموعة الحساب.
        /// </summary>
        public string SelectedAccountGroup
        {
            get;
            private set;
        } = string.Empty;

        #endregion

        #region المنشئات

        /// <summary>
        /// فتح الشاشة بدون نص بحث.
        /// </summary>
        public FrmAccountLookup()
            : this(string.Empty)
        {
        }
        public FrmAccountLookup(string? searchText)
        {
            InitializeComponent();

            ConfigureAccountsGrid();

            _searchText =
                searchText?.Trim()
                ?? string.Empty;

            RegisterEvents();
        }



        /// <summary>
        /// فتح الشاشة مع نص بحث.
        /// </summary>
        //     public FrmAccountLookup(string? searchText)
        //    {
        //         InitializeComponent();

        //         ConfigureAccountsGrid();

        //         _searchText =
        //            searchText?.Trim()
        //             ?? string.Empty;

        //         RegisterEvents();
        //     }

        #endregion
        #region إعداد جدول الحسابات

        /// <summary>
        /// ربط الأعمدة المصممة مسبقًا ببيانات الحسابات،
        /// ومنع الجدول من إنشاء أعمدة إضافية تلقائيًا.
        /// </summary>
        private void ConfigureAccountsGrid()
        {
            // مهم جدًا: يمنع ظهور أعمدة جديدة بجانب الأعمدة المصممة.
            dgvAccounts.AutoGenerateColumns = false;
            colAccountId.DataPropertyName =

            nameof(AccountLookupRow.Account_ID);
            colAccountId.Visible = false;

            // ربط الأعمدة الموجودة أصلًا في المصمم.
            colAccountCode.DataPropertyName =
                nameof(AccountLookupRow.Account_Code);

            colAccountName.DataPropertyName =
                nameof(AccountLookupRow.Account_Name_AR);

            colAccountGroup.DataPropertyName =
                nameof(AccountLookupRow.Account_Group);
        }

        #endregion




        #region تسجيل الأحداث

        /// <summary>
        /// تسجيل أحداث الشاشة.
        /// </summary>



        private void RegisterEvents()
        {
            txtSearch.TextChanged -= txtSearch_TextChanged;
            txtSearch.TextChanged += txtSearch_TextChanged;

            txtSearch.KeyDown -= txtSearch_KeyDown;
            txtSearch.KeyDown += txtSearch_KeyDown;

            chkSearchAsYouType.CheckedChanged -=
                chkSearchAsYouType_CheckedChanged;

            chkSearchAsYouType.CheckedChanged +=
                chkSearchAsYouType_CheckedChanged;

            dgvAccounts.CellDoubleClick -=
                dgvAccounts_CellDoubleClick;

            dgvAccounts.CellDoubleClick +=
                dgvAccounts_CellDoubleClick;

            dgvAccounts.KeyDown -= dgvAccounts_KeyDown;
            dgvAccounts.KeyDown += dgvAccounts_KeyDown;

            KeyPreview = true;

            KeyDown -= FrmAccountLookup_KeyDown;
            KeyDown += FrmAccountLookup_KeyDown;
        }




        #endregion


        #region تحميل الحسابات

        /// <summary>
        /// عند فتح الشاشة.
        /// </summary>
        //     private async void FrmAccountLookup_Load(
        //         object? sender,
        //         EventArgs e)
        //    {
        //        await LoadAccountsAsync();    
        //    }


        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_hasLoadedAccounts)
            {
                return;
            }

            _hasLoadedAccounts = true;

            // السماح للشاشة بأن ترسم نفسها أولًا.
            await Task.Yield();

            await LoadAccountsAsync();

            if (!IsDisposed && txtSearch.CanFocus)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
            }
        }

        /// <summary>
        /// تحميل الحسابات من الـ API.
        /// </summary>

        private async Task LoadAccountsAsync()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;
            UseWaitCursor = true;

            try
            {
                string companyId =
                    CurrentSession.Company_ID?.Trim()
                    ?? string.Empty;

                if (string.IsNullOrWhiteSpace(companyId))
                {
                    MessageBox.Show(
                        "معرف الشركة غير موجود.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string url =
                    $"ChartOfAccounts/GetLookup?companyId={companyId}";

                using CancellationTokenSource cancellation =
                    new CancellationTokenSource(
                        TimeSpan.FromSeconds(10));

                List<AccountLookupRow>? data =
                    await _client.GetFromJsonAsync<List<AccountLookupRow>>(
                        url,
                        cancellation.Token);

                if (IsDisposed)
                {
                    return;
                }

                _accounts =
                    data ?? new List<AccountLookupRow>();

                txtSearch.Text =
                    _searchText;

                ApplyAccountFilter();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    "انتهت مهلة تحميل الحسابات.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"تعذر الاتصال بخدمة الحسابات.\n\n{ex.Message}",
                    "خطأ في الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "خطأ في تحميل الحسابات",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isLoading = false;
                UseWaitCursor = false;
                Cursor = Cursors.Default;
            }
        }
        #endregion
        #region البحث والتصفية

        /// <summary>
        /// تطبيق البحث على الحسابات.
        ///
        /// البحث يتم بواسطة:
        /// - رقم الحساب.
        /// - اسم الحساب العربي.
        /// - اسم الحساب الإنجليزي.
        /// - مجموعة الحساب.
        /// </summary>
        private void ApplyAccountFilter()
        {
            string searchText =
                txtSearch.Text.Trim();

            IEnumerable<AccountLookupRow> result =
                _accounts;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                result =
                    _accounts.Where(account =>
                        ContainsText(account.Account_Code, searchText) ||
                        ContainsText(account.Account_Name_AR, searchText) ||
                        ContainsText(account.Account_Name_EN, searchText) ||
                        ContainsText(account.Account_Group, searchText));
            }

            dgvAccounts.DataSource =
                result
                    .OrderBy(account => account.Account_Code)
                    .ToList();

            if (dgvAccounts.Rows.Count == 0)
            {
                return;
            }

            dgvAccounts.ClearSelection();

            dgvAccounts.Rows[0].Selected = true;

            if (colAccountCode.Index >= 0 &&
                colAccountCode.Index <
                dgvAccounts.Rows[0].Cells.Count)
            {
                dgvAccounts.CurrentCell =
                    dgvAccounts.Rows[0]
                        .Cells[colAccountCode.Index];
            }
        }
        /// <summary>
        /// البحث أثناء الكتابة.
        /// </summary>
        private void txtSearch_TextChanged(
     object? sender,
     EventArgs e)
        {
            if (_isLoading)
            {
                return;
            }

            if (chkSearchAsYouType.Checked)
            {
                ApplyAccountFilter();
            }
        }

        /// <summary>
        /// الضغط على Enter داخل مربع البحث.
        /// </summary>
        private void txtSearch_KeyDown(
     object? sender,
     KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                MoveFocusToGrid();

                e.Handled = true;
                e.SuppressKeyPress = true;

                return;
            }

            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            ApplyAccountFilter();

            if (dgvAccounts.Rows.Count == 1)
            {
                SelectCurrentAccount();
            }
            else
            {
                MoveFocusToGrid();
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// تغيير خيار البحث أثناء الكتابة.
        /// </summary>
        private void chkSearchAsYouType_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            if (_isLoading)
                return;

            ApplyAccountFilter();
        }

        /// <summary>
        /// نقل المؤشر إلى جدول النتائج.
        /// </summary>
        private void MoveFocusToGrid()
        {
            if (dgvAccounts.Rows.Count == 0)
                return;

            dgvAccounts.Focus();

            dgvAccounts.ClearSelection();

            dgvAccounts.Rows[0].Selected = true;

            dgvAccounts.CurrentCell =
                dgvAccounts.Rows[0]
                .Cells[colAccountCode.Index];

        }

        #endregion

        #region اختيار الحساب

        /// <summary>
        /// اختيار الحساب الحالي.
        /// </summary>
        private void SelectCurrentAccount()
        {
            if (dgvAccounts.CurrentRow == null)
            {
                return;
            }

            AccountLookupRow? account =
                dgvAccounts.CurrentRow.DataBoundItem
                as AccountLookupRow;

            if (account == null)
            {
                return;
            }

            SelectedAccountId =
                account.Account_ID;

            SelectedAccountCode =
                account.Account_Code;

            SelectedAccountName =
                account.Account_Name_AR;
                

            SelectedAccountGroup =
                account.Account_Group;

            DialogResult =
                DialogResult.OK;

            Close();
        }

        /// <summary>
        /// اختيار الحساب بالنقر المزدوج.
        /// </summary>
        private void dgvAccounts_CellDoubleClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            SelectCurrentAccount();

        }

        /// <summary>
        /// اختصارات لوحة المفاتيح داخل الجدول.
        /// </summary>
        private void dgvAccounts_KeyDown(
     object? sender,
     KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:

                    SelectCurrentAccount();

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    break;

                case Keys.Escape:

                    DialogResult =
                        DialogResult.Cancel;

                    Close();

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    break;

                case Keys.F9:

                    txtSearch.Focus();
                    txtSearch.SelectAll();

                    e.Handled = true;
                    e.SuppressKeyPress = true;

                    break;
            }
        }
        /// <summary>
        /// اختصارات الشاشة.
        /// </summary>
        private void FrmAccountLookup_KeyDown(
            object? sender,
            KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape)
            {
                return;
            }

            DialogResult =
                DialogResult.Cancel;

            Close();

            e.Handled = true;

            e.SuppressKeyPress = true;
        }

        #endregion

        #region نموذج بيانات الحساب

        /// <summary>
        /// نموذج بيانات الحساب المستخدم داخل شاشة البحث.
        /// </summary>
        private sealed class AccountLookupRow
        {
            /// <summary>
            /// معرف الحساب الحقيقي.
            /// </summary>
            public string Account_ID { get; set; }
                = string.Empty;

            /// <summary>
            /// رقم الحساب.
            /// </summary>
            public string Account_Code { get; set; }
                = string.Empty;

            /// <summary>
            /// اسم الحساب العربي.
            /// </summary>
            public string Account_Name_AR { get; set; }
                = string.Empty;

            /// <summary>
            /// اسم الحساب الإنجليزي.
            /// </summary>
            public string Account_Name_EN { get; set; }
                = string.Empty;

            /// <summary>
            /// مجموعة الحساب.
            /// </summary>
            public string Account_Group { get; set; }
                = string.Empty;
        }

       // #endregion
        /// <summary>
        /// حدث الضغط على محتوى خلايا الجدول.
        /// لا يوجد منطق مطلوب حاليًا.
        /// </summary>
        private void dataGridView1_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
        }



        #endregion

        /// <summary>
        /// التحقق هل النص يحتوي على قيمة البحث بدون التأثر بحالة الأحرف.
        /// </summary>
        private static bool ContainsText(
            string? sourceText,
            string? searchText)
        {
            if (string.IsNullOrWhiteSpace(sourceText) ||
                string.IsNullOrWhiteSpace(searchText))
            {
                return false;
            }

            return sourceText.Contains(
                searchText.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }


    }

}