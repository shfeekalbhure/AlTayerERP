using System.Drawing.Printing;
using System.Net.Http.Json;
using System.Text.Json;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة قراءة فقط لسجل التدقيق والرقابة؛ تعرض الأثر ولا تسمح بتغييره أو حذفه.
    /// </summary>
    public sealed class FrmAuditLogs : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly DataGridView _grid = new();
        private readonly DateTimePicker _from = new() { Width = 135, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _to = new() { Width = 135, Format = DateTimePickerFormat.Short };
        private readonly TextBox _search = SearchBox("بحث في السجل");
        private readonly TextBox _table = SearchBox("الشاشة/الجدول");
        private readonly TextBox _action = SearchBox("العملية");
        private readonly TextBox _record = SearchBox("رقم العملية");
        private readonly TextBox _userId = SearchBox("رقم المستخدم");
        private readonly TextBox _branchId = SearchBox("رقم الفرع");
        private readonly TextBox _device = SearchBox("الجهاز");
        private readonly TextBox _channel = SearchBox("القناة");
        private readonly Label _count = new() { AutoSize = true, Padding = new Padding(10, 7, 10, 0) };
        private readonly Button _previous = new() { Text = "السابق", Width = 82 };
        private readonly Button _next = new() { Text = "التالي", Width = 82 };
        private int _page = 1;
        private const int PageSize = 100;
        private int _total;
        private int _printIndex;

        public FrmAuditLogs()
        {
            Text = "سجل التدقيق والرقابة";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(1050, 650);
            Width = 1350;
            Height = 760;
            Font = new Font("Segoe UI", 9.5F);

            _from.Value = DateTime.Today.AddDays(-7);
            _to.Value = DateTime.Today.AddDays(1).AddTicks(-1);
            ConfigureGrid();

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 46, Padding = new Padding(10, 7, 10, 4),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.White
            };
            var refresh = ActionButton("تحديث");
            var print = ActionButton("طباعة");
            var export = ActionButton("تصدير CSV");
            var details = ActionButton("تفاصيل السجل");
            refresh.Click += async (_, _) => { _page = 1; await LoadAsync(); };
            print.Click += async (_, _) => await PrintAsync();
            export.Click += async (_, _) => await ExportAsync();
            details.Click += async (_, _) => await ShowDetailsAsync();
            actions.Controls.AddRange(new Control[] { refresh, print, export, details });

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 90, Padding = new Padding(10, 8, 10, 8),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = true, BackColor = Color.FromArgb(248, 250, 252)
            };
            filters.Controls.AddRange(new Control[]
            {
                LabelOf("بحث:"), _search, LabelOf("من:"), _from, LabelOf("إلى:"), _to,
                _table, _action, _record, _userId, _branchId, _device, _channel
            });

            var pager = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(10, 5, 10, 4),
                FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.White
            };
            _previous.Click += async (_, _) => { if (_page > 1) { _page--; await LoadAsync(); } };
            _next.Click += async (_, _) => { if (_page * PageSize < _total) { _page++; await LoadAsync(); } };
            pager.Controls.AddRange(new Control[] { _next, _previous, _count });

            Controls.Add(_grid);
            Controls.Add(pager);
            Controls.Add(filters);
            Controls.Add(actions);
            _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDetailsAsync(); };
            KeyPreview = true;
            KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.F5) { await LoadAsync(); e.SuppressKeyPress = true; }
                if (e.Control && e.KeyCode == Keys.F) { _search.Focus(); _search.SelectAll(); e.SuppressKeyPress = true; }
            };
            Load += async (_, _) => await LoadAsync();
        }

        private static TextBox SearchBox(string placeholder) => new()
        {
            Width = 132, Height = 29, PlaceholderText = placeholder, TextAlign = HorizontalAlignment.Right
        };

        private static Label LabelOf(string text) => new() { Text = text, AutoSize = true, Padding = new Padding(4, 7, 0, 0) };
        private static Button ActionButton(string text) => new() { Text = text, Width = 110, Height = 30, FlatStyle = FlatStyle.Flat };

        private void ConfigureGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowHeadersVisible = false;
            _grid.RightToLeft = RightToLeft.Yes;
            _grid.RowTemplate.Height = 30;
            _grid.ColumnHeadersHeight = 36;
            AddColumn("Audit_ID", "رقم السجل", 75);
            AddColumn("Table_Name", "الشاشة", 130);
            AddColumn("Record_ID", "رقم العملية", 110);
            AddColumn("Action_Type", "العملية", 105);
            AddColumn("User_Name", "المستخدم", 140);
            AddColumn("Branch_Name", "الفرع", 120);
            AddColumn("Action_At", "التاريخ والوقت", 145, "yyyy/MM/dd HH:mm:ss");
            AddColumn("Action_Channel", "القناة", 80);
            AddColumn("Device_Name", "الجهاز", 140);
            AddColumn("IP_Address", "العنوان", 115);
            AddColumn("Notes", "ملاحظات", 220);
        }

        private void AddColumn(string property, string header, int width, string? format = null)
        {
            var column = new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = header, Width = width, Name = "col" + property };
            if (!string.IsNullOrWhiteSpace(format)) column.DefaultCellStyle.Format = format;
            _grid.Columns.Add(column);
        }

        private async Task LoadAsync()
        {
            try
            {
                UseWaitCursor = true;
                var response = await _client.GetFromJsonAsync<AuditSearchResponse>(BuildRoute("AuditLogs"));
                _grid.DataSource = response?.Rows ?? new List<AuditLogRow>();
                _total = response?.Total ?? 0;
                _page = response?.Page ?? 1;
                _count.Text = $"السجلات: {_total:N0} | الصفحة: {_page}";
                _previous.Enabled = _page > 1;
                _next.Enabled = _page * PageSize < _total;
                _grid.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل سجل التدقيق والرقابة.\n" + ex.Message, "سجل التدقيق", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { UseWaitCursor = false; }
        }

        private string BuildRoute(string endpoint)
        {
            var values = new Dictionary<string, string>
            {
                ["from"] = _from.Value.ToUniversalTime().ToString("O"), ["to"] = _to.Value.ToUniversalTime().ToString("O"),
                ["page"] = _page.ToString(), ["pageSize"] = PageSize.ToString(), ["search"] = _search.Text.Trim(),
                ["table_Name"] = _table.Text.Trim(), ["action_Type"] = _action.Text.Trim(), ["record_ID"] = _record.Text.Trim(),
                ["user_ID"] = _userId.Text.Trim(), ["branch_ID"] = _branchId.Text.Trim(), ["device_Name"] = _device.Text.Trim(),
                ["action_Channel"] = _channel.Text.Trim()
            };
            return endpoint + "?" + string.Join("&", values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
        }

        private async Task ShowDetailsAsync()
        {
            if (_grid.CurrentRow?.DataBoundItem is not AuditLogRow selected) return;
            try
            {
                var detail = await _client.GetFromJsonAsync<AuditDetail>($"AuditLogs/{selected.Audit_ID}");
                if (detail == null) return;
                using var dialog = new Form
                {
                    Text = "تفاصيل سجل التدقيق", Width = 900, Height = 610, StartPosition = FormStartPosition.CenterParent,
                    RightToLeft = RightToLeft.Yes, RightToLeftLayout = true, Font = Font
                };
                var summary = new Label { Dock = DockStyle.Top, Height = 86, Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle,
                    Text = $"المستخدم: {detail.Row.User_Name}   |   العملية: {detail.Row.Action_Type}   |   الشاشة: {detail.Row.Table_Name}\n" +
                           $"رقم العملية: {detail.Row.Record_ID}   |   الفرع: {detail.Row.Branch_Name}   |   التاريخ: {detail.Row.Action_At.ToLocalTime():yyyy/MM/dd HH:mm:ss}\n" +
                           $"الجهاز: {detail.Device_Name ?? "—"}   |   IP: {detail.IP_Address ?? "—"}\nالملاحظات: {detail.Notes ?? "—"}" };
                var tabs = new TabControl { Dock = DockStyle.Fill, RightToLeftLayout = true };
                tabs.TabPages.Add(CreateJsonPage("القيم قبل التعديل", detail.Old_Values));
                tabs.TabPages.Add(CreateJsonPage("القيم بعد التعديل", detail.New_Values));
                dialog.Controls.Add(tabs); dialog.Controls.Add(summary); dialog.ShowDialog(this);
            }
            catch (Exception ex) { MessageBox.Show("تعذر تحميل تفاصيل السجل.\n" + ex.Message, "سجل التدقيق", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private static TabPage CreateJsonPage(string title, string? json)
        {
            var box = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 10F), Text = PrettyJson(json) };
            var page = new TabPage(title); page.Controls.Add(box); return page;
        }

        private static string PrettyJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "لا توجد قيم مسجلة.";
            try { using var document = JsonDocument.Parse(json); return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true }); }
            catch { return json; }
        }

        private async Task ExportAsync()
        {
            using var dialog = new SaveFileDialog { Filter = "CSV UTF-8|*.csv", FileName = $"سجل_التدقيق_{DateTime.Now:yyyyMMdd_HHmm}.csv" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                UseWaitCursor = true;
                await File.WriteAllBytesAsync(dialog.FileName, await _client.GetByteArrayAsync(BuildRoute("AuditLogs/export")));
                MessageBox.Show("تم تصدير سجل التدقيق بنجاح.", "التصدير", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("تعذر تصدير السجل.\n" + ex.Message, "التصدير", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { UseWaitCursor = false; }
        }

        private async Task PrintAsync()
        {
            if (_grid.Rows.Count == 0) { MessageBox.Show("لا توجد سجلات للطباعة.", "الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            using var document = new PrintDocument { DocumentName = "سجل التدقيق والرقابة" };
            document.DefaultPageSettings.Landscape = true;
            document.PrintPage += PrintPage;
            using var preview = new PrintPreviewDialog { Document = document, Width = 1150, Height = 760, StartPosition = FormStartPosition.CenterParent };
            _printIndex = 0; preview.ShowDialog(this);
            try { await _client.PostAsync("AuditLogs/print", null); } catch { /* الطباعة لا تتعطل إذا تعذر تسجيل أثرها. */ }
        }

        private void PrintPage(object? sender, PrintPageEventArgs e)
        {
            if (e.Graphics == null) return;
            using var title = new Font("Segoe UI", 15F, FontStyle.Bold); using var header = new Font("Segoe UI", 8F, FontStyle.Bold);
            using var rowFont = new Font("Segoe UI", 7F); using var pen = new Pen(Color.Black);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.DirectionRightToLeft };
            var bounds = e.MarginBounds; e.Graphics.DrawString("سجل التدقيق والرقابة", title, Brushes.Black, bounds.Left, bounds.Top);
            var columns = _grid.Columns.Cast<DataGridViewColumn>().Where(x => x.Visible).ToList();
            var width = Math.Max(58, bounds.Width / columns.Count); const int height = 27; var y = bounds.Top + 38;
            DrawRow(e.Graphics, columns.Select(x => x.HeaderText), bounds.Right, y, width, height, header, pen, format); y += height;
            while (_printIndex < _grid.Rows.Count)
            {
                var row = _grid.Rows[_printIndex++]; DrawRow(e.Graphics, columns.Select(x => row.Cells[x.Index].FormattedValue?.ToString() ?? string.Empty), bounds.Right, y, width, height, rowFont, pen, format); y += height;
                if (y + height > bounds.Bottom) { e.HasMorePages = _printIndex < _grid.Rows.Count; return; }
            }
            _printIndex = 0; e.HasMorePages = false;
        }

        private static void DrawRow(Graphics graphics, IEnumerable<string> values, int right, int y, int width, int height, Font font, Pen pen, StringFormat format)
        {
            var x = right - width;
            foreach (var value in values) { var rect = new Rectangle(x, y, width, height); graphics.DrawRectangle(pen, rect); graphics.DrawString(value, font, Brushes.Black, rect, format); x -= width; }
        }

        private sealed class AuditSearchResponse { public int Total { get; set; } public int Page { get; set; } public List<AuditLogRow> Rows { get; set; } = new(); }
        private sealed class AuditDetail { public AuditLogRow Row { get; set; } = new(); public string? Old_Values { get; set; } public string? New_Values { get; set; } public string? Notes { get; set; } public string? IP_Address { get; set; } public string? Device_Name { get; set; } }
        private sealed class AuditLogRow
        {
            public long Audit_ID { get; set; } public string Table_Name { get; set; } = string.Empty; public string Record_ID { get; set; } = string.Empty; public string Action_Type { get; set; } = string.Empty;
            public string User_Name { get; set; } = "غير متاح"; public string Branch_Name { get; set; } = "غير متاح"; public DateTime Action_At { get; set; }
            public string Action_Channel { get; set; } = string.Empty; public string? Device_Name { get; set; } public string? IP_Address { get; set; } public string? Notes { get; set; }
        }
    }
}
