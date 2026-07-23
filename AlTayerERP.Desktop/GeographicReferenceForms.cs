using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

public enum GeographicReferenceType { Country, Governorate, City }

public class FrmGeographicReference : Form
{
    private readonly GeographicReferenceType _type;
    private readonly DataGridView _grid = new();
    private readonly TextBox _txtCode = new(), _txtNameAr = new(), _txtNameEn = new(), _txtNotes = new() { Multiline = true };
    private readonly TextBox _txtA = new(), _txtB = new(), _txtC = new(), _txtD = new();
    private readonly ComboBox _cmbCountry = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbGovernorate = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _numSort = new() { Maximum = 99999 };
    private readonly CheckBox _chkActive = new() { Text = "نشط", Checked = true, AutoSize = true };
    private readonly TextBox _txtSearch = new() { PlaceholderText = "بحث بالكود أو الاسم…" };
    private readonly Label _lblCount = new() { AutoSize = true };
    private readonly Dictionary<int, Dictionary<string, JsonElement>> _rows = new();
    private int _selectedId;

    private string Resource => _type switch { GeographicReferenceType.Country => "countries", GeographicReferenceType.Governorate => "governorates", _ => "cities" };

    protected FrmGeographicReference(GeographicReferenceType type)
    {
        _type = type;
        Text = type switch { GeographicReferenceType.Country => "الدول", GeographicReferenceType.Governorate => "المحافظات", _ => "المدن" };
        Width = 1240; Height = 760; MinimumSize = new Size(1000, 640);
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; KeyPreview = true;
        Font = new Font("Segoe UI", 9.5F); BackColor = Color.FromArgb(247, 249, 252);
        Build();
        Load += async (_, _) => await InitializeAsync();
        KeyDown += OnKeyDown;
    }

