using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Drawing.Printing;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop.Forms;

/// <summary>إدارة الدول: نموذج مصمم في CountriesForm.Designer.cs ومتصل بالـ API فقط.</summary>
public partial class CountriesForm : BaseForm
{
    private enum EditorMode { View, New, Edit }

    private readonly BindingSource _rows = new();
    private List<CountryRow> _allRows = new();
    private int _selectedId;
    private string? _selectedCurrencyCode;
    private bool _selectedIsActive;
    private EditorMode _editorMode = EditorMode.View;

    public CountriesForm()
    {
        InitializeComponent();
        ApplyBaseFormStyle();
        KeyPreview = true;
        cmbCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" });
        cmbFilterStatus.SelectedIndex = 0;
        WireEvents();
        SetEditorMode(EditorMode.View);
        Load += async (_, _) => await InitializeAsync();
        KeyDown += HandleKeys;
    }

    private void WireEvents()
    {
        btnNew.Click += (_, _) => StartNew();
        btnSave.Click += async (_, _) => await SaveAsync();
        btnEdit.Click += async (_, _) =>
        {
            if (_editorMode == EditorMode.New) await CancelChangesAsync();
            else BeginEdit();
        };
        btnDeactivate.Click += async (_, _) => await ChangeStatusAsync(false);
        btnReactivate.Click += async (_, _) => await ChangeStatusAsync(true);
        btnCancel.Click += async (_, _) => await CancelChangesAsync();
        btnReset.Click += (_, _) => ResetCurrentInput();
        btnRefresh.Click += async (_, _) => await LoadRowsAsync();
        btnSearch.Click += (_, _) => txtSearch.Focus();
        btnPrint.Click += async (_, _) => await PrintAsync();
        btnClose.Click += (_, _) => Close();
        btnApplyFilter.Click += (_, _) => ApplyFilter();
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        cmbFilterStatus.SelectedIndexChanged += (_, _) => ApplyFilter();
        dgvCountries.SelectionChanged += async (_, _) => await LoadSelectedAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadCurrenciesAsync();
        await LoadRowsAsync();
    }

    private async Task LoadCurrenciesAsync()
    {
        try
        {
            var currencies = await ApiService.Client.GetFromJsonAsync<List<CurrencyRow>>("Currencies") ?? new();
            cmbCurrency.DataSource = currencies.Where(x => x.Is_Active).ToList();
            cmbCurrency.DisplayMember = nameof(CurrencyRow.DisplayName);
            cmbCurrency.ValueMember = nameof(CurrencyRow.Currency_Code);
            cmbCurrency.SelectedIndex = -1;
        }
        catch
        {
            // عدم السماح بإدخال رمز حر؛ تظهر الرسالة عند الحفظ إذا لزم الأمر.
            cmbCurrency.DataSource = null;
        }
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            _allRows = await ApiService.Client.GetFromJsonAsync<List<CountryRow>>("GeographicReferences/countries") ?? new();
            ApplyFilter();
            ClearEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل بيانات الدول.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        var text = txtSearch.Text.Trim();
        var status = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";
        var result = _allRows.Where(x =>
            (string.IsNullOrWhiteSpace(text) || x.Country_Code.Contains(text, StringComparison.CurrentCultureIgnoreCase) || x.Country_Name_AR.Contains(text, StringComparison.CurrentCultureIgnoreCase) || (x.Country_Name_EN ?? string.Empty).Contains(text, StringComparison.CurrentCultureIgnoreCase)) &&
            (status == "الكل" || x.Status == status)).ToList();
        _rows.DataSource = result;
        dgvCountries.DataSource = _rows;
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        foreach (DataGridViewColumn column in dgvCountries.Columns)
            column.Visible = column.Name is nameof(CountryRow.Country_ID) or nameof(CountryRow.Country_Code) or nameof(CountryRow.Country_Name_AR) or nameof(CountryRow.Country_Name_EN) or nameof(CountryRow.ISO2) or nameof(CountryRow.ISO3) or nameof(CountryRow.Phone_Code) or nameof(CountryRow.Currency_Code) or nameof(CountryRow.Status);
        SetHeader(nameof(CountryRow.Country_ID), "المعرف"); SetHeader(nameof(CountryRow.Country_Code), "كود الدولة"); SetHeader(nameof(CountryRow.Country_Name_AR), "الاسم بالعربية"); SetHeader(nameof(CountryRow.Country_Name_EN), "الاسم بالإنجليزية"); SetHeader(nameof(CountryRow.ISO2), "ISO2"); SetHeader(nameof(CountryRow.ISO3), "ISO3"); SetHeader(nameof(CountryRow.Phone_Code), "مفتاح الاتصال"); SetHeader(nameof(CountryRow.Currency_Code), "العملة"); SetHeader(nameof(CountryRow.Status), "الحالة");
    }

