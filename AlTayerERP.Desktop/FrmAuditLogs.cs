using System.Net.Http.Json;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة قراءة فقط لسجل التدقيق والرقابة. لا توجد بها أزرار تعديل أو حذف،
    /// لأن مصدر الحقيقة هو Audit_Logs الذي يكتبه API مع كل عملية حساسة.
    /// </summary>
    public sealed class FrmAuditLogs : Form
    {
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false
        };
        private readonly DateTimePicker _from = new() { Width = 150, Format = DateTimePickerFormat.Short };
        private readonly DateTimePicker _to = new() { Width = 150, Format = DateTimePickerFormat.Short };
        private readonly TextBox _table = new() { Width = 150, PlaceholderText = "الجدول" };
        private readonly TextBox _action = new() { Width = 130, PlaceholderText = "العملية" };
        private readonly Label _count = new() { AutoSize = true };

        public FrmAuditLogs()
        {
            Text = "سجل التدقيق والرقابة";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Width = 1180;
            Height = 680;
            Font = new Font("Segoe UI", 9.5F);

            _from.Value = DateTime.Today.AddDays(-7);
            _to.Value = DateTime.Today.AddDays(1).AddTicks(-1);

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(10),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White
            };
            var refresh = new Button { Text = "تحديث", Width = 105, Height = 30 };
            refresh.Click += async (_, _) => await LoadAsync();
            toolbar.Controls.AddRange(new Control[]
            {
                refresh, new Label { Text = "من:", AutoSize = true, Padding = new Padding(4,7,0,0) }, _from,
                new Label { Text = "إلى:", AutoSize = true, Padding = new Padding(4,7,0,0) }, _to,
                _table, _action, _count
            });

            Controls.Add(_grid);
            Controls.Add(toolbar);
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var route = $"AuditLogs?from={Uri.EscapeDataString(_from.Value.ToUniversalTime().ToString("O"))}" +
                            $"&to={Uri.EscapeDataString(_to.Value.ToUniversalTime().ToString("O"))}" +
                            $"&tableName={Uri.EscapeDataString(_table.Text.Trim())}" +
                            $"&actionType={Uri.EscapeDataString(_action.Text.Trim())}";
                var response = await ApiService.Client.GetFromJsonAsync<AuditSearchResponse>(route);
                _grid.DataSource = response?.Rows ?? new List<AuditLogRow>();
                _count.Text = $"عدد السجلات: {response?.Total ?? 0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل سجل التدقيق.\n" + ex.Message,
                    "سجل التدقيق", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private sealed class AuditSearchResponse
        {
            public int Total { get; set; }
            public List<AuditLogRow> Rows { get; set; } = new();
        }

        private sealed class AuditLogRow
        {
            public long Audit_ID { get; set; }
            public string Table_Name { get; set; } = string.Empty;
            public string Record_ID { get; set; } = string.Empty;
            public string Action_Type { get; set; } = string.Empty;
            public string? User_ID { get; set; }
            public string? Branch_ID { get; set; }
            public DateTime Action_At { get; set; }
            public string Action_Channel { get; set; } = string.Empty;
            public string? Device_Name { get; set; }
            public string? IP_Address { get; set; }
            public string? Notes { get; set; }
        }
    }
}
