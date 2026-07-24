using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>شاشة المجموعات التجارية وفق التصميم الموحد والربط المرجعي المعتمد.</summary>
public sealed class FrmTenantGroups : BaseForm, IWorkspaceDirtyAware
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _code = Input(), _nameAr = Input(), _nameEn = Input(), _shortName = Input();
    private readonly ComboBox _type = Combo(), _parent = Combo(), _mainCompany = Combo(), _currency = Combo();
    private readonly ComboBox _country = Combo(), _city = Combo();
    private readonly TextBox _address = Input(), _phone = Input(), _email = Input(), _manager = Input(), _notes = new() { Multiline = true };
    private readonly CheckBox _showInLogin = new() { Text = "تظهر في شاشة اختيار الشركة", Checked = true, AutoSize = true };
    private readonly CheckBox _active = new() { Text = "نشطة", Checked = true, Enabled = false, AutoSize = true };
    private readonly NumericUpDown _sort = new() { Minimum = 0, Maximum = 9999, TextAlign = HorizontalAlignment.Right };
    private readonly TextBox _search = new() { PlaceholderText = "ابحث بالكود أو الاسم أو النوع…" };
    private readonly Label _count = new() { AutoSize = true }, _companiesCount = new() { Text = "0", AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
    private readonly Button _save, _deactivate, _reactivate;
    private List<GroupRow> _groups = new();
    private string? _selectedId;
    private bool _dirty, _binding;

    public bool HasUnsavedChanges => _dirty;

    public FrmTenantGroups()
    {
        Text = "المجموعات التجارية"; Width = 1420; Height = 870; MinimumSize = new Size(1150, 740); StartPosition = FormStartPosition.CenterParent;
        ApplyBaseFormStyle();
        _save = Button("حفظ  Ctrl+S", async (_, _) => await SaveAsync(), true);
        _deactivate = Button("إيقاف", async (_, _) => await ChangeStatusAsync(false), danger: true);
        _reactivate = Button("إعادة تفعيل", async (_, _) => await ChangeStatusAsync(true));
        _type.DataSource = new[] { "مجموعة استثمارية", "مجموعة صناعية", "مجموعة خدمية", "مجموعة قابضة", "أخرى" };
        Build();
        Load += async (_, _) => await InitializeAsync();
        KeyDown += HandleKeys;
        _country.SelectedIndexChanged += async (_, _) => { if (!_binding) await LoadCitiesAsync(); };
        _grid.SelectionChanged += (_, _) => BindSelected();
        _search.TextChanged += (_, _) => Filter();
        foreach (var c in new Control[] { _code, _nameAr, _nameEn, _shortName, _type, _parent, _mainCompany, _currency, _country, _city, _address, _phone, _email, _manager, _notes, _showInLogin, _sort })
        {
            c.TextChanged += (_, _) => MarkDirty();
            if (c is ComboBox cb) cb.SelectedIndexChanged += (_, _) => MarkDirty();
        }
        _showInLogin.CheckedChanged += (_, _) => MarkDirty(); _sort.ValueChanged += (_, _) => MarkDirty();
    }

    private void Build()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(16) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 345)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        shell.Controls.Add(Title("المجموعات التجارية"), 0, 0); shell.Controls.Add(Toolbar(), 0, 1);
        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 8, 0, 8) };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56)); editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        editor.Controls.Add(IdentityCard(), 0, 0); editor.Controls.Add(ContactCard(), 1, 0); shell.Controls.Add(editor, 0, 2);
        _search.Dock = DockStyle.Fill; _search.Margin = new Padding(0, 5, 0, 5); shell.Controls.Add(_search, 0, 3);
        ConfigureGrid(); shell.Controls.Add(Card("قائمة المجموعات التجارية", _grid), 0, 4);
        var footer = new Panel { Dock = DockStyle.Fill }; _count.Dock = DockStyle.Left; footer.Controls.Add(_count);
        footer.Controls.Add(new Label { Text = "ترتبط كل شركة بمجموعة تجارية واحدة، وتستخدم الدولة والمدينة والعملة من القوائم المرجعية.", Dock = DockStyle.Right, Width = 760, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 85, 130) });
        shell.Controls.Add(footer, 0, 5); shell.Controls.Add(CreateSessionStatusStrip(), 0, 6); Controls.Add(shell);
    }

    private Control Toolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(4, 7, 4, 5), BackColor = Color.White };
        bar.Controls.AddRange(new Control[] { Button("جديد  Ctrl+N", (_, _) => ClearForm()), _save, Button("تعديل", async (_, _) => await SaveAsync()), _deactivate, _reactivate, Button("تحديث  F5", async (_, _) => await LoadGroupsAsync()), Button("بحث  Ctrl+F", (_, _) => _search.Focus()), Button("إغلاق  Esc", (_, _) => Close()) });
        return bar;
    }

    private Control IdentityCard()
    {
        var t = FormTable(6);
        AddRow(t, 0, "كود المجموعة *", _code, "اسم المجموعة بالعربية *", _nameAr);
        AddRow(t, 1, "اسم المجموعة بالإنجليزية", _nameEn, "الاسم المختصر *", _shortName);
        AddRow(t, 2, "نوع المجموعة *", _type, "المجموعة الأم", _parent);
        AddRow(t, 3, "الشركة الرئيسية", _mainCompany, "العملة الافتراضية", _currency);
        AddRow(t, 4, "الدولة", _country, "المدينة", _city);
        AddRow(t, 5, "العنوان المختصر", _address, "عدد الشركات التابعة", _companiesCount);
        return Card("بيانات المجموعة التجارية", t);
    }

    private Control ContactCard()
    {
        var t = FormTable(7);
        AddSingle(t, 0, "الهاتف", _phone); AddSingle(t, 1, "البريد الإلكتروني", _email); AddSingle(t, 2, "المدير المسؤول", _manager);
        AddSingle(t, 3, "الحالة", _active); AddSingle(t, 4, "إعدادات شاشة الدخول", _showInLogin); AddSingle(t, 5, "ترتيب الظهور", _sort); AddSingle(t, 6, "ملاحظات", _notes);
        return Card("الاتصال والإدارة", t);
    }

    private async Task InitializeAsync()
    {
        _binding = true;
        try
        {
            await Task.WhenAll(LoadCountriesAsync(), LoadCurrenciesAsync(), LoadCompaniesAsync());
            await LoadGroupsAsync();
        }
        finally { _binding = false; }
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            _binding = true;
            _groups = await ApiService.Client.GetFromJsonAsync<List<GroupRow>>("TenantGroups") ?? new();
            _grid.DataSource = _groups.ToList(); _count.Text = $"عدد السجلات: {_groups.Count}";
            _parent.DataSource = _groups.Where(x => x.Group_ID != _selectedId && x.Is_Active).ToList(); _parent.DisplayMember = nameof(GroupRow.Group_Name_AR); _parent.ValueMember = nameof(GroupRow.Group_ID); _parent.SelectedIndex = -1;
            ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("تعذر تحميل المجموعات التجارية.\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { _binding = false; }
    }

    private async Task LoadCountriesAsync()
    {
        var data = await ApiService.Client.GetFromJsonAsync<List<CountryLookup>>("GeographicReferences/countries?activeOnly=true") ?? new();
        _country.DataSource = data; _country.DisplayMember = nameof(CountryLookup.Country_Name_AR); _country.ValueMember = nameof(CountryLookup.Country_ID); _country.SelectedIndex = -1;
    }

    private async Task LoadCitiesAsync()
    {
        var id = Convert.ToInt32(_country.SelectedValue ?? 0);
        var data = id > 0 ? await ApiService.Client.GetFromJsonAsync<List<CityLookup>>($"GeographicReferences/cities?countryId={id}&activeOnly=true") ?? new() : new();
        _city.DataSource = data; _city.DisplayMember = nameof(CityLookup.City_Name_AR); _city.ValueMember = nameof(CityLookup.City_ID); _city.SelectedIndex = -1;
    }

    private async Task LoadCurrenciesAsync()
    {
        try
        {
            var data = await ApiService.Client.GetFromJsonAsync<List<CurrencyLookup>>("Currencies") ?? new();
            data = data.Where(x => x.Is_Active).ToList(); _currency.DataSource = data; _currency.DisplayMember = nameof(CurrencyLookup.Display_Name); _currency.ValueMember = nameof(CurrencyLookup.Currency_Code); _currency.SelectedIndex = -1;
        }
        catch { _currency.DataSource = null; }
    }

    private async Task LoadCompaniesAsync()
    {
        var data = await ApiService.Client.GetFromJsonAsync<List<CompanyLookup>>("Companies") ?? new();
        _mainCompany.DataSource = data.Where(x => x.Is_Active).ToList(); _mainCompany.DisplayMember = nameof(CompanyLookup.Company_Name_AR); _mainCompany.ValueMember = nameof(CompanyLookup.Company_ID); _mainCompany.SelectedIndex = -1;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_code.Text) || string.IsNullOrWhiteSpace(_nameAr.Text) || string.IsNullOrWhiteSpace(_shortName.Text) || _type.SelectedItem is null)
        { MessageBox.Show("كود المجموعة والاسم العربي والاسم المختصر ونوع المجموعة حقول مطلوبة."); return; }
        var dto = new
        {
            Group_Code = _code.Text.Trim(), Group_Name_AR = _nameAr.Text.Trim(), Group_Name_EN = Text(_nameEn.Text), Short_Name = _shortName.Text.Trim(), Group_Type = _type.Text,
            Parent_Group_ID = _parent.SelectedValue?.ToString(), Main_Company_ID = _mainCompany.SelectedValue?.ToString(), Default_Currency_Code = _currency.SelectedValue?.ToString(),
            Country_Name = (_country.SelectedItem as CountryLookup)?.Country_Name_AR, City_Name = (_city.SelectedItem as CityLookup)?.City_Name_AR, Short_Address = Text(_address.Text),
            Phone = Text(_phone.Text), Email = Text(_email.Text), Manager_Name = Text(_manager.Text), Show_In_Login = _showInLogin.Checked, Sort_Order = (int)_sort.Value, Notes = Text(_notes.Text), Is_Active = true
        };
        var response = string.IsNullOrWhiteSpace(_selectedId) ? await ApiService.Client.PostAsJsonAsync("TenantGroups", dto) : await ApiService.Client.PutAsJsonAsync($"TenantGroups/{_selectedId}", dto);
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadGroupsAsync(); MessageBox.Show("تم حفظ المجموعة التجارية بنجاح.");
    }

    private async Task ChangeStatusAsync(bool reactivate)
    {
        if (string.IsNullOrWhiteSpace(_selectedId)) { MessageBox.Show("اختر مجموعة أولاً."); return; }
        if (reactivate == _active.Checked) return;
        var action = reactivate ? "إعادة تفعيل" : "إيقاف";
        var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} المجموعة:", action, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("السبب إلزامي للتدقيق."); return; }
        HttpResponseMessage response;
        if (reactivate) response = await ApiService.Client.PostAsJsonAsync($"TenantGroups/{_selectedId}/reactivate", new { Reason = reason });
        else { using var request = new HttpRequestMessage(HttpMethod.Delete, $"TenantGroups/{_selectedId}") { Content = JsonContent.Create(new { Reason = reason }) }; response = await ApiService.Client.SendAsync(request); }
        if (!response.IsSuccessStatusCode) { MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        await LoadGroupsAsync();
    }

    private async void BindSelected()
    {
        if (_binding || _grid.SelectedRows.Count == 0) return;
        if (_grid.SelectedRows[0].DataBoundItem is not GroupRow row) return;
        _binding = true;
        try
        {
            _selectedId = row.Group_ID; _code.Text = row.Group_Code; _nameAr.Text = row.Group_Name_AR; _nameEn.Text = row.Group_Name_EN; _shortName.Text = row.Short_Name; _type.Text = row.Group_Type;
            _parent.SelectedValue = row.Parent_Group_ID; _mainCompany.SelectedValue = row.Main_Company_ID; _currency.SelectedValue = row.Default_Currency_Code;
            if (!string.IsNullOrWhiteSpace(row.Country_Name))
            {
                var country = (_country.DataSource as List<CountryLookup>)?.FirstOrDefault(x => x.Country_Name_AR == row.Country_Name); if (country is not null) _country.SelectedValue = country.Country_ID;
                await LoadCitiesAsync(); var city = (_city.DataSource as List<CityLookup>)?.FirstOrDefault(x => x.City_Name_AR == row.City_Name); if (city is not null) _city.SelectedValue = city.City_ID;
            }
            _address.Text = row.Short_Address; _phone.Text = row.Phone; _email.Text = row.Email; _manager.Text = row.Manager_Name; _showInLogin.Checked = row.Show_In_Login; _sort.Value = row.Sort_Order; _notes.Text = row.Notes; _active.Checked = row.Is_Active;
            _companiesCount.Text = row.Companies_Count.ToString(); _dirty = false; UpdateButtons();
        }
        finally { _binding = false; }
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill; _grid.AutoGenerateColumns = false; _grid.AllowUserToAddRows = false; _grid.ReadOnly = true; _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; _grid.MultiSelect = false; _grid.RowHeadersVisible = false;
        _grid.Columns.Add(Col(nameof(GroupRow.Group_Code), "كود المجموعة", 125)); _grid.Columns.Add(Col(nameof(GroupRow.Group_Name_AR), "اسم المجموعة", 230)); _grid.Columns.Add(Col(nameof(GroupRow.Group_Type), "النوع", 160));
        _grid.Columns.Add(Col(nameof(GroupRow.Default_Currency_Code), "العملة", 90)); _grid.Columns.Add(Col(nameof(GroupRow.Country_Name), "الدولة", 130)); _grid.Columns.Add(Col(nameof(GroupRow.City_Name), "المدينة", 130)); _grid.Columns.Add(Col(nameof(GroupRow.Status_Text), "الحالة", 90));
    }

    private void Filter() { var q = _search.Text.Trim(); _grid.DataSource = _groups.Where(x => string.IsNullOrWhiteSpace(q) || ($"{x.Group_Code} {x.Group_Name_AR} {x.Group_Name_EN} {x.Group_Type} {x.Country_Name} {x.City_Name}").Contains(q, StringComparison.CurrentCultureIgnoreCase)).ToList(); }
    private void ClearForm() { _binding = true; try { _selectedId = null; foreach (var t in new[] { _code, _nameAr, _nameEn, _shortName, _address, _phone, _email, _manager, _notes }) t.Clear(); _type.SelectedIndex = 0; _parent.SelectedIndex = -1; _mainCompany.SelectedIndex = -1; _currency.SelectedIndex = -1; _country.SelectedIndex = -1; _city.DataSource = null; _showInLogin.Checked = true; _sort.Value = 0; _active.Checked = true; _companiesCount.Text = "0"; _grid.ClearSelection(); _dirty = false; UpdateButtons(); } finally { _binding = false; } _code.Focus(); }
    private void MarkDirty() { if (!_binding) _dirty = true; }
    private void UpdateButtons() { _deactivate.Enabled = !string.IsNullOrWhiteSpace(_selectedId) && _active.Checked; _reactivate.Enabled = !string.IsNullOrWhiteSpace(_selectedId) && !_active.Checked; _mainCompany.Enabled = !string.IsNullOrWhiteSpace(_selectedId); }
    private void HandleKeys(object? s, KeyEventArgs e) { if (e.Control && e.KeyCode == Keys.N) { ClearForm(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.S) { _ = SaveAsync(); e.SuppressKeyPress = true; } else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.F5) { _ = LoadGroupsAsync(); e.SuppressKeyPress = true; } else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; } }

    private static string? Text(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    private static TextBox Input() => new(); private static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private static Button Button(string text, EventHandler handler, bool primary = false, bool danger = false) { var b = new Button { Text = text, Width = 132, Height = 34, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White, ForeColor = primary ? Color.White : danger ? Color.Firebrick : Color.FromArgb(8, 55, 112) }; b.FlatAppearance.BorderColor = Color.FromArgb(205, 217, 232); b.Click += handler; return b; }
    private static Panel Title(string text) => new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 55, 112), Controls = { new Label { Text = text, Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter } } };
    private static Panel Card(string title, Control body) { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8) }; body.Dock = DockStyle.Fill; p.Controls.Add(body); p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) }); return p; }
    private static TableLayoutPanel FormTable(int rows) { var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = rows, Padding = new Padding(12) }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); for (var i = 0; i < rows; i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); return t; }
    private static void AddRow(TableLayoutPanel t, int row, string c1, Control x1, string c2, Control x2) { t.Controls.Add(Caption(c1), 0, row); t.Controls.Add(Field(x1), 1, row); t.Controls.Add(Caption(c2), 2, row); t.Controls.Add(Field(x2), 3, row); }
    private static void AddSingle(TableLayoutPanel t, int row, string c, Control x) { t.Controls.Add(Caption(c), 0, row); t.Controls.Add(Field(x), 1, row); t.SetColumnSpan(x, 3); }
    private static Label Caption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 58, 92) };
    private static Control Field(Control c) { c.Dock = DockStyle.Fill; c.Margin = new Padding(4, 7, 4, 7); return c; }
    private static DataGridViewTextBoxColumn Col(string property, string header, int width) => new() { DataPropertyName = property, HeaderText = header, Width = width };

    private sealed class GroupRow
    {
        public string Group_ID { get; set; } = string.Empty; public string Group_Code { get; set; } = string.Empty; public string Group_Name_AR { get; set; } = string.Empty; public string Group_Name_EN { get; set; } = string.Empty;
        public string Short_Name { get; set; } = string.Empty; public string Group_Type { get; set; } = string.Empty; public string? Parent_Group_ID { get; set; } public string? Main_Company_ID { get; set; } public string? Default_Currency_Code { get; set; }
        public string? Country_Name { get; set; } public string? City_Name { get; set; } public string? Short_Address { get; set; } public string? Phone { get; set; } public string? Email { get; set; } public string? Manager_Name { get; set; }
        public bool Show_In_Login { get; set; } public int Sort_Order { get; set; } public string? Notes { get; set; } public bool Is_Active { get; set; } public int Companies_Count { get; set; }
        public string Status_Text => Is_Active ? "نشطة" : "موقوفة";
    }
    private sealed class CountryLookup { public int Country_ID { get; set; } public string Country_Name_AR { get; set; } = string.Empty; }
    private sealed class CityLookup { public int City_ID { get; set; } public string City_Name_AR { get; set; } = string.Empty; }
    private sealed class CurrencyLookup { public int Currency_ID { get; set; } public string Currency_Code { get; set; } = string.Empty; public string Currency_Name_AR { get; set; } = string.Empty; public bool Is_Active { get; set; } public string Display_Name => $"{Currency_Code} - {Currency_Name_AR}"; }
    private sealed class CompanyLookup { public string Company_ID { get; set; } = string.Empty; public string Company_Name_AR { get; set; } = string.Empty; public bool Is_Active { get; set; } }
}
