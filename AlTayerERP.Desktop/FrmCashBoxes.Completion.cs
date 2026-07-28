using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// نقطة التهيئة الموحدة لشاشة الصناديق.
    /// </summary>
    public partial class FrmCashBoxes
    {
        private Button? _btnReactivateCashBox;
        private bool _completionInitialized;

        protected override async void OnLoad(EventArgs e)
        {
            if (!_completionInitialized)
            {
                _completionInitialized = true;
                ConfigureCashBoxDropdowns();
                ApplyCashBoxAccountingControls();
                ApplyCashBoxDeliveryReadiness();
                InitializeCashBoxCompletionControls();
            }

            base.OnLoad(e);

            try
            {
                await ReloadUnifiedCashBoxLookupsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر تحديث منسدلات الصناديق من المسار الموحد: " + ex.Message,
                    "الصناديق",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            UpdateCashBoxActionState();
        }

        private void InitializeCashBoxCompletionControls()
        {
            if (_btnReactivateCashBox != null)
                return;

            _btnReactivateCashBox = new Button
            {
                Name = "btnReactivateCashBox",
                Text = "إعادة تفعيل",
                Size = btnDelete.Size,
                Font = btnDelete.Font,
                BackColor = Color.FromArgb(230, 247, 237),
                ForeColor = Color.FromArgb(20, 108, 67),
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Visible = false,
                AccessibleName = "إعادة تفعيل الصندوق"
            };
            _btnReactivateCashBox.FlatAppearance.BorderColor = Color.FromArgb(82, 183, 136);
            _btnReactivateCashBox.Click += btnReactivateCashBox_Click;

            if (btnDelete.Parent is Control parent)
            {
                parent.Controls.Add(_btnReactivateCashBox);
                _btnReactivateCashBox.Location = btnDelete.Location;
                _btnReactivateCashBox.BringToFront();
            }

            dgvCashBoxes.SelectionChanged += (_, _) => UpdateCashBoxActionState();
        }

        private async Task ReloadUnifiedCashBoxLookupsAsync()
        {
            var lookups = await _client.GetFromJsonAsync<CashBoxUnifiedLookups>(
                $"{_baseUrl}CashBoxes/Lookups");

            if (lookups == null)
                throw new InvalidOperationException("لم يرجع الخادم بيانات المنسدلات.");

            if (lookups.Branches == null || lookups.Branches.Count == 0)
                throw new InvalidOperationException("فرع الجلسة غير متاح ضمن الفروع الفعالة.");
            if (lookups.Currencies == null || lookups.Currencies.Count == 0)
                throw new InvalidOperationException("لا توجد عملات فعالة للشركة الحالية.");
            if (lookups.Accounts == null || lookups.Accounts.Count == 0)
                throw new InvalidOperationException("لا توجد حسابات تجميعية متاحة للصناديق.");

            var selectedCurrency = cmbCurrency.SelectedValue?.ToString();
            var selectedAccount = cmbAccount.SelectedValue?.ToString();

            _isBinding = true;
            try
            {
                cmbBranch.DataSource = lookups.Branches;
                cmbBranch.DisplayMember = nameof(BranchCashLookup.Branch_Name);
                cmbBranch.ValueMember = nameof(BranchCashLookup.Branch_ID);
                cmbBranch.SelectedValue = CurrentSession.Branch_ID;

                cmbCurrency.DataSource = lookups.Currencies;
                cmbCurrency.DisplayMember = nameof(CurrencyCashLookup.Currency_Name_AR);
                cmbCurrency.ValueMember = nameof(CurrencyCashLookup.Currency_Code);
                cmbCurrency.SelectedValue = selectedCurrency;
                if (cmbCurrency.SelectedIndex < 0)
                    cmbCurrency.SelectedIndex = -1;

                cmbAccount.DataSource = lookups.Accounts;
                cmbAccount.DisplayMember = nameof(AccountCashLookup.Account_Name_AR);
                cmbAccount.ValueMember = nameof(AccountCashLookup.Account_ID);
                cmbAccount.SelectedValue = selectedAccount;
                if (cmbAccount.SelectedIndex < 0)
                    cmbAccount.SelectedIndex = -1;
            }
            finally
            {
                _isBinding = false;
            }
        }

        private async void btnReactivateCashBox_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_selectedCashBoxId))
            {
                MessageBox.Show("اختر صندوقاً موقوفاً أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string reason = "إعادة تفعيل من شاشة الصناديق";
            if (MessageBox.Show(
                    "هل تريد إعادة تفعيل الصندوق وحسابه المحاسبي المرتبط؟",
                    "تأكيد إعادة التفعيل",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

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

                MessageBox.Show(
                    "تمت إعادة تفعيل الصندوق وحسابه المرتبط بنجاح.",
                    "الصناديق",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await LoadCashBoxesAsync();
                ClearForm();
                UpdateCashBoxActionState();
            }
            catch (Exception ex)
            {
                ShowError("حدث خطأ أثناء إعادة تفعيل الصندوق", ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void UpdateCashBoxActionState()
        {
            var selected = dgvCashBoxes.CurrentRow?.DataBoundItem as Models.CashBoxModel;
            var hasSelection = selected != null && !string.IsNullOrWhiteSpace(selected.ID);
            var isActive = selected?.IsActive ?? true;

            btnEdit.Enabled = hasSelection && isActive && !UseWaitCursor;
            btnDelete.Visible = !hasSelection || isActive;
            btnDelete.Enabled = hasSelection && isActive && !UseWaitCursor;

            if (_btnReactivateCashBox != null)
            {
                _btnReactivateCashBox.Visible = hasSelection && !isActive;
                _btnReactivateCashBox.Enabled = hasSelection && !isActive && !UseWaitCursor;
            }

            chkIsActive.Enabled = false;
        }

        private sealed class CashBoxUnifiedLookups
        {
            public List<BranchCashLookup>? Branches { get; set; }
            public List<CurrencyCashLookup>? Currencies { get; set; }
            public List<AccountCashLookup>? Accounts { get; set; }
        }
    }
}
