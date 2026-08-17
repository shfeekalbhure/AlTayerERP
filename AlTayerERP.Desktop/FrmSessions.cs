using System.Net.Http.Json;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// شاشة الجلسات الحية. لا تعرض رموز JWT أو Refresh Token؛ تعرض البيانات اللازمة
    /// للرقابة فقط، ويسمح API بإبطال جلسة المستخدم نفسه أو أي جلسة لمدير النظام.
    /// </summary>
    public sealed class FrmSessions : Form
    {
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        public FrmSessions()
        {
            Text = "الجلسات النشطة";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterParent;
            Width = 1040;
            Height = 620;
            Font = new Font("Segoe UI", 9.5F);

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(10),
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.White
            };
            var refresh = new Button { Text = "تحديث", Width = 105, Height = 30 };
            var revoke = new Button { Text = "إبطال الجلسة", Width = 125, Height = 30, BackColor = Color.Firebrick, ForeColor = Color.White };
            refresh.Click += async (_, _) => await LoadAsync();
            revoke.Click += async (_, _) => await RevokeSelectedAsync();
            panel.Controls.Add(revoke);
            panel.Controls.Add(refresh);

            Controls.Add(_grid);
            Controls.Add(panel);
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var rows = await ApiService.Client.GetFromJsonAsync<List<SessionRow>>("Sessions");
                _grid.DataSource = rows ?? new List<SessionRow>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل الجلسات.\n" + ex.Message,
                    "الجلسات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task RevokeSelectedAsync()
        {
            if (_grid.CurrentRow?.DataBoundItem is not SessionRow row || string.IsNullOrWhiteSpace(row.Session_ID))
                return;

            if (MessageBox.Show("هل تريد إبطال هذه الجلسة؟", "تأكيد", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            var response = await ApiService.Client.PostAsync($"Sessions/{Uri.EscapeDataString(row.Session_ID)}/revoke", null);
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر الإبطال",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadAsync();
        }

        private sealed class SessionRow
        {
            public string Session_ID { get; set; } = string.Empty;
            public int User_ID { get; set; }
            public int Role_ID { get; set; }
            public string Company_ID { get; set; } = string.Empty;
            public int Branch_ID { get; set; }
            public int Year_ID { get; set; }
            public string Device_ID { get; set; } = string.Empty;
            public DateTime Issued_At { get; set; }
            public DateTime Expires_At { get; set; }
        }
    }
}