    private void SetHeader(string property, string caption)
    {
        if (dgvCountries.Columns[property] is { } column) column.HeaderText = caption;
    }

    private async Task LoadSelectedAsync()
    {
        if (dgvCountries.CurrentRow?.DataBoundItem is not CountryRow row) return;
        _selectedId = row.Country_ID;
        _selectedCurrencyCode = row.Currency_Code;
        _selectedIsActive = row.Is_Active;
        txtCountryCode.Text = row.Country_Code; txtCountryNameAr.Text = row.Country_Name_AR; txtCountryNameEn.Text = row.Country_Name_EN ?? string.Empty;
        txtIso2.Text = row.ISO2 ?? string.Empty; txtIso3.Text = row.ISO3 ?? string.Empty; txtPhoneKey.Text = row.Phone_Code ?? string.Empty; txtNotes.Text = row.Notes ?? string.Empty;
        numDisplayOrder.Value = Math.Clamp(row.Sort_Order, (int)numDisplayOrder.Minimum, (int)numDisplayOrder.Maximum);
        if (!string.IsNullOrWhiteSpace(row.Currency_Code)) cmbCurrency.SelectedValue = row.Currency_Code;
        txtNationality.Text = row.Nationality_Name_AR ?? string.Empty;
        SetEditorMode(EditorMode.View);
        await LoadAuditAsync(row.Country_ID);
    }

    private async Task LoadAuditAsync(int countryId)
    {
        try
        {
            var audit = await ApiService.Client.GetFromJsonAsync<CountryAudit>("GeographicReferences/countries/" + countryId + "/audit-info");
            lblCreatedBy.Text = "أنشئ بواسطة: " + (audit?.Created_By ?? "غير متاح"); lblCreatedAt.Text = "تاريخ الإنشاء: " + FormatDate(audit?.Created_At);
            lblModifiedBy.Text = "عدل بواسطة: " + (audit?.Updated_By ?? "غير متاح"); lblModifiedAt.Text = "تاريخ التعديل: " + FormatDate(audit?.Updated_At);
            lblEditCount.Text = "عدد التعديلات: " + (audit?.Edit_Count ?? 0); lblPrintCount.Text = "عدد مرات الطباعة: " + (audit?.Print_Count ?? 0);
        }
        catch { ClearAudit(); }
    }

