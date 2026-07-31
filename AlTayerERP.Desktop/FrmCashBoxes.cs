using AlTayerERP.Desktop.Models;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة تعريف الصناديق وربطها بالحسابات والعملات ضمن نطاق جلسة المستخدم.
    /// جميع الحركات المالية تبقى في السندات ودفتر الأستاذ، ولا تنشأ من شاشة التعريف.
    /// </summary>
    public partial class FrmCashBoxes : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly ErrorProvider _errors = new();
        private readonly ToolTip _toolTip = new();

        private readonly List<CashBoxModel> _cashBoxes = new();
        private string _selectedCashBoxId = string.Empty;
        private bool _isBinding;
        private bool _isBusy;
        private bool _masterDataReady;
        private bool _isNewMode;
        private bool _isEditMode;
        private bool _canAdd;
        private bool _canEdit;
        private bool _canDeactivate;
        private bool _canReactivate;
        private bool _canPrint;
        private int _printRowIndex;
        private Button? _btnReactivate;
        private Label? _lblCurrentBalance;
        private GroupBox? _grpAudit;
        private Label? _lblCreatedAudit;
        private Label? _lblUpdatedAudit;
        private Label? _lblCountersAudit;

        public FrmCashBoxes()
        {
            InitializeComponent();
            ConfigureScreen();
            WireEvents();
        }

        #region نماذج المنسدلات

        public sealed class BranchCashLookup
        {
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = string.Empty;
        }

        public sealed class CurrencyCashLookup
        {
            public string Currency_Code { get; set; } = string.Empty;
            public string Currency_Name_AR { get; set; } = string.Empty;
            public string Display_Name => string.IsNullOrWhiteSpace(Currency_Name_AR)
                ? Currency_Code
                : $"{Currency_Code} - {Currency_Name_AR}";
        }

        public sealed class AccountCashLookup
        {
            public string Account_ID { get; set; } = string.Empty;
            public string Account_Code { get; set; } = string.Empty;
            public string Account_Name_AR { get; set; } = string.Empty;
            public string Display_Name { get; set; } = string.Empty;
        }

        public sealed class CashBoxPermissions
        {
            public bool Can_Add { get; set; }
            public bool Can_Edit { get; set; }
            public bool Can_Deactivate { get; set; }
            public bool Can_Reactivate { get; set; }
            public bool Can_Print { get; set; }
        }

        private sealed class CashBoxLookupsResponse
        {
            public List<BranchCashLookup>? Branches { get; set; }
            public List<CurrencyCashLookup>? Currencies { get; set; }
            public List<AccountCashLookup>? Accounts { get; set; }
            public CashBoxPermissions? Permissions { get; set; }
        }

        private sealed class CashBoxAuditSummary
        {
            public string? Created_By { get; set; }
            public DateTime? Created_At { get; set; }
            public string? Updated_By { get; set; }
            public DateTime? Updated_At { get; set; }
            public int Edit_Count { get; set; }
            public int Print_Count { get; set; }
        }

        #endregion

        #region التهيئة

        private void ConfigureScreen()
        {
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            KeyPreview = true;
            StartPosition = FormStartPosition.CenterParent;

            _errors.ContainerControl = this;
            _errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            cmbBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            ConfigureDropdown(cmbBranch, 180, 8);
            ConfigureDropdown(cmbCurrency, 220, 10);
            ConfigureDropdown(cmbAccount, 260, 12);
            cmbBranch.Enabled = false;

            txtCashBoxCode.ReadOnly = true;
            txtCashBoxCode.TabStop = false;
            txtCashBoxNameAR.MaxLength = 200;
            txtCashBoxNameEN.MaxLength = 200;
            txtNotes.MaxLength = 1000;
            txtSearch.MaxLength = 200;

            ConfigureMoney(numOpeningBalance);
            ConfigureMoney(numMinimumLimit);
            ConfigureMoney(numMaximumLimit);
            numOpeningBalance.Enabled = false;
            chkIsActive.Enabled = false;
            dgvCashBoxCurrencies.Visible = false;

            txtCashBoxNameAR.TabIndex = 0;
            txtCashBoxNameEN.TabIndex = 1;
            cmbBranch.TabIndex = 2;
            cmbCurrency.TabIndex = 3;
            cmbAccount.TabIndex = 4;
            numMinimumLimit.TabIndex = 5;
            numMaximumLimit.TabIndex = 6;
            txtNotes.TabIndex = 7;

            txtCashBoxCode.AccessibleName = "كود الصندوق";
            txtCashBoxNameAR.AccessibleName = "اسم الصندوق بالعربية";
            txtCashBoxNameEN.AccessibleName = "اسم الصندوق بالإنجليزية";
            cmbBranch.AccessibleName = "الفرع";
            cmbCurrency.AccessibleName = "عملة الصندوق";
            cmbAccount.AccessibleName = "الحساب المالي للصندوق";
            numOpeningBalance.AccessibleName = "الرصيد الافتتاحي";
            numMinimumLimit.AccessibleName = "الحد الأدنى";
            numMaximumLimit.AccessibleName = "الحد الأعلى";
            txtNotes.AccessibleName = "ملاحظات الصندوق";
            dgvCashBoxes.AccessibleName = "قائمة الصناديق";

            btnDelete.Text = "إيقاف";
            AcceptButton = btnSave;
            CancelButton = btnClose;

            SetupGrid();
            CreateCurrentBalanceLabel();
            CreateReactivateButton();
            CreateAuditPanel();

            _toolTip.SetToolTip(numOpeningBalance,
                "الرصيد الافتتاحي ينشأ من مستند أرصدة افتتاحية أو قيد مرحل، وليس من شاشة تعريف الصندوق.");
            _toolTip.SetToolTip(cmbAccount,
                "اختر الحساب المالي النقدي النهائي الذي ستُرحّل عليه حركات الصندوق.");
            _toolTip.SetToolTip(cmbCurrency,
                "يمكن تغيير العملة فقط قبل وجود أول حركة مالية مرحلة.");
        }

        private void WireEvents()
        {
            btnNew.Click += btnNew_Click;
            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += (_, _) => ApplySearch();
            btnRefresh.Click += btnRefresh_Click;
            btnPrint.Click += btnPrint_Click;
            btnClose.Click += (_, _) => Close();

            dgvCashBoxes.CellClick += dgvCashBoxes_CellClick;
            dgvCashBoxes.CellFormatting += dgvCashBoxes_CellFormatting;
            dgvCashBoxes.DataBindingComplete += (_, _) => dgvCashBoxes.ClearSelection();
            dgvCashBoxes.SelectionChanged += (_, _) => UpdateActionState();

            txtSearch.TextChanged += (_, _) => ApplySearch();
            txtCashBoxNameAR.TextChanged += (_, _) => _errors.SetError(txtCashBoxNameAR, string.Empty);
            cmbCurrency.SelectedIndexChanged += (_, _) => _errors.SetError(cmbCurrency, string.Empty);
            cmbAccount.SelectedIndexChanged += cmbAccount_SelectedIndexChanged;
            numMinimumLimit.ValueChanged += (_, _) => ValidateCashBoxLimitsInline();
            numMaximumLimit.ValueChanged += (_, _) => ValidateCashBoxLimitsInline();

            KeyDown += FrmCashBoxes_KeyDown;
            FormClosing += FrmCashBoxes_FormClosing;
        }

        private static void ConfigureDropdown(ComboBox combo, int height, int items)
        {
            combo.IntegralHeight = false;
            combo.DropDownHeight = height;
            combo.MaxDropDownItems = items;
        }

        private static void ConfigureMoney(NumericUpDown control)
        {
            control.DecimalPlaces = 2;
            control.ThousandsSeparator = true;
            control.Minimum = 0m;
            control.Maximum = 999999999999m;
            control.TextAlign = HorizontalAlignment.Left;
        }

        private void SetupGrid()
        {
            dgvCashBoxes.AutoGenerateColumns = false;
            dgvCashBoxes.AllowUserToAddRows = false;
            dgvCashBoxes.AllowUserToDeleteRows = false;
            dgvCashBoxes.AllowUserToResizeRows = false;
            dgvCashBoxes.ReadOnly = true;
            dgvCashBoxes.MultiSelect = false;
            dgvCashBoxes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCashBoxes.RowHeadersVisible = false;
            dgvCashBoxes.RowTemplate.Height = 30;
            dgvCashBoxes.ColumnHeadersHeight = 36;
            dgvCashBoxes.StandardTab = true;
        }

        private void CreateCurrentBalanceLabel()
        {
            if (_lblCurrentBalance != null)
                return;

            _lblCurrentBalance = new Label
            {
                Name = "lblCurrentBalance",
                Text = "الرصيد الدفتري الحالي: 0.00",
                AutoSize = false,
                Height = 28,
                Width = Math.Max(220, grpAdditionalData.ClientSize.Width - 36),
                Left = 18,
                Top = Math.Max(20, grpAdditionalData.ClientSize.Height - 38),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font(Font, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 78, 121)
            };
            grpAdditionalData.Controls.Add(_lblCurrentBalance);
            _lblCurrentBalance.BringToFront();
        }

        private void CreateReactivateButton()
        {
            if (_btnReactivate != null)
                return;

            _btnReactivate = new Button
            {
                Name = "btnReactivateCashBox",
                Text = "إعادة تفعيل",
                Size = btnDelete.Size,
                Location = btnDelete.Location,
                Font = btnDelete.Font,
                BackColor = Color.FromArgb(230, 247, 237),
                ForeColor = Color.FromArgb(20, 108, 67),
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            _btnReactivate.FlatAppearance.BorderColor = Color.FromArgb(82, 183, 136);
            _btnReactivate.Click += btnReactivate_Click;

            btnDelete.Parent?.Controls.Add(_btnReactivate);
            _btnReactivate.BringToFront();
        }

        /// <summary>يعرض بيانات الإنشاء والتعديل والعدادات أسفل الشاشة من دون جعلها قابلة للتعديل.</summary>
        private void CreateAuditPanel()
        {
            if (_grpAudit != null)
                return;

            _grpAudit = new GroupBox
            {
                Name = "grpCashBoxAudit",
                Text = "بيانات الإنشاء والتعديل والعدادات",
                Dock = DockStyle.Bottom,
                Height = 102,
                RightToLeft = RightToLeft.Yes,
                Padding = new Padding(10)
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                RightToLeft = RightToLeft.Yes
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            _lblCreatedAudit = CreateAuditCard("بيانات الإنشاء");
            _lblUpdatedAudit = CreateAuditCard("بيانات التعديل");
            _lblCountersAudit = CreateAuditCard("العدادات");
            layout.Controls.Add(_lblCreatedAudit, 0, 0);
            layout.Controls.Add(_lblUpdatedAudit, 1, 0);
            layout.Controls.Add(_lblCountersAudit, 2, 0);
            _grpAudit.Controls.Add(layout);
            Controls.Add(_grpAudit);
            Controls.SetChildIndex(_grpAudit, 0);
            UpdateAuditPanel(null);
        }

        private static Label CreateAuditCard(string title) => new()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Text = title,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(10, 6, 10, 6),
            Margin = new Padding(5),
            ForeColor = Color.FromArgb(31, 78, 121)
        };

        #endregion

        #region التحميل والمنسدلات

        private async void FrmCashBoxes_Load(object? sender, EventArgs e)
        {
            await ReloadScreenAsync();
        }

        private async Task ReloadScreenAsync()
        {
            try
            {
                SetBusy(true);
                await LoadUnifiedLookupsAsync();
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("تعذر تحميل شاشة الصناديق", ex);
            }
            finally
            {
                SetBusy(false);
                UpdateActionState();
            }
        }

        private async Task LoadUnifiedLookupsAsync()
        {
            var response = await _client.GetAsync($"{_baseUrl}CashBoxes/Lookups");
            if (!response.IsSuccessStatusCode)
            {
                await ShowApiErrorAsync(response, "تعذر تحميل بيانات الصناديق المرجعية");
                BindEmptyLookups();
                return;
            }

            var lookups = await response.Content.ReadFromJsonAsync<CashBoxLookupsResponse>();
            if (lookups == null)
            {
                BindEmptyLookups();
                return;
            }

            var branches = lookups.Branches ?? new List<BranchCashLookup>();
            var currencies = lookups.Currencies ?? new List<CurrencyCashLookup>();
            var accounts = lookups.Accounts ?? new List<AccountCashLookup>();
            var permissions = lookups.Permissions ?? new CashBoxPermissions();

            _isBinding = true;
            try
            {
                cmbBranch.DataSource = branches;
                cmbBranch.DisplayMember = nameof(BranchCashLookup.Branch_Name);
                cmbBranch.ValueMember = nameof(BranchCashLookup.Branch_ID);
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;

                cmbCurrency.DataSource = currencies;
                cmbCurrency.DisplayMember = nameof(CurrencyCashLookup.Display_Name);
                cmbCurrency.ValueMember = nameof(CurrencyCashLookup.Currency_Code);
                cmbCurrency.SelectedIndex = -1;

                cmbAccount.DataSource = accounts;
                cmbAccount.DisplayMember = nameof(AccountCashLookup.Display_Name);
                cmbAccount.ValueMember = nameof(AccountCashLookup.Account_ID);
                cmbAccount.SelectedIndex = -1;
            }
            finally
            {
                _isBinding = false;
            }

            _masterDataReady = branches.Count > 0 && currencies.Count > 0 && accounts.Count > 0;
            _canAdd = permissions.Can_Add;
            _canEdit = permissions.Can_Edit;
            _canDeactivate = permissions.Can_Deactivate;
            _canReactivate = permissions.Can_Reactivate;
            _canPrint = permissions.Can_Print;
            SetReferenceErrors(branches.Count, currencies.Count, accounts.Count);
        }

        private void BindEmptyLookups()
        {
            cmbBranch.DataSource = new List<BranchCashLookup>();
            cmbCurrency.DataSource = new List<CurrencyCashLookup>();
            cmbAccount.DataSource = new List<AccountCashLookup>();
            _masterDataReady = false;
            _canAdd = _canEdit = _canDeactivate = _canReactivate = _canPrint = false;
            SetReferenceErrors(0, 0, 0);
        }

        private void SetReferenceErrors(int branchCount, int currencyCount, int accountCount)
        {
            _errors.SetError(cmbBranch, branchCount == 0 ? "فرع الجلسة غير متاح أو موقوف." : string.Empty);
            _errors.SetError(cmbCurrency, currencyCount == 0 ? "لا توجد عملات فعالة للشركة الحالية." : string.Empty);
            _errors.SetError(cmbAccount, accountCount == 0
                ? "لا يوجد حساب صناديق أب نشط وتجميعي. عرّف الحساب في دليل الحسابات ثم اضغط تحديث."
                : string.Empty);
        }

        private async Task LoadCashBoxesAsync()
        {
            var response = await _client.GetAsync($"{_baseUrl}CashBoxes");
            if (!response.IsSuccessStatusCode)
            {
                await ShowApiErrorAsync(response, "تعذر تحميل الصناديق");
                return;
            }

            var data = await response.Content.ReadFromJsonAsync<List<CashBoxModel>>() ?? new();
            _cashBoxes.Clear();
            _cashBoxes.AddRange(data);
            ApplySearch();
        }

        #endregion

        #region العمليات

        private async void btnNew_Click(object? sender, EventArgs e)
        {
            if (!_masterDataReady)
            {
                MessageBox.Show("استكمل الفرع والعملة وحساب الصناديق الأب أولًا.", "بيانات مرجعية ناقصة",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ClearForm();
            _isNewMode = true;
            UpdateActionState();
            await GenerateNextCodeAsync();
            txtCashBoxNameAR.Focus();
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (!_masterDataReady || (!_isNewMode && !_isEditMode) || !ValidateForm())
                return;

            if (_isNewMode)
                await ExecuteWriteAsync(
                    () => _client.PostAsJsonAsync($"{_baseUrl}CashBoxes", BuildRequest()),
                    "تم حفظ الصندوق بنجاح.", "تعذر حفظ الصندوق");
            else
                await ExecuteWriteAsync(
                    () => _client.PutAsJsonAsync($"{_baseUrl}CashBoxes/{_selectedCashBoxId}", BuildRequest()),
                    "تم تعديل الصندوق بنجاح.", "تعذر تعديل الصندوق");
        }

        private async void btnEdit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقًا من الجدول أولًا.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _isEditMode = true;
            UpdateActionState();
            txtCashBoxNameAR.Focus();
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقًا من الجدول أولًا.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var reason = PromptReason("سبب إيقاف الصندوق", "أدخل سبب الإيقاف:");
            if (string.IsNullOrWhiteSpace(reason))
                return;

            if (MessageBox.Show("سيتم إيقاف الصندوق وحسابه المرتبط دون حذف تاريخه. هل تريد المتابعة؟",
                    "تأكيد الإيقاف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                SetBusy(true);
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{_baseUrl}CashBoxes/{_selectedCashBoxId}")
                {
                    Content = JsonContent.Create(new { Reason = reason })
                };
                var response = await _client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, "تعذر إيقاف الصندوق");
                    return;
                }

                MessageBox.Show("تم إيقاف الصندوق بنجاح.", "الصناديق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("حدث خطأ أثناء إيقاف الصندوق", ex);
            }
            finally
            {
                SetBusy(false);
                UpdateActionState();
            }
        }

        private async void btnReactivate_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
                return;

            var reason = PromptReason("سبب إعادة التفعيل", "أدخل سبب إعادة تفعيل الصندوق:");
            if (string.IsNullOrWhiteSpace(reason))
                return;

            try
            {
                SetBusy(true);
                var response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}CashBoxes/{_selectedCashBoxId}/reactivate",
                    new { Reason = reason });
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, "تعذر إعادة تفعيل الصندوق");
                    return;
                }

                MessageBox.Show("تمت إعادة تفعيل الصندوق وحسابه المرتبط بنجاح.", "الصناديق",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("حدث خطأ أثناء إعادة التفعيل", ex);
            }
            finally
            {
                SetBusy(false);
                UpdateActionState();
            }
        }

        private async void btnRefresh_Click(object? sender, EventArgs e) =>
            await ReloadScreenAsync();

        private async Task ExecuteWriteAsync(Func<Task<HttpResponseMessage>> operation, string successMessage, string errorTitle)
        {
            try
            {
                SetBusy(true);
                var response = await operation();
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, errorTitle);
                    return;
                }

                MessageBox.Show(successMessage, "الصناديق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError(errorTitle, ex);
            }
            finally
            {
                SetBusy(false);
                UpdateActionState();
            }
        }

        #endregion

        #region اختيار السجل والبحث

        private async void dgvCashBoxes_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCashBoxes.Rows[e.RowIndex].DataBoundItem is not CashBoxModel row)
                return;

            _isBinding = true;
            try
            {
                _selectedCashBoxId = row.ID;
                _isNewMode = false;
                _isEditMode = false;
                txtCashBoxCode.Text = row.Code;
                txtCashBoxNameAR.Text = row.NameAR;
                txtCashBoxNameEN.Text = row.NameEN ?? string.Empty;
                cmbBranch.SelectedValue = row.Branch_ID;
                cmbCurrency.SelectedValue = row.Currency_Code;
                cmbAccount.SelectedValue = row.Account_ID;
                SetNumericValue(numOpeningBalance, row.Opening_Balance);
                SetNumericValue(numMinimumLimit, row.Min_Limit);
                SetNumericValue(numMaximumLimit, row.Max_Limit);
                chkIsActive.Checked = row.IsActive;
                txtNotes.Text = row.Notes ?? string.Empty;

                cmbAccount.Enabled = !row.Has_Posted_Movement;
                cmbCurrency.Enabled = !row.Has_Posted_Movement;
                UpdateBalanceLabel(row);
            }
            finally
            {
                _isBinding = false;
            }

            UpdateActionState();
            await LoadAuditSummaryAsync(row.ID);
        }

        private void ApplySearch()
        {
            var text = txtSearch.Text.Trim();
            var rows = string.IsNullOrWhiteSpace(text)
                ? _cashBoxes.ToList()
                : _cashBoxes.Where(x =>
                    Contains(x.Code, text) || Contains(x.NameAR, text) || Contains(x.NameEN, text) ||
                    Contains(x.Account_Name_AR, text) || Contains(x.Currency_Code, text)).ToList();

            dgvCashBoxes.DataSource = null;
            dgvCashBoxes.DataSource = rows;
        }

        private void dgvCashBoxes_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCashBoxes.Rows[e.RowIndex].DataBoundItem is not CashBoxModel row)
                return;

            var style = dgvCashBoxes.Rows[e.RowIndex].DefaultCellStyle;
            style.ForeColor = row.IsActive ? SystemColors.ControlText : Color.DimGray;
            style.BackColor = row.IsActive ? Color.White : Color.FromArgb(245, 245, 245);

            var property = dgvCashBoxes.Columns[e.ColumnIndex].DataPropertyName;
            if (property == nameof(CashBoxModel.Is_Active))
            {
                e.Value = row.Status_Name;
                e.FormattingApplied = true;
            }
            if (property is "Opening_Balance" or "Current_Balance" or "Max_Limit" or "Min_Limit" && e.Value is decimal amount)
            {
                e.Value = amount.ToString("N2");
                e.FormattingApplied = true;
            }
        }

        #endregion

        #region التحقق والحالة

        private CashBoxModel BuildRequest() => new()
        {
            Company_ID = CurrentSession.Company_ID,
            Branch_ID = CurrentSession.Branch_ID,
            Code = txtCashBoxCode.Text.Trim(),
            NameAR = txtCashBoxNameAR.Text.Trim(),
            NameEN = NullIfWhiteSpace(txtCashBoxNameEN.Text),
            Account_ID = cmbAccount.SelectedValue?.ToString() ?? string.Empty,
            Currency_Code = cmbCurrency.SelectedValue?.ToString() ?? string.Empty,
            Opening_Balance = 0m,
            Min_Limit = numMinimumLimit.Value,
            Max_Limit = numMaximumLimit.Value,
            IsActive = true,
            Notes = NullIfWhiteSpace(txtNotes.Text)
        };

        private bool ValidateForm()
        {
            _errors.Clear();
            var valid = true;

            if (string.IsNullOrWhiteSpace(txtCashBoxNameAR.Text))
            {
                _errors.SetError(txtCashBoxNameAR, "اسم الصندوق العربي مطلوب.");
                valid = false;
            }
            if (cmbAccount.SelectedValue == null)
            {
                _errors.SetError(cmbAccount, "اختر الحساب المالي للصندوق.");
                valid = false;
            }
            if (cmbCurrency.SelectedValue == null)
            {
                _errors.SetError(cmbCurrency, "اختر عملة الصندوق.");
                valid = false;
            }
            if (numMinimumLimit.Value > numMaximumLimit.Value)
            {
                ValidateCashBoxLimitsInline();
                valid = false;
            }

            var duplicate = _cashBoxes.Any(x => x.ID != _selectedCashBoxId &&
                string.Equals(x.NameAR?.Trim(), txtCashBoxNameAR.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                _errors.SetError(txtCashBoxNameAR, "يوجد صندوق آخر بالاسم نفسه في الفرع الحالي.");
                valid = false;
            }

            if (!valid)
                MessageBox.Show("راجع الحقول المعلّمة ثم أعد المحاولة.", "بيانات غير مكتملة",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return valid;
        }

        private void ValidateCashBoxLimitsInline()
        {
            var message = numMinimumLimit.Value > numMaximumLimit.Value
                ? "الحد الأدنى يجب ألا يتجاوز الحد الأعلى."
                : string.Empty;
            _errors.SetError(numMinimumLimit, message);
            _errors.SetError(numMaximumLimit, message);
        }

        private void ClearForm()
        {
            _isBinding = true;
            try
            {
                _isNewMode = false;
                _isEditMode = false;
                _selectedCashBoxId = string.Empty;
                txtCashBoxCode.Clear();
                txtCashBoxNameAR.Clear();
                txtCashBoxNameEN.Clear();
                txtNotes.Clear();
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;
                cmbCurrency.SelectedIndex = -1;
                cmbAccount.SelectedIndex = -1;
                cmbAccount.Enabled = true;
                cmbCurrency.Enabled = true;
                numOpeningBalance.Value = 0m;
                numMinimumLimit.Value = 0m;
                numMaximumLimit.Value = 0m;
                chkIsActive.Checked = true;
                dgvCashBoxes.ClearSelection();
                _errors.Clear();
                UpdateBalanceLabel(null);
                UpdateAuditPanel(null);
            }
            finally
            {
                _isBinding = false;
            }

            UpdateActionState();
        }

        private void UpdateActionState()
        {
            var selected = dgvCashBoxes.CurrentRow?.DataBoundItem as CashBoxModel;
            var hasSelection = selected != null && !string.IsNullOrWhiteSpace(selected.ID);
            var isActive = selected?.IsActive ?? true;

            var isInputMode = _isNewMode || _isEditMode;
            btnNew.Enabled = !_isBusy && _canAdd && !isInputMode;
            btnSave.Enabled = !_isBusy && _masterDataReady && (_isNewMode ? _canAdd : _isEditMode && _canEdit);
            btnEdit.Enabled = !_isBusy && _canEdit && hasSelection && isActive && !isInputMode;
            btnDelete.Visible = _canDeactivate && hasSelection && isActive && !isInputMode;
            btnDelete.Enabled = !_isBusy && _canDeactivate && hasSelection && isActive && !isInputMode;
            btnSearch.Enabled = !_isBusy;
            btnRefresh.Enabled = !_isBusy;
            btnPrint.Enabled = !_isBusy && _canPrint;

            if (_btnReactivate != null)
            {
                _btnReactivate.Visible = _canReactivate && hasSelection && !isActive && !isInputMode;
                _btnReactivate.Enabled = !_isBusy && _canReactivate && hasSelection && !isActive && !isInputMode;
            }
        }

        private void SetBusy(bool busy)
        {
            _isBusy = busy;
            UseWaitCursor = busy;
            UpdateActionState();
        }

        private void UpdateBalanceLabel(CashBoxModel? row)
        {
            if (_lblCurrentBalance == null)
                return;

            _lblCurrentBalance.Text = row == null
                ? "الرصيد الدفتري الحالي: 0.00"
                : $"الرصيد الدفتري الحالي: {row.Current_Balance:N2} {row.Currency_Code}";
            _lblCurrentBalance.ForeColor = row?.Current_Balance < 0
                ? Color.Firebrick
                : Color.FromArgb(31, 78, 121);
        }

        private async Task LoadAuditSummaryAsync(string cashBoxId)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}CashBoxes/{cashBoxId}/audit-summary");
                if (!response.IsSuccessStatusCode)
                {
                    UpdateAuditPanel(null);
                    return;
                }

                var summary = await response.Content.ReadFromJsonAsync<CashBoxAuditSummary>();
                if (cashBoxId == _selectedCashBoxId)
                    UpdateAuditPanel(summary);
            }
            catch
            {
                // لا تمنع بيانات التدقيق استعمال شاشة الصناديق عند تعذر تحميلها.
                UpdateAuditPanel(null);
            }
        }

        private void UpdateAuditPanel(CashBoxAuditSummary? summary)
        {
            if (_lblCreatedAudit == null || _lblUpdatedAudit == null || _lblCountersAudit == null)
                return;

            _lblCreatedAudit.Text = summary == null
                ? "بيانات الإنشاء\nأنشئ بواسطة: —\nتاريخ الإنشاء: —"
                : $"بيانات الإنشاء\nأنشئ بواسطة: {summary.Created_By ?? "غير متاح"}\nتاريخ الإنشاء: {FormatAuditDate(summary.Created_At)}";
            _lblUpdatedAudit.Text = summary == null || summary.Updated_At == null
                ? "بيانات التعديل\nعُدّل بواسطة: —\nتاريخ التعديل: —"
                : $"بيانات التعديل\nعُدّل بواسطة: {summary.Updated_By ?? "غير متاح"}\nتاريخ التعديل: {FormatAuditDate(summary.Updated_At)}";
            _lblCountersAudit.Text = summary == null
                ? "العدادات\nعدد التعديلات: 0\nعدد الطباعة: 0"
                : $"العدادات\nعدد التعديلات: {summary.Edit_Count}\nعدد الطباعة: {summary.Print_Count}";
        }

        private static string FormatAuditDate(DateTime? value) =>
            value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—";

        #endregion

        #region توليد الكود والطباعة

        private async void cmbAccount_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _errors.SetError(cmbAccount, string.Empty);
            if (_isBinding || cmbAccount.SelectedValue == null || !string.IsNullOrWhiteSpace(_selectedCashBoxId))
                return;
            if (string.IsNullOrWhiteSpace(txtCashBoxCode.Text))
                await GenerateNextCodeAsync();
        }

        private async Task GenerateNextCodeAsync()
        {
            var response = await _client.GetAsync($"{_baseUrl}CashBoxes/GetNextCode");
            if (!response.IsSuccessStatusCode)
            {
                await ShowApiErrorAsync(response, "تعذر توليد كود الصندوق");
                return;
            }
            txtCashBoxCode.Text = (await response.Content.ReadAsStringAsync()).Trim().Trim('"');
        }

        private async void btnPrint_Click(object? sender, EventArgs e)
        {
            if (dgvCashBoxes.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.", "الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var document = new PrintDocument { DocumentName = "قائمة الصناديق" };
            document.DefaultPageSettings.Landscape = true;
            document.PrintPage += PrintCashBoxesPage;
            using var preview = new PrintPreviewDialog
            {
                Document = document,
                Width = 1100,
                Height = 750,
                StartPosition = FormStartPosition.CenterParent
            };
            _printRowIndex = 0;
            preview.ShowDialog(this);
            await RegisterPrintAsync();
        }

        /// <summary>يسجل فتح معاينة الطباعة في سجل التدقيق دون تعطيل المستخدم عند تعذر التسجيل.</summary>
        private async Task RegisterPrintAsync()
        {
            try
            {
                var endpoint = string.IsNullOrWhiteSpace(_selectedCashBoxId)
                    ? $"{_baseUrl}CashBoxes/print"
                    : $"{_baseUrl}CashBoxes/{_selectedCashBoxId}/print";
                var response = await _client.PostAsync(endpoint, null);
                if (!response.IsSuccessStatusCode)
                    await ShowApiErrorAsync(response, "تعذر تسجيل عملية الطباعة");
                else if (!string.IsNullOrWhiteSpace(_selectedCashBoxId))
                    await LoadAuditSummaryAsync(_selectedCashBoxId);
            }
            catch (Exception ex)
            {
                ShowError("تعذر تسجيل عملية الطباعة", ex);
            }
        }

        private void PrintCashBoxesPage(object? sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null)
                return;

            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var rowFont = new Font("Segoe UI", 8F);
            using var pen = new Pen(Color.Black);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            var bounds = e.MarginBounds;
            e.Graphics.DrawString("قائمة الصناديق", titleFont, Brushes.Black, bounds.Left, bounds.Top);
            var columns = dgvCashBoxes.Columns.Cast<DataGridViewColumn>().Where(x => x.Visible).ToList();
            if (columns.Count == 0)
                return;

            var width = Math.Max(80, bounds.Width / columns.Count);
            const int height = 30;
            var y = bounds.Top + 45;

            DrawPrintRow(e.Graphics, columns.Select(x => x.HeaderText), bounds.Right, y, width, height, headerFont, pen, format);
            y += height;

            while (_printRowIndex < dgvCashBoxes.Rows.Count)
            {
                var row = dgvCashBoxes.Rows[_printRowIndex];
                DrawPrintRow(e.Graphics, columns.Select(x => row.Cells[x.Index].FormattedValue?.ToString() ?? string.Empty),
                    bounds.Right, y, width, height, rowFont, pen, format);
                y += height;
                _printRowIndex++;

                if (y + height > bounds.Bottom)
                {
                    e.HasMorePages = _printRowIndex < dgvCashBoxes.Rows.Count;
                    return;
                }
            }

            e.HasMorePages = false;
            _printRowIndex = 0;
        }

        private static void DrawPrintRow(Graphics graphics, IEnumerable<string> values, int right, int y,
            int width, int height, Font font, Pen pen, StringFormat format)
        {
            var x = right - width;
            foreach (var value in values)
            {
                var rect = new Rectangle(x, y, width, height);
                graphics.DrawRectangle(pen, rect);
                graphics.DrawString(value, font, Brushes.Black, rect, format);
                x -= width;
            }
        }

        #endregion

        #region مساعدات عامة

        private async Task ShowApiErrorAsync(HttpResponseMessage response, string title)
        {
            var raw = await response.Content.ReadAsStringAsync();
            var message = DefaultApiMessage(response.StatusCode);
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var value) && IsSafeArabicMessage(value.GetString()))
                    message = value.GetString()!;
            }
            catch (JsonException)
            {
                // لا تعرض الاستجابة الخام للمستخدم.
            }
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static string DefaultApiMessage(System.Net.HttpStatusCode statusCode) => statusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "انتهت جلسة الدخول. يرجى تسجيل الدخول من جديد.",
            System.Net.HttpStatusCode.Forbidden => "ليس لديك صلاحية لتنفيذ هذه العملية.",
            System.Net.HttpStatusCode.NotFound => "البيانات المطلوبة غير موجودة أو لا يسمح لك بالوصول إليها.",
            System.Net.HttpStatusCode.Conflict => "تم تعديل البيانات من مستخدم آخر أو تغيّرت حالتها. حمّل أحدث البيانات ثم أعد المحاولة.",
            _ => "تعذر تنفيذ العملية. حاول مرة أخرى أو تواصل مع مسؤول النظام."
        };

        private static bool IsSafeArabicMessage(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value!.Any(char.IsLetter) &&
            value.Any(ch => ch >= '\u0600' && ch <= '\u06FF') &&
            !value.Contains("http", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("json", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("exception", StringComparison.OrdinalIgnoreCase) &&
            !value.Contains("stack", StringComparison.OrdinalIgnoreCase);

        private static string? NullIfWhiteSpace(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool Contains(string? source, string value) =>
            !string.IsNullOrWhiteSpace(source) && source.Contains(value, StringComparison.OrdinalIgnoreCase);

        private static void SetNumericValue(NumericUpDown control, decimal value) =>
            control.Value = Math.Min(control.Maximum, Math.Max(control.Minimum, value));

        private static void ShowError(string title, Exception ex) =>
            MessageBox.Show($"{title}. تعذر الاتصال بخدمة النظام أو إكمال العملية. حاول مرة أخرى.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static string? PromptReason(string title, string labelText)
        {
            using var dialog = new Form
            {
                Text = title,
                Width = 480,
                Height = 210,
                StartPosition = FormStartPosition.CenterParent,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            // خاصية Right للقراءة فقط؛ نحدد موضع البداية عبر Left.
            var label = new Label { Text = labelText, AutoSize = true, Left = 25, Top = 20 };
            var text = new TextBox { Multiline = true, Width = 420, Height = 75, Left = 25, Top = 45, MaxLength = 500 };
            var ok = new Button { Text = "موافق", DialogResult = DialogResult.OK, Width = 90, Left = 255, Top = 130 };
            var cancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Width = 90, Left = 155, Top = 130 };
            dialog.Controls.AddRange(new Control[] { label, text, ok, cancel });
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;

            return dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(text.Text)
                ? text.Text.Trim()
                : null;
        }

        private void FrmCashBoxes_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N) btnNew.PerformClick();
            else if (e.Control && e.KeyCode == Keys.S) btnSave.PerformClick();
            else if (e.Control && e.KeyCode == Keys.F) { txtSearch.Focus(); txtSearch.SelectAll(); }
            else if (e.KeyCode == Keys.F5) btnRefresh.PerformClick();
            else if (e.KeyCode == Keys.Escape && !_isBusy) Close();
            else return;
            e.SuppressKeyPress = true;
        }

        private void FrmCashBoxes_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_isBusy)
                return;
            e.Cancel = true;
            MessageBox.Show("انتظر حتى تكتمل العملية الحالية قبل إغلاق الشاشة.", "عملية قيد التنفيذ",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // معالجات متوافقة مع ملف المصمم الحالي.
        private void chkIsActive_CheckedChanged(object sender, EventArgs e) { }
        private void groupBox3_Enter(object sender, EventArgs e) { }
        private void groupBox4_Enter(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }

        #endregion
    }
}
