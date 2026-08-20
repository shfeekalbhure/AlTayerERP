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

namespace AlTayerERP.Desktop;

/// <summary>شاشة القيد اليومي. لا تتصل بقاعدة البيانات؛ كل العمليات تمر عبر API.</summary>
public sealed class FrmJournalVoucher : BaseForm
{
    private readonly DataGridView _lines = new() { Dock = DockStyle.Fill, AllowUserToAddRows = true, AutoGenerateColumns = false, SelectionMode = DataGridViewSelectionMode.CellSelect };
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 190 };
    private readonly ComboBox _currency = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, Width = 125 };
    private readonly TextBox _number = new() { Width = 170, ReadOnly = true };
    private readonly TextBox _narration = new() { Width = 340 };
    private readonly TextBox _reason = new() { Width = 235, PlaceholderText = "سبب العملية عند الطلب" };
    private readonly Label _total = new() { AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(10) };
    private readonly Dictionary<string, AccountRow> _accounts = new(StringComparer.Ordinal);
    private List<LookupType> _types = new();
    private List<LookupStatus> _statuses = new();
    private List<LookupCurrency> _currencies = new();
    private long _voucherId;
    private bool _isPosted;

    public FrmJournalVoucher()
    {
        Text = "سند القيد اليومي";
        ApplyBaseFormStyle();
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        KeyPreview = true;
        BuildLayout();
        Load += async (_, _) => await LoadReferencesAsync();
        KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.F2) { NewEntry(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { await SaveAsync(); e.Handled = true; }
            else if (e.KeyCode == Keys.F9) { await SearchAsync(); e.Handled = true; }
            else if (e.KeyCode == Keys.F4) { await ChooseAccountAsync(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) Close();
        };
    }

    private void BuildLayout()
    {
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 78, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10), WrapContents = true };
        header.Controls.AddRange(new Control[] { LabelFor("رقم القيد"), _number, LabelFor("التاريخ"), _date, LabelFor("النوع"), _type, LabelFor("عملة الرأس"), _currency, LabelFor("البيان"), _narration });
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 80, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10, 5, 10, 5), WrapContents = true };
        toolbar.Controls.AddRange(new Control[] {
            ButtonFor("جديد F2", (_, _) => NewEntry()), ButtonFor("حفظ F5", async (_, _) => await SaveAsync()), ButtonFor("بحث F9", async (_, _) => await SearchAsync()),
            ButtonFor("اختيار حساب F4", async (_, _) => await ChooseAccountAsync()), ButtonFor("حذف السطر", (_, _) => DeleteCurrentLine()),
            ButtonFor("حذف السند", async (_, _) => await DeleteVoucherAsync()),
            ButtonFor("مراجعة", async (_, _) => await WorkflowAsync("review", false)), ButtonFor("اعتماد", async (_, _) => await WorkflowAsync("approve", false)),
            ButtonFor("ترحيل", async (_, _) => await WorkflowAsync("post", false)), LabelFor("السبب"), _reason,
            ButtonFor("فك الترحيل", async (_, _) => await WorkflowAsync("unpost", true)), ButtonFor("طباعة", async (_, _) => await PrintAsync())
        });
        AddColumns();
        _lines.DefaultValuesNeeded += (_, e) => SetRowDefaults(e.Row);
        _lines.CellEndEdit += async (_, e) => { if (e.ColumnIndex == _lines.Columns["Account"].Index) await ChooseAccountAsync(e.RowIndex); RecalculateRow(_lines.Rows[e.RowIndex]); RecalculateTotals(); };
        _lines.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0 && _lines.Columns[e.ColumnIndex].Name == "Account") await ChooseAccountAsync(e.RowIndex); };
        _lines.RowsRemoved += (_, _) => RecalculateTotals();
        Controls.Add(_lines); Controls.Add(_total); _total.Dock = DockStyle.Bottom; Controls.Add(toolbar); Controls.Add(header);
    }

    private void AddColumns()
    {
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Line", HeaderText = "#", Width = 38, ReadOnly = true });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Account", HeaderText = "الحساب", Width = 210 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Currency", HeaderText = "العملة", Width = 70 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rate", HeaderText = "سعر الصرف", Width = 90 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Foreign", HeaderText = "المبلغ الأجنبي", Width = 105 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Debit", HeaderText = "مدين محلي", Width = 110 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Credit", HeaderText = "دائن محلي", Width = 110 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reference", HeaderText = "المرجع", Width = 110 });
        _lines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "بيان السطر", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
    }

    private static Label LabelFor(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(7, 8, 3, 3) };
    private static Button ButtonFor(string text, EventHandler handler) { var button = new Button { Text = text, AutoSize = true, Height = 29 }; button.Click += handler; return button; }

    private async Task LoadReferencesAsync()
    {
        try
        {
            var lookup = await ApiService.Client.GetFromJsonAsync<LookupResponse>($"FinancialVoucherLookups?companyId={Uri.EscapeDataString(CurrentSession.Company_ID)}&branchId={CurrentSession.Branch_ID}") ?? throw new InvalidOperationException();
            _types = lookup.VoucherTypes.Where(x => x.Is_Active && string.Equals(x.Voucher_Type_Code, "JOURNAL", StringComparison.OrdinalIgnoreCase)).ToList();
            _statuses = lookup.VoucherStatuses.Where(x => x.Is_Active).ToList();
            _currencies = lookup.Currencies.Where(x => x.Is_Active).ToList();
            Bind(_type, _types, nameof(LookupType.Voucher_Type_Name_AR), nameof(LookupType.Voucher_Type_ID));
            Bind(_currency, _currencies, nameof(LookupCurrency.Currency_Name_AR), nameof(LookupCurrency.Currency_ID));
            if (_currencies.FirstOrDefault(x => x.Is_Local_Currency) is { } local) _currency.SelectedValue = local.Currency_ID;
            await LoadAccountsAsync();
            NewEntry();
        }
        catch { ShowSafeError("تعذر تحميل بيانات القيد المرجعية. تحقق من الاتصال والصلاحية."); }
    }

    private async Task LoadAccountsAsync()
    {
        if (_type.SelectedValue is not int typeId) return;
        var response = await ApiService.Client.GetAsync($"FinancialVoucher/accounts?voucherTypeId={typeId}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        _accounts.Clear();
        foreach (var item in document.RootElement.GetProperty("data").EnumerateArray())
        {
            var row = new AccountRow(item.GetProperty("account_ID").GetString() ?? "", item.TryGetProperty("parent_Account_ID", out var parent) ? parent.GetString() : null, item.GetProperty("account_Code").GetString() ?? "", item.GetProperty("account_Name_AR").GetString() ?? "", item.GetProperty("isSelectable").GetBoolean());
            _accounts[row.Id] = row;
        }
    }

    private static void Bind<T>(ComboBox box, List<T> source, string display, string value) { box.DataSource = null; box.DisplayMember = display; box.ValueMember = value; box.DataSource = source; }

    private void NewEntry()
    {
        _voucherId = 0; _isPosted = false; _number.Clear(); _narration.Clear(); _reason.Clear(); _date.Value = DateTime.Today;
        _lines.Rows.Clear(); _lines.Rows.Add(); _lines.Rows.Add(); RecalculateTotals(); ApplyEditability();
    }

    private void SetRowDefaults(DataGridViewRow row)
    {
        row.Cells["Line"].Value = row.Index + 1;
        if (_currency.SelectedValue is int currencyId) row.Cells["Currency"].Value = currencyId;
        row.Cells["Rate"].Value = 1m;
    }

    private static decimal Amount(DataGridViewRow row, string key) => decimal.TryParse(row.Cells[key].Value?.ToString(), out var value) ? value : 0m;

    private void RecalculateRow(DataGridViewRow row)
    {
        if (row.IsNewRow) return;
        row.Cells["Line"].Value = row.Index + 1;
        var debit = Amount(row, "Debit"); var credit = Amount(row, "Credit");
        if (debit > 0m && credit > 0m) { row.Cells["Credit"].Value = 0m; credit = 0m; }
        if (int.TryParse(row.Cells["Currency"].Value?.ToString(), out var currencyId) && _currencies.FirstOrDefault(x => x.Currency_ID == currencyId) is { } currency && currency.Is_Local_Currency)
        {
            row.Cells["Rate"].Value = 1m; row.Cells["Foreign"].Value = 0m;
        }
    }

    private void RecalculateTotals()
    {
        var rows = DataRows().ToList(); var debit = rows.Sum(r => Amount(r, "Debit")); var credit = rows.Sum(r => Amount(r, "Credit"));
        _total.Text = $"إجمالي المدين: {debit:N2}    إجمالي الدائن: {credit:N2}    الفرق: {debit - credit:N2}    {(decimal.Round(debit, 2) == decimal.Round(credit, 2) && debit > 0 ? "✓ متوازن" : "غير متوازن")}";
    }

    private IEnumerable<DataGridViewRow> DataRows() => _lines.Rows.Cast<DataGridViewRow>().Where(x => !x.IsNewRow && x.Tag is string);

    private async Task ChooseAccountAsync(int? rowIndex = null)
    {
        var index = rowIndex ?? (_lines.CurrentCell?.RowIndex ?? -1);
        if (_isPosted || index < 0 || _lines.Rows[index].IsNewRow || _type.SelectedValue is not int) return;
        using var dialog = new AccountTreePicker(_accounts.Values);
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Selected is null) return;
        var row = _lines.Rows[index]; row.Tag = dialog.Selected.Id; row.Cells["Account"].Value = $"{dialog.Selected.Code} - {dialog.Selected.Name}";
        if (row.Cells["Currency"].Value is null && _currency.SelectedValue is int currencyId) row.Cells["Currency"].Value = currencyId;
        RecalculateTotals();
    }

    private void DeleteCurrentLine()
    {
        if (_isPosted || _lines.CurrentCell is null || _lines.Rows[_lines.CurrentCell.RowIndex].IsNewRow) return;
        _lines.Rows.RemoveAt(_lines.CurrentCell.RowIndex); RecalculateTotals();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (_isPosted) { ShowSafeError("لا يمكن تعديل سند مرحّل. أنشئ قيداً عكسياً من دورة الترحيل."); return; }
            var details = BuildDetails(out var error);
            if (error is not null) { ShowSafeError(error); return; }
            var debit = details.Sum(x => x.Debit_Amount); var credit = details.Sum(x => x.Credit_Amount);
            if (string.IsNullOrWhiteSpace(_narration.Text) || details.Count < 2 || debit <= 0 || decimal.Round(debit, 2) != decimal.Round(credit, 2)) { ShowSafeError("البيان وسطران متوازنان على الأقل مطلوبان قبل الحفظ."); return; }
            if (_type.SelectedValue is not int typeId || _currency.SelectedValue is not int currencyId) { ShowSafeError("نوع القيد وعملته مطلوبان."); return; }
            var currency = _currencies.Single(x => x.Currency_ID == currencyId);
            var draftStatus = _statuses.FirstOrDefault(x => string.Equals(x.Voucher_Status_Code, "DRAFT", StringComparison.OrdinalIgnoreCase));
            if (draftStatus is null) { ShowSafeError("حالة المسودة غير مهيأة في البيانات المرجعية."); return; }
            var payload = new { Voucher_Type_ID = typeId, Voucher_Status_ID = draftStatus.Voucher_Status_ID, Voucher_Date = _date.Value.Date, Transaction_Date = _date.Value.Date, Cash_Account_ID = "", Currency_ID = currencyId, Exchange_Rate = currency.Is_Local_Currency ? 1m : currency.Exchange_Rate, Amount = debit, Foreign_Total = 0m, Local_Total = debit, Against_Text = _narration.Text.Trim(), Description = _narration.Text.Trim(), Requires_Approval = true, Details = details };
            HttpResponseMessage response = _voucherId == 0
                ? await ApiService.Client.PostAsJsonAsync("FinancialVoucher", payload)
                : await ApiService.Client.PutAsJsonAsync($"FinancialVoucher/{_voucherId}", new { Voucher_ID = _voucherId, payload.Voucher_Type_ID, payload.Voucher_Status_ID, payload.Voucher_Date, payload.Transaction_Date, payload.Cash_Account_ID, payload.Currency_ID, payload.Exchange_Rate, payload.Amount, payload.Foreign_Total, payload.Local_Total, payload.Against_Text, payload.Description, payload.Requires_Approval, Details = details });
            var responseBody = await response.Content.ReadAsStringAsync();
            var message = ReadSafeMessage(responseBody, response.IsSuccessStatusCode ? "تم حفظ سند القيد بنجاح." : "تعذر حفظ سند القيد.");
            if (!response.IsSuccessStatusCode) { ShowSafeError(message); return; }
            if (_voucherId == 0)
            {
                using var result = JsonDocument.Parse(responseBody);
                _voucherId = result.RootElement.GetProperty("voucher_ID").GetInt64(); _number.Text = result.RootElement.GetProperty("voucher_No").GetString() ?? "";
            }
            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch { ShowSafeError("تعذر إكمال الحفظ حالياً."); }
    }

    private List<LineDto> BuildDetails(out string? error)
    {
        error = null; var result = new List<LineDto>();
        foreach (var row in _lines.Rows.Cast<DataGridViewRow>().Where(x => !x.IsNewRow && (x.Tag is not null || !string.IsNullOrWhiteSpace(x.Cells["Account"].Value?.ToString()))))
        {
            if (row.Tag is not string accountId || !_accounts.TryGetValue(accountId, out var account) || !account.Selectable) { error = "اختر حساباً نهائياً ونشطاً من شجرة الحسابات لكل سطر."; return result; }
            if (!int.TryParse(row.Cells["Currency"].Value?.ToString(), out var currencyId) || !_currencies.Any(x => x.Currency_ID == currencyId)) { error = "عملة كل سطر مطلوبة ونشطة."; return result; }
            var debit = Amount(row, "Debit"); var credit = Amount(row, "Credit"); var rate = Amount(row, "Rate"); var foreign = Amount(row, "Foreign");
            if ((debit > 0 && credit > 0) || (debit <= 0 && credit <= 0) || rate <= 0 || foreign < 0) { error = "كل سطر يجب أن يكون مديناً أو دائناً فقط، بسعر صرف صحيح."; return result; }
            result.Add(new LineDto { Line_No = result.Count + 1, Account_ID = accountId, Currency_ID = currencyId, Exchange_Rate = rate, Foreign_Amount = foreign, Local_Amount = debit + credit, Debit_Amount = debit, Credit_Amount = credit, Reference_No = row.Cells["Reference"].Value?.ToString(), Description = row.Cells["Description"].Value?.ToString(), Line_Type = 2 });
        }
        return result;
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(_number.Text)) { _number.ReadOnly = false; _number.Focus(); return; }
        try
        {
            if (_type.SelectedValue is not int typeId) return;
            var response = await ApiService.Client.GetAsync($"FinancialVoucher/ByNumber?voucherNumber={Uri.EscapeDataString(_number.Text.Trim())}&voucherTypeId={typeId}");
            if (!response.IsSuccessStatusCode) { ShowSafeError(ReadSafeMessage(await response.Content.ReadAsStringAsync(), "لم يتم العثور على السند.")); return; }
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync()); var data = document.RootElement.GetProperty("data");
            _voucherId = data.GetProperty("voucher_ID").GetInt64(); _number.Text = data.GetProperty("voucher_No").GetString() ?? ""; _number.ReadOnly = true; _narration.Text = data.TryGetProperty("against_Text", out var narration) ? narration.GetString() ?? "" : ""; _isPosted = data.TryGetProperty("is_Posted", out var posted) && posted.GetBoolean();
            _date.Value = data.GetProperty("voucher_Date").GetDateTime(); if (data.TryGetProperty("currency_ID", out var headerCurrency)) _currency.SelectedValue = headerCurrency.GetInt32();
            _lines.Rows.Clear();
            foreach (var line in data.GetProperty("details").EnumerateArray())
            {
                var accountId = line.GetProperty("account_ID").GetString() ?? ""; var rowIndex = _lines.Rows.Add(); var row = _lines.Rows[rowIndex]; row.Tag = accountId;
                row.Cells["Account"].Value = _accounts.TryGetValue(accountId, out var account) ? $"{account.Code} - {account.Name}" : accountId;
                row.Cells["Line"].Value = rowIndex + 1; row.Cells["Currency"].Value = line.GetProperty("currency_ID").GetInt32(); row.Cells["Rate"].Value = line.GetProperty("exchange_Rate").GetDecimal(); row.Cells["Foreign"].Value = line.GetProperty("foreign_Amount").GetDecimal(); row.Cells["Debit"].Value = line.GetProperty("debit_Amount").GetDecimal(); row.Cells["Credit"].Value = line.GetProperty("credit_Amount").GetDecimal(); row.Cells["Reference"].Value = line.TryGetProperty("reference_No", out var reference) ? reference.GetString() : null; row.Cells["Description"].Value = line.TryGetProperty("description", out var description) ? description.GetString() : null;
            }
            ApplyEditability(); RecalculateTotals();
        }
        catch { ShowSafeError("تعذر تحميل سند القيد حالياً."); }
    }

    private void ApplyEditability()
    {
        _lines.ReadOnly = _isPosted; _narration.ReadOnly = _isPosted; _date.Enabled = !_isPosted; _currency.Enabled = !_isPosted;
    }

    private async Task WorkflowAsync(string action, bool needsReason)
    {
        if (_voucherId <= 0) { ShowSafeError("احفظ أو حمّل السند أولاً."); return; }
        if (needsReason && string.IsNullOrWhiteSpace(_reason.Text)) { ShowSafeError("سبب فك الترحيل إلزامي."); _reason.Focus(); return; }
        try
        {
            var response = needsReason ? await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}", new { Reason = _reason.Text.Trim(), Action_Channel = "DESKTOP" }) : await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}", new { Notes = _narration.Text.Trim(), Action_Channel = "DESKTOP" });
            var message = ReadSafeMessage(await response.Content.ReadAsStringAsync(), response.IsSuccessStatusCode ? "تم تنفيذ العملية." : "تعذر تنفيذ العملية.");
            if (!response.IsSuccessStatusCode) { ShowSafeError(message); return; }
            if (action == "post") { _isPosted = true; ApplyEditability(); }
            if (action == "unpost") { _isPosted = false; ApplyEditability(); }
            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch { ShowSafeError("تعذر تنفيذ دورة السند حالياً."); }
    }

    private async Task DeleteVoucherAsync()
    {
        if (_voucherId <= 0) { ShowSafeError("احفظ أو حمّل السند أولاً."); return; }
        if (_isPosted) { ShowSafeError("لا يمكن حذف سند مرحّل؛ استخدم القيد العكسي وفق دورة الترحيل."); return; }
        if (MessageBox.Show("هل تريد حذف السند الحالي؟", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            var response = await ApiService.Client.DeleteAsync($"FinancialVoucher/{_voucherId}");
            var message = ReadSafeMessage(await response.Content.ReadAsStringAsync(), response.IsSuccessStatusCode ? "تم حذف السند." : "تعذر حذف السند.");
            if (!response.IsSuccessStatusCode) { ShowSafeError(message); return; }
            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information); NewEntry();
        }
        catch { ShowSafeError("تعذر حذف السند حالياً."); }
    }

    private async Task PrintAsync()
    {
        if (_voucherId <= 0) { ShowSafeError("احفظ السند أولاً قبل الطباعة."); return; }
        var response = await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/record-print", new { Action_Channel = "DESKTOP" });
        if (!response.IsSuccessStatusCode) { ShowSafeError(ReadSafeMessage(await response.Content.ReadAsStringAsync(), "لا تملك صلاحية طباعة السند.")); return; }
        using var print = new PrintDocument(); print.PrintPage += (_, e) =>
        {
            if (e.Graphics is not { } graphics) return;
            using var font = new Font("Segoe UI", 12);
            graphics.DrawString($"سند قيد يومي\nرقم: {_number.Text}\nتاريخ: {_date.Value:yyyy/MM/dd}\nالبيان: {_narration.Text}\n\n{_total.Text}", font, Brushes.Black, new RectangleF(70, 70, e.MarginBounds.Width, e.MarginBounds.Height));
        };
        using var preview = new PrintPreviewDialog { Document = print, RightToLeft = RightToLeft.Yes, RightToLeftLayout = true }; preview.ShowDialog(this);
    }

    private static string ReadSafeMessage(string responseBody, string fallback)
    {
        try { using var document = JsonDocument.Parse(responseBody); return document.RootElement.TryGetProperty("message", out var message) && !string.IsNullOrWhiteSpace(message.GetString()) ? message.GetString()! : fallback; } catch { return fallback; }
    }
    private void ShowSafeError(string message) => MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private sealed class LineDto { public int Line_No { get; set; } public string Account_ID { get; set; } = ""; public int Currency_ID { get; set; } public decimal Exchange_Rate { get; set; } public decimal Foreign_Amount { get; set; } public decimal Local_Amount { get; set; } public decimal Debit_Amount { get; set; } public decimal Credit_Amount { get; set; } public string? Reference_No { get; set; } public string? Description { get; set; } public byte Line_Type { get; set; } }
    private sealed class LookupResponse { public List<LookupType> VoucherTypes { get; set; } = new(); public List<LookupStatus> VoucherStatuses { get; set; } = new(); public List<LookupCurrency> Currencies { get; set; } = new(); }
    private sealed class LookupType { public int Voucher_Type_ID { get; set; } public string Voucher_Type_Code { get; set; } = ""; public string Voucher_Type_Name_AR { get; set; } = ""; public bool Is_Active { get; set; } }
    private sealed class LookupStatus { public int Voucher_Status_ID { get; set; } public string Voucher_Status_Code { get; set; } = ""; public bool Is_Active { get; set; } }
    private sealed class LookupCurrency { public int Currency_ID { get; set; } public string Currency_Name_AR { get; set; } = ""; public decimal Exchange_Rate { get; set; } = 1m; public bool Is_Local_Currency { get; set; } public bool Is_Active { get; set; } }
    private sealed record AccountRow(string Id, string? ParentId, string Code, string Name, bool Selectable);

    private sealed class AccountTreePicker : Form
    {
        private readonly TreeView _tree = new() { Dock = DockStyle.Fill, HideSelection = false, RightToLeft = RightToLeft.Yes };
        public AccountRow? Selected { get; private set; }
        public AccountTreePicker(IEnumerable<AccountRow> accounts)
        {
            Text = "شجرة الحسابات"; Width = 520; Height = 650; StartPosition = FormStartPosition.CenterParent; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            var byParent = accounts.GroupBy(x => x.ParentId ?? string.Empty).ToDictionary(x => x.Key, x => x.OrderBy(a => a.Code).ToList());
            foreach (var root in byParent.GetValueOrDefault(string.Empty, new())) AddNode(_tree.Nodes, root, byParent);
            _tree.NodeMouseDoubleClick += (_, e) => SelectNode(e.Node); var choose = new Button { Text = "اختيار", Dock = DockStyle.Bottom, Height = 35 }; choose.Click += (_, _) => SelectNode(_tree.SelectedNode); Controls.Add(_tree); Controls.Add(choose);
        }
        private static void AddNode(TreeNodeCollection nodes, AccountRow account, Dictionary<string, List<AccountRow>> children) { var node = nodes.Add($"{account.Code} - {account.Name}"); node.Tag = account; foreach (var child in children.GetValueOrDefault(account.Id, new())) AddNode(node.Nodes, child, children); }
        private void SelectNode(TreeNode? node) { if (node?.Tag is not AccountRow account || !account.Selectable) { MessageBox.Show("اختر حساباً نهائياً قابلاً للترحيل.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } Selected = account; DialogResult = DialogResult.OK; }
    }
}
