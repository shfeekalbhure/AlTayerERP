using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;
using System.Drawing.Printing;

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
    private readonly ComboBox _statusFilter = Combo();
    private readonly Label _count = new() { AutoSize = true };
    // حقول التدقيق للعرض فقط؛ تملأ من API عند اختيار دولة.
    private readonly Label _auditCreatedBy = AuditValue(), _auditCreatedAt = AuditValue();
    private readonly Label _auditUpdatedBy = AuditValue(), _auditUpdatedAt = AuditValue();
    private readonly Label _auditEditCount = AuditValue(), _auditPrintCount = AuditValue();
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
        // تتبع شاشة الدول المقاس الأدنى للتصميم المرجعي مع بقاء البطاقة قابلة للتمدد.
        // أما المحافظات والمدن فتحتفظ بمقاسها السابق لأنها تشترك في هذه الفئة.
        Width = _type == GeographicReferenceType.Country ? 1024 : 1180;
        Height = _type == GeographicReferenceType.Country ? 720 : 760;
        if (_type == GeographicReferenceType.Country)
            MinimumSize = new Size(900, 650);
        StartPosition = FormStartPosition.CenterParent;
        ApplyBaseFormStyle();

        _save = ActionButton("حفظ", async (_, _) => await SaveAsync(), primary: true);
        _deactivate = ActionButton("إيقاف", async (_, _) => await ChangeStatusAsync(false), danger: true);
        _reactivate = ActionButton("إعادة تفعيل", async (_, _) => await ChangeStatusAsync(true));

        Build();
        Load += async (_, _) => await InitializeAsync();
        KeyDown += HandleKeys;
    }

    private void Build()
    {
        Controls.Clear();

        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(244, 247, 251)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        // بطاقة الدولة تحتوي خمسة صفوف إضافة إلى الملاحظات؛ الارتفاع القديم كان
        // يقص الحقول الأخيرة عند فتح الشاشة داخل مساحة العمل.
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, _type == GeographicReferenceType.Country ? 330 : 270));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        shell.Controls.Add(new BrandHeaderControl(ScreenTitle), 0, 0);
        shell.Controls.Add(BuildToolbar(), 0, 1);
        shell.Controls.Add(BuildEditor(), 0, 2);

        shell.Controls.Add(BuildSearchPanel(), 0, 3);

        shell.Controls.Add(BuildGridCard(), 0, 4);
        shell.Controls.Add(BuildFooter(), 0, 5);
        shell.Controls.Add(CreateSessionStatusStrip(), 0, 6);
        Controls.Add(shell);
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(3, 5, 3, 3),
            BackColor = Color.White
        };

        bar.Controls.AddRange(new Control[]
        {
            ActionButton("جديد", (_, _) => ClearForm()),
            _save,
            ActionButton("تعديل", async (_, _) => await SaveAsync()),
            ActionButton("إعادة", (_, _) => ClearForm()),
            _deactivate,
            _reactivate,
            ActionButton("طباعة", async (_, _) => await PrintSelectedCountryAsync()),
            ActionButton("بحث", (_, _) => _search.Focus()),
            ActionButton("تحديث", async (_, _) => await LoadRowsAsync()),
            ActionButton("إغلاق", (_, _) => Close())
        });
        return bar;
    }

    private Control BuildEditor()
    {
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = _type == GeographicReferenceType.Country ? 5 : 4,
            Padding = new Padding(12)
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        // ارتفاع موحد كافٍ للحقول من دون استهلاك مساحة الجدول.
        for (var i = 0; i < body.RowCount; i++)
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        AddRow(body, 0, $"كود {Singular} *", _code, $"اسم {Singular} بالعربية *", _nameAr);
        AddRow(body, 1, $"اسم {Singular} بالإنجليزية", _nameEn, "ترتيب الظهور", _sort);

        if (_type == GeographicReferenceType.Country)
        {
            AddRow(body, 2, "رمز ISO2", _iso2, "رمز ISO3", _iso3);
            AddRow(body, 3, "مفتاح الاتصال", _phoneCode, "العملة الرسمية", _currencyCode);
            AddRow(body, 4, "اسم الجنسية بالعربية", _nationalityAr, "الحالة", _active);
        }
        else if (_type == GeographicReferenceType.Governorate)
        {
            AddRow(body, 2, "الدولة *", _country, "الحالة", _active);
            AddFullRow(body, 3, "ملاحظات", _notes);
        }
        else
        {
            AddRow(body, 2, "الدولة *", _country, "المحافظة *", _governorate);
            AddRow(body, 3, "الرمز البريدي", _postalCode, "الحالة", _active);
        }

        if (_type == GeographicReferenceType.Country)
        {
            var wrapper = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            wrapper.Controls.Add(body, 0, 0);
            wrapper.Controls.Add(BuildNotesRow(), 0, 1);
            return Card("بيانات " + Singular, wrapper);
        }

        if (_type == GeographicReferenceType.City)
        {
            var wrapper = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            wrapper.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            wrapper.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            wrapper.Controls.Add(body, 0, 0);
            wrapper.Controls.Add(BuildNotesRow(), 0, 1);
            return Card("بيانات " + Singular, wrapper);
        }

        return Card("بيانات " + Singular, body);
    }

    private Control BuildNotesRow()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12, 2, 12, 4) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.Controls.Add(Caption("ملاحظات"), 0, 0);
        _notes.Dock = DockStyle.Fill;
        table.Controls.Add(_notes, 1, 0);
        return table;
    }

    private Control BuildSearchPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            Padding = new Padding(10, 6, 10, 6),
            BackColor = Color.White,
            RightToLeft = RightToLeft.Yes
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));

        _search.Dock = DockStyle.Fill;
        _search.Margin = new Padding(3, 0, 8, 0);
        _search.TextChanged += (_, _) => Filter();

        _statusFilter.Items.Clear();
        _statusFilter.Items.AddRange(new object[] { "الكل", "نشط", "موقوف" });
        _statusFilter.SelectedIndex = 0;
        _statusFilter.Dock = DockStyle.Fill;
        _statusFilter.Margin = new Padding(3, 0, 8, 0);
        _statusFilter.SelectedIndexChanged += (_, _) => Filter();

        _count.Dock = DockStyle.Fill;
        _count.TextAlign = ContentAlignment.MiddleLeft;
        _count.ForeColor = Color.FromArgb(75, 85, 99);

        panel.Controls.Add(Caption("البحث:"), 0, 0);
        panel.Controls.Add(_search, 1, 0);
        panel.Controls.Add(Caption("الحالة:"), 2, 0);
        panel.Controls.Add(_statusFilter, 3, 0);
        var applyFilter = ActionButton("تطبيق", (_, _) => Filter());
        applyFilter.Width = 74;
        applyFilter.Height = 26;
        applyFilter.Margin = new Padding(3, 0, 3, 0);
        panel.Controls.Add(applyFilter, 4, 0);
        panel.Controls.Add(_count, 5, 0);
        return Card("البحث والتصفية", panel);
    }

    private Control BuildGridCard()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionChanged += async (_, _) => await SelectCurrentAsync();
        return Card("قائمة " + ScreenTitle, _grid);
    }

    private Control BuildFooter()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(0, 4, 0, 0),
            RightToLeft = RightToLeft.Yes
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        panel.Controls.Add(CreateAuditCard("بيانات الإنشاء", "أنشئ بواسطة:", _auditCreatedBy, "تاريخ الإنشاء:", _auditCreatedAt), 0, 0);
        panel.Controls.Add(CreateAuditCard("بيانات التعديل", "عُدّل بواسطة:", _auditUpdatedBy, "تاريخ التعديل:", _auditUpdatedAt), 1, 0);
        panel.Controls.Add(CreateAuditCard("العدادات", "عدد مرات التعديل:", _auditEditCount, "عدد مرات الطباعة:", _auditPrintCount), 2, 0);
        return panel;
    }

    // ينشئ بطاقة تدقيق قراءة فقط بحدود واضحة للحفاظ على اتساق واجهات النظام.
    private static Control CreateAuditCard(string title, string firstCaption, Label firstValue, string secondCaption, Label secondValue)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(7, 2, 7, 2), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        var header = new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) };
        table.Controls.Add(header, 0, 0);
        table.SetColumnSpan(header, 2);
        table.Controls.Add(AuditCaption(firstCaption), 0, 1);
        table.Controls.Add(firstValue, 1, 1);
        table.Controls.Add(AuditCaption(secondCaption), 0, 2);
        table.Controls.Add(secondValue, 1, 2);
        return table;
    }

    private async Task InitializeAsync()
    {
        _loadingLookups = true;
        try
        {
            if (_type != GeographicReferenceType.Country) await LoadCountriesAsync(false);
            if (_type == GeographicReferenceType.City) await LoadGovernoratesAsync(false);
            _country.SelectedIndexChanged += async (_, _) =>
            {
                if (!_loadingLookups && _type == GeographicReferenceType.City)
                    await LoadGovernoratesAsync(false);
            };
            await LoadRowsAsync();
        }
        finally { _loadingLookups = false; }
    }

    private async Task LoadCountriesAsync(bool preserve)
    {
        var selected = preserve ? Convert.ToInt32(_country.SelectedValue ?? 0) : 0;
        var data = await ApiService.Client.GetFromJsonAsync<List<CountryLookup>>("GeographicReferences/countries?activeOnly=true") ?? new();
        _country.DataSource = data;
        _country.DisplayMember = nameof(CountryLookup.Country_Name_AR);
        _country.ValueMember = nameof(CountryLookup.Country_ID);
        _country.SelectedIndex = -1;
        if (selected > 0) _country.SelectedValue = selected;
    }

    private async Task LoadGovernoratesAsync(bool preserve, int requestedId = 0)
    {
        var selected = requestedId > 0 ? requestedId : preserve ? Convert.ToInt32(_governorate.SelectedValue ?? 0) : 0;
        var countryId = Convert.ToInt32(_country.SelectedValue ?? 0);
        var data = countryId > 0
            ? await ApiService.Client.GetFromJsonAsync<List<GovernorateLookup>>($"GeographicReferences/governorates?countryId={countryId}&activeOnly=true") ?? new()
            : new();
        _governorate.DataSource = data;
        _governorate.DisplayMember = nameof(GovernorateLookup.Governorate_Name_AR);
        _governorate.ValueMember = nameof(GovernorateLookup.Governorate_ID);
        _governorate.SelectedIndex = -1;
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
            _grid.DataSource = rows.Select(DisplayRow).ToList();
            _count.Text = $"عدد السجلات: {rows.Count}";
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل البيانات. تأكد من تشغيل ترقية المراجع الجغرافية.\n\n" + ex.Message, base.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
        _sort.Value = decimal.TryParse(Get("Sort_Order"), out var sort) ? sort : 0;
        _active.Checked = IsTrue(Get("Is_Active"));
        _notes.Text = Get("Notes");

        if (_type == GeographicReferenceType.Country)
        {
            _iso2.Text = Get("ISO2");
            _iso3.Text = Get("ISO3");
            _phoneCode.Text = Get("Phone_Code");
            _currencyCode.Text = Get("Currency_Code");
            _nationalityAr.Text = Get("Nationality_Name_AR");
        }
        else
        {
            _loadingLookups = true;
            try
            {
                _country.SelectedValue = Convert.ToInt32(Get("Country_ID"));
                if (_type == GeographicReferenceType.City)
                {
                    await LoadGovernoratesAsync(false, Convert.ToInt32(Get("Governorate_ID")));
                    _postalCode.Text = Get("Postal_Code");
                }
            }
            finally { _loadingLookups = false; }
        }
        UpdateButtons();
        if (_type == GeographicReferenceType.Country)
            await LoadCountryAuditAsync(_selectedId);
    }

    private async Task SaveAsync()
    {
        var validation = ValidateForm();
        if (validation is not null)
        {
            MessageBox.Show(validation, base.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        object dto = _type switch
        {
            GeographicReferenceType.Country => new
            {
                Country_ID = _selectedId,
                Country_Code = _code.Text.Trim().ToUpperInvariant(),
                Country_Name_AR = _nameAr.Text.Trim(),
                Country_Name_EN = CleanText(_nameEn.Text),
                ISO2 = CleanText(_iso2.Text)?.ToUpperInvariant(),
                ISO3 = CleanText(_iso3.Text)?.ToUpperInvariant(),
                Phone_Code = CleanText(_phoneCode.Text),
                Currency_Code = CleanText(_currencyCode.Text)?.ToUpperInvariant(),
                Nationality_Name_AR = CleanText(_nationalityAr.Text),
                Sort_Order = (int)_sort.Value,
                Notes = CleanText(_notes.Text)
            },
            GeographicReferenceType.Governorate => new
            {
                Governorate_ID = _selectedId,
                Country_ID = Convert.ToInt32(_country.SelectedValue),
                Governorate_Code = _code.Text.Trim().ToUpperInvariant(),
                Governorate_Name_AR = _nameAr.Text.Trim(),
                Governorate_Name_EN = CleanText(_nameEn.Text),
                Sort_Order = (int)_sort.Value,
                Notes = CleanText(_notes.Text)
            },
            _ => new
            {
                City_ID = _selectedId,
                Country_ID = Convert.ToInt32(_country.SelectedValue),
                Governorate_ID = Convert.ToInt32(_governorate.SelectedValue),
                City_Code = _code.Text.Trim().ToUpperInvariant(),
                City_Name_AR = _nameAr.Text.Trim(),
                City_Name_EN = CleanText(_nameEn.Text),
                Postal_Code = CleanText(_postalCode.Text),
                Sort_Order = (int)_sort.Value,
                Notes = CleanText(_notes.Text)
            }
        };

        var response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}", dto);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await LoadRowsAsync();
        MessageBox.Show("تم الحفظ بنجاح.");
    }

    private string? ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(_code.Text) || string.IsNullOrWhiteSpace(_nameAr.Text))
            return $"كود {Singular} واسمه العربي مطلوبان.";
        if (_type != GeographicReferenceType.Country && Convert.ToInt32(_country.SelectedValue ?? 0) <= 0)
            return "اختر الدولة.";
        if (_type == GeographicReferenceType.City && Convert.ToInt32(_governorate.SelectedValue ?? 0) <= 0)
            return "اختر المحافظة.";
        if (_type == GeographicReferenceType.Country)
        {
            if (!string.IsNullOrWhiteSpace(_iso2.Text) && _iso2.Text.Trim().Length != 2)
                return "رمز ISO2 يجب أن يتكون من حرفين.";
            if (!string.IsNullOrWhiteSpace(_iso3.Text) && _iso3.Text.Trim().Length != 3)
                return "رمز ISO3 يجب أن يتكون من ثلاثة أحرف.";
            if (!string.IsNullOrWhiteSpace(_currencyCode.Text) && _currencyCode.Text.Trim().Length != 3)
                return "رمز العملة يجب أن يتكون من ثلاثة أحرف.";
        }
        return null;
    }

    private async Task ChangeStatusAsync(bool reactivate)
    {
        if (_selectedId <= 0)
        {
            MessageBox.Show("اختر سجلاً أولاً.");
            return;
        }
        if (reactivate == _active.Checked) return;

        var action = reactivate ? "إعادة تفعيل" : "إيقاف";
        var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} السجل:", action, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason))
        {
            MessageBox.Show("السبب إلزامي للتدقيق.");
            return;
        }

        HttpResponseMessage response;
        if (reactivate)
            response = await ApiService.Client.PostAsJsonAsync($"GeographicReferences/{Resource}/{_selectedId}/reactivate", new { Reason = reason });
        else
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"GeographicReferences/{Resource}/{_selectedId}")
            {
                Content = JsonContent.Create(new { Reason = reason })
            };
            response = await ApiService.Client.SendAsync(request);
        }

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تنفيذ العملية", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await LoadRowsAsync();
    }

    private void Filter()
    {
        var query = _search.Text.Trim();
        var status = _statusFilter.SelectedItem?.ToString() ?? "الكل";
        foreach (DataGridViewRow row in _grid.Rows)
        {
            var matchesText = string.IsNullOrWhiteSpace(query) || row.Cells.Cast<DataGridViewCell>()
                .Any(c => (c.Value?.ToString() ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase));
            var matchesStatus = status == "الكل" ||
                (row.Cells["الحالة"].Value?.ToString() ?? string.Empty) == status;
            row.Visible = matchesText && matchesStatus;
        }
    }

    private void ClearForm()
    {
        _selectedId = 0;
        foreach (var text in new[] { _code, _nameAr, _nameEn, _notes, _iso2, _iso3, _phoneCode, _currencyCode, _nationalityAr, _postalCode })
            text.Clear();
        _sort.Value = 0;
        _active.Checked = true;
        _grid.ClearSelection();
        if (_type != GeographicReferenceType.Country) _country.SelectedIndex = -1;
        if (_type == GeographicReferenceType.City) _governorate.SelectedIndex = -1;
        ClearAuditInfo();
        UpdateButtons();
        _code.Focus();
    }

    private void UpdateButtons()
    {
        _deactivate.Enabled = _selectedId > 0 && _active.Checked;
        _reactivate.Enabled = _selectedId > 0 && !_active.Checked;
    }

    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.N) { ClearForm(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.S) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { _ = LoadRowsAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    // يستدعي API لتحميل حقول التدقيق الخاصة بالدولة المحددة.
    private async Task LoadCountryAuditAsync(int countryId)
    {
        try
        {
            var audit = await ApiService.Client.GetFromJsonAsync<CountryAuditInfo>($"GeographicReferences/countries/{countryId}/audit-info");
            if (audit is null) { ClearAuditInfo(); return; }
            _auditCreatedBy.Text = audit.Created_By ?? "غير متاح";
            _auditCreatedAt.Text = FormatAuditDate(audit.Created_At);
            _auditUpdatedBy.Text = audit.Updated_By ?? "غير متاح";
            _auditUpdatedAt.Text = FormatAuditDate(audit.Updated_At);
            _auditEditCount.Text = audit.Edit_Count.ToString();
            _auditPrintCount.Text = audit.Print_Count.ToString();
        }
        catch
        {
            // لا نمنع عرض الدولة عندما لا يكون سجل التدقيق متاحاً.
            ClearAuditInfo();
        }
    }

    // تسجل الطباعة في API ثم تفتح معاينة بسيطة لبيانات الدولة.
    private async Task PrintSelectedCountryAsync()
    {
        if (_type != GeographicReferenceType.Country || _selectedId <= 0)
        {
            MessageBox.Show("اختر دولة أولاً.");
            return;
        }

        var response = await ApiService.Client.PostAsync($"GeographicReferences/countries/{_selectedId}/print", null);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تسجيل الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var document = new PrintDocument();
        document.DocumentName = $"بيانات الدولة - {_nameAr.Text}";
        document.PrintPage += (_, e) =>
        {
            using var titleFont = new Font("Segoe UI", 16, FontStyle.Bold);
            using var bodyFont = new Font("Segoe UI", 11);
            e.Graphics.DrawString("بيانات الدولة", titleFont, Brushes.Navy, 80, 80);
            e.Graphics.DrawString($"الكود: {_code.Text}\nالاسم بالعربية: {_nameAr.Text}\nالاسم بالإنجليزية: {_nameEn.Text}\nISO2: {_iso2.Text}\nISO3: {_iso3.Text}\nرمز العملة: {_currencyCode.Text}", bodyFont, Brushes.Black, new RectangleF(80, 135, 650, 300));
        };
        using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700, RightToLeft = RightToLeft.Yes };
        preview.ShowDialog(this);
        await LoadCountryAuditAsync(_selectedId);
    }

    private void ClearAuditInfo()
    {
        foreach (var value in new[] { _auditCreatedBy, _auditCreatedAt, _auditUpdatedBy, _auditUpdatedAt, _auditEditCount, _auditPrintCount })
            value.Text = "غير متاح";
    }

    private static string FormatAuditDate(DateTime? value) => value?.ToString("yyyy/MM/dd HH:mm") ?? "غير متاح";
    private static TextBox Input() => new() { BorderStyle = BorderStyle.FixedSingle };
    private static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private static Label AuditValue() => new() { Dock = DockStyle.Fill, Text = "غير متاح", TextAlign = ContentAlignment.MiddleRight, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(4, 0, 4, 0), ForeColor = Color.FromArgb(55, 65, 81) };
    private static Label AuditCaption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8F, FontStyle.Bold), ForeColor = Color.FromArgb(75, 85, 99) };
    private static bool IsTrue(string value) => value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    private static string? CleanText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Button ActionButton(string text, EventHandler handler, bool primary = false, bool danger = false)
    {
        var button = new Button
        {
            Text = text,
            Width = 96,
            Height = 34,
            Margin = new Padding(3),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Color.FromArgb(15, 103, 208) : Color.White,
            ForeColor = primary ? Color.White : danger ? Color.FromArgb(174, 35, 35) : Color.FromArgb(8, 55, 112)
        };
        button.FlatAppearance.BorderColor = danger ? Color.FromArgb(238, 188, 188) : Color.FromArgb(205, 217, 232);
        button.Click += handler;
        return button;
    }

    private static Panel Card(string title, Control body)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(8)
        };
        body.Dock = DockStyle.Fill;
        panel.Controls.Add(body);
        panel.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 55, 112)
        });
        return panel;
    }

    private static Label Caption(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleRight,
        Font = new Font("Segoe UI", 9.3F, FontStyle.Bold),
        ForeColor = Color.FromArgb(31, 58, 92)
    };

    private static void AddRow(TableLayoutPanel table, int row, string c1, Control x1, string c2, Control x2)
    {
        table.Controls.Add(Caption(c1), 0, row);
        x1.Dock = DockStyle.Fill;
        table.Controls.Add(x1, 1, row);
        table.Controls.Add(Caption(c2), 2, row);
        x2.Dock = DockStyle.Fill;
        table.Controls.Add(x2, 3, row);
    }

    private static void AddFullRow(TableLayoutPanel table, int row, string caption, Control input)
    {
        table.Controls.Add(Caption(caption), 0, row);
        input.Dock = DockStyle.Fill;
        table.Controls.Add(input, 1, row);
        table.SetColumnSpan(input, 3);
    }

    private sealed class CountryAuditInfo
    {
        public string? Created_By { get; set; }
        public DateTime? Created_At { get; set; }
        public string? Updated_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public int Edit_Count { get; set; }
        public int Print_Count { get; set; }
    }

    private sealed class CountryLookup
    {
        public int Country_ID { get; set; }
        public string Country_Name_AR { get; set; } = string.Empty;
    }

    private sealed class GovernorateLookup
    {
        public int Governorate_ID { get; set; }
        public string Governorate_Name_AR { get; set; } = string.Empty;
    }
}

public sealed class FrmCountries : FrmGeographicReference { public FrmCountries() : base(GeographicReferenceType.Country) { } }
public sealed class FrmGovernorates : FrmGeographicReference { public FrmGovernorates() : base(GeographicReferenceType.Governorate) { } }
public sealed class FrmCities : FrmGeographicReference { public FrmCities() : base(GeographicReferenceType.City) { } }
