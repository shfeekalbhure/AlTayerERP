using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// عرض طرق السداد المرجعية التي تعتمد عليها سندات القبض.
    /// وضع القراءة فقط إلى حين اعتماد طبقة صلاحيات الكتابة المالية في الخادم.
    /// </summary>
    public class FrmPaymentMethods : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;
        private readonly DataGridView dgvItems = new();
        private readonly Button btnRefresh = new();

        public FrmPaymentMethods()
        {
            Text = "طرق السداد";
            StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 10F);
            Width = 1060;
            Height = 600;
            MinimumSize = new Size(840, 480);

            btnRefresh.Text = "تحديث";
            btnRefresh.AutoSize = true;
            btnRefresh.Click += async (_, _) => await LoadItemsAsync();

            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(8),
                FlowDirection = FlowDirection.RightToLeft
            };
            header.Controls.Add(btnRefresh);
            header.Controls.Add(new Label
            {
                Text = "هذه القائمة مرجعية لسند القبض، والتعديل الإداري محكوم بصلاحيات الخادم.",
                AutoSize = true,
                Padding = new Padding(10)
            });

            dgvItems.Dock = DockStyle.Fill;
            dgvItems.ReadOnly = true;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.AllowUserToOrderColumns = false;
            dgvItems.MultiSelect = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AutoGenerateColumns = true;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.RowHeadersVisible = false;

            Controls.Add(dgvItems);
            Controls.Add(header);
            Load += async (_, _) => await LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                List<PaymentMethodItem>? items =
                    await _client.GetFromJsonAsync<List<PaymentMethodItem>>(
                        _baseUrl + "PaymentMethods");

                dgvItems.DataSource = items ?? new List<PaymentMethodItem>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "تعذر تحميل طرق السداد.\n" + ex.Message,
                    "طرق السداد",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
            }
        }

        private sealed class PaymentMethodItem
        {
            public int Payment_Method_ID { get; set; }
            public string Payment_Method_Code { get; set; } = string.Empty;
            public string Payment_Method_Name_AR { get; set; } = string.Empty;
            public string? Payment_Method_Name_EN { get; set; }
            public bool Requires_Reference { get; set; }
            public bool Requires_Reference_Date { get; set; }
            public bool Is_Cash { get; set; }
            public bool Is_Bank { get; set; }
            public bool Is_Active { get; set; }
            public int Sort_Order { get; set; }
        }
    }
}