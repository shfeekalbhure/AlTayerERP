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
    public partial class FrmCashBoxes : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private string _selectedCashBoxId = "";
        private List<CashBoxModel> _cashBoxesCache = new();
        private bool _isBinding;
        private int _printRowIndex;

        public FrmCashBoxes()
        {
            InitializeComponent();

            // حدث Load مربوط في ملف المصمم؛ لا يعاد ربطه هنا حتى لا تحمل الشاشة والمنسدلات مرتين.
            btnNew.Click += btnNew_Click;
            btnSave.Click += btnSave_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnSearch.Click += btnSearch_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnPrint.Click += btnPrint_Click;
            btnClose.Click += btnClose_Click;
            dgvCashBoxes.CellClick += dgvCashBoxes_CellClick;
            cmbAccount.SelectedIndexChanged += cmbAccount_SelectedIndexChanged;
            txtSearch.TextChanged += (_, _) => ApplySearch();

            cmbBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccount.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCashBoxCode.ReadOnly = true;
            btnDelete.Text = "إيقاف";
        }

        public sealed class BranchCashLookup
        {
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = "";
        }

        public sealed class CurrencyCashLookup
        {
            public string Currency_Code { get; set; } = "";
            public string Currency_Name_AR { get; set; } = "";
        }

        public sealed class AccountCashLookup
        {
            public string Account_ID { get; set; } = "";
            public string Account_Name_AR { get; set; } = "";
        }

        private async void FrmCashBoxes_Load(object? sender, EventArgs e)
        {
            try
            {
                SetBusy(true);
                _isBinding = true;
                SetupGrid();
                await LoadBranchesAsync();
                await LoadCurrenciesAsync();
                await LoadAccountsAsync();
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("تعذر فتح شاشة الصناديق", ex);
            }
            finally
            {
                _isBinding = false;
                SetBusy(false);
            }
        }

        private void SetupGrid()
        {
            dgvCashBoxes.AutoGenerateColumns = false;
            dgvCashBoxes.AllowUserToAddRows = false;
            dgvCashBoxes.AllowUserToDeleteRows = false;
            dgvCashBoxes.ReadOnly = true;
            dgvCashBoxes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCashBoxes.MultiSelect = false;
        }

        private async Task LoadBranchesAsync()
        {
            var data = await _client.GetFromJsonAsync<List<BranchCashLookup>>(
                $"{_baseUrl}Branches/GetActiveBranchesLookup?companyId={CurrentSession.Company_ID}") ?? new();

            var currentBranch = data.FirstOrDefault(x => x.Branch_ID == CurrentSession.Branch_ID);
            cmbBranch.DataSource = currentBranch == null
                ? new List<BranchCashLookup>()
                : new List<BranchCashLookup> { currentBranch };
            cmbBranch.DisplayMember = nameof(BranchCashLookup.Branch_Name);
            cmbBranch.ValueMember = nameof(BranchCashLookup.Branch_ID);
            cmbBranch.Enabled = false;

            if (currentBranch == null)
                throw new InvalidOperationException("الفرع الحالي غير موجود ضمن الفروع الفعالة.");
        }

        private async Task LoadCurrenciesAsync()
        {
            var data = await _client.GetFromJsonAsync<List<CurrencyCashLookup>>(
                $"{_baseUrl}Currencies/GetLookup?companyId={CurrentSession.Company_ID}") ?? new();

            cmbCurrency.DataSource = data;
            cmbCurrency.DisplayMember = nameof(CurrencyCashLookup.Currency_Name_AR);
            cmbCurrency.ValueMember = nameof(CurrencyCashLookup.Currency_Code);
            cmbCurrency.SelectedIndex = -1;

            if (data.Count == 0)
                throw new InvalidOperationException("لا توجد عملات فعالة. يجب تعريف عملة فعالة أولاً.");
        }

        private async Task LoadAccountsAsync()
        {
            var data = await _client.GetFromJsonAsync<List<AccountCashLookup>>(
                $"{_baseUrl}ChartOfAccounts/GetCashParentLookup?companyId={CurrentSession.Company_ID}") ?? new();

            cmbAccount.DataSource = data;
            cmbAccount.DisplayMember = nameof(AccountCashLookup.Account_Name_AR);
            cmbAccount.ValueMember = nameof(AccountCashLookup.Account_ID);
            cmbAccount.SelectedIndex = -1;

            if (data.Count == 0)
                throw new InvalidOperationException("لا يوجد حساب صناديق أب نشط وتجميعي في دليل الحسابات.");
        }

        private async Task LoadCashBoxesAsync()
        {
            _cashBoxesCache = await _client.GetFromJsonAsync<List<CashBoxModel>>(
                $"{_baseUrl}CashBoxes") ?? new();
            ApplySearch();
        }

        private async void btnNew_Click(object? sender, EventArgs e)
        {
            ClearForm();
            await GenerateNextCodeAsync();
            txtCashBoxNameAR.Focus();
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                SetBusy(true);
                var response = await _client.PostAsJsonAsync($"{_baseUrl}CashBoxes", BuildRequest());
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, "تعذر حفظ الصندوق");
                    return;
                }

                MessageBox.Show("تم حفظ الصندوق بنجاح.", "الصناديق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("حدث خطأ أثناء حفظ الصندوق", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnEdit_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقاً من الجدول أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateForm()) return;

            try
            {
                SetBusy(true);
                var response = await _client.PutAsJsonAsync($"{_baseUrl}CashBoxes/{_selectedCashBoxId}", BuildRequest());
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, "تعذر تعديل الصندوق");
                    return;
                }

                MessageBox.Show("تم تعديل الصندوق بنجاح.", "الصناديق", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("حدث خطأ أثناء تعديل الصندوق", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقاً من الجدول أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                    "سيتم إيقاف الصندوق وحسابه المرتبط دون حذف البيانات. هل تريد المتابعة؟",
                    "تأكيد إيقاف الصندوق",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                SetBusy(true);
                var reason = Uri.EscapeDataString("إيقاف من شاشة الصناديق");
                var response = await _client.DeleteAsync($"{_baseUrl}CashBoxes/{_selectedCashBoxId}?reason={reason}");
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
            }
        }

        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                SetBusy(true);
                await LoadCashBoxesAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError("تعذر تحديث بيانات الصناديق", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnPrint_Click(object? sender, EventArgs e)
        {
            if (dgvCashBoxes.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد بيانات للطباعة.", "الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var document = new PrintDocument
            {
                DocumentName = "قائمة الصناديق",
                DefaultPageSettings = { Landscape = true }
            };
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
        }

        private void PrintCashBoxesPage(object? sender, PrintPageEventArgs e)
        {
            var graphics = e.Graphics;
            if (graphics == null) return;

            using var titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            using var headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var rowFont = new Font("Segoe UI", 8F);
            using var pen = new Pen(Color.Black);

            var bounds = e.MarginBounds;
            graphics.DrawString("قائمة الصناديق", titleFont, Brushes.Black, bounds.Left, bounds.Top);
            var y = bounds.Top + 45;

            var printableColumns = dgvCashBoxes.Columns
                .Cast<DataGridViewColumn>()
                .Where(x => x.Visible)
                .ToList();

            if (printableColumns.Count == 0) return;

            var columnWidth = Math.Max(80, bounds.Width / printableColumns.Count);
            var rowHeight = 30;
            var x = bounds.Right - columnWidth;

            foreach (var column in printableColumns)
            {
                var rect = new Rectangle(x, y, columnWidth, rowHeight);
                graphics.DrawRectangle(pen, rect);
                graphics.DrawString(column.HeaderText, headerFont, Brushes.Black, rect, CenteredStringFormat());
                x -= columnWidth;
            }

            y += rowHeight;
            while (_printRowIndex < dgvCashBoxes.Rows.Count)
            {
                var row = dgvCashBoxes.Rows[_printRowIndex];
                x = bounds.Right - columnWidth;

                foreach (var column in printableColumns)
                {
                    var rect = new Rectangle(x, y, columnWidth, rowHeight);
                    graphics.DrawRectangle(pen, rect);
                    var value = row.Cells[column.Index].FormattedValue?.ToString() ?? "";
                    graphics.DrawString(value, rowFont, Brushes.Black, rect, CenteredStringFormat());
                    x -= columnWidth;
                }

                y += rowHeight;
                _printRowIndex++;

                if (y + rowHeight > bounds.Bottom)
                {
                    e.HasMorePages = _printRowIndex < dgvCashBoxes.Rows.Count;
                    return;
                }
            }

            e.HasMorePages = false;
            _printRowIndex = 0;
        }

        private static StringFormat CenteredStringFormat() => new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.DirectionRightToLeft
        };

        private void dgvCashBoxes_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvCashBoxes.Rows[e.RowIndex].DataBoundItem is not CashBoxModel row)
                return;

            _isBinding = true;
            try
            {
                _selectedCashBoxId = row.ID;
                txtCashBoxCode.Text = row.Code;
                txtCashBoxNameAR.Text = row.NameAR;
                txtCashBoxNameEN.Text = row.NameEN ?? "";
                chkIsActive.Checked = row.IsActive;
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;
                cmbCurrency.SelectedValue = row.Currency_Code;
                cmbAccount.SelectedValue = row.Account_ID;
                SetNumericValue(numOpeningBalance, row.Opening_Balance);
                SetNumericValue(numMaximumLimit, row.Max_Limit);
                SetNumericValue(numMinimumLimit, row.Min_Limit);
                txtNotes.Text = row.Notes ?? "";
            }
            finally
            {
                _isBinding = false;
            }
        }

        private void btnSearch_Click(object? sender, EventArgs e) => ApplySearch();

        private void ApplySearch()
        {
            var text = txtSearch.Text.Trim();
            var rows = string.IsNullOrWhiteSpace(text)
                ? _cashBoxesCache
                : _cashBoxesCache.Where(x =>
                    Contains(x.Code, text) ||
                    Contains(x.NameAR, text) ||
                    Contains(x.NameEN, text) ||
                    Contains(x.Account_Name_AR, text) ||
                    Contains(x.Currency_Code, text)).ToList();

            dgvCashBoxes.DataSource = null;
            dgvCashBoxes.DataSource = rows;
        }

        private void btnClose_Click(object? sender, EventArgs e) => Close();

        private CashBoxModel BuildRequest()
        {
            return new CashBoxModel
            {
                Company_ID = CurrentSession.Company_ID,
                Branch_ID = CurrentSession.Branch_ID,
                Code = txtCashBoxCode.Text.Trim(),
                NameAR = txtCashBoxNameAR.Text.Trim(),
                NameEN = NullIfWhiteSpace(txtCashBoxNameEN.Text),
                Account_ID = cmbAccount.SelectedValue?.ToString() ?? "",
                Currency_Code = cmbCurrency.SelectedValue?.ToString() ?? "",
                Opening_Balance = numOpeningBalance.Value,
                Max_Limit = numMaximumLimit.Value,
                Min_Limit = numMinimumLimit.Value,
                IsActive = chkIsActive.Checked,
                Notes = NullIfWhiteSpace(txtNotes.Text)
            };
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtCashBoxNameAR.Text))
                return ValidationError("أدخل اسم الصندوق العربي.", txtCashBoxNameAR);
            if (cmbAccount.SelectedValue == null)
                return ValidationError("اختر حساب الصناديق الأب.", cmbAccount);
            if (cmbCurrency.SelectedValue == null)
                return ValidationError("اختر عملة الصندوق.", cmbCurrency);
            if (numOpeningBalance.Value < 0)
                return ValidationError("الرصيد الافتتاحي لا يمكن أن يكون سالباً.", numOpeningBalance);
            if (numMinimumLimit.Value < 0 || numMaximumLimit.Value < 0)
                return ValidationError("حدود الصندوق لا يمكن أن تكون سالبة.", numMinimumLimit);
            if (numMinimumLimit.Value > numMaximumLimit.Value)
                return ValidationError("الحد الأدنى يجب ألا يتجاوز الحد الأعلى.", numMinimumLimit);

            var duplicate = _cashBoxesCache.Any(x =>
                x.ID != _selectedCashBoxId &&
                string.Equals(x.NameAR?.Trim(), txtCashBoxNameAR.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (duplicate)
                return ValidationError("يوجد صندوق آخر بالاسم نفسه في الفرع الحالي.", txtCashBoxNameAR);

            return true;
        }

        private async void cmbAccount_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isBinding || cmbAccount.SelectedValue == null || !string.IsNullOrWhiteSpace(_selectedCashBoxId))
                return;
            if (string.IsNullOrWhiteSpace(txtCashBoxCode.Text))
                await GenerateNextCodeAsync();
        }

        private async Task GenerateNextCodeAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}CashBoxes/GetNextCode");
                if (!response.IsSuccessStatusCode)
                {
                    await ShowApiErrorAsync(response, "تعذر توليد كود الصندوق");
                    return;
                }
                txtCashBoxCode.Text = (await response.Content.ReadAsStringAsync()).Trim().Trim('"');
            }
            catch (Exception ex)
            {
                ShowError("تعذر توليد كود الصندوق", ex);
            }
        }

        private void ClearForm()
        {
            _isBinding = true;
            try
            {
                _selectedCashBoxId = "";
                txtCashBoxCode.Clear();
                txtCashBoxNameAR.Clear();
                txtCashBoxNameEN.Clear();
                txtNotes.Clear();
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;
                cmbCurrency.SelectedIndex = -1;
                cmbAccount.SelectedIndex = -1;
                numOpeningBalance.Value = 0;
                numMaximumLimit.Value = 0;
                numMinimumLimit.Value = 0;
                chkIsActive.Checked = true;
                dgvCashBoxes.ClearSelection();
            }
            finally
            {
                _isBinding = false;
            }
        }

        private void SetBusy(bool busy)
        {
            btnNew.Enabled = !busy;
            btnSave.Enabled = !busy;
            btnEdit.Enabled = !busy;
            btnDelete.Enabled = !busy;
            btnSearch.Enabled = !busy;
            btnRefresh.Enabled = !busy;
            btnPrint.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private async Task ShowApiErrorAsync(HttpResponseMessage response, string title)
        {
            var raw = await response.Content.ReadAsStringAsync();
            var message = raw;
            try
            {
                using var json = JsonDocument.Parse(raw);
                if (json.RootElement.TryGetProperty("message", out var value))
                    message = value.GetString() ?? raw;
            }
            catch (JsonException) { }
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static bool ValidationError(string message, Control control)
        {
            MessageBox.Show(message, "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
            return false;
        }

        private static void SetNumericValue(NumericUpDown control, decimal value)
        {
            control.Value = Math.Min(control.Maximum, Math.Max(control.Minimum, value));
        }

        private static bool Contains(string? source, string value) =>
            !string.IsNullOrWhiteSpace(source) && source.Contains(value, StringComparison.OrdinalIgnoreCase);

        private static string? NullIfWhiteSpace(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static void ShowError(string title, Exception ex) =>
            MessageBox.Show($"{title}: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void chkIsActive_CheckedChanged(object sender, EventArgs e) { }
        private void groupBox3_Enter(object sender, EventArgs e) { }
        private void groupBox4_Enter(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
    }
}