    private async Task SaveAsync()
    {
        if (_editorMode == EditorMode.View) return;
        if (string.IsNullOrWhiteSpace(txtCountryCode.Text) || string.IsNullOrWhiteSpace(txtCountryNameAr.Text)) { MessageBox.Show("كود الدولة واسمها بالعربية مطلوبان.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var countryCode = txtCountryCode.Text.Trim().ToUpperInvariant();
        if (_allRows.Any(x => x.Country_ID != _selectedId && string.Equals(x.Country_Code, countryCode, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("كود الدولة \"" + countryCode + "\" مستخدم مسبقاً. أدخل كوداً آخر.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCountryCode.Focus();
            return;
        }
        if (txtIso2.Text.Trim().Length is not 0 and not 2 || txtIso3.Text.Trim().Length is not 0 and not 3) { MessageBox.Show("رمز ISO2 من حرفين وISO3 من ثلاثة أحرف.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        btnSave.Enabled = false;
        try
        {
            var currencyCode = cmbCurrency.SelectedValue?.ToString() ?? _selectedCurrencyCode;
            var dto = new { Country_ID = _selectedId, Country_Code = countryCode, Country_Name_AR = txtCountryNameAr.Text.Trim(), Country_Name_EN = Empty(txtCountryNameEn.Text), ISO2 = Empty(txtIso2.Text)?.ToUpperInvariant(), ISO3 = Empty(txtIso3.Text)?.ToUpperInvariant(), Phone_Code = Empty(txtPhoneKey.Text), Currency_Code = currencyCode, Nationality_Name_AR = Empty(txtNationality.Text), Sort_Order = (int)numDisplayOrder.Value, Notes = Empty(txtNotes.Text) };
            var response = await ApiService.Client.PostAsJsonAsync("GeographicReferences/countries", dto);
            if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            await LoadRowsAsync(); MessageBox.Show("تم الحفظ بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show("تعذر الاتصال بالخادم أثناء الحفظ.\n" + ex.Message, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { btnSave.Enabled = true; }
    }

    private void BeginEdit()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر دولة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        SetEditorMode(EditorMode.Edit);
        txtCountryCode.Focus();
    }

    private async Task CancelChangesAsync()
    {
        if (_editorMode == EditorMode.New) { ClearEditor(); return; }
        if (_editorMode == EditorMode.Edit && dgvCountries.CurrentRow?.DataBoundItem is CountryRow)
        {
            await LoadSelectedAsync();
            return;
        }
        ClearEditor();
    }

    private void StartNew()
    {
        _selectedId = 0; _selectedCurrencyCode = null; _selectedIsActive = true;
        ClearInputFields();
        dgvCountries.ClearSelection();
        ClearAudit();
        SetEditorMode(EditorMode.New);
        txtCountryCode.Focus();
    }

    private void ResetCurrentInput()
    {
        if (_editorMode == EditorMode.New) { StartNew(); return; }
        if (_editorMode == EditorMode.Edit) _ = LoadSelectedAsync();
    }

    private void ClearEditor()
    {
        _selectedId = 0; _selectedCurrencyCode = null; _selectedIsActive = true;
        ClearInputFields();
        dgvCountries.ClearSelection();
        ClearAudit();
        SetEditorMode(EditorMode.View);
    }

    private void ClearInputFields()
    {
        txtCountryCode.Clear(); txtCountryNameAr.Clear(); txtCountryNameEn.Clear(); txtIso2.Clear(); txtIso3.Clear(); txtPhoneKey.Clear(); txtNationality.Clear(); txtNotes.Clear(); numDisplayOrder.Value = 0; cmbCurrency.SelectedIndex = -1;
    }

    private void SetEditorMode(EditorMode mode)
    {
        _editorMode = mode;
        var editable = mode is EditorMode.New or EditorMode.Edit;
        foreach (var control in new Control[] { txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtIso2, txtIso3, txtPhoneKey, txtNationality, txtNotes, numDisplayOrder, cmbCurrency }) control.Enabled = editable;

        btnSave.Enabled = editable;
        btnSave.Text = mode == EditorMode.New ? "حفظ جديد" : mode == EditorMode.Edit ? "حفظ التعديل" : "حفظ";
        btnEdit.Text = mode == EditorMode.New ? "تراجع" : "تعديل";
        btnCancel.Text = editable ? "تراجع" : "إلغاء";
        btnNew.Enabled = mode == EditorMode.View;
        btnReset.Enabled = editable;
        btnRefresh.Enabled = mode == EditorMode.View;
        btnDeactivate.Enabled = mode == EditorMode.View && _selectedId > 0 && _selectedIsActive;
        btnReactivate.Enabled = mode == EditorMode.View && _selectedId > 0 && !_selectedIsActive;
        btnPrint.Enabled = mode == EditorMode.View && _selectedId > 0;
        dgvCountries.Enabled = mode == EditorMode.View;
    }

    private async Task ChangeStatusAsync(bool reactivate)
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر دولة أولاً."); return; }
        var action = reactivate ? "إعادة تفعيل" : "إيقاف";
        var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} الدولة:", action, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("السبب إلزامي للتدقيق."); return; }
        try
        {
            HttpResponseMessage response;
            if (reactivate) response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/countries/{_selectedId}/reactivate", new { Reason = reason });
            else { using var request = new HttpRequestMessage(HttpMethod.Delete, $"GeographicReferences/countries/{_selectedId}") { Content = JsonContent.Create(new { Reason = reason }) }; response = await ApiService.Client.SendAsync(request); }
            if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            await LoadRowsAsync();
        }
        catch (Exception ex) { MessageBox.Show("تعذر الاتصال بالخادم.\n" + ex.Message, "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task PrintAsync()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر دولة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var response = await ApiService.Client.PostAsync("GeographicReferences/countries/" + _selectedId + "/print", null);
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تسجيل الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var document = new PrintDocument { DocumentName = "بيانات الدولة - " + txtCountryNameAr.Text };
        document.PrintPage += (_, e) => { using var font = new Font("Segoe UI", 11); e.Graphics.DrawString($"بيانات الدولة\nالكود: {txtCountryCode.Text}\nالاسم: {txtCountryNameAr.Text}\nالجنسية: {txtNationality.Text}\nISO2: {txtIso2.Text}\nISO3: {txtIso3.Text}", font, Brushes.Black, 70, 70); };
        using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700, RightToLeft = RightToLeft.Yes };
        preview.ShowDialog(this); await LoadAuditAsync(_selectedId);
    }

    private void HandleKeys(object? sender, KeyEventArgs e) { if (e.KeyCode == Keys.F2 && _editorMode != EditorMode.View) { _ = SaveAsync(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F3 && _editorMode == EditorMode.View) { StartNew(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F5 && _editorMode == EditorMode.View) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F9) { txtSearch.Focus(); e.SuppressKeyPress = true; } }
    private void ClearAudit() { lblCreatedBy.Text = "أنشئ بواسطة: -"; lblCreatedAt.Text = "تاريخ الإنشاء: -"; lblModifiedBy.Text = "عدل بواسطة: -"; lblModifiedAt.Text = "تاريخ التعديل: -"; lblEditCount.Text = "عدد التعديلات: 0"; lblPrintCount.Text = "عدد مرات الطباعة: 0"; }
    private static string? Empty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim(); private static string FormatDate(DateTime? value) => value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "-";

    private sealed class CurrencyRow { public string Currency_Code { get; set; } = string.Empty; public string Currency_Name_AR { get; set; } = string.Empty; public bool Is_Active { get; set; } public string DisplayName => $"{Currency_Code} - {Currency_Name_AR}"; }
    private sealed class CountryRow { public int Country_ID { get; set; } public string Country_Code { get; set; } = string.Empty; public string Country_Name_AR { get; set; } = string.Empty; public string? Country_Name_EN { get; set; } public string? ISO2 { get; set; } public string? ISO3 { get; set; } public string? Phone_Code { get; set; } public string? Currency_Code { get; set; } public string? Nationality_Name_AR { get; set; } public int Sort_Order { get; set; } public bool Is_Active { get; set; } public string? Notes { get; set; } public string Status => Is_Active ? "نشط" : "موقوف"; }
    private sealed class CountryAudit { public string? Created_By { get; set; } public DateTime? Created_At { get; set; } public string? Updated_By { get; set; } public DateTime? Updated_At { get; set; } public int Edit_Count { get; set; } public int Print_Count { get; set; } }
}
