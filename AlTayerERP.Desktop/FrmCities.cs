using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Drawing.Printing;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>شاشة مستقلة للمدن؛ تصميمها في FrmCities.Designer.cs.</summary>
public partial class FrmCities : BaseForm
{
    private enum EditorMode { View, New, Edit }
    private readonly BindingSource _rows = new();
    private List<CityRow> _allRows = new();
    private int _selectedId;
    private EditorMode _editorMode = EditorMode.View;

    public FrmCities()
    {
        InitializeComponent(); ApplyBaseFormStyle(); KeyPreview = true;
        cmbGovernorate.DropDownStyle = ComboBoxStyle.DropDownList; cmbFilterGovernorate.DropDownStyle = ComboBoxStyle.DropDownList; cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" }); cmbFilterStatus.SelectedIndex = 0; chkIsActive.Enabled = false;
        WireEvents(); SetEditorMode(EditorMode.View); Load += async (_, _) => await InitializeAsync(); KeyDown += HandleKeys;
    }

    private void WireEvents()
    {
        btnNew.Click += (_, _) => StartNew(); btnSave.Click += async (_, _) => await SaveAsync();
        btnEdit.Click += async (_, _) => { if (_editorMode == EditorMode.New) await CancelChangesAsync(); else BeginEdit(); };
        btnCancel.Click += async (_, _) => await CancelChangesAsync(); btnReset.Click += (_, _) => ResetCurrentInput(); btnRefresh.Click += async (_, _) => await LoadRowsAsync(); btnSearch.Click += (_, _) => txtSearch.Focus(); btnPrint.Click += (_, _) => PrintSelected(); btnClose.Click += (_, _) => Close();
        btnApplyFilter.Click += (_, _) => ApplyFilter(); txtSearch.TextChanged += (_, _) => ApplyFilter(); cmbFilterGovernorate.SelectedIndexChanged += (_, _) => ApplyFilter(); cmbFilterStatus.SelectedIndexChanged += (_, _) => ApplyFilter(); dgvCities.SelectionChanged += (_, _) => LoadSelected();
    }

    private async Task InitializeAsync() { await LoadGovernoratesAsync(); await LoadRowsAsync(); }