    private void Build()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12), BackColor = BackColor };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 245));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        shell.Controls.Add(BuildToolbar(), 0, 0);
        shell.Controls.Add(BuildEditor(), 0, 1);
        shell.Controls.Add(BuildGrid(), 0, 2);
        shell.Controls.Add(BuildFooter(), 0, 3);
        Controls.Add(shell);
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(7), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        bar.Controls.Add(MakeButton("جديد  Ctrl+N", (_, _) => ClearForm()));
        bar.Controls.Add(MakeButton("حفظ  Ctrl+S", async (_, _) => await SaveAsync(), true));
        bar.Controls.Add(MakeButton("تعديل", async (_, _) => await SaveAsync()));
        bar.Controls.Add(MakeButton("إيقاف/تفعيل", async (_, _) => await ChangeStatusAsync(), false, true));
        bar.Controls.Add(MakeButton("تحديث  F5", async (_, _) => await LoadRowsAsync()));
        bar.Controls.Add(MakeButton("بحث  Ctrl+F", (_, _) => _txtSearch.Focus()));
        bar.Controls.Add(MakeButton("إغلاق  Esc", (_, _) => Close()));
        return bar;
    }

    private Control BuildEditor()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4 };
        for (int i = 0; i < 4; i++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (int i = 0; i < 4; i++) table.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

        AddField(table, Caption("الكود"), _txtCode, 0, 0);
        AddField(table, Caption("الاسم بالعربية"), _txtNameAr, 1, 0);
        AddField(table, Caption("الاسم بالإنجليزية", false), _txtNameEn, 2, 0);
        AddField(table, "ترتيب الظهور", _numSort, 3, 0);

        if (_type == GeographicReferenceType.Country)
        {
            AddField(table, "رمز ISO2", _txtA, 0, 1); AddField(table, "رمز ISO3", _txtB, 1, 1);
            AddField(table, "مفتاح الاتصال", _txtC, 2, 1); AddField(table, "رمز العملة", _txtD, 3, 1);
            AddField(table, "اسم الجنسية بالعربية", new TextBox { Name = "Nationality" }, 0, 2);
        }
        else
        {
            AddField(table, "الدولة *", _cmbCountry, 0, 1);
            if (_type == GeographicReferenceType.City)
            {
                AddField(table, "المحافظة *", _cmbGovernorate, 1, 1);
                AddField(table, "الرمز البريدي", _txtA, 2, 1);
            }
        }

        // الحالة للعرض فقط؛ الإيقاف وإعادة التفعيل عبر عملية منفصلة ذات سبب وتدقيق.
        _chkActive.Enabled = false;
        AddField(table, "الحالة", _chkActive, 3, 2);
        AddField(table, "ملاحظات", _txtNotes, 0, 3, 4);
        card.Controls.Add(table);
        return card;
    }

    private string Caption(string name, bool required = true) => _type switch
    {
        GeographicReferenceType.Country => name.Replace("الكود", "كود الدولة").Replace("الاسم", "اسم الدولة") + (required ? " *" : ""),
        GeographicReferenceType.Governorate => name.Replace("الكود", "كود المحافظة").Replace("الاسم", "اسم المحافظة") + (required ? " *" : ""),
        _ => name.Replace("الكود", "كود المدينة").Replace("الاسم", "اسم المدينة") + (required ? " *" : "")
    };

    private static void AddField(TableLayoutPanel table, string caption, Control input, int col, int row, int span = 1)
    {
        var box = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), Padding = new Padding(6), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
        input.Dock = DockStyle.Bottom; input.Height = input is TextBox t && t.Multiline ? 48 : 30;
        box.Controls.Add(input);
        box.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(31, 58, 92), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
        table.Controls.Add(box, col, row); if (span > 1) table.SetColumnSpan(box, span);
    }

    private Control BuildGrid()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8) };
        _txtSearch.Dock = DockStyle.Top; _txtSearch.Height = 31; _txtSearch.TextChanged += (_, _) => FilterRows();
        _grid.Dock = DockStyle.Fill; _grid.ReadOnly = true; _grid.AllowUserToAddRows = false; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false; _grid.RowHeadersVisible = false; _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; _grid.SelectionChanged += (_, _) => SelectCurrent();
        card.Controls.Add(_grid); card.Controls.Add(_txtSearch); return card;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill };
        _lblCount.Dock = DockStyle.Left;
        footer.Controls.Add(_lblCount);
        footer.Controls.Add(new Label { Text = "الترابط المعتمد: الدولة ← المحافظة ← المدينة.", Dock = DockStyle.Right, Width = 560, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(75, 85, 99) });
        return footer;
    }

    private static Button MakeButton(string text, EventHandler click, bool primary = false, bool danger = false)
    {
        var b = new Button { Text = text, Width = 140, Height = 33, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White, ForeColor = primary ? Color.White : danger ? Color.Firebrick : Color.FromArgb(8, 55, 112), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderColor = Color.FromArgb(208, 220, 235); b.Click += click; return b;
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
        _cmbCountry.DataSource = rows; _cmbCountry.DisplayMember = nameof(CountryLookup.Country_Name_AR); _cmbCountry.ValueMember = nameof(CountryLookup.Country_ID); _cmbCountry.SelectedIndex = rows.Count > 0 ? 0 : -1;
    }

    private async Task LoadGovernoratesAsync()
    {
        int countryId = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0);
        var rows = countryId > 0 ? await ApiService.Client.GetFromJsonAsync<List<GovernorateLookup>>($"GeographicReferences/governorates?countryId={countryId}&activeOnly=true") ?? new() : new();
        _cmbGovernorate.DataSource = rows; _cmbGovernorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR); _cmbGovernorate.ValueMember = nameof(GovernorateLookup.Governorate_ID);
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            var rows = await ApiService.Client.GetFromJsonAsync<List<Dictionary<string, JsonElement>>>($"GeographicReferences/{Resource}") ?? new();
            _rows.Clear();
            string idKey = _type == GeographicReferenceType.Country ? "Country_ID" : _type == GeographicReferenceType.Governorate ? "Governorate_ID" : "City_ID";
            foreach (var row in rows) _rows[Convert.ToInt32(row[idKey].ToString())] = row;
            _grid.DataSource = rows.Select(DisplayRow).ToList(); _lblCount.Text = $"عدد السجلات: {rows.Count}"; ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل البيانات. نفّذ سكربت المراجع الجغرافية أولًا.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private object DisplayRow(Dictionary<string, JsonElement> row)
    {
        string G(string k) => row.TryGetValue(k, out var v) ? v.ToString() : "";
        string status = G("Is_Active") == "1" || G("Is_Active").Equals("true", StringComparison.OrdinalIgnoreCase) ? "نشط" : "موقوف";
        return _type switch
        {
            GeographicReferenceType.Country => new { ID = G("Country_ID"), الكود = G("Country_Code"), الاسم_العربي = G("Country_Name_AR"), الاسم_الإنجليزي = G("Country_Name_EN"), المفتاح = G("Phone_Code"), العملة = G("Currency_Code"), الحالة = status },
            GeographicReferenceType.Governorate => new { ID = G("Governorate_ID"), الدولة = G("Country_Name_AR"), الكود = G("Governorate_Code"), الاسم_العربي = G("Governorate_Name_AR"), الاسم_الإنجليزي = G("Governorate_Name_EN"), الحالة = status },
            _ => new { ID = G("City_ID"), الدولة = G("Country_Name_AR"), المحافظة = G("Governorate_Name_AR"), الكود = G("City_Code"), الاسم_العربي = G("City_Name_AR"), الاسم_الإنجليزي = G("City_Name_EN"), الرمز_البريدي = G("Postal_Code"), الحالة = status }
        };
    }

    private void FilterRows()
    {
        string q = _txtSearch.Text.Trim();
        foreach (DataGridViewRow row in _grid.Rows) row.Visible = string.IsNullOrWhiteSpace(q) || row.Cells.Cast<DataGridViewCell>().Any(c => (c.Value?.ToString() ?? "").Contains(q, StringComparison.CurrentCultureIgnoreCase));
    }

    private void SelectCurrent()
    {
        if (_grid.SelectedRows.Count == 0) return;
        _selectedId = Convert.ToInt32(_grid.SelectedRows[0].Cells["ID"].Value);
        if (!_rows.TryGetValue(_selectedId, out var row)) return;
        string G(string k) => row.TryGetValue(k, out var v) ? v.ToString() : "";
        bool B(string k) => G(k) == "1" || G(k).Equals("true", StringComparison.OrdinalIgnoreCase);
        _txtCode.Text = G(_type == GeographicReferenceType.Country ? "Country_Code" : _type == GeographicReferenceType.Governorate ? "Governorate_Code" : "City_Code");
        _txtNameAr.Text = G(_type == GeographicReferenceType.Country ? "Country_Name_AR" : _type == GeographicReferenceType.Governorate ? "Governorate_Name_AR" : "City_Name_AR");
        _txtNameEn.Text = G(_type == GeographicReferenceType.Country ? "Country_Name_EN" : _type == GeographicReferenceType.Governorate ? "Governorate_Name_EN" : "City_Name_EN");
        if (_type == GeographicReferenceType.Country) { _txtA.Text = G("ISO2"); _txtB.Text = G("ISO3"); _txtC.Text = G("Phone_Code"); _txtD.Text = G("Currency_Code"); }
        else { _cmbCountry.SelectedValue = Convert.ToInt32(G("Country_ID")); if (_type == GeographicReferenceType.City) { _cmbGovernorate.SelectedValue = Convert.ToInt32(G("Governorate_ID")); _txtA.Text = G("Postal_Code"); } }
        _numSort.Value = decimal.TryParse(G("Sort_Order"), out var s) ? s : 0; _chkActive.Checked = B("Is_Active"); _txtNotes.Text = G("Notes");
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtNameAr.Text)) { MessageBox.Show("الكود والاسم العربي مطلوبان."); return; }
        object dto = _type switch
        {
            GeographicReferenceType.Country => new { Country_ID = _selectedId, Country_Code = _txtCode.Text.Trim(), Country_Name_AR = _txtNameAr.Text.Trim(), Country_Name_EN = _txtNameEn.Text.Trim(), ISO2 = _txtA.Text.Trim(), ISO3 = _txtB.Text.Trim(), Phone_Code = _txtC.Text.Trim(), Currency_Code = _txtD.Text.Trim(), Nationality_Name_AR = "", Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim() },
            GeographicReferenceType.Governorate => new { Governorate_ID = _selectedId, Country_ID = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0), Governorate_Code = _txtCode.Text.Trim(), Governorate_Name_AR = _txtNameAr.Text.Trim(), Governorate_Name_EN = _txtNameEn.Text.Trim(), Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim() },
            _ => new { City_ID = _selectedId, Country_ID = Convert.ToInt32(_cmbCountry.SelectedValue ?? 0), Governorate_ID = Convert.ToInt32(_cmbGovernorate.SelectedValue ?? 0), City_Code = _txtCode.Text.Trim(), City_Name_AR = _txtNameAr.Text.Trim(), City_Name_EN = _txtNameEn.Text.Trim(), Postal_Code = _txtA.Text.Trim(), Sort_Order = (int)_numSort.Value, Is_Active = _chkActive.Checked, Notes = _txtNotes.Text.Trim() }
        };
        var response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}", dto);
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadRowsAsync(); MessageBox.Show("تم الحفظ بنجاح.");
    }

    /// <summary>إيقاف أو إعادة تفعيل المرجع الجغرافي بعملية مستقلة وسبب إلزامي.</summary>
    private async Task ChangeStatusAsync()
    {
        if (_selectedId <= 0) { MessageBox.Show("اختر سجلًا أولًا."); return; }

        var isActive = _chkActive.Checked;
        var action = isActive ? "إيقاف" : "إعادة تفعيل";
        var reason = Microsoft.VisualBasic.Interaction.InputBox(
            $"أدخل سبب {action} السجل (حقل إلزامي للتدقيق):",
            action + " السجل",
            string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            MessageBox.Show($"لا يمكن تنفيذ {action} دون سبب.");
            return;
        }

        HttpResponseMessage response;
        if (isActive)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"GeographicReferences/{Resource}/{_selectedId}")
            {
                Content = JsonContent.Create(new { Reason = reason })
            };
            response = await ApiService.Client.SendAsync(request);
        }
        else
        {
            response = await ApiService.Client.PostAsJsonAsync(
                $"GeographicReferences/{Resource}/{_selectedId}/reactivate",
                new { Reason = reason });
        }

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await LoadRowsAsync();
    }

    private void ClearForm()
    {
        _selectedId = 0; _txtCode.Clear(); _txtNameAr.Clear(); _txtNameEn.Clear(); _txtA.Clear(); _txtB.Clear(); _txtC.Clear(); _txtD.Clear(); _txtNotes.Clear(); _numSort.Value = 0; _chkActive.Checked = true; _grid.ClearSelection();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
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