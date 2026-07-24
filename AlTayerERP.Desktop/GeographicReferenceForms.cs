using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

public enum GeographicReferenceType { Country, Governorate, City }

/// <summary>الشاشة الموحدة للمراجع الجغرافية: الدولة ← المحافظة ← المدينة.</summary>
public class FrmGeographicReference : BaseForm
{
    private readonly GeographicReferenceType _type;
    private readonly DataGridView _grid = new();
    private readonly TextBox _code = Input(), _nameAr = Input(), _nameEn = Input(), _notes = new() { Multiline = true };
    private readonly TextBox _iso2 = Input(), _iso3 = Input(), _phoneCode = Input(), _currencyCode = Input();
    private readonly TextBox _nationalityAr = Input(), _postalCode = Input();
    private readonly ComboBox _country = Combo(), _governorate = Combo();
    private readonly NumericUpDown _sort = new() { Maximum = 99999, TextAlign = HorizontalAlignment.Right };
    private readonly CheckBox _active = new() { Text = "نشط", Checked = true, AutoSize = true, Enabled = false };
    private readonly TextBox _search = new() { PlaceholderText = "ابحث بالكود أو الاسم…" };
    private readonly Label _count = new() { AutoSize = true };
    private readonly Button _save, _deactivate, _reactivate;
    private readonly Dictionary<int, Dictionary<string, JsonElement>> _rows = new();
    private int _selectedId;
    private bool _loadingLookups;

    private string Resource => _type switch { GeographicReferenceType.Country => "countries", GeographicReferenceType.Governorate => "governorates", _ => "cities" };
    private string Singular => _type switch { GeographicReferenceType.Country => "الدولة", GeographicReferenceType.Governorate => "المحافظة", _ => "المدينة" };
    private string ScreenTitle => _type switch { GeographicReferenceType.Country => "الدول", GeographicReferenceType.Governorate => "المحافظات", _ => "المدن" };

    protected FrmGeographicReference(GeographicReferenceType type)
    {
        _type = type;
        base.Text = ScreenTitle;
        Width = 1320; Height = 820; MinimumSize = new Size(1050, 680); StartPosition = FormStartPosition.CenterParent;
        ApplyBaseFormStyle();
        _save = Button("حفظ  Ctrl+S", async (_, _) => await SaveAsync(), true);
        _deactivate = Button("إيقاف", async (_, _) => await ChangeStatusAsync(false), danger: true);
        _reactivate = Button("إعادة تفعيل", async (_, _) => await ChangeStatusAsync(true));
        Build();
        Load += async (_, _) => await InitializeAsync();
        KeyDown += HandleKeys;
    }

