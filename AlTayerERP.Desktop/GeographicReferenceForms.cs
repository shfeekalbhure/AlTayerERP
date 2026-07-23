using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

internal enum GeographicReferenceType { Country, Governorate, City }

/// <summary>محرر موحد للمراجع الجغرافية مع ترابط الدولة ← المحافظة ← المدينة.</summary>
internal class FrmGeographicReference : Form
{
    private readonly GeographicReferenceType _type;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtCode = new();
    private readonly TextBox _txtNameAr = new();
    private readonly TextBox _txtNameEn = new();
    private readonly TextBox _txtExtra1 = new();
    private readonly TextBox _txtExtra2 = new();
    private readonly TextBox _txtNotes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly ComboBox _cmbCountry = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbGovernorate = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _numSort = new() { Maximum = 99999 };
    private readonly CheckBox _chkActive = new() { Text = "نشط", Checked = true, AutoSize = true };
    private readonly TextBox _txtSearch = new() { PlaceholderText = "بحث بالكود أو الاسم…" };
    private readonly Label _lblCount = new() { AutoSize = true };
    private readonly Dictionary<string, JsonElement> _rows = new();
    private int _selectedId;

    private string Resource => _type switch
    {
        GeographicReferenceType.Country => "countries",
        GeographicReferenceType.Governorate => "governorates",
        _ => "cities"
    };

    protected FrmGeographicReference(GeographicReferenceType type)
    {
        _type = type;
        Text = type switch
        {
            GeographicReferenceType.Country => "الدول",
            GeographicReferenceType.Governorate => "المحافظات",
            _ => "المدن"
        };
        Width = 1260;
        Height = 760;
        MinimumSize = new Size(1050, 650);
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(247, 249, 252);
        KeyPreview = true;

        BuildLayout();
        Load += async (_, _) => await InitializeAsync();
        KeyDown += OnFormKeyDown;
    }

