using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Drawing.Printing;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>شاشة المجموعات التجارية المختصرة للمرحلة الحالية.</summary>
public sealed class FrmTenantGroups : BaseForm, IWorkspaceDirtyAware
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _code = Input();
    private readonly TextBox _nameAr = Input();
    private readonly TextBox _nameEn = Input();
    private readonly CheckBox _active = new() { Text = "نشطة", Checked = true, Enabled = false, AutoSize = true };
    private readonly CheckBox _isDefault = new() { Text = "مجموعة افتراضية", AutoSize = true };
    private readonly CheckBox _showInLogin = new() { Text = "تظهر في شاشة اختيار الشركة", Checked = true, AutoSize = true };
    private readonly CheckBox _showInTree = new() { Text = "تظهر في شجرة النظام", Checked = true, AutoSize = true };
    private readonly TextBox _notes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly TextBox _search = new() { PlaceholderText = "ابحث بالكود أو الاسم…" };
    private readonly ComboBox _filterStatus = Combo();
    private readonly Label _createdBy = AuditValue();
    private readonly Label _createdAt = AuditValue();
    private readonly Label _updatedBy = AuditValue();
    private readonly Label _updatedAt = AuditValue();
    private readonly Label _editCount = AuditValue();
    private readonly Label _printCount = AuditValue();
    private readonly Button _save;
    private readonly Button _edit;
    private readonly Button _deactivate;
    private readonly Button _reactivate;
    private List<GroupRow> _groups = new();
    private string? _selectedId;
    private bool _dirty;
    private bool _binding;
    private bool _editing;

    public bool HasUnsavedChanges => _dirty;

    public FrmTenantGroups()
    {
        Text = "المجموعات التجارية";
        Width = 1180;
        Height = 760;
        MinimumSize = new Size(980, 650);
        StartPosition = FormStartPosition.CenterParent;
        ApplyBaseFormStyle();

        _save = Button("✔ حفظ", async (_, _) => await SaveAsync(), primary: true);
        _edit = Button("✎ تعديل", (_, _) => BeginEdit());
        _deactivate = Button("إيقاف", async (_, _) => await ChangeStatusAsync(false), danger: true);
        _reactivate = Button("إعادة تفعيل", async (_, _) => await ChangeStatusAsync(true));

        _filterStatus.Items.AddRange(new object[] { "كل الحالات", "نشطة", "موقوفة" });
        _filterStatus.SelectedIndex = 0;

        Build();
        Load += async (_, _) => await LoadGroupsAsync();
        KeyDown += HandleKeys;
        _grid.SelectionChanged += async (_, _) => await BindSelectedAsync();
        _search.TextChanged += (_, _) => Filter();
        _filterStatus.SelectedIndexChanged += (_, _) => Filter();

        foreach (var control in new Control[] { _code, _nameAr, _nameEn, _notes, _isDefault, _showInLogin, _showInTree })
        {
            control.TextChanged += (_, _) => MarkDirty();
            if (control is ComboBox combo) combo.SelectedIndexChanged += (_, _) => MarkDirty();
            if (control is CheckBox check) check.CheckedChanged += (_, _) => MarkDirty();
        }
    }

    private void Build()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(6),
            BackColor = Color.FromArgb(248, 250, 252)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 205));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));

        shell.Controls.Add(new BrandHeaderControl("المجموعات التجارية"), 0, 0);
        shell.Controls.Add(Toolbar(), 0, 1);
        shell.Controls.Add(EditorCard(), 0, 2);
        shell.Controls.Add(Card("البحث والتصفية", FilterBar()), 0, 3);
        ConfigureGrid();
        shell.Controls.Add(Card("قائمة المجموعات التجارية", _grid), 0, 4);
        shell.Controls.Add(BuildAuditFooter(), 0, 5);
        Controls.Add(shell);
    }

    private Control Toolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            RightToLeft = RightToLeft.Yes,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4),
            BackColor = Color.White
        };
        var add = ToolbarButton("+ جديد", (_, _) => ClearForm(), Color.FromArgb(13, 148, 136));
        ApplyToolbarColor(_save, Color.FromArgb(37, 99, 235));
        ApplyToolbarColor(_edit, Color.FromArgb(217, 119, 6));
        ApplyToolbarColor(_deactivate, Color.FromArgb(220, 38, 38));
        ApplyToolbarColor(_reactivate, Color.FromArgb(5, 150, 105));
        var print = ToolbarButton("🖨 طباعة", async (_, _) => await PrintSelectedAsync(), Color.FromArgb(124, 58, 237));
        var search = ToolbarButton("🔍 بحث", (_, _) => _search.Focus(), Color.FromArgb(5, 150, 105));
        var refresh = ToolbarButton("↻ تحديث", async (_, _) => await LoadGroupsAsync(), Color.FromArgb(2, 132, 199));
        var close = ToolbarButton("إغلاق", (_, _) => Close(), Color.FromArgb(100, 116, 139));
        bar.Controls.AddRange(new Control[] { add, _save, _edit, _deactivate, _reactivate, print, search, refresh, close });
        return bar;
    }

    private Control EditorCard()
    {
        var table = FormTable(4);
        AddRow(table, 0, "كود المجموعة *", _code, "اسم المجموعة عربي *", _nameAr);
        table.Controls.Add(Caption("اسم المجموعة إنجليزي"), 0, 1);
        table.Controls.Add(Field(_nameEn), 1, 1);
        table.SetColumnSpan(_nameEn, 3);

        var settings = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(8, 10, 8, 0)
        };
        settings.Controls.AddRange(new Control[] { _active, _isDefault, _showInLogin, _showInTree });
        table.Controls.Add(Caption("الإعدادات"), 0, 2);
        table.Controls.Add(settings, 1, 2);
        table.SetColumnSpan(settings, 3);

        table.Controls.Add(Caption("ملاحظة"), 0, 3);
        table.Controls.Add(Field(_notes), 1, 3);
        table.SetColumnSpan(_notes, 3);
        return Card("بيانات المجموعة التجارية", table);
    }

    private Control FilterBar()
    {
        var bar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, Padding = new Padding(6, 6, 6, 2), RightToLeft = RightToLeft.Yes };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        _search.Dock = DockStyle.Fill;
        _filterStatus.Dock = DockStyle.Fill;
        var apply = ToolbarButton("تطبيق", (_, _) => Filter(), Color.FromArgb(37, 99, 235));
        apply.Dock = DockStyle.Fill;
        bar.Controls.Add(Caption("بحث:"), 0, 0);
        bar.Controls.Add(_search, 1, 0);
        bar.Controls.Add(Caption("الحالة:"), 2, 0);
        bar.Controls.Add(_filterStatus, 3, 0);
        bar.Controls.Add(apply, 4, 0);
        return bar;
    }

    private Control BuildAuditFooter()
    {
        var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0, 4, 0, 0) };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        footer.Controls.Add(AuditCard("بيانات الإنشاء", "أنشئ بواسطة:", _createdBy, "تاريخ الإنشاء:", _createdAt), 0, 0);
        footer.Controls.Add(AuditCard("بيانات التعديل", "عدل بواسطة:", _updatedBy, "تاريخ التعديل:", _updatedAt), 1, 0);
        footer.Controls.Add(AuditCard("العدادات", "عدد التعديلات:", _editCount, "عدد مرات الطباعة:", _printCount), 2, 0);
        return footer;
    }

    private async Task LoadGroupsAsync()
    {
        try
        {
            _binding = true;
            _groups = await ApiService.Client.GetFromJsonAsync<List<GroupRow>>("TenantGroups") ?? new();
            _grid.DataSource = _groups.ToList();
            ClearForm();
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("تعذر تحميل المجموعات التجارية من الخدمة. تأكد من تشغيل الـ API واتصال قاعدة البيانات.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception)
        {
            MessageBox.Show("حدث خطأ غير متوقع أثناء تحميل المجموعات التجارية.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _binding = false; }
    }

    private async Task SaveAsync()
    {
        if (!_editing)
        {
            MessageBox.Show("اختر «تعديل» قبل الحفظ، أو استخدم «جديد» لإنشاء مجموعة.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_code.Text) || string.IsNullOrWhiteSpace(_nameAr.Text))
        {
            MessageBox.Show("كود المجموعة والاسم العربي حقول مطلوبة.");
            return;
        }

        _save.Enabled = false;
        try
        {
            var dto = new
            {
                Group_Code = _code.Text.Trim(),
                Group_Name_AR = _nameAr.Text.Trim(),
                Group_Name_EN = CleanText(_nameEn.Text),
                Is_Default = _isDefault.Checked,
                Show_In_Login = _showInLogin.Checked,
                Show_In_Tree = _showInTree.Checked,
                Notes = CleanText(_notes.Text)
            };
            var response = string.IsNullOrWhiteSpace(_selectedId)
                ? await ApiService.Client.PostAsJsonAsync("TenantGroups", dto)
                : await ApiService.Client.PutAsJsonAsync($"TenantGroups/{_selectedId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadGroupsAsync();
            MessageBox.Show("تم حفظ المجموعة التجارية بنجاح.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر الاتصال بالخادم أثناء الحفظ.\n" + ex.Message, "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { _save.Enabled = true; }
    }

    private async Task BindSelectedAsync()
    {
        if (_binding || _grid.SelectedRows.Count == 0 || _grid.SelectedRows[0].DataBoundItem is not GroupRow row) return;
        _binding = true;
        try
        {
            _selectedId = row.Group_ID;
            _code.Text = row.Group_Code;
            _nameAr.Text = row.Group_Name_AR;
            _nameEn.Text = row.Group_Name_EN;
            _active.Checked = row.Is_Active;
            _isDefault.Checked = row.Is_Default;
            _showInLogin.Checked = row.Show_In_Login;
            _showInTree.Checked = row.Show_In_Tree;
            _notes.Text = row.Notes;
            _editing = false;
            _dirty = false;
            SetEditorEnabled(false);
            UpdateButtons();
            await LoadAuditAsync(row.Group_ID);
        }
        finally { _binding = false; }
    }

    private async Task ChangeStatusAsync(bool reactivate)
    {
        if (string.IsNullOrWhiteSpace(_selectedId)) { MessageBox.Show("اختر مجموعة أولاً."); return; }
        if (reactivate == _active.Checked) return;
        var action = reactivate ? "إعادة تفعيل" : "إيقاف";
        var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} المجموعة:", action, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("السبب إلزامي للتدقيق."); return; }

        HttpResponseMessage response;
        if (reactivate)
            response = await ApiService.Client.PostAsJsonAsync($"TenantGroups/{_selectedId}/reactivate", new { Reason = reason });
        else
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"TenantGroups/{_selectedId}")
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
        await LoadGroupsAsync();
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.AllowUserToAddRows = false;
        _grid.ReadOnly = true;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(Col(nameof(GroupRow.Group_Code), "كود المجموعة", 110));
        _grid.Columns.Add(Col(nameof(GroupRow.Group_Name_AR), "اسم المجموعة عربي", 220));
        _grid.Columns.Add(Col(nameof(GroupRow.Group_Name_EN), "اسم المجموعة إنجليزي", 190));
        _grid.Columns.Add(Col(nameof(GroupRow.Default_Text), "افتراضية", 90));
        _grid.Columns.Add(Col(nameof(GroupRow.Login_Text), "اختيار الشركة", 105));
        _grid.Columns.Add(Col(nameof(GroupRow.Tree_Text), "الشجرة", 90));
        _grid.Columns.Add(Col(nameof(GroupRow.Status_Text), "الحالة", 85));
    }

    private void Filter()
    {
        var q = _search.Text.Trim();
        var status = _filterStatus.Text;
        _grid.DataSource = _groups.Where(x =>
            (status == "كل الحالات" || (status == "نشطة" && x.Is_Active) || (status == "موقوفة" && !x.Is_Active)) &&
            (string.IsNullOrWhiteSpace(q) || $"{x.Group_Code} {x.Group_Name_AR} {x.Group_Name_EN}".Contains(q, StringComparison.CurrentCultureIgnoreCase)))
            .ToList();
    }

    private void ClearForm()
    {
        _binding = true;
        try
        {
            _selectedId = null;
            _code.Clear();
            _nameAr.Clear();
            _nameEn.Clear();
            _notes.Clear();
            _active.Checked = true;
            _isDefault.Checked = false;
            _showInLogin.Checked = true;
            _showInTree.Checked = true;
            _grid.ClearSelection();
            ClearAudit();
            _editing = true;
            _dirty = false;
            SetEditorEnabled(true);
            UpdateButtons();
        }
        finally { _binding = false; }
        _code.Focus();
    }

    private void BeginEdit()
    {
        if (string.IsNullOrWhiteSpace(_selectedId)) { ClearForm(); return; }
        if (!_active.Checked) { MessageBox.Show("أعد تفعيل المجموعة أولاً قبل تعديلها."); return; }
        _editing = true;
        SetEditorEnabled(true);
        UpdateButtons();
        _nameAr.Focus();
    }

    private void SetEditorEnabled(bool enabled)
    {
        foreach (var control in new Control[] { _nameAr, _nameEn, _notes, _isDefault, _showInLogin, _showInTree })
            control.Enabled = enabled;
        _code.Enabled = enabled && string.IsNullOrWhiteSpace(_selectedId);
    }

    private void UpdateButtons()
    {
        _save.Enabled = _editing;
        _edit.Enabled = !string.IsNullOrWhiteSpace(_selectedId) && _active.Checked && !_editing;
        _deactivate.Enabled = !string.IsNullOrWhiteSpace(_selectedId) && _active.Checked && !_editing;
        _reactivate.Enabled = !string.IsNullOrWhiteSpace(_selectedId) && !_active.Checked;
    }

    private void MarkDirty() { if (!_binding) _dirty = true; }

    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F2 || (e.Control && e.KeyCode == Keys.S)) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F3 || (e.Control && e.KeyCode == Keys.N)) { ClearForm(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F4) { BeginEdit(); e.SuppressKeyPress = true; }
        else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { _ = LoadGroupsAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    private async Task LoadAuditAsync(string groupId)
    {
        try
        {
            var audit = await ApiService.Client.GetFromJsonAsync<GroupAudit>($"TenantGroups/{groupId}/audit-info");
            _createdBy.Text = audit?.Created_By ?? "غير متاح";
            _createdAt.Text = AuditDate(audit?.Created_At);
            _updatedBy.Text = audit?.Updated_By ?? "غير متاح";
            _updatedAt.Text = AuditDate(audit?.Updated_At);
            _editCount.Text = (audit?.Edit_Count ?? 0).ToString();
            _printCount.Text = (audit?.Print_Count ?? 0).ToString();
        }
        catch { ClearAudit(); }
    }

    private async Task PrintSelectedAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedId)) { MessageBox.Show("اختر مجموعة أولاً."); return; }
        var response = await ApiService.Client.PostAsync($"TenantGroups/{_selectedId}/print", null);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر تسجيل الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var document = new PrintDocument { DocumentName = "بيانات المجموعة التجارية - " + _nameAr.Text };
        document.PrintPage += (_, e) =>
        {
            using var title = new Font("Segoe UI", 15F, FontStyle.Bold);
            using var body = new Font("Segoe UI", 11F);
            e.Graphics.DrawString("بيانات المجموعة التجارية", title, Brushes.Navy, 70, 70);
            e.Graphics.DrawString($"الكود: {_code.Text}\nالاسم العربي: {_nameAr.Text}\nالاسم الإنجليزي: {_nameEn.Text}\nالحالة: {(_active.Checked ? "نشطة" : "موقوفة")}\nافتراضية: {(_isDefault.Checked ? "نعم" : "لا")}\nتظهر في اختيار الشركة: {(_showInLogin.Checked ? "نعم" : "لا")}\nتظهر في الشجرة: {(_showInTree.Checked ? "نعم" : "لا")}\nالملاحظة: {_notes.Text}", body, Brushes.Black, new RectangleF(70, 120, 700, 420));
        };
        using var preview = new PrintPreviewDialog { Document = document, Width = 900, Height = 700, RightToLeft = RightToLeft.Yes };
        preview.ShowDialog(this);
        await LoadAuditAsync(_selectedId);
    }

    private void ClearAudit()
    {
        _createdBy.Text = _createdAt.Text = _updatedBy.Text = _updatedAt.Text = "غير متاح";
        _editCount.Text = _printCount.Text = "0";
    }

    private static string AuditDate(DateTime? value) => value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "غير متاح";
    private static string? CleanText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static TextBox Input() => new();
    private static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private static Label AuditValue() => new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(4, 0, 4, 0), Text = "غير متاح" };
    private static Button Button(string text, EventHandler handler, bool primary = false, bool danger = false)
    {
        var button = new Button { Text = text, Width = 120, Height = 34, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White, ForeColor = primary ? Color.White : danger ? Color.Firebrick : Color.FromArgb(8, 55, 112) };
        button.FlatAppearance.BorderColor = Color.FromArgb(205, 217, 232);
        button.Click += handler;
        return button;
    }
    private static Button ToolbarButton(string text, EventHandler handler, Color color) { var button = Button(text, handler); ApplyToolbarColor(button, color); return button; }
    private static void ApplyToolbarColor(Button button, Color color) { button.Width = 94; button.Height = 34; button.Margin = new Padding(3); button.BackColor = color; button.ForeColor = Color.White; button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0; button.Cursor = Cursors.Hand; }
    private static Panel Card(string title, Control body)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8) };
        body.Dock = DockStyle.Fill;
        panel.Controls.Add(body);
        panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) });
        return panel;
    }
    private static TableLayoutPanel FormTable(int rows)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = rows, Padding = new Padding(8) };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var row = 0; row < rows; row++)
            table.RowStyles.Add(new RowStyle(row == rows - 1 ? SizeType.Percent : SizeType.Absolute, row == rows - 1 ? 100 : 42));
        return table;
    }
    private static void AddRow(TableLayoutPanel table, int row, string caption1, Control field1, string caption2, Control field2)
    {
        table.Controls.Add(Caption(caption1), 0, row);
        table.Controls.Add(Field(field1), 1, row);
        table.Controls.Add(Caption(caption2), 2, row);
        table.Controls.Add(Field(field2), 3, row);
    }
    private static Label Caption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 58, 92) };
    private static Control Field(Control control) { control.Dock = DockStyle.Fill; control.Margin = new Padding(4, 7, 4, 7); return control; }
    private static DataGridViewTextBoxColumn Col(string property, string header, int width) => new() { DataPropertyName = property, HeaderText = header, Width = width };
    private static Control AuditCard(string title, string firstCaption, Label firstValue, string secondCaption, Label secondValue)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(6, 2, 6, 2), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 21));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        var header = new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) };
        table.Controls.Add(header, 0, 0);
        table.SetColumnSpan(header, 2);
        table.Controls.Add(Caption(firstCaption), 0, 1);
        table.Controls.Add(firstValue, 1, 1);
        table.Controls.Add(Caption(secondCaption), 0, 2);
        table.Controls.Add(secondValue, 1, 2);
        return table;
    }

    private sealed class GroupRow
    {
        public string Group_ID { get; set; } = string.Empty;
        public string Group_Code { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
        public string Group_Name_EN { get; set; } = string.Empty;
        public bool Is_Default { get; set; }
        public bool Show_In_Login { get; set; }
        public bool Show_In_Tree { get; set; }
        public string? Notes { get; set; }
        public bool Is_Active { get; set; }
        public string Status_Text => Is_Active ? "نشطة" : "موقوفة";
        public string Default_Text => Is_Default ? "نعم" : "لا";
        public string Login_Text => Show_In_Login ? "نعم" : "لا";
        public string Tree_Text => Show_In_Tree ? "نعم" : "لا";
    }

    private sealed class GroupAudit
    {
        public string? Created_By { get; set; }
        public DateTime? Created_At { get; set; }
        public string? Updated_By { get; set; }
        public DateTime? Updated_At { get; set; }
        public int Edit_Count { get; set; }
        public int Print_Count { get; set; }
    }
}
