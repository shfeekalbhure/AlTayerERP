using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// بحث موحد عن المستندات المالية ضمن سياق جلسة المستخدم.
/// لا يقبل فرعاً أو سنة أو شركة من الشاشة؛ الخادم يفرضها من ServerSession.
/// </summary>
public sealed class FrmDocumentSearch : BaseForm
{
    private readonly HttpClient _client = ApiService.Client;
    private readonly TextBox _txtNumber = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _cmbType = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false
    };
    private readonly Dictionary<string, int> _typeIds = new(StringComparer.OrdinalIgnoreCase);

    public FrmDocumentSearch()
    {
        Text = "البحث عن المستندات";
        Width = 1050;
        Height = 650;
        ApplyBaseFormStyle();
        BuildLayout();
        Load += async (_, _) => await LoadVoucherTypesAsync();
    }

    private void BuildLayout()
    {
        var filters = new TableLayoutPanel { Dock = DockStyle.Top, Height = 68, Padding = new Padding(12), ColumnCount = 5 };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        filters.Controls.Add(new Label { Text = "نوع المستند", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
        filters.Controls.Add(_cmbType, 1, 0);
        filters.Controls.Add(new Label { Text = "رقم المستند", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 2, 0);
        filters.Controls.Add(_txtNumber, 3, 0);
        var search = new Button { Text = "بحث", Dock = DockStyle.Fill, BackColor = Color.FromArgb(38, 107, 201), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        search.Click += async (_, _) => await SearchAsync();
        filters.Controls.Add(search, 4, 0);

        _grid.Columns.Add("VoucherNo", "رقم المستند");
        _grid.Columns.Add("VoucherType", "نوع المستند");
        _grid.Columns.Add("VoucherDate", "التاريخ");
        _grid.Columns.Add("Status", "الحالة");
        _grid.Columns.Add("JournalNo", "رقم القيد");
        _grid.CellDoubleClick += (_, _) => OpenJournalForSelectedVoucher();

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        body.Controls.Add(_grid, 0, 0);
        body.Controls.Add(CreateAuditInfoPanel(), 0, 1);

        Controls.Add(body);
        Controls.Add(filters);
        AcceptButton = search;
    }

    private async Task LoadVoucherTypesAsync()
    {
        try
        {
            using var response = await _client.GetAsync("FinancialVoucherLookups");
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            if (!json.RootElement.TryGetProperty("voucherTypes", out var voucherTypes))
                return;

            foreach (var type in voucherTypes.EnumerateArray())
            {
                var code = GetString(type, "voucher_Type_Code");
                if (code is not ("RECEIPT" or "PAYMENT" or "JOURNAL"))
                    continue;

                var id = GetInt(type, "voucher_Type_ID");
                _typeIds[code] = id;
                _cmbType.Items.Add(new VoucherTypeItem(code, GetString(type, "voucher_Type_Name_AR")));
            }

            if (_cmbType.Items.Count > 0)
                _cmbType.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل أنواع المستندات من API.\n{ex.Message}", "البحث", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task SearchAsync()
    {
        var number = _txtNumber.Text.Trim();
        if (string.IsNullOrWhiteSpace(number) || _cmbType.SelectedItem is not VoucherTypeItem type || !_typeIds.TryGetValue(type.Code, out var typeId))
        {
            MessageBox.Show("اختر نوع المستند وأدخل رقمه.", "البحث", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            using var response = await _client.GetAsync($"FinancialVoucher/ByNumber?voucherNumber={Uri.EscapeDataString(number)}&voucherTypeId={typeId}");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync(), "البحث", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!json.RootElement.TryGetProperty("data", out var data))
                return;

            _grid.Rows.Clear();
            var rowIndex = _grid.Rows.Add(
                GetString(data, "voucher_No"),
                type.Name,
                GetDate(data, "voucher_Date"),
                GetBool(data, "is_Posted") ? "مرحل" : "غير مرحل",
                GetString(data, "journal_Entry_No"));
            _grid.Rows[rowIndex].Tag = new VoucherResult(GetString(data, "voucher_No"), typeId);
            SetAuditInfo(new AuditInfoView(GetString(data, "created_By"), GetDateTime(data, "created_At"),
                GetString(data, "updated_By"), GetDateTime(data, "updated_At"), GetInt(data, "edit_Count"),
                GetInt(data, "print_Count"), GetBool(data, "is_Posted") ? "مرحل" : "غير مرحل", true));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تنفيذ البحث.\n{ex.Message}", "البحث", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenJournalForSelectedVoucher()
    {
        if (_grid.CurrentRow?.Tag is VoucherResult result)
            new FrmJournalEntryView(result.VoucherNo, result.VoucherTypeId).ShowDialog(this);
    }

    private static string GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : string.Empty;
    private static int GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var result) ? result : 0;
    private static bool GetBool(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    private static string GetDate(JsonElement element, string name) =>
        GetDateTime(element, name)?.ToString("yyyy/MM/dd") ?? "—";
    private static DateTime? GetDateTime(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetDateTime(out var result) ? result : null;

    private sealed record VoucherTypeItem(string Code, string Name)
    {
        public override string ToString() => Name;
    }
    private sealed record VoucherResult(string VoucherNo, int VoucherTypeId);
}
