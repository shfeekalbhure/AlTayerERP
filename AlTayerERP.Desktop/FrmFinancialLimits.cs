using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>
/// إدارة السقوف المالية وحركات استخدامها. تحفظ عبر API فقط، ويظل قرار
/// الوصول الخادمي الحالي مقصوراً على مدير النظام.
/// </summary>
public sealed class FrmFinancialLimits : BaseForm
{
    private readonly Label _id = new() { Text = "(جديد)", BorderStyle = BorderStyle.FixedSingle, TextAlign = ContentAlignment.MiddleRight };
    private readonly ComboBox _entityType = Choice("فرع", "عميل", "مورد", "حساب", "صندوق", "بنك");
    private readonly TextBox _entityId = Input();
    private readonly ComboBox _limitType = Choice("PAYMENT", "RECEIPT", "CREDIT", "DISCOUNT");
    private readonly TextBox _currency = Input("YER");
    private readonly NumericUpDown _amount = new() { Maximum = 999999999999m, DecimalPlaces = 2, ThousandsSeparator = true, TextAlign = HorizontalAlignment.Right };
    private readonly ComboBox _period = Choice("Daily", "Monthly", "Yearly");
    private readonly CheckBox _requiresApproval = new() { Text = "يتطلب اعتماداً عند التجاوز", Checked = true, AutoSize = true };
    private readonly CheckBox _active = new() { Text = "نشطة", Checked = true, AutoSize = true };
    private readonly TextBox _search = new() { PlaceholderText = "بحث بالجهة أو نوع السقف أو العملة…" };
    private readonly DataGridView _grid = new();
    private int _selectedId;