    private void BuildLayout()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(14),
            BackColor = BackColor
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 235));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        shell.Controls.Add(BuildToolbar(), 0, 0);
        shell.Controls.Add(BuildEditor(), 0, 1);
        shell.Controls.Add(BuildGrid(), 0, 2);
        shell.Controls.Add(BuildFooter(), 0, 3);
        Controls.Add(shell);
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(8),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        bar.Controls.Add(Button("جديد  Ctrl+N", (_, _) => ClearForm(), true));
        bar.Controls.Add(Button("حفظ  Ctrl+S", async (_, _) => await SaveAsync(), true, primary: true));
        bar.Controls.Add(Button("تعديل", async (_, _) => await SaveAsync(), true));
        bar.Controls.Add(Button("حذف", async (_, _) => await DeleteAsync(), true, danger: true));
        bar.Controls.Add(Button("تحديث  F5", async (_, _) => await LoadRowsAsync(), true));
        bar.Controls.Add(Button("بحث  Ctrl+F", (_, _) => _txtSearch.Focus(), true));
        bar.Controls.Add(Button("إغلاق  Esc", (_, _) => Close(), true));
        return bar;
    }

    private Control BuildEditor()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(14) };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(4) };
        for (var i = 0; i < 4; i++) fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var i = 0; i < 4; i++) fields.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        var codeCaption = _type == GeographicReferenceType.Country ? "كود الدولة *" : _type == GeographicReferenceType.Governorate ? "كود المحافظة *" : "كود المدينة *";
        var arCaption = _type == GeographicReferenceType.Country ? "اسم الدولة بالعربية *" : _type == GeographicReferenceType.Governorate ? "اسم المحافظة بالعربية *" : "اسم المدينة بالعربية *";
        var enCaption = _type == GeographicReferenceType.Country ? "اسم الدولة بالإنجليزية" : _type == GeographicReferenceType.Governorate ? "اسم المحافظة بالإنجليزية" : "اسم المدينة بالإنجليزية";

        AddField(fields, codeCaption, _txtCode, 0, 0);
        AddField(fields, arCaption, _txtNameAr, 1, 0);
        AddField(fields, enCaption, _txtNameEn, 2, 0);
        AddField(fields, "ترتيب الظهور", _numSort, 3, 0);

        if (_type != GeographicReferenceType.Country)
            AddField(fields, "الدولة *", _cmbCountry, 0, 1);
        if (_type == GeographicReferenceType.City)
            AddField(fields, "المحافظة *", _cmbGovernorate, 1, 1);

        if (_type == GeographicReferenceType.Country)
        {
            AddField(fields, "رمز ISO2", _txtExtra1, 0, 1);
            AddField(fields, "رمز ISO3", _txtExtra2, 1, 1);
            AddField(fields, "مفتاح الاتصال", NewText("PhoneCode"), 2, 1);
            AddField(fields, "رمز العملة", NewText("CurrencyCode"), 3, 1);
            AddField(fields, "اسم الجنسية بالعربية", NewText("Nationality"), 0, 2);
        }
        else if (_type == GeographicReferenceType.City)
        {
            AddField(fields, "الرمز البريدي", _txtExtra1, 2, 1);
        }

        AddField(fields, "الحالة", _chkActive, 3, 2);
        AddField(fields, "ملاحظات", _txtNotes, 0, 3, 4);
        card.Controls.Add(fields);
        return card;
    }

    private readonly Dictionary<string, TextBox> _extraInputs = new();
    private TextBox NewText(string key)
    {
        var box = new TextBox();
        _extraInputs[key] = box;
        return box;
    }

    private static void AddField(TableLayoutPanel panel, string caption, Control input, int column, int row, int span = 1)
    {
        var box = new Panel { Dock = DockStyle.Fill, Margin = new Padding(6), Padding = new Padding(6), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
        box.Controls.Add(input);
        input.Dock = DockStyle.Bottom;
        input.Height = input is TextBox text && text.Multiline ? 55 : 31;
        input.Font = new Font("Segoe UI", 9.5F);
        box.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(31, 58, 92), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        panel.Controls.Add(box, column, row);
        if (span > 1) panel.SetColumnSpan(box, span);
    }

    private Control BuildGrid()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
        _txtSearch.Dock = DockStyle.Top;
        _txtSearch.Height = 33;
        _txtSearch.TextChanged += (_, _) => FilterRows();
        card.Controls.Add(_grid);
        card.Controls.Add(_txtSearch);
        _grid.Dock = DockStyle.Fill;
        _grid.Top = 42;
        _grid.AutoGenerateColumns = true;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionChanged += (_, _) => SelectCurrentRow();
        return card;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        _lblCount.Dock = DockStyle.Left;
        footer.Controls.Add(_lblCount);
        footer.Controls.Add(new Label
        {
            Text = "المراجع الجغرافية مترابطة: الدولة ← المحافظة ← المدينة. لا يُحذف سجل مستخدم في مستوى أدنى.",
            Dock = DockStyle.Right,
            Width = 760,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(75, 85, 99)
        });
        return footer;
    }

    private static Button Button(string text, EventHandler click, bool enabled, bool primary = false, bool danger = false)
    {
        var button = new Button { Text = text, Width = 145, Height = 34, Enabled = enabled, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = Color.FromArgb(208, 220, 235);
        button.BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White;
        button.ForeColor = primary ? Color.White : danger ? Color.Firebrick : Color.FromArgb(8, 55, 112);
        button.Click += click;
        return button;
    }

    private async Task InitializeAsync()
    {
        if (_type != GeographicReferenceType.Country) await LoadCountriesAsync();
        if (_type == GeographicReferenceType.City) await LoadGovernoratesAsync();
        _cmbCountry.SelectedIndexChanged += async (_, _) => { if (_type == GeographicReferenceType.City) await LoadGovernoratesAsync(); };
        await LoadRowsAsync();
    }

    private async Task LoadCountriesAsync()
    {
        var rows = await ApiService.Client.GetFromJsonAsync<List<CountryLookup>>("GeographicReferences/countries?activeOnly=true") ?? new();
        _cmbCountry.DataSource = rows;
        _cmbCountry.DisplayMember = nameof(CountryLookup.Country_Name_AR);
        _cmbCountry.ValueMember = nameof(CountryLookup.Country_ID);
        _cmbCountry.SelectedIndex = rows.Count > 0 ? 0 : -1;
    }

    private async Task LoadGovernoratesAsync()
    {
        var countryId = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0);
        var rows = countryId > 0
            ? await ApiService.Client.GetFromJsonAsync<List<GovernorateLookup>>($"GeographicReferences/governorates?countryId={countryId}&activeOnly=true") ?? new()
            : new List<GovernorateLookup>();
        _cmbGovernorate.DataSource = rows;
        _cmbGovernorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR);
        _cmbGovernorate.ValueMember = nameof(GovernorateLookup.Governorate_ID);
    }

    private async Task LoadRowsAsync()
    {
        UseWaitCursor = true;
        try
        {
            var rows = await ApiService.Client.GetFromJsonAsync<List<Dictionary<string, JsonElement>>>($"GeographicReferences/{Resource}") ?? new();
            _rows.Clear();
            foreach (var row in rows)
            {
                var idName = _type == GeographicReferenceType.Country ? "Country_ID" : _type == GeographicReferenceType.Governorate ? "Governorate_ID" : "City_ID";
                _rows[row[idName].ToString()] = JsonSerializer.SerializeToElement(row);
            }
            _grid.DataSource = rows.Select(ToDisplayRow).ToList();
            _lblCount.Text = $"عدد السجلات: {rows.Count}";
            ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل البيانات. نفّذ سكربت المراجع الجغرافية وتحقق من API.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    private object ToDisplayRow(Dictionary<string, JsonElement> row)
    {
        string Get(string key) => row.TryGetValue(key, out var value) ? value.ToString() : "";
        return _type switch
        {
            GeographicReferenceType.Country => new { ID = Get("Country_ID"), الكود = Get("Country_Code"), الاسم_العربي = Get("Country_Name_AR"), الاسم_الإنجليزي = Get("Country_Name_EN"), المفتاح = Get("Phone_Code"), العملة = Get("Currency_Code"), الحالة = Get("Is_Active") == "1" || Get("Is_Active").Equals("true", StringComparison.OrdinalIgnoreCase) ? "نشط" : "موقوف" },
            GeographicReferenceType.Governorate => new { ID = Get("Governorate_ID"), الدولة = Get("Country_Name_AR"), الكود = Get("Governorate_Code"), الاسم_العربي = Get("Governorate_Name_AR"), الاسم_الإنجليزي = Get("Governorate_Name_EN"), الحالة = Get("Is_Active") == "1" || Get("Is_Active").Equals("true", StringComparison.OrdinalIgnoreCase) ? "نشط" : "موقوف" },
            _ => new { ID = Get("City_ID"), الدولة = Get("Country_Name_AR"), المحافظة = Get("Governorate_Name_AR"), الكود = Get("City_Code"), الاسم_العربي = Get("City_Name_AR"), الاسم_الإنجليزي = Get("City_Name_EN"), الرمز_البريدي = Get("Postal_Code"), الحالة = Get("Is_Active") == "1" || Get("Is_Active").Equals("true", StringComparison.OrdinalIgnoreCase) ? "نشط" : "موقوف" }
        };
    }

    private void FilterRows()
    {
        var query = _txtSearch.Text.Trim();
        foreach (DataGridViewRow row in _grid.Rows)
            row.Visible = string.IsNullOrWhiteSpace(query) || row.Cells.Cast<DataGridViewCell>().Any(c => (c.Value?.ToString() ?? "").Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void SelectCurrentRow()
    {
        if (_grid.SelectedRows.Count == 0) return;
        _selectedId = Convert.ToInt32(_grid.SelectedRows[0].Cells["ID"].Value);
        if (!_rows.TryGetValue(_selectedId.ToString(), out var row)) return;
        string Get(string key) => row.TryGetProperty(key, out var value) ? value.ToString() : "";
        bool Bool(string key) => Get(key) == "1" || Get(key).Equals("true", StringComparison.OrdinalIgnoreCase);

        if (_type == GeographicReferenceType.Country)
        {
            _txtCode.Text = Get("Country_Code"); _txtNameAr.Text = Get("Country_Name_AR"); _txtNameEn.Text = Get("Country_Name_EN"); _txtExtra1.Text = Get("ISO2"); _txtExtra2.Text = Get("ISO3");
            if (_extraInputs.TryGetValue("PhoneCode", out var phone)) phone.Text = Get("Phone_Code");
            if (_extraInputs.TryGetValue("CurrencyCode", out var currency)) currency.Text = Get("Currency_Code");
            if (_extraInputs.TryGetValue("Nationality", out var nationality)) nationality.Text = Get("Nationality_Name_AR");
        }
        else if (_type == GeographicReferenceType.Governorate)
        {
            _cmbCountry.SelectedValue = Convert.ToInt32(Get("Country_ID")); _txtCode.Text = Get("Governorate_Code"); _txtNameAr.Text = Get("Governorate_Name_AR"); _txtNameEn.Text = Get("Governorate_Name_EN");
        }
        else
        {
            _cmbCountry.SelectedValue = Convert.ToInt32(Get("Country_ID")); _cmbGovernorate.SelectedValue = Convert.ToInt32(Get("Governorate_ID")); _txtCode.Text = Get("City_Code"); _txtNameAr.Text = Get("City_Name_AR"); _txtNameEn.Text = Get("City_Name_EN"); _txtExtra1.Text = Get("Postal_Code");
        }
        _numSort.Value = decimal.TryParse(Get("Sort_Order"), out var sort) ? Math.Min(sort, _numSort.Maximum) : 0;
        _chkActive.Checked = Bool("Is_Active");
        _txtNotes.Text = Get("Notes");
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtNameAr.Text)) { MessageBox.Show("الكود والاسم العربي مطلوبان."); return; }
        object request = _type switch
        {
            GeographicReferenceType.Country => new
            {
                Country_ID = _selectedId, Country_Code = _txtCode.Text.Trim(), Country_Name_AR = _txtNameAr.Text.Trim(), Country_Name_EN = _txtNameEn.Text.Trim(), ISO2 = _txtExtra1.Text.Trim(), ISO3 = _txtExtra2.Text.Trim(),
                Phone_Code = _extraInputs.GetValueOrDefault("PhoneCode")?.Text.Trim(), Currency_Code = _extraInputs.GetValueOrDefault("CurrencyCode")?.Text.Trim(), Nationality_Name_AR = _extraInputs.GetValueOrDefault("Nationality")?.Text.Trim(), Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim()
            },
            GeographicReferenceType.Governorate => new { Governorate_ID = _selectedId, Country_ID = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0), Governorate_Code = _txtCode.Text.Trim(), Governorate_Name_AR = _txtNameAr.Text.Trim(), Governorate_Name_EN = _txtNameEn.Text.Trim(), Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim() },
            _ => new { City_ID = _selectedId, Country_ID = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0), Governorate_ID = Convert.ToInt32(_cmbGovernorate.SelectedValue ?? 0), City_Code = _txtCode.Text.Trim(), City_Name_AR = _txtNameAr.Text.Trim(), City_Name_EN = _txtNameEn.Text.Trim(), Postal_Code = _txtExtra1.Text.Trim(), Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim() }
        };
        var response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}", request);
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadRowsAsync();
        MessageBox.Show("تم الحفظ بنجاح.");
    }

    private async Task DeleteAsync()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر سجلًا أولًا."); return; }
        if (MessageBox.Show("هل تريد حذف السجل المحدد؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var response = await ApiService.Client.DeleteAsync($"GeographicReferences/{Resource}/{_selectedId}");
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحذف", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadRowsAsync();
    }

    private void ClearForm()
    {
        _selectedId = 0; _txtCode.Clear(); _txtNameAr.Clear(); _txtNameEn.Clear(); _txtExtra1.Clear(); _txtExtra2.Clear(); _txtNotes.Clear(); _numSort.Value = 0; _chkActive.Checked = true;
        foreach (var input in _extraInputs.Values) input.Clear();
        _grid.ClearSelection();
        _txtCode.Focus();
    }

    private void OnFormKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.N) { ClearForm(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.S) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.F) { _txtSearch.Focus(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    private sealed class CountryLookup { public int Country_ID { get; set; } public string Country_Name_AR { get; set; } = ""; }
    private sealed class GovernorateLookup { public int Governorate_ID { get; set; } public string Governorate_Name_AR { get; set; } = ""; }
}

public sealed class FrmCountries : FrmGeographicReference { public FrmCountries() : base(GeographicReferenceType.Country) { } }
public sealed class FrmGovernorates : FrmGeographicReference { public FrmGovernorates() : base(GeographicReferenceType.Governorate) { } }
public sealed class FrmCities : FrmGeographicReference { public FrmCities() : base(GeographicReferenceType.City) { } }
