using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Drawing.Printing;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>شاشة مستقلة للمحافظات؛ تصميمها في FrmGovernorates.Designer.cs.</summary>
public partial class FrmGovernorates : BaseForm
{
    private enum EditorMode { View, New, Edit }

    private readonly BindingSource _rows = new();
    private List<GovernorateRow> _allRows = new();
    private int _selectedId;
    private EditorMode _editorMode = EditorMode.View;

    public FrmGovernorates()
    {
        InitializeComponent();
        ApplyBaseFormStyle();
        KeyPreview = true;
        cmbCountry.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterCountry.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" });
        cmbFilterStatus.SelectedIndex = 0;
        chkIsActive.Enabled = false;
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
        btnCancel.Click += async (_, _) => await CancelChangesAsync();
        btnReset.Click += (_, _) => ResetCurrentInput();
        btnRefresh.Click += async (_, _) => await LoadRowsAsync();
        btnSearch.Click += (_, _) => txtSearch.Focus();
        btnPrint.Click += (_, _) => PrintSelected();
        btnClose.Click += (_, _) => Close();
        btnApplyFilter.Click += (_, _) => ApplyFilter();
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        cmbFilterCountry.SelectedIndexChanged += (_, _) => ApplyFilter();
        cmbFilterStatus.SelectedIndexChanged += (_, _) => ApplyFilter();
        dgvGovernorates.SelectionChanged += (_, _) => LoadSelected();
    }

    private async Task InitializeAsync()
    {
        await LoadCountriesAsync();
        await LoadRowsAsync();
    }

