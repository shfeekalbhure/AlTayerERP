using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// القيد اليومي المستقل: إدخال مدين/دائن متوازن، وبحث ودورة مراجعة/اعتماد/ترحيل
    /// عبر API فقط. لا يرث نموذج سند القبض حتى لا يفرض حقول الصندوق على القيد اليدوي.
    /// </summary>
    public sealed class FrmJournalVoucher : BaseForm
    {
        private readonly DataGridView _lines = new() { Dock = DockStyle.Fill, AllowUserToAddRows = true, AutoGenerateColumns = false };
        private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
        private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly ComboBox _currency = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, Width = 130 };
        private readonly TextBox _narration = new() { Width = 360 };
        private readonly TextBox _number = new() { Width = 170, ReadOnly = true };
        private readonly TextBox _reason = new() { Width = 260, PlaceholderText = "سبب إلزامي لفك الترحيل/الإعادة" };
        private readonly Label _totals = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        private long _voucherId;
        private List<LookupType> _types = new();
        private List<LookupStatus> _statuses = new();
        private List<LookupCurrency> _currencies = new();

        public FrmJournalVoucher()
        {
            Text = "القيد اليومي";
            ApplyBaseFormStyle();
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            KeyPreview = true;
            BuildLayout();
            Load += async (_, _) => await LoadLookupsAsync();
            KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.F2) { NewEntry(); e.Handled = true; }
                if (e.Control && e.KeyCode == Keys.S) { await SaveAsync(); e.Handled = true; }
                if (e.KeyCode == Keys.F5) { await LoadLookupsAsync(); e.Handled = true; }
                if (e.Control && e.KeyCode == Keys.F) { _number.ReadOnly = false; _number.Focus(); e.Handled = true; }
                if (e.KeyCode == Keys.Escape) Close();
            };
        }

        private void BuildLayout()
        {
            var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 106, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(12), WrapContents = true };
            header.Controls.AddRange(new Control[] {
                LabelFor("رقم القيد"), _number, LabelFor("التاريخ"), _date, LabelFor("النوع"), _type,
                LabelFor("الحالة"), _status, LabelFor("العملة"), _currency, LabelFor("البيان"), _narration, _reason
            });
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10,6,10,6) };
            bar.Controls.AddRange(new Control[] {
                ButtonFor("جديد F2", (_, _) => NewEntry()),
                ButtonFor("حفظ Ctrl+S", async (_, _) => await SaveAsync()),
                ButtonFor("بحث", async (_, _) => await SearchAsync()),
                ButtonFor("مراجعة", async (_, _) => await LifecycleAsync("review", false)),
                ButtonFor("اعتماد", async (_, _) => await LifecycleAsync("approve", false)),
                ButtonFor("ترحيل", async (_, _) => await LifecycleAsync("post", false)),
                ButtonFor("فك ترحيل", async (_, _) => await LifecycleAsync("unpost", true)),
                ButtonFor("عرض القيد", async (_, _) => await ViewJournalAsync())
            });
            _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Account", HeaderText = "الحساب", Width = 180 });
            _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Debit", HeaderText = "مدين", Width = 120 });
            _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Credit", HeaderText = "دائن", Width = 120 });
            _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "البيان", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _lines.CellValueChanged += (_, _) => CalculateTotals();
            _lines.RowsRemoved += (_, _) => CalculateTotals();
            Controls.Add(_lines); Controls.Add(_totals); _totals.Dock = DockStyle.Bottom; _totals.Padding = new Padding(12);
            Controls.Add(bar); Controls.Add(header);
        }

        private static Label LabelFor(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(9, 8, 3, 3) };
        private static Button ButtonFor(string text, EventHandler click) { var b = new Button { Text = text, AutoSize = true, Height = 30 }; b.Click += click; return b; }

        private async Task LoadLookupsAsync()
        {
            try
            {
                var u = $"FinancialVoucherLookups?companyId={Uri.EscapeDataString(CurrentSession.Company_ID)}&branchId={CurrentSession.Branch_ID}";
                var lookup = await ApiService.Client.GetFromJsonAsync<LookupResponse>(u) ?? throw new InvalidOperationException("لم تصل بيانات القوائم المرجعية.");
                _types = lookup.VoucherTypes.Where(x => x.Is_Active && string.Equals(x.Voucher_Type_Code, "JOURNAL", StringComparison.OrdinalIgnoreCase)).ToList();
                _statuses = lookup.VoucherStatuses.Where(x => x.Is_Active).ToList();
                _currencies = lookup.Currencies.Where(x => x.Is_Active).ToList();
                Bind(_type, _types, nameof(LookupType.Voucher_Type_Name_AR), nameof(LookupType.Voucher_Type_ID));
                Bind(_status, _statuses, nameof(LookupStatus.Voucher_Status_Name_AR), nameof(LookupStatus.Voucher_Status_ID));
                Bind(_currency, _currencies, nameof(LookupCurrency.Currency_Name_AR), nameof(LookupCurrency.Currency_ID));
                var draft = _statuses.FirstOrDefault(x => string.Equals(x.Voucher_Status_Code, "DRAFT", StringComparison.OrdinalIgnoreCase));
                if (draft != null) _status.SelectedValue = draft.Voucher_Status_ID;
                var local = _currencies.FirstOrDefault(x => x.Is_Local_Currency) ?? _currencies.FirstOrDefault();
                if (local != null) _currency.SelectedValue = local.Currency_ID;
                NewEntry();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "القيد اليومي", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private static void Bind<T>(ComboBox box, List<T> source, string display, string value)
        {
            box.DataSource = null; box.DisplayMember = display; box.ValueMember = value; box.DataSource = source;
        }

        private void NewEntry()
        {
            _voucherId = 0; _number.ReadOnly = true; _number.Clear(); _narration.Clear(); _reason.Clear(); _lines.Rows.Clear();
            _lines.Rows.Add(); _lines.Rows.Add(); CalculateTotals();
        }

        private decimal CellDecimal(DataGridViewRow row, string key) =>
            decimal.TryParse(row.Cells[key].Value?.ToString(), out var value) ? value : 0m;

        private void CalculateTotals()
        {
            var valid = _lines.Rows.Cast<DataGridViewRow>().Where(x => !x.IsNewRow).ToList();
            var debit = valid.Sum(x => CellDecimal(x, "Debit")); var credit = valid.Sum(x => CellDecimal(x, "Credit"));
            _totals.Text = $"إجمالي مدين: {debit:N2}   إجمالي دائن: {credit:N2}   الفرق: {(debit-credit):N2}";
        }

        private async Task SaveAsync()
        {
            var rows = _lines.Rows.Cast<DataGridViewRow>().Where(x => !x.IsNewRow && !string.IsNullOrWhiteSpace(x.Cells["Account"].Value?.ToString())).ToList();
            var debit = rows.Sum(x => CellDecimal(x, "Debit")); var credit = rows.Sum(x => CellDecimal(x, "Credit"));
            if (_type.SelectedValue == null || _status.SelectedValue == null || _currency.SelectedValue == null || string.IsNullOrWhiteSpace(_narration.Text) || rows.Count < 2 || debit <= 0 || Math.Round(debit,2) != Math.Round(credit,2))
            { MessageBox.Show("أدخل بياناً وسطرين على الأقل، ويجب أن يتوازن المدين مع الدائن.", "تحقق القيد", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (rows.Any(x => (CellDecimal(x,"Debit") > 0 && CellDecimal(x,"Credit") > 0) || (CellDecimal(x,"Debit") == 0 && CellDecimal(x,"Credit") == 0)))
            { MessageBox.Show("كل سطر يجب أن يكون مديناً أو دائناً فقط.", "تحقق القيد", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var currencyId = Convert.ToInt32(_currency.SelectedValue);
            var currency = _currencies.First(x => x.Currency_ID == currencyId);
            var dto = new {
                Voucher_Type_ID = Convert.ToInt32(_type.SelectedValue), Voucher_Status_ID = Convert.ToInt32(_status.SelectedValue),
                Voucher_Date = _date.Value.Date, Transaction_Date = _date.Value, Cash_Account_ID = "", Currency_ID = currencyId,
                Exchange_Rate = currency.Is_Local_Currency ? 1m : currency.Exchange_Rate, Amount = debit, Foreign_Total = debit, Local_Total = debit,
                Against_Text = _narration.Text.Trim(), Description = _narration.Text.Trim(), Details = rows.Select((r,i) => new {
                    Line_No = i + 1, Account_ID = r.Cells["Account"].Value!.ToString(), Description = r.Cells["Description"].Value?.ToString(),
                    Currency_ID = currencyId, Exchange_Rate = currency.Is_Local_Currency ? 1m : currency.Exchange_Rate,
                    Foreign_Amount = CellDecimal(r, "Debit") + CellDecimal(r, "Credit"), Local_Amount = CellDecimal(r, "Debit") + CellDecimal(r, "Credit"),
                    Debit_Amount = CellDecimal(r, "Debit"), Credit_Amount = CellDecimal(r, "Credit"), Line_Type = (byte)2
                }).ToList()
            };
            var response = await ApiService.Client.PostAsJsonAsync("FinancialVoucher", dto);
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) { MessageBox.Show(text, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var doc = JsonDocument.Parse(text);
            _voucherId = doc.RootElement.TryGetProperty("voucher_ID", out var id) ? id.GetInt64() : doc.RootElement.GetProperty("voucher_ID").GetInt64();
            _number.Text = doc.RootElement.TryGetProperty("voucher_No", out var no) ? no.GetString() ?? "" : "";
            MessageBox.Show("تم حفظ القيد كمسودة. نفذ المراجعة ثم الاعتماد والترحيل من الأزرار.", "القيد اليومي", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(_number.Text)) { _number.ReadOnly = false; _number.Focus(); return; }
            var typeId = _type.SelectedValue == null ? 0 : Convert.ToInt32(_type.SelectedValue);
            var response = await ApiService.Client.GetAsync($"FinancialVoucher/ByNumber?voucherNumber={Uri.EscapeDataString(_number.Text.Trim())}&voucherTypeId={typeId}");
            var text = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) { MessageBox.Show(text, "البحث", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            using var doc = JsonDocument.Parse(text); var data = doc.RootElement.GetProperty("data");
            _voucherId = data.GetProperty("voucher_ID").GetInt64(); _number.Text = data.GetProperty("voucher_No").GetString() ?? "";
            _narration.Text = data.TryGetProperty("against_Text", out var n) ? n.GetString() ?? "" : "";
            _lines.Rows.Clear();
            foreach(var line in data.GetProperty("details").EnumerateArray()) _lines.Rows.Add(line.GetProperty("account_ID").GetString(), line.GetProperty("debit_Amount").GetDecimal(), line.GetProperty("credit_Amount").GetDecimal(), line.TryGetProperty("description", out var d) ? d.GetString() : "");
            CalculateTotals();
        }

        private async Task LifecycleAsync(string action, bool needsReason)
        {
            if (_voucherId <= 0) { MessageBox.Show("احفظ أو ابحث عن القيد أولاً."); return; }
            if (needsReason && string.IsNullOrWhiteSpace(_reason.Text)) { MessageBox.Show("سبب فك الترحيل إلزامي."); _reason.Focus(); return; }
            var body = needsReason ? new { Reason = _reason.Text.Trim(), Action_Channel = "DESKTOP" } : new { Notes = _narration.Text.Trim(), Action_Channel = "DESKTOP" };
            var response = await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}", body);
            var text = await response.Content.ReadAsStringAsync();
            MessageBox.Show(response.IsSuccessStatusCode ? "تم تنفيذ العملية بنجاح." : text, "دورة القيد", MessageBoxButtons.OK, response.IsSuccessStatusCode ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private async Task ViewJournalAsync()
        {
            if (string.IsNullOrWhiteSpace(_number.Text)) return;
            var typeId = _type.SelectedValue == null ? 0 : Convert.ToInt32(_type.SelectedValue);
            var response = await ApiService.Client.GetAsync($"FinancialVoucher/journal-entry/{Uri.EscapeDataString(_number.Text)}?voucherTypeId={typeId}");
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "عرض القيد الناتج", MessageBoxButtons.OK, response.IsSuccessStatusCode ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private sealed class LookupResponse { public List<LookupType> VoucherTypes { get; set; } = new(); public List<LookupStatus> VoucherStatuses { get; set; } = new(); public List<LookupCurrency> Currencies { get; set; } = new(); }
        private sealed class LookupType { public int Voucher_Type_ID { get; set; } public string Voucher_Type_Code { get; set; } = ""; public string Voucher_Type_Name_AR { get; set; } = ""; public bool Is_Active { get; set; } = true; }
        private sealed class LookupStatus { public int Voucher_Status_ID { get; set; } public string Voucher_Status_Code { get; set; } = ""; public string Voucher_Status_Name_AR { get; set; } = ""; public bool Is_Active { get; set; } = true; }
        private sealed class LookupCurrency { public int Currency_ID { get; set; } public string Currency_Name_AR { get; set; } = ""; public decimal Exchange_Rate { get; set; } = 1m; public bool Is_Local_Currency { get; set; } public bool Is_Active { get; set; } = true; }
    }
}