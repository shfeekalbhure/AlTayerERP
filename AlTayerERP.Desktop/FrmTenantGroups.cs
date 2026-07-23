using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>
/// شاشة المجموعات التجارية وفق التصميم الموحد المعتمد للمرحلة الأولى.
/// </summary>
public sealed class FrmTenantGroups : Form
{
    private readonly HttpClient _client = ApiService.Client;
    private readonly DataGridView dgvGroups = new();
    private readonly TextBox txtCode = Input();
    private readonly TextBox txtNameAr = Input();
    private readonly TextBox txtNameEn = Input();
    private readonly TextBox txtShortName = Input();
    private readonly ComboBox cmbType = Combo("مجموعة استثمارية", "مجموعة صناعية", "مجموعة خدمية", "مجموعة قابضة", "أخرى");
    private readonly ComboBox cmbParent = Combo();
    private readonly ComboBox cmbMainCompany = Combo();
    private readonly ComboBox cmbCurrency = Combo("YER", "SAR", "USD", "AED", "EUR");
    private readonly TextBox txtCountry = Input();
    private readonly TextBox txtCity = Input();
    private readonly TextBox txtAddress = Input();
    private readonly TextBox txtPhone = Input();
    private readonly TextBox txtEmail = Input();
    private readonly TextBox txtManager = Input();
    private readonly RadioButton rbActive = new() { Text = "نشطة", Checked = true, AutoSize = true };
    private readonly RadioButton rbStopped = new() { Text = "موقوفة", AutoSize = true };
    private readonly CheckBox chkShowInLogin = new() { Text = "تظهر في شاشة اختيار الشركة", Checked = true, AutoSize = true };
    private readonly NumericUpDown numSort = new() { Minimum = 0, Maximum = 9999, Width = 220, TextAlign = HorizontalAlignment.Right };
    private readonly TextBox txtNotes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Height = 85, Dock = DockStyle.Fill };
    private readonly TextBox txtSearch = Input();
    private readonly Label lblCount = new() { AutoSize = true };
    private readonly Label lblCompaniesCount = new() { Text = "0", AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
    private readonly Button btnSave;
    private string? _selectedId;
    private List<GroupRow> _cache = new();

    public FrmTenantGroups()
    {
        Text = "المجموعة التجارية";
        StartPosition = FormStartPosition.CenterParent;
        Width = 1400;
        Height = 850;
        MinimumSize = new Size(1120, 720);
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(247, 249, 252);
        KeyPreview = true;

        btnSave = ToolButton("حفظ", true);
        Controls.Add(BuildShell());
        ConfigureGrid();

        Load += async (_, _) => await LoadAsync();
        btnSave.Click += async (_, _) => await SaveAsync();
        txtSearch.TextChanged += (_, _) => FilterGrid();
        dgvGroups.SelectionChanged += (_, _) => BindSelected();
        KeyDown += HandleShortcuts;
    }

    private Control BuildShell()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(16) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        shell.Controls.Add(BuildTitle(), 0, 0);
        shell.Controls.Add(BuildToolbar(), 0, 1);
        shell.Controls.Add(BuildEditor(), 0, 2);
        shell.Controls.Add(BuildGridCard(), 0, 3);
        shell.Controls.Add(BuildFooter(), 0, 4);
        return shell;
    }

    private Control BuildTitle()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(8, 55, 112),
            Controls = { new Label { Text = "المجموعة التجارية", Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter } }
        };
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(4, 8, 4, 6), BackColor = Color.White };
        var btnNew = ToolButton("جديد");
        var btnEdit = ToolButton("تعديل");
        var btnDelete = ToolButton("حذف");
        var btnRefresh = ToolButton("تحديث");
        var btnSearch = ToolButton("بحث");
        var btnClose = ToolButton("إغلاق");
        btnNew.Click += (_, _) => ClearForm();
        btnEdit.Click += async (_, _) => await SaveAsync();
        btnDelete.Click += async (_, _) => await DeleteAsync();
        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnSearch.Click += (_, _) => txtSearch.Focus();
        btnClose.Click += (_, _) => Close();
        bar.Controls.AddRange(new Control[] { btnNew, btnSave, btnEdit, btnDelete, btnRefresh, btnSearch, btnClose });
        return bar;
    }

    private Control BuildEditor()
    {
        var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 8, 0, 8) };
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
        columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        columns.Controls.Add(BuildIdentityCard(), 0, 0);
        columns.Controls.Add(BuildContactCard(), 1, 0);
        return columns;
    }

    private Control BuildIdentityCard()
    {
        var card = Card("بيانات المجموعة التجارية");
        var grid = FormGrid();
        AddRow(grid, 0, "كود المجموعة *", txtCode, "اسم المجموعة بالعربية *", txtNameAr);
        AddRow(grid, 1, "اسم المجموعة بالإنجليزية", txtNameEn, "الاسم المختصر *", txtShortName);
        AddRow(grid, 2, "نوع المجموعة *", cmbType, "المجموعة الأم", cmbParent);
        AddRow(grid, 3, "الشركة الرئيسية التابعة للمجموعة", cmbMainCompany, "العملة الافتراضية", cmbCurrency);
        AddRow(grid, 4, "الدولة", txtCountry, "المدينة", txtCity);
        AddRow(grid, 5, "العنوان المختصر", txtAddress, "عدد الشركات التابعة", lblCompaniesCount);
        card.Controls.Add(grid);
        return card;
    }

    private Control BuildContactCard()
    {
        var card = Card("معلومات الاتصال والإدارة");
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(12) };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddSingle(grid, 0, "الهاتف", txtPhone);
        AddSingle(grid, 1, "البريد الإلكتروني", txtEmail);
        AddSingle(grid, 2, "المدير المسؤول", txtManager);
        var status = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        status.Controls.AddRange(new Control[] { rbActive, rbStopped });
        AddSingle(grid, 3, "الحالة *", status);
        AddSingle(grid, 4, "إعدادات شاشة الدخول", chkShowInLogin);
        AddSingle(grid, 5, "ترتيب الظهور", numSort);
        AddSingle(grid, 6, "وصف / ملاحظات", txtNotes);
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(grid);
        return card;
    }

    private Control BuildGridCard()
    {
        var card = Card("قائمة المجموعات التجارية");
        var top = new Panel { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8) };
        txtSearch.PlaceholderText = "ابحث بالكود أو الاسم أو النوع…";
        txtSearch.Dock = DockStyle.Fill;
        top.Controls.Add(txtSearch);
        card.Controls.Add(dgvGroups);
        card.Controls.Add(top);
        return card;
    }

    private Control BuildFooter()
    {
        var footer = new Panel { Dock = DockStyle.Fill };
        lblCount.Dock = DockStyle.Left;
        lblCount.TextAlign = ContentAlignment.MiddleLeft;
        footer.Controls.Add(lblCount);
        footer.Controls.Add(new Label
        {
            Text = "تُربط كل شركة بمجموعة تجارية واحدة، ويمكن استخدام المجموعات لتنظيم شاشة الدخول وهيكل الشركات.",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(55, 85, 130),
            TextAlign = ContentAlignment.MiddleRight
        });
        return footer;
    }

    private void ConfigureGrid()
    {
        dgvGroups.Dock = DockStyle.Fill;
        dgvGroups.AutoGenerateColumns = false;
        dgvGroups.AllowUserToAddRows = false;
        dgvGroups.ReadOnly = true;
        dgvGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvGroups.MultiSelect = false;
        dgvGroups.RowHeadersVisible = false;
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Group_Code), "كود المجموعة", 120));
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Group_Name_AR), "اسم المجموعة", 230));
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Group_Type), "النوع", 160));
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Main_Company_ID), "الشركة الرئيسية", 190));
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Sort_Order), "ترتيب الظهور", 100));
        dgvGroups.Columns.Add(TextCol(nameof(GroupRow.Is_Active_Text), "الحالة", 100));
    }

    private async Task LoadAsync()
    {
        try
        {
            UseWaitCursor = true;
            _cache = await _client.GetFromJsonAsync<List<GroupRow>>("TenantGroups") ?? new();
            dgvGroups.DataSource = _cache.ToList();
            cmbParent.DataSource = _cache.Where(x => x.Group_ID != _selectedId).ToList();
            cmbParent.DisplayMember = nameof(GroupRow.Group_Name_AR);
            cmbParent.ValueMember = nameof(GroupRow.Group_ID);
            cmbParent.SelectedIndex = -1;
            lblCount.Text = $"عدد السجلات: {_cache.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل المجموعات التجارية.\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtNameAr.Text) ||
            string.IsNullOrWhiteSpace(txtShortName.Text) || string.IsNullOrWhiteSpace(cmbType.Text))
        {
            MessageBox.Show("أدخل كود المجموعة والاسم العربي والاسم المختصر ونوع المجموعة.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var request = new
        {
            Group_Code = txtCode.Text.Trim(), Group_Name_AR = txtNameAr.Text.Trim(), Group_Name_EN = txtNameEn.Text.Trim(),
            Short_Name = txtShortName.Text.Trim(), Group_Type = cmbType.Text, Parent_Group_ID = cmbParent.SelectedValue?.ToString(),
            Main_Company_ID = cmbMainCompany.SelectedValue?.ToString(), Default_Currency_Code = cmbCurrency.Text,
            Country_Name = txtCountry.Text.Trim(), City_Name = txtCity.Text.Trim(), Short_Address = txtAddress.Text.Trim(),
            Phone = txtPhone.Text.Trim(), Email = txtEmail.Text.Trim(), Manager_Name = txtManager.Text.Trim(),
            Show_In_Login = chkShowInLogin.Checked, Sort_Order = (int)numSort.Value, Notes = txtNotes.Text.Trim(), Is_Active = rbActive.Checked
        };

        HttpResponseMessage response = _selectedId == null
            ? await _client.PostAsJsonAsync("TenantGroups", request)
            : await _client.PutAsJsonAsync($"TenantGroups/{_selectedId}", request);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        MessageBox.Show("تم حفظ المجموعة التجارية بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        ClearForm();
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (_selectedId == null) return;
        if (MessageBox.Show("هل تريد حذف المجموعة المحددة؟", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var response = await _client.DeleteAsync($"TenantGroups/{_selectedId}");
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        ClearForm();
        await LoadAsync();
    }

    private void BindSelected()
    {
        if (dgvGroups.CurrentRow?.DataBoundItem is not GroupRow row) return;
        _selectedId = row.Group_ID;
        txtCode.Text = row.Group_Code;
        txtNameAr.Text = row.Group_Name_AR;
        txtNameEn.Text = row.Group_Name_EN;
        txtShortName.Text = row.Short_Name;
        cmbType.Text = row.Group_Type;
        cmbParent.SelectedValue = row.Parent_Group_ID;
        cmbMainCompany.SelectedValue = row.Main_Company_ID;
        cmbCurrency.Text = row.Default_Currency_Code;
        txtCountry.Text = row.Country_Name;
        txtCity.Text = row.City_Name;
        txtAddress.Text = row.Short_Address;
        txtPhone.Text = row.Phone;
        txtEmail.Text = row.Email;
        txtManager.Text = row.Manager_Name;
        chkShowInLogin.Checked = row.Show_In_Login;
        numSort.Value = Math.Max(numSort.Minimum, Math.Min(numSort.Maximum, row.Sort_Order));
        txtNotes.Text = row.Notes;
        rbActive.Checked = row.Is_Active;
        rbStopped.Checked = !row.Is_Active;
        lblCompaniesCount.Text = row.Companies_Count.ToString();
    }

    private void ClearForm()
    {
        _selectedId = null;
        foreach (var text in new[] { txtCode, txtNameAr, txtNameEn, txtShortName, txtCountry, txtCity, txtAddress, txtPhone, txtEmail, txtManager, txtNotes }) text.Clear();
        cmbType.SelectedIndex = -1; cmbParent.SelectedIndex = -1; cmbMainCompany.SelectedIndex = -1; cmbCurrency.SelectedIndex = -1;
        rbActive.Checked = true; chkShowInLogin.Checked = true; numSort.Value = 0; lblCompaniesCount.Text = "0";
        dgvGroups.ClearSelection(); txtCode.Focus();
    }

    private void FilterGrid()
    {
        var q = txtSearch.Text.Trim();
        dgvGroups.DataSource = string.IsNullOrWhiteSpace(q) ? _cache.ToList() : _cache.Where(x =>
            x.Group_Code.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            x.Group_Name_AR.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            x.Group_Type.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void HandleShortcuts(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.S) { _ = SaveAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F5) { _ = LoadAsync(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.F2) { ClearForm(); e.SuppressKeyPress = true; }
        else if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    private static Panel Card(string title)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
        card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112), TextAlign = ContentAlignment.MiddleRight });
        return card;
    }

    private static TableLayoutPanel FormGrid()
    {
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 6, Padding = new Padding(10) };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        return grid;
    }

    private static void AddRow(TableLayoutPanel grid, int row, string label1, Control input1, string label2, Control input2)
    {
        grid.Controls.Add(LabelFor(label1), 0, row); grid.Controls.Add(input1, 1, row);
        grid.Controls.Add(LabelFor(label2), 2, row); grid.Controls.Add(input2, 3, row);
        input1.Dock = DockStyle.Fill; input2.Dock = DockStyle.Fill;
    }

    private static void AddSingle(TableLayoutPanel grid, int row, string label, Control input)
    {
        grid.Controls.Add(LabelFor(label), 0, row); grid.Controls.Add(input, 1, row); input.Dock = DockStyle.Fill;
    }

    private static Label LabelFor(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(31, 52, 82) };
    private static TextBox Input() => new() { BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Right };
    private static ComboBox Combo(params string[] items) { var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; if (items.Length > 0) c.Items.AddRange(items); return c; }
    private static Button ToolButton(string text, bool primary = false) => new() { Text = text, Width = 125, Height = 36, Margin = new Padding(5, 0, 5, 0), FlatStyle = FlatStyle.Flat, BackColor = primary ? Color.FromArgb(14, 93, 216) : Color.White, ForeColor = primary ? Color.White : Color.FromArgb(8, 55, 112), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
    private static DataGridViewTextBoxColumn TextCol(string property, string title, int width) => new() { DataPropertyName = property, HeaderText = title, Width = width, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill };

    private sealed class GroupRow
    {
        public string Group_ID { get; set; } = "";
        public string Group_Code { get; set; } = "";
        public string Group_Name_AR { get; set; } = "";
        public string Group_Name_EN { get; set; } = "";
        public string Short_Name { get; set; } = "";
        public string Group_Type { get; set; } = "";
        public string? Parent_Group_ID { get; set; }
        public string? Main_Company_ID { get; set; }
        public string Default_Currency_Code { get; set; } = "";
        public string Country_Name { get; set; } = "";
        public string City_Name { get; set; } = "";
        public string Short_Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Manager_Name { get; set; } = "";
        public bool Show_In_Login { get; set; }
        public int Sort_Order { get; set; }
        public string Notes { get; set; } = "";
        public bool Is_Active { get; set; }
        public int Companies_Count { get; set; }
        public string Is_Active_Text => Is_Active ? "نشطة" : "موقوفة";
    }
}