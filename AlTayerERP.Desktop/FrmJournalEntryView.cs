using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// عرض القيد الناتج عن السند فقط. القراءة من API ولا يسمح بالتعديل من هذه الشاشة.
/// </summary>
public sealed class FrmJournalEntryView : BaseForm
{
    private readonly string _voucherNo;
    private readonly int _voucherTypeId;
    private readonly HttpClient _client = ApiService.Client;
    private readonly Label _header = new() { Dock = DockStyle.Top, Height = 42, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = false };

    public FrmJournalEntryView(string voucherNo, int voucherTypeId)
    {
        _voucherNo = string.IsNullOrWhiteSpace(voucherNo) ? throw new ArgumentException("رقم السند مطلوب.", nameof(voucherNo)) : voucherNo;
        _voucherTypeId = voucherTypeId;
        Text = "عرض القيد المحاسبي";
        Width = 1000;
        Height = 620;
        ApplyBaseFormStyle();
        BuildLayout();
        Load += async (_, _) => await LoadJournalAsync();
    }

    private void BuildLayout()
    {
        _grid.Columns.Add("Line", "م");
        _grid.Columns.Add("Account", "الحساب");
        _grid.Columns.Add("Description", "البيان");
        _grid.Columns.Add("Debit", "مدين");
        _grid.Columns.Add("Credit", "دائن");
        _grid.Columns.Add("Currency", "العملة");

        var body = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        body.Controls.Add(_grid, 0, 0);
        body.Controls.Add(CreateAuditInfoPanel(), 0, 1);

        var statusHost = new Panel { Dock = DockStyle.Bottom, Height = 25 };
        statusHost.Controls.Add(CreateSessionStatusStrip());
        Controls.Add(body);
        Controls.Add(_header);
        Controls.Add(statusHost);
    }

    private async Task LoadJournalAsync()
    {
        try
        {
            using var response = await _client.GetAsync($"FinancialVoucher/journal-entry/{Uri.EscapeDataString(_voucherNo)}?voucherTypeId={_voucherTypeId}");
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync(), "عرض القيد", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!json.RootElement.TryGetProperty("data", out var data))
                return;

            _header.Text = $"القيد المحاسبي للسند: {_voucherNo} — رقم القيد: {GetString(data, "entry_No")}";
            if (data.TryGetProperty("details", out var details))
            {
                foreach (var detail in details.EnumerateArray())
                    _grid.Rows.Add(GetString(detail, "line_No"), GetString(detail, "account_Name"),
                        GetString(detail, "description"), GetString(detail, "debit_Amount"),
                        GetString(detail, "credit_Amount"), GetString(detail, "currency_Code"));
            }

            SetAuditInfo(new AuditInfoView(GetString(data, "created_By"), GetDate(data, "created_At"),
                GetString(data, "posted_By"), GetDate(data, "posted_At"), 0, 0, "مرحل", true));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"تعذر تحميل القيد المحاسبي.\n{ex.Message}", "عرض القيد", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value.ToString() : string.Empty;
    private static DateTime? GetDate(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetDateTime(out var result) ? result : null;
}