    private async Task LoadGovernoratesAsync()
    {
        try
        {
            var governorates = await ApiService.Client.GetFromJsonAsync<List<GovernorateLookup>>("GeographicReferences/governorates?activeOnly=true") ?? new();
            cmbGovernorate.DataSource = governorates; cmbGovernorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR); cmbGovernorate.ValueMember = nameof(GovernorateLookup.Governorate_ID); cmbGovernorate.SelectedIndex = -1;
            var filterItems = new List<GovernorateLookup> { new() { Governorate_ID = 0, Governorate_Name_AR = "الكل" } }; filterItems.AddRange(governorates);
            cmbFilterGovernorate.DataSource = filterItems; cmbFilterGovernorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR); cmbFilterGovernorate.ValueMember = nameof(GovernorateLookup.Governorate_ID); cmbFilterGovernorate.SelectedIndex = 0;
        }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل قائمة المحافظات.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private async Task LoadRowsAsync()
    {
        try { _allRows = await ApiService.Client.GetFromJsonAsync<List<CityRow>>("GeographicReferences/cities") ?? new(); ApplyFilter(); ClearEditor(); }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل بيانات المدن.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ApplyFilter()
    {
        var query = txtSearch.Text.Trim(); var governorateId = Convert.ToInt32(cmbFilterGovernorate.SelectedValue ?? 0); var status = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";
        _rows.DataSource = _allRows.Where(x => (governorateId == 0 || x.Governorate_ID == governorateId) && (status == "الكل" || (status == "نشط" && x.Is_Active) || (status == "موقوف" && !x.Is_Active)) && (string.IsNullOrWhiteSpace(query) || x.City_Code.Contains(query, StringComparison.CurrentCultureIgnoreCase) || x.City_Name_AR.Contains(query, StringComparison.CurrentCultureIgnoreCase) || (x.City_Name_EN ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase) || x.Governorate_Name_AR.Contains(query, StringComparison.CurrentCultureIgnoreCase))).ToList();
        dgvCities.DataSource = _rows; ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        foreach (DataGridViewColumn column in dgvCities.Columns) column.Visible = column.Name is nameof(CityRow.City_ID) or nameof(CityRow.Country_Name_AR) or nameof(CityRow.Governorate_Name_AR) or nameof(CityRow.City_Code) or nameof(CityRow.City_Name_AR) or nameof(CityRow.City_Name_EN) or nameof(CityRow.Status);
        SetHeader(nameof(CityRow.City_ID), "المعرف"); SetHeader(nameof(CityRow.Country_Name_AR), "الدولة"); SetHeader(nameof(CityRow.Governorate_Name_AR), "المحافظة"); SetHeader(nameof(CityRow.City_Code), "كود المدينة"); SetHeader(nameof(CityRow.City_Name_AR), "الاسم بالعربية"); SetHeader(nameof(CityRow.City_Name_EN), "الاسم بالإنجليزية"); SetHeader(nameof(CityRow.Status), "الحالة");
    }
    private void SetHeader(string property, string caption) { if (dgvCities.Columns[property] is { } column) column.HeaderText = caption; }

    private void LoadSelected()
    {
        if (dgvCities.CurrentRow?.DataBoundItem is not CityRow row) return;
        _selectedId = row.City_ID; txtCityCode.Text = row.City_Code; txtCityNameAr.Text = row.City_Name_AR; txtCityNameEn.Text = row.City_Name_EN ?? string.Empty; txtNotes.Text = row.Notes ?? string.Empty; numDisplayOrder.Value = Math.Clamp(row.Sort_Order, (int)numDisplayOrder.Minimum, (int)numDisplayOrder.Maximum); cmbGovernorate.SelectedValue = row.Governorate_ID; chkIsActive.Checked = row.Is_Active; ClearAudit(); SetEditorMode(EditorMode.View);
    }

    private async Task SaveAsync()
    {
        if (_editorMode == EditorMode.View) return;
        var code = txtCityCode.Text.Trim().ToUpperInvariant(); var governorateId = Convert.ToInt32(cmbGovernorate.SelectedValue ?? 0); var governorate = cmbGovernorate.SelectedItem as GovernorateLookup;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(txtCityNameAr.Text) || governorateId <= 0 || governorate is null) { MessageBox.Show("كود المدينة واسمها بالعربية والمحافظة التابعة حقول مطلوبة.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (_allRows.Any(x => x.City_ID != _selectedId && x.Governorate_ID == governorateId && string.Equals(x.City_Code, code, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("كود المدينة \"" + code + "\" مستخدم مسبقاً داخل المحافظة المختارة.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCityCode.Focus(); return; }
        btnSave.Enabled = false;
        try
        {
            var dto = new { City_ID = _selectedId, Country_ID = governorate.Country_ID, Governorate_ID = governorateId, City_Code = code, City_Name_AR = txtCityNameAr.Text.Trim(), City_Name_EN = Empty(txtCityNameEn.Text), Postal_Code = (string?)null, Sort_Order = (int)numDisplayOrder.Value, Notes = Empty(txtNotes.Text) };
            var response = await ApiService.Client.PostAsJsonAsync("GeographicReferences/cities", dto);
            if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            await LoadRowsAsync(); MessageBox.Show("تم الحفظ بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show("تعذر الاتصال بالخادم أثناء الحفظ.\n" + ex.Message, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { if (_editorMode != EditorMode.View) btnSave.Enabled = true; }
    }

    private void BeginEdit() { if (_selectedId <= 0) { MessageBox.Show("اختر مدينة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; } SetEditorMode(EditorMode.Edit); txtCityCode.Focus(); }
    private async Task CancelChangesAsync() { if (_editorMode == EditorMode.New) { ClearEditor(); return; } if (_editorMode == EditorMode.Edit && dgvCities.CurrentRow?.DataBoundItem is CityRow) { LoadSelected(); return; } await Task.CompletedTask; ClearEditor(); }
    private void StartNew() { _selectedId = 0; ClearInputFields(); dgvCities.ClearSelection(); ClearAudit(); SetEditorMode(EditorMode.New); txtCityCode.Focus(); }
    private void ResetCurrentInput() { if (_editorMode == EditorMode.New) StartNew(); else if (_editorMode == EditorMode.Edit) LoadSelected(); }
    private void ClearEditor() { _selectedId = 0; ClearInputFields(); dgvCities.ClearSelection(); ClearAudit(); SetEditorMode(EditorMode.View); }
    private void ClearInputFields() { txtCityCode.Clear(); txtCityNameAr.Clear(); txtCityNameEn.Clear(); txtNotes.Clear(); numDisplayOrder.Value = 0; cmbGovernorate.SelectedIndex = -1; chkIsActive.Checked = true; }
    private void SetEditorMode(EditorMode mode)
    {
        _editorMode = mode; var editable = mode is EditorMode.New or EditorMode.Edit; foreach (var control in new Control[] { txtCityCode, txtCityNameAr, txtCityNameEn, txtNotes, numDisplayOrder, cmbGovernorate }) control.Enabled = editable;
        btnSave.Enabled = editable; btnSave.Text = mode == EditorMode.New ? "✔ حفظ جديد" : mode == EditorMode.Edit ? "✔ حفظ التعديل" : "✔ حفظ"; btnEdit.Text = mode == EditorMode.New ? "↩ تراجع" : "✎ تعديل"; btnCancel.Text = editable ? "✖ تراجع" : "✖ إلغاء"; btnNew.Enabled = mode == EditorMode.View; btnReset.Enabled = editable; btnRefresh.Enabled = mode == EditorMode.View; dgvCities.Enabled = mode == EditorMode.View;
    }
    private void PrintSelected()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر مدينة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var document = new PrintDocument { DocumentName = "بيانات المدينة - " + txtCityNameAr.Text }; document.PrintPage += (_, e) => { using var font = new Font("Segoe UI", 11); e.Graphics.DrawString($"بيانات المدينة\nالكود: {txtCityCode.Text}\nالاسم: {txtCityNameAr.Text}\nالمحافظة: {cmbGovernorate.Text}", font, Brushes.Black, 70, 70); }; using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700, RightToLeft = RightToLeft.Yes }; preview.ShowDialog(this);
    }
    private void HandleKeys(object? sender, KeyEventArgs e) { if (e.KeyCode == Keys.F2 && _editorMode != EditorMode.View) { _ = SaveAsync(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F3 && _editorMode == EditorMode.View) { StartNew(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F4 && _editorMode == EditorMode.View) { BeginEdit(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F5 && _editorMode == EditorMode.View) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F9) { txtSearch.Focus(); e.SuppressKeyPress = true; } }
    private void ClearAudit() { lblCreatedBy.Text = "أنشئ بواسطة: -"; lblCreatedAt.Text = "تاريخ الإنشاء: -"; lblModifiedBy.Text = "عدل بواسطة: -"; lblModifiedAt.Text = "تاريخ التعديل: -"; lblEditCount.Text = "عدد التعديلات: -"; lblPrintCount.Text = "عدد مرات الطباعة: -"; }
    private static string? Empty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private sealed class GovernorateLookup { public int Governorate_ID { get; set; } public int Country_ID { get; set; } public string Governorate_Name_AR { get; set; } = string.Empty; }
    private sealed class CityRow { public int City_ID { get; set; } public int Country_ID { get; set; } public string Country_Name_AR { get; set; } = string.Empty; public int Governorate_ID { get; set; } public string Governorate_Name_AR { get; set; } = string.Empty; public string City_Code { get; set; } = string.Empty; public string City_Name_AR { get; set; } = string.Empty; public string? City_Name_EN { get; set; } public int Sort_Order { get; set; } public bool Is_Active { get; set; } public string? Notes { get; set; } public string Status => Is_Active ? "نشط" : "موقوف"; }
}