    private void Build()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(16) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, _type == GeographicReferenceType.Country ? 300 : 245));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 42)); shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        shell.Controls.Add(Title(ScreenTitle), 0, 0); shell.Controls.Add(Toolbar(), 0, 1); shell.Controls.Add(Editor(), 0, 2);
        _search.Dock = DockStyle.Fill; _search.Margin = new Padding(0, 5, 0, 5); _search.TextChanged += (_, _) => Filter(); shell.Controls.Add(_search, 0, 3);
        shell.Controls.Add(GridCard(), 0, 4); shell.Controls.Add(Footer(), 0, 5); shell.Controls.Add(CreateSessionStatusStrip(), 0, 6); Controls.Add(shell);
    }

    private Control Toolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(4, 7, 4, 5), BackColor = Color.White };
        bar.Controls.AddRange(new Control[]
        {
            Button("جديد  Ctrl+N", (_, _) => ClearForm()), _save, Button("تعديل", async (_, _) => await SaveAsync()),
            _deactivate, _reactivate, Button("تحديث  F5", async (_, _) => await LoadRowsAsync()),
            Button("بحث  Ctrl+F", (_, _) => _search.Focus()), Button("إغلاق  Esc", (_, _) => Close())
        });
        return bar;
    }

    private Control Editor()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(12) };
        for (var i = 0; i < 4; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var i = 0; i < 4; i++) table.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        Field(table, $"كود {Singular} *", _code, 0, 0); Field(table, $"اسم {Singular} بالعربية *", _nameAr, 1, 0);
        Field(table, $"اسم {Singular} بالإنجليزية", _nameEn, 2, 0); Field(table, "ترتيب الظهور", _sort, 3, 0);
        if (_type == GeographicReferenceType.Country)
        {
            Field(table, "رمز ISO2", _iso2, 0, 1); Field(table, "رمز ISO3", _iso3, 1, 1);
            Field(table, "مفتاح الاتصال", _phoneCode, 2, 1); Field(table, "رمز العملة", _currencyCode, 3, 1);
            Field(table, "اسم الجنسية بالعربية", _nationalityAr, 0, 2, 2); Field(table, "الحالة", _active, 3, 2);
        }
        else
        {
            Field(table, "الدولة *", _country, 0, 1);
            if (_type == GeographicReferenceType.City)
            {
                Field(table, "المحافظة *", _governorate, 1, 1); Field(table, "الرمز البريدي", _postalCode, 2, 1);
            }
            Field(table, "الحالة", _active, 3, 2);
        }
        Field(table, "ملاحظات", _notes, 0, 3, 4);
        return Card("بيانات " + Singular, table);
    }

    private Control GridCard()
    {
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionChanged += async (_, _) => await SelectCurrentAsync();
        return Card("قائمة " + ScreenTitle, _grid);
    }

    private Control Footer()
    {
        var panel = new Panel { Dock = DockStyle.Fill }; _count.Dock = DockStyle.Left; panel.Controls.Add(_count);
        panel.Controls.Add(new Label { Text = "الترابط المعتمد: الدولة ← المحافظة ← المدينة. الإيقاف وإعادة التفعيل يتطلبان سبباً ويسجلان في التدقيق.", Dock = DockStyle.Right, Width = 760, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(75, 85, 99) });
        return panel;
    }

    private async Task InitializeAsync()
    {
        _loadingLookups = true;
        try
        {
            if (_type != GeographicReferenceType.Country) await LoadCountriesAsync(false);
            if (_type == GeographicReferenceType.City) await LoadGovernoratesAsync(false);
            _country.SelectedIndexChanged += async (_, _) => { if (!_loadingLookups && _type == GeographicReferenceType.City) await LoadGovernoratesAsync(false); };
            await LoadRowsAsync();
        }
        finally { _loadingLookups = false; }
    }

    private async Task LoadCountriesAsync(bool preserve)
    {
        var selected = preserve ? Convert.ToInt32(_country.SelectedValue ?? 0) : 0;
        var data = await ApiService.Client.GetFromJsonAsync<List<CountryLookup>>("GeographicReferences/countries?activeOnly=true") ?? new();
        _country.DataSource = data; _country.DisplayMember = nameof(CountryLookup.Country_Name_AR); _country.ValueMember = nameof(CountryLookup.Country_ID); _country.SelectedIndex = -1;
        if (selected > 0) _country.SelectedValue = selected;
    }

    private async Task LoadGovernoratesAsync(bool preserve, int requestedId = 0)
    {
        var selected = requestedId > 0 ? requestedId : preserve ? Convert.ToInt32(_governorate.SelectedValue ?? 0) : 0;
        var countryId = Convert.ToInt32(_country.SelectedValue ?? 0);
        var data = countryId > 0 ? await ApiService.Client.GetFromJsonAsync<List<GovernorateLookup>>($"GeographicReferences/governorates?countryId={countryId}&activeOnly=true") ?? new() : new();
        _governorate.DataSource = data; _governorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR); _governorate.ValueMember = nameof(GovernorateLookup.Governorate_ID); _governorate.SelectedIndex = -1;
        if (selected > 0) _governorate.SelectedValue = selected;
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            var rows = await ApiService.Client.GetFromJsonAsync<List<Dictionary<string, JsonElement>>>($"GeographicReferences/{Resource}") ?? new();
            _rows.Clear();
            var idKey = _type == GeographicReferenceType.Country ? "Country_ID" : _type == GeographicReferenceType.Governorate ? "Governorate_ID" : "City_ID";
            foreach (var row in rows) _rows[Convert.ToInt32(row[idKey].ToString())] = row;
            _grid.DataSource = rows.Select(DisplayRow).ToList(); _count.Text = $"عدد السجلات: {rows.Count}"; ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل البيانات. تأكد من تشغيل ترقية المراجع الجغرافية.\n\n" + ex.Message, base.Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private object DisplayRow(Dictionary<string, JsonElement> row)
    {
        string Get(string key) => row.TryGetValue(key, out var value) ? value.ToString() : string.Empty;
        var status = IsTrue(Get("Is_Active")) ? "نشط" : "موقوف";
        return _type switch
        {
            GeographicReferenceType.Country => new { ID = Get("Country_ID"), الكود = Get("Country_Code"), الاسم_العربي = Get("Country_Name_AR"), الاسم_الإنجليزي = Get("Country_Name_EN"), الجنسية = Get("Nationality_Name_AR"), المفتاح = Get("Phone_Code"), العملة = Get("Currency_Code"), الحالة = status },
            GeographicReferenceType.Governorate => new { ID = Get("Governorate_ID"), الدولة = Get("Country_Name_AR"), الكود = Get("Governorate_Code"), الاسم_العربي = Get("Governorate_Name_AR"), الاسم_الإنجليزي = Get("Governorate_Name_EN"), الحالة = status },
            _ => new { ID = Get("City_ID"), الدولة = Get("Country_Name_AR"), المحافظة = Get("Governorate_Name_AR"), الكود = Get("City_Code"), الاسم_العربي = Get("City_Name_AR"), الاسم_الإنجليزي = Get("City_Name_EN"), الرمز_البريدي = Get("Postal_Code"), الحالة = status }
        };
    }

    private async Task SelectCurrentAsync()
    {
        if (_grid.SelectedRows.Count == 0) return;
        _selectedId = Convert.ToInt32(_grid.SelectedRows[0].Cells["ID"].Value);
        if (!_rows.TryGetValue(_selectedId, out var row)) return;
        string Get(string key) => row.TryGetValue(key, out var value) ? value.ToString() : string.Empty;
        _code.Text = Get(_type == GeographicReferenceType.Country ? "Country_Code" : _type == GeographicReferenceType.Governorate ? "Governorate_Code" : "City_Code");
        _nameAr.Text = Get(_type == GeographicReferenceType.Country ? "Country_Name_AR" : _type == GeographicReferenceType.Governorate ? "Governorate_Name_AR" : "City_Name_AR");
        _nameEn.Text = Get(_type == GeographicReferenceType.Country ? "Country_Name_EN" : _type == GeographicReferenceType.Governorate ? "Governorate_Name_EN" : "City_Name_EN");
        _sort.Value = decimal.TryParse(Get("Sort_Order"), out var sort) ? sort : 0; _active.Checked = IsTrue(Get("Is_Active")); _notes.Text = Get("Notes");
        if (_type == GeographicReferenceType.Country)
        {
            _iso2.Text = Get("ISO2"); _iso3.Text = Get("ISO3"); _phoneCode.Text = Get("Phone_Code"); _currencyCode.Text = Get("Currency_Code"); _nationalityAr.Text = Get("Nationality_Name_AR");
        }
        else
        {
            _loadingLookups = true;
            try
            {
                _country.SelectedValue = Convert.ToInt32(Get("Country_ID"));
                if (_type == GeographicReferenceType.City)
                {
                    await LoadGovernoratesAsync(false, Convert.ToInt32(Get("Governorate_ID"))); _postalCode.Text = Get("Postal_Code");
                }
            }
            finally { _loadingLookups = false; }
        }
        UpdateButtons();
    }

    private async Task SaveAsync()
    {
        var validation = ValidateForm();
        if (validation is not null) { MessageBox.Show(validation, base.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        object dto = _type switch
        {
            GeographicReferenceType.Country => new
            {
                Country_ID = _selectedId, Country_Code = _code.Text.Trim().ToUpperInvariant(), Country_Name_AR = _nameAr.Text.Trim(), Country_Name_EN = CleanText(_nameEn.Text),
                ISO2 = CleanText(_iso2.Text)?.ToUpperInvariant(), ISO3 = CleanText(_iso3.Text)?.ToUpperInvariant(), Phone_Code = CleanText(_phoneCode.Text),
                Currency_Code = CleanText(_currencyCode.Text)?.ToUpperInvariant(), Nationality_Name_AR = CleanText(_nationalityAr.Text), Sort_Order = (int)_sort.Value, Notes = CleanText(_notes.Text)
            },
            GeographicReferenceType.Governorate => new
            {
                Governorate_ID = _selectedId, Country_ID = Convert.ToInt32(_country.SelectedValue), Governorate_Code = _code.Text.Trim().ToUpperInvariant(),
                Governorate_Name_AR = _nameAr.Text.Trim(), Governorate_Name_EN = CleanText(_nameEn.Text), Sort_Order = (int)_sort.Value, Notes = CleanText(_notes.Text)
            },
            _ => new
            {
                City_ID = _selectedId, Country_ID = Convert.ToInt32(_country.SelectedValue), Governorate_ID = Convert.ToInt32(_governorate.SelectedValue),
                City_Code = _code.Text.Trim().ToUpperInvariant(), City_Name_AR = _nameAr.Text.Trim(), City_Name_EN = CleanText(_nameEn.Text),
                Postal_Code = CleanText(_postalCode.Text), Sort_Order = (int)_sort.Value, Notes = CleanText(_notes.Text)
            }
        };
        var response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}", dto);
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadRowsAsync(); MessageBox.Show("تم الحفظ بنجاح.");
    }

    private string? ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(_code.Text) || string.IsNullOrWhiteSpace(_nameAr.Text)) return $"كود {Singular} واسمه العربي مطلوبان.";
        if (_type != GeographicReferenceType.Country && Convert.ToInt32(_country.SelectedValue ?? 0) <= 0) return "اختر الدولة.";
        if (_type == GeographicReferenceType.City && Convert.ToInt32(_governorate.SelectedValue ?? 0) <= 0) return "اختر المحافظة.";
        if (_type == GeographicReferenceType.Country)
        {
            if (!string.IsNullOrWhiteSpace(_iso2.Text) && _iso2.Text.Trim().Length != 2) return "رمز ISO2 يجب أن يتكون من حرفين.";
            if (!string.IsNullOrWhiteSpace(_iso3.Text) && _iso3.Text.Trim().Length != 3) return "رمز ISO3 يجب أن يتكون من ثلاثة أحرف.";
            if (!string.IsNullOrWhiteSpace(_currencyCode.Text) && _currencyCode.Text.Trim().Length != 3) return "رمز العملة يجب أن يتكون من ثلاثة أحرف.";
        }
        return null;
    }

    private async Task ChangeStatusAsync(bool reactivate)
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر سجلاً أولاً."); return; }
        if (reactivate == _active.Checked) return;
        var action = reactivate ? "إعادة تفعيل" : "إيقاف";
        var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} السجل:", action, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("السبب إلزامي للتدقيق."); return; }
        HttpResponseMessage response;
        if (reactivate) response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}/{_selectedId}/reactivate", new { Reason = reason });
        else { using var request = new HttpRequestMessage(HttpMethod.Delete, $"GeographicReferences/{Resource}/{_selectedId}") { Content = JsonContent.Create(new { Reason = reason }) }; response = await ApiService.Client.SendAsync(request); }
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadRowsAsync();
    }

    private void Filter()
    {
        var query = _search.Text.Trim();
        foreach (DataGridViewRow row in _grid.Rows)
            row.Visible = string.IsNullOrWhiteSpace(query) || row.Cells.Cast<DataGridViewCell>().Any(c => (c.Value?.ToString() ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ClearForm()
    {
        _selectedId = 0; _code.Clear(); _nameAr.Clear(); _nameEn.Clear(); _notes.Clear(); _iso2.Clear(); _iso3.Clear(); _phoneCode.Clear(); _currencyCode.Clear(); _nationalityAr.Clear(); _postalCode.Clear();
        _sort.Value = 0; _active.Checked = true; _grid.ClearSelection();
        if (_type != GeographicReferenceType.Country) _country.SelectedIndex = -1;
        if (_type == GeographicReferenceType.City) _governorate.SelectedIndex = -1;
        UpdateButtons(); _code.Focus();
    }

    private void UpdateButtons() { _deactivate.Enabled = _selectedId > 0 && _active.Checked; _reactivate.Enabled = _selectedId > 0 && !_active.Checked; }
    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.N) { ClearForm(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.S) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    private static bool IsTrue(string value) => value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    private static string? CleanText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static TextBox Input() => new();
    private static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private static Button Button(string text, EventHandler handler, bool primary = false, bool danger = false)
    {
        var button = new Button { Text = text, Width = 132, Height = 34, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White, ForeColor = primary ? Color.White : danger ? Color.Firebrick : Color.FromArgb(8, 55, 112) };
        button.FlatAppearance.BorderColor = Color.FromArgb(205, 217, 232); button.Click += handler; return button;
    }
    private static Panel Title(string text) => new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 55, 112), Controls = { new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter } } };
    private static Panel Card(string title, Control body) { var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8) }; body.Dock = DockStyle.Fill; panel.Controls.Add(body); panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) }); return panel; }
    private static void Field(TableLayoutPanel table, string caption, Control input, int col, int row, int span = 1)
    {
        var box = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), Padding = new Padding(6), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
        input.Dock = DockStyle.Bottom; input.Height = input is TextBox textBox && textBox.Multiline ? 48 : 30;
        box.Controls.Add(input); box.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(31, 58, 92), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        table.Controls.Add(box, col, row); if (span > 1) table.SetColumnSpan(box, span);
    }

    private sealed class CountryLookup { public int Country_ID { get; set; } public string Country_Name_AR { get; set; } = string.Empty; }
    private sealed class GovernorateLookup { public int Governorate_ID { get; set; } public string Governorate_Name_AR { get; set; } = string.Empty; }
}

public sealed class FrmCountries : FrmGeographicReference { public FrmCountries() : base(GeographicReferenceType.Country) { } }
public sealed class FrmGovernorates : FrmGeographicReference { public FrmGovernorates() : base(GeographicReferenceType.Governorate) { } }
public sealed class FrmCities : FrmGeographicReference { public FrmCities() : base(GeographicReferenceType.City) { } }