    private async Task LoadCountriesAsync()
    {
        try
        {
            var countries = await ApiService.Client.GetFromJsonAsync<List<CountryLookup>>("GeographicReferences/countries?activeOnly=true") ?? new();
            cmbCountry.DataSource = countries;
            cmbCountry.DisplayMember = nameof(CountryLookup.Country_Name_AR);
            cmbCountry.ValueMember = nameof(CountryLookup.Country_ID);
            cmbCountry.SelectedIndex = -1;

            var filterItems = new List<CountryLookup> { new() { Country_ID = 0, Country_Name_AR = "الكل" } };
            filterItems.AddRange(countries);
            cmbFilterCountry.DataSource = filterItems;
            cmbFilterCountry.DisplayMember = nameof(CountryLookup.Country_Name_AR);
            cmbFilterCountry.ValueMember = nameof(CountryLookup.Country_ID);
            cmbFilterCountry.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل قائمة الدول.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            _allRows = await ApiService.Client.GetFromJsonAsync<List<GovernorateRow>>("GeographicReferences/governorates") ?? new();
            ApplyFilter();
            ClearEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل بيانات المحافظات.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyFilter()
    {
        var query = txtSearch.Text.Trim();
        var countryId = Convert.ToInt32(cmbFilterCountry.SelectedValue ?? 0);
        var status = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";
        _rows.DataSource = _allRows.Where(x =>
            (countryId == 0 || x.Country_ID == countryId) &&
            (status == "الكل" || (status == "نشط" && x.Is_Active) || (status == "موقوف" && !x.Is_Active)) &&
            (string.IsNullOrWhiteSpace(query) || x.Governorate_Code.Contains(query, StringComparison.CurrentCultureIgnoreCase) || x.Governorate_Name_AR.Contains(query, StringComparison.CurrentCultureIgnoreCase) || (x.Governorate_Name_EN ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase) || x.Country_Name_AR.Contains(query, StringComparison.CurrentCultureIgnoreCase))).ToList();
        dgvGovernorates.DataSource = _rows;
        ConfigureGrid();
    }

    private void ConfigureGrid()
    {
        foreach (DataGridViewColumn column in dgvGovernorates.Columns)
            column.Visible = column.Name is nameof(GovernorateRow.Governorate_ID) or nameof(GovernorateRow.Country_Name_AR) or nameof(GovernorateRow.Governorate_Code) or nameof(GovernorateRow.Governorate_Name_AR) or nameof(GovernorateRow.Governorate_Name_EN) or nameof(GovernorateRow.Status);
        SetHeader(nameof(GovernorateRow.Governorate_ID), "المعرف");
        SetHeader(nameof(GovernorateRow.Country_Name_AR), "الدولة");
        SetHeader(nameof(GovernorateRow.Governorate_Code), "كود المحافظة");
        SetHeader(nameof(GovernorateRow.Governorate_Name_AR), "الاسم بالعربية");
        SetHeader(nameof(GovernorateRow.Governorate_Name_EN), "الاسم بالإنجليزية");
        SetHeader(nameof(GovernorateRow.Status), "الحالة");
    }

    private void SetHeader(string property, string caption)
    {
        if (dgvGovernorates.Columns[property] is { } column) column.HeaderText = caption;
    }

    private void LoadSelected()
    {
        if (dgvGovernorates.CurrentRow?.DataBoundItem is not GovernorateRow row) return;
        _selectedId = row.Governorate_ID;
        txtGovCode.Text = row.Governorate_Code;
        txtGovNameAr.Text = row.Governorate_Name_AR;
        txtGovNameEn.Text = row.Governorate_Name_EN ?? string.Empty;
        txtNotes.Text = row.Notes ?? string.Empty;
        numDisplayOrder.Value = Math.Clamp(row.Sort_Order, (int)numDisplayOrder.Minimum, (int)numDisplayOrder.Maximum);
        cmbCountry.SelectedValue = row.Country_ID;
        chkIsActive.Checked = row.Is_Active;
        ClearAudit();
        SetEditorMode(EditorMode.View);
    }

    private async Task SaveAsync()
    {
        if (_editorMode == EditorMode.View) return;
        var code = txtGovCode.Text.Trim().ToUpperInvariant();
        var countryId = Convert.ToInt32(cmbCountry.SelectedValue ?? 0);
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(txtGovNameAr.Text) || countryId <= 0)
        {
            MessageBox.Show("كود المحافظة واسمها بالعربية والدولة التابعة حقول مطلوبة.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_allRows.Any(x => x.Governorate_ID != _selectedId && x.Country_ID == countryId && string.Equals(x.Governorate_Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("كود المحافظة \"" + code + "\" مستخدم مسبقاً داخل الدولة المختارة.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtGovCode.Focus();
            return;
        }

        btnSave.Enabled = false;
        try
        {
            var dto = new { Governorate_ID = _selectedId, Country_ID = countryId, Governorate_Code = code, Governorate_Name_AR = txtGovNameAr.Text.Trim(), Governorate_Name_EN = Empty(txtGovNameEn.Text), Sort_Order = (int)numDisplayOrder.Value, Notes = Empty(txtNotes.Text) };
            var response = await ApiService.Client.PostAsJsonAsync("GeographicReferences/governorates", dto);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadRowsAsync();
            MessageBox.Show("تم الحفظ بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر الاتصال بالخادم أثناء الحفظ.\n" + ex.Message, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            if (_editorMode != EditorMode.View) btnSave.Enabled = true;
        }
    }

    private void BeginEdit()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر محافظة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        SetEditorMode(EditorMode.Edit);
        txtGovCode.Focus();
    }

    private async Task CancelChangesAsync()
    {
        if (_editorMode == EditorMode.New) { ClearEditor(); return; }
        if (_editorMode == EditorMode.Edit && dgvGovernorates.CurrentRow?.DataBoundItem is GovernorateRow) { LoadSelected(); return; }
        await Task.CompletedTask;
        ClearEditor();
    }

    private void StartNew()
    {
        _selectedId = 0;
        ClearInputFields();
        dgvGovernorates.ClearSelection();
        ClearAudit();
        SetEditorMode(EditorMode.New);
        txtGovCode.Focus();
    }

    private void ResetCurrentInput()
    {
        if (_editorMode == EditorMode.New) StartNew();
        else if (_editorMode == EditorMode.Edit) LoadSelected();
    }

    private void ClearEditor()
    {
        _selectedId = 0;
        ClearInputFields();
        dgvGovernorates.ClearSelection();
        ClearAudit();
        SetEditorMode(EditorMode.View);
    }

    private void ClearInputFields()
    {
        txtGovCode.Clear(); txtGovNameAr.Clear(); txtGovNameEn.Clear(); txtNotes.Clear();
        numDisplayOrder.Value = 0; cmbCountry.SelectedIndex = -1; chkIsActive.Checked = true;
    }

    private void SetEditorMode(EditorMode mode)
    {
        _editorMode = mode;
        var editable = mode is EditorMode.New or EditorMode.Edit;
        foreach (var control in new Control[] { txtGovCode, txtGovNameAr, txtGovNameEn, txtNotes, numDisplayOrder, cmbCountry }) control.Enabled = editable;
        btnSave.Enabled = editable;
        btnSave.Text = mode == EditorMode.New ? "✔ حفظ جديد" : mode == EditorMode.Edit ? "✔ حفظ التعديل" : "✔ حفظ";
        btnEdit.Text = mode == EditorMode.New ? "↩ تراجع" : "✎ تعديل";
        btnCancel.Text = editable ? "✖ تراجع" : "✖ إلغاء";
        btnNew.Enabled = mode == EditorMode.View;
        btnReset.Enabled = editable;
        btnRefresh.Enabled = mode == EditorMode.View;
        dgvGovernorates.Enabled = mode == EditorMode.View;
    }

    private void PrintSelected()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر محافظة أولاً.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var document = new PrintDocument { DocumentName = "بيانات المحافظة - " + txtGovNameAr.Text };
        document.PrintPage += (_, e) =>
        {
            using var font = new Font("Segoe UI", 11);
            e.Graphics.DrawString($"بيانات المحافظة\nالكود: {txtGovCode.Text}\nالاسم: {txtGovNameAr.Text}\nالدولة: {cmbCountry.Text}", font, Brushes.Black, 70, 70);
        };
        using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700, RightToLeft = RightToLeft.Yes };
        preview.ShowDialog(this);
    }

    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F2 && _editorMode != EditorMode.View) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F3 && _editorMode == EditorMode.View) { StartNew(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F4 && _editorMode == EditorMode.View) { BeginEdit(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5 && _editorMode == EditorMode.View) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F9) { txtSearch.Focus(); e.SuppressKeyPress = true; }
    }

    private void ClearAudit()
    {
        lblCreatedBy.Text = "أنشئ بواسطة: -"; lblCreatedAt.Text = "تاريخ الإنشاء: -";
        lblModifiedBy.Text = "عدل بواسطة: -"; lblModifiedAt.Text = "تاريخ التعديل: -";
        lblEditCount.Text = "عدد التعديلات: -"; lblPrintCount.Text = "عدد مرات الطباعة: -";
    }

    private static string? Empty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class CountryLookup { public int Country_ID { get; set; } public string Country_Name_AR { get; set; } = string.Empty; }
    private sealed class GovernorateRow
    {
        public int Governorate_ID { get; set; }
        public int Country_ID { get; set; }
        public string Country_Name_AR { get; set; } = string.Empty;
        public string Governorate_Code { get; set; } = string.Empty;
        public string Governorate_Name_AR { get; set; } = string.Empty;
        public string? Governorate_Name_EN { get; set; }
        public int Sort_Order { get; set; }
        public bool Is_Active { get; set; }
        public string? Notes { get; set; }
        public string Status => Is_Active ? "نشط" : "موقوف";
    }
}