    public FrmFinancialLimits()
    {
        Text = "السقوف المالية وحركات الاستخدام";
        ApplyBaseFormStyle();
        Width = 1180;
        Height = 760;
        StartPosition = FormStartPosition.CenterParent;
        Build();
        Load += async (_, _) => await LoadRowsAsync();
        KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.F3) { ClearForm(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.F2) { await SaveAsync(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.F5) { await LoadRowsAsync(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) Close();
        };
    }

    private void Build()
    {
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(10), BackColor = Color.FromArgb(244, 247, 251) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 158));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.Controls.Add(new BrandHeaderControl(Text), 0, 0);
        shell.Controls.Add(BuildToolbar(), 0, 1);
        shell.Controls.Add(BuildEditor(), 0, 2);
        shell.Controls.Add(BuildSearch(), 0, 3);
        shell.Controls.Add(BuildGrid(), 0, 4);
        Controls.Add(shell);
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, AutoScroll = true, Padding = new Padding(4, 6, 4, 3), BackColor = Color.White };
        bar.Controls.AddRange(new Control[]
        {
            Action("جديد F3", (_, _) => ClearForm(), Color.FromArgb(37, 99, 235)),
            Action("حفظ F2", async (_, _) => await SaveAsync(), Color.FromArgb(22, 125, 84)),
            Action("بحث", (_, _) => _search.Focus(), Color.FromArgb(75, 85, 99)),
            Action("تحديث F5", async (_, _) => await LoadRowsAsync(), Color.FromArgb(14, 116, 144)),
            Action("إغلاق", (_, _) => Close(), Color.FromArgb(71, 85, 105))
        });
        return bar;
    }

    private Control BuildEditor()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(12), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 4; i++) table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        Add(table, 0, "رقم السياسة:", _id, "نوع الجهة *:", _entityType);
        Add(table, 1, "معرف الجهة *:", _entityId, "نوع السقف *:", _limitType);
        Add(table, 2, "رمز العملة *:", _currency, "قيمة السقف *:", _amount);
        Add(table, 3, "الفترة:", _period, "الحالة:", BuildChecks());
        return Card("بيانات السقف المالي", table);
    }

    private Control BuildChecks()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        panel.Controls.Add(_active);
        panel.Controls.Add(_requiresApproval);
        return panel;
    }

    private Control BuildSearch()
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10, 6, 10, 6), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _search.Dock = DockStyle.Fill;
        _search.TextChanged += (_, _) => Filter();
        panel.Controls.Add(Caption("البحث:"), 0, 0);
        panel.Controls.Add(_search, 1, 0);
        return panel;
    }

    private Control BuildGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.SelectionChanged += (_, _) => SelectCurrent();
        return Card("قائمة السقوف المالية", _grid);
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            var rows = await ApiService.Client.GetFromJsonAsync<List<FinancialLimitRow>>("FinancialPolicies") ?? new();
            _grid.DataSource = rows;
            ClearForm(clearGrid: false);
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل السقوف المالية. هذه الشاشة تتطلب صلاحية مدير النظام حالياً.\n\n" + ex.Message,
                Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SelectCurrent()
    {
        if (_grid.SelectedRows.Count == 0) return;
        if (_grid.SelectedRows[0].DataBoundItem is not FinancialLimitRow row) return;
        _selectedId = row.Limit_ID;
        _id.Text = row.Limit_ID.ToString();
        _entityType.SelectedItem = row.Entity_Type;
        _entityId.Text = row.Entity_ID ?? string.Empty;
        _limitType.SelectedItem = row.Limit_Type;
        _currency.Text = row.Currency_Code ?? string.Empty;
        _amount.Value = Math.Min(_amount.Maximum, Math.Max(_amount.Minimum, row.Limit_Amount));
        _period.SelectedItem = row.Period_Type;
        _requiresApproval.Checked = row.Requires_Approval;
        _active.Checked = row.Is_Active;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_entityId.Text) || string.IsNullOrWhiteSpace(_currency.Text))
        {
            MessageBox.Show("معرف الجهة ورمز العملة مطلوبان.");
            return;
        }

        var payload = new
        {
            Limit_ID = _selectedId,
            Entity_Type = _entityType.SelectedItem?.ToString(),
            Entity_ID = _entityId.Text.Trim(),
            Limit_Type = _limitType.SelectedItem?.ToString(),
            Currency_Code = _currency.Text.Trim().ToUpperInvariant(),
            Limit_Amount = _amount.Value,
            Period_Type = _period.SelectedItem?.ToString(),
            Requires_Approval = _requiresApproval.Checked,
            Is_Active = _active.Checked
        };
        var response = await ApiService.Client.PostAsJsonAsync("FinancialPolicies", payload);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await LoadRowsAsync();
        MessageBox.Show("تم حفظ السقف المالي بنجاح.");
    }

    private void Filter()
    {
        var query = _search.Text.Trim();
        foreach (DataGridViewRow row in _grid.Rows)
            row.Visible = string.IsNullOrWhiteSpace(query) || row.Cells.Cast<DataGridViewCell>()
                .Any(x => (x.Value?.ToString() ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase));
    }

    private void ClearForm(bool clearGrid = true)
    {
        _selectedId = 0;
        _id.Text = "(جديد)";
        _entityType.SelectedIndex = 0;
        _entityId.Clear();
        _limitType.SelectedIndex = 0;
        _currency.Text = "YER";
        _amount.Value = 0;
        _period.SelectedIndex = 1;
        _requiresApproval.Checked = true;
        _active.Checked = true;
        if (clearGrid) _grid.ClearSelection();
        _entityId.Focus();
    }

    private static ComboBox Choice(params string[] values)
    {
        var box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        box.Items.AddRange(values);
        box.SelectedIndex = 0;
        return box;
    }

    private static TextBox Input(string? value = null) => new() { Text = value ?? string.Empty, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Right };
    private static Label Caption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 58, 92) };
    private static Button Action(string text, EventHandler handler, Color color) { var b = new Button { Text = text, Width = 110, Height = 33, Margin = new Padding(3), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White }; b.FlatAppearance.BorderSize = 0; b.Click += handler; return b; }

    private static void Add(TableLayoutPanel table, int row, string leftCaption, Control left, string rightCaption, Control right)
    {
        table.Controls.Add(Caption(rightCaption), 0, row);
        right.Dock = DockStyle.Fill;
        table.Controls.Add(right, 1, row);
        table.Controls.Add(Caption(leftCaption), 2, row);
        left.Dock = DockStyle.Fill;
        table.Controls.Add(left, 3, row);
    }

    private static Panel Card(string title, Control body)
    {
        var card = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Padding = new Padding(8) };
        body.Dock = DockStyle.Fill;
        card.Controls.Add(body);
        card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) });
        return card;
    }

    private sealed class FinancialLimitRow
    {
        public int Limit_ID { get; set; }
        public string? Entity_Type { get; set; }
        public string? Entity_ID { get; set; }
        public string? Limit_Type { get; set; }
        public string? Currency_Code { get; set; }
        public decimal Limit_Amount { get; set; }
        public decimal Used_Amount { get; set; }
        public string? Period_Type { get; set; }
        public bool Requires_Approval { get; set; }
        public bool Is_Active { get; set; }
    }
}
