using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>
/// شاشة مركز الاعتمادات. لا تنشئ طلباً يدوياً؛ تعرض الطلبات التي أنشأتها
/// خدمات الأعمال وتسمح بالقرار وفق صلاحية الجلسة.
/// </summary>
public sealed class FrmApprovalRequests : BaseForm
{
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _search = new() { PlaceholderText = "ابحث بالمرجع أو الجهة أو السبب…" };
    private readonly TextBox _reason = new() { Multiline = true, PlaceholderText = "سبب القرار (إلزامي للاعتماد والرفض والإرجاع)" };
    private readonly Label _details = new() { AutoSize = false, BorderStyle = BorderStyle.FixedSingle };
    private readonly Label _count = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleLeft };
    private readonly DataGridView _grid = new();
    private int _selectedId;
    private string _selectedStatus = string.Empty;
    private Button _review = null!, _approve = null!, _reject = null!, _return = null!;

    public FrmApprovalRequests()
    {
        Text = "طلبات الاعتماد";
        ApplyBaseFormStyle();
        Width = 1180;
        Height = 760;
        StartPosition = FormStartPosition.CenterParent;
        Build();
        Load += async (_, _) => await LoadRowsAsync();
        KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.F5) { await LoadRowsAsync(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) Close();
        };
    }

    private void Build()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(244, 247, 251)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        // زيادة بطاقة التفاصيل بمقدار يقارب 2 سم حتى تظهر بيانات الطلب وسبب القرار بوضوح.
        // تنخفض مساحة القائمة بالقدر نفسه، وتبقى قابلة للتمدد مع حجم النافذة.
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 168));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        shell.Controls.Add(new BrandHeaderControl(Text), 0, 0);
        shell.Controls.Add(BuildToolbar(), 0, 1);
        shell.Controls.Add(BuildDecisionCard(), 0, 2);
        shell.Controls.Add(BuildGridCard(), 0, 3);
        Controls.Add(shell);

        _status.Items.AddRange(new object[]
        {
            new StatusOption("الكل", null),
            new StatusOption("بانتظار الإجراء", "Pending"),
            new StatusOption("تحت المراجعة", "UnderReview"),
            new StatusOption("معتمد", "Approved"),
            new StatusOption("مرفوض", "Rejected"),
            new StatusOption("معاد للتعديل", "Returned")
        });
        _status.SelectedIndex = 0;
        _status.SelectedIndexChanged += async (_, _) => await LoadRowsAsync();
        _search.TextChanged += (_, _) => ApplyFilter();
    }

    private Control BuildToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4, 6, 4, 3),
            BackColor = Color.White
        };

        _review = Button("مراجعة", async (_, _) => await DecideAsync("review"), Color.FromArgb(14, 116, 144));
        _approve = Button("اعتماد", async (_, _) => await DecideAsync("approve"), Color.FromArgb(22, 125, 84));
        _reject = Button("رفض", async (_, _) => await DecideAsync("reject"), Color.FromArgb(185, 28, 28));
        _return = Button("إرجاع", async (_, _) => await DecideAsync("return"), Color.FromArgb(180, 83, 9));

        bar.Controls.AddRange(new Control[]
        {
            _review, _approve, _reject, _return,
            Button("بحث", (_, _) => _search.Focus(), Color.FromArgb(75, 85, 99)),
            Button("تحديث", async (_, _) => await LoadRowsAsync(), Color.FromArgb(37, 99, 235)),
            Button("إغلاق", (_, _) => Close(), Color.FromArgb(71, 85, 105))
        });
        return bar;
    }

    private Control BuildDecisionCard()
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));

        _status.Dock = DockStyle.Fill;
        _search.Dock = DockStyle.Fill;
        _reason.Dock = DockStyle.Fill;
        _details.Dock = DockStyle.Fill;
        _details.Padding = new Padding(6);
        _details.TextAlign = ContentAlignment.MiddleRight;
        _details.Text = "اختر طلباً لعرض التفاصيل.";

        table.Controls.Add(Caption("التصفية:"), 0, 0);
        table.Controls.Add(_status, 1, 0);
        table.Controls.Add(Caption("البحث:"), 2, 0);
        table.Controls.Add(_search, 3, 0);
        table.Controls.Add(Caption("التفاصيل:"), 0, 1);
        table.Controls.Add(_details, 1, 1);
        table.SetColumnSpan(_details, 3);
        table.Controls.Add(Caption("سبب القرار:"), 0, 2);
        table.Controls.Add(_reason, 1, 2);
        table.SetColumnSpan(_reason, 3);
        _count.ForeColor = Color.FromArgb(75, 85, 99);
        _count.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        table.Controls.Add(_count, 0, 3);
        table.SetColumnSpan(_count, 4);
        return table;
    }

    private Control BuildGridCard()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _grid.Columns.Clear();
        AddColumn(nameof(ApprovalRow.Approval_ID), "رقم الطلب", 75);
        AddColumn(nameof(ApprovalRow.Request_Type), "نوع الطلب", 130);
        AddColumn(nameof(ApprovalRow.Reference_No), "المرجع", 130);
        AddColumn(nameof(ApprovalRow.Entity_Name), "الجهة", 150);
        AddColumn(nameof(ApprovalRow.Amount_Display), "المبلغ", 125, DataGridViewContentAlignment.MiddleLeft);
        AddColumn(nameof(ApprovalRow.Status_Display), "الحالة", 120);
        AddColumn(nameof(ApprovalRow.Requested_By), "مقدم الطلب", 115);
        AddColumn(nameof(ApprovalRow.Requested_At_Display), "تاريخ الطلب", 145);
        AddColumn(nameof(ApprovalRow.Reason), "سبب الطلب", 260);
        _grid.SelectionChanged += (_, _) => SelectCurrent();
        return Card("قائمة طلبات الاعتماد", _grid);
    }

    private async Task LoadRowsAsync()
    {
        try
        {
            var value = (_status.SelectedItem as StatusOption)?.Value;
            var url = string.IsNullOrWhiteSpace(value) ? "approval-requests" : $"approval-requests?status={Uri.EscapeDataString(value)}";
            var rows = await ApiService.Client.GetFromJsonAsync<List<ApprovalRow>>(url) ?? new();
            _grid.DataSource = rows;
            _grid.ClearSelection();
            _selectedId = 0;
            _selectedStatus = string.Empty;
            _details.Text = "اختر طلباً لعرض التفاصيل.";
            _count.Text = $"عدد الطلبات الظاهرة: {rows.Count}";
            ApplyButtons();
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل طلبات الاعتماد.\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // تصفية محلية فورية من دون إعادة طلب الخادم عند الكتابة.
    private void ApplyFilter()
    {
        var query = _search.Text.Trim();
        foreach (DataGridViewRow row in _grid.Rows)
            row.Visible = string.IsNullOrWhiteSpace(query) ||
                row.Cells.Cast<DataGridViewCell>().Any(x =>
                    (x.Value?.ToString() ?? string.Empty).Contains(query, StringComparison.CurrentCultureIgnoreCase));
        _count.Text = $"عدد الطلبات الظاهرة: {_grid.Rows.Cast<DataGridViewRow>().Count(x => x.Visible)}";
    }

    private void SelectCurrent()
    {
        if (_grid.SelectedRows.Count == 0) return;
        var row = _grid.SelectedRows[0].DataBoundItem as ApprovalRow;
        if (row is null) return;

        _selectedId = row.Approval_ID;
        _selectedStatus = row.Status ?? string.Empty;
        _details.Text = $"نوع الطلب: {row.Request_Type ?? "—"}   |   المرجع: {row.Reference_No}   |   الجهة: {row.Entity_Name}\nالحالة: {row.Status_Display}   |   المبلغ: {row.Amount_Display}\nسبب الطلب: {row.Reason ?? "—"}";
        _reason.Clear();
        ApplyButtons();
    }

    private async Task DecideAsync(string action)
    {
        if (_selectedId <= 0)
        {
            MessageBox.Show("اختر طلب اعتماد أولاً.");
            return;
        }

        var requiresReason = action is "approve" or "reject" or "return";
        if (requiresReason && string.IsNullOrWhiteSpace(_reason.Text))
        {
            MessageBox.Show("سبب القرار إلزامي للتدقيق.");
            _reason.Focus();
            return;
        }

        var response = await ApiService.Client.PostAsJsonAsync($"approval-requests/{_selectedId}/{action}", new { reason = _reason.Text.Trim() });
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "طلبات الاعتماد", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await LoadRowsAsync();
        MessageBox.Show("تم تنفيذ القرار وتسجيله في التدقيق.");
    }

    private void ApplyButtons()
    {
        var pending = string.Equals(_selectedStatus, "Pending", StringComparison.OrdinalIgnoreCase);
        var underReview = string.Equals(_selectedStatus, "UnderReview", StringComparison.OrdinalIgnoreCase);
        _review.Enabled = pending;
        _approve.Enabled = pending || underReview;
        _reject.Enabled = pending || underReview;
        _return.Enabled = pending || underReview;
    }

    private static Label Caption(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleRight,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = Color.FromArgb(31, 58, 92)
    };

    private static Button Button(string text, EventHandler handler, Color color)
    {
        var button = new Button { Text = text, Width = 106, Height = 33, Margin = new Padding(3), FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White };
        button.FlatAppearance.BorderSize = 0;
        button.Click += handler;
        return button;
    }

    private void AddColumn(string property, string title, int width, DataGridViewContentAlignment alignment = DataGridViewContentAlignment.MiddleRight) =>
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            Name = property,
            HeaderText = title,
            Width = width,
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = alignment, NullValue = "—" }
        });

    private static Panel Card(string title, Control body)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Padding = new Padding(8) };
        body.Dock = DockStyle.Fill;
        panel.Controls.Add(body);
        panel.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) });
        return panel;
    }

    private sealed class ApprovalRow
    {
        public int Approval_ID { get; set; }
        public string? Request_Type { get; set; }
        public string? Reference_Type { get; set; }
        public string? Reference_ID { get; set; }
        public string? Entity_Type { get; set; }
        public string? Entity_ID { get; set; }
        public string? Currency_Code { get; set; }
        public decimal? Amount { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public string? Requested_By { get; set; }
        public DateTime Requested_At { get; set; }

        public string Reference_No => string.IsNullOrWhiteSpace(Reference_ID) ? "—" : Reference_ID;
        public string Entity_Name => string.IsNullOrWhiteSpace(Entity_ID) ? "—" : Entity_ID;
        public string Amount_Display => Amount.HasValue ? $"{Amount.Value:N2} {Currency_Code}" : "—";
        public string Requested_At_Display => Requested_At == default ? "—" : Requested_At.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
        public string Status_Display => Status switch
        {
            "Pending" => "بانتظار الإجراء",
            "UnderReview" => "تحت المراجعة",
            "Approved" => "معتمد",
            "Rejected" => "مرفوض",
            "Returned" => "معاد للتعديل",
            "Canceled" => "ملغي",
            _ => Status ?? "—"
        };
    }

    private sealed record StatusOption(string Display, string? Value)
    {
        public override string ToString() => Display;
    }
}
