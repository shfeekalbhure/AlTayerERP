using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Common
{
    /// <summary>
    /// نافذة بحث موحدة للمراجع المحاسبية التي تستدعى باختصار F9.
    /// </summary>
    public sealed class FrmReferenceLookup : Form
    {
        private readonly List<ReferenceLookupItem> _items;
        private readonly TextBox txtSearch = new();
        private readonly DataGridView dgvItems = new();

        public string SelectedId { get; private set; } = string.Empty;
        public string SelectedCode { get; private set; } = string.Empty;
        public string SelectedName { get; private set; } = string.Empty;

        public FrmReferenceLookup(
            string title,
            IEnumerable<ReferenceLookupItem> items,
            string initialSearch = "")
        {
            _items = items?.ToList() ?? new List<ReferenceLookupItem>();
            Text = title;
            BuildLayout();
            txtSearch.Text = initialSearch ?? string.Empty;
            ApplySearch();
        }

        private void BuildLayout()
        {
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(760, 480);
            MinimumSize = new Size(600, 360);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            Font = new Font("Tahoma", 9F);
            BackColor = Color.FromArgb(245, 245, 240);
            KeyPreview = true;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = Color.FromArgb(52, 123, 177)
            };
            header.Controls.Add(new Label
            {
                Text = Text,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Tahoma", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            });

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(8)
            };
            searchPanel.Controls.Add(new Label
            {
                Text = "بحث:",
                Dock = DockStyle.Right,
                Width = 50,
                TextAlign = ContentAlignment.MiddleRight
            });
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.BackColor = Color.FromArgb(255, 255, 224);
            searchPanel.Controls.Add(txtSearch);

            dgvItems.Dock = DockStyle.Fill;
            dgvItems.ReadOnly = true;
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.AllowUserToOrderColumns = false;
            dgvItems.MultiSelect = false;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.AutoGenerateColumns = false;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItems.RowHeadersVisible = false;
            dgvItems.BackgroundColor = Color.White;
            dgvItems.EnableHeadersVisualStyles = false;
            dgvItems.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(225, 242, 246),
                ForeColor = Color.Black,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 9F, FontStyle.Bold)
            };
            dgvItems.RowsDefaultCellStyle = new DataGridViewCellStyle
            {
                SelectionBackColor = Color.FromArgb(206, 244, 246),
                SelectionForeColor = Color.Black
            };
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ReferenceLookupItem.Code),
                HeaderText = "الكود",
                FillWeight = 25
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ReferenceLookupItem.Name),
                HeaderText = "الاسم",
                FillWeight = 75
            });

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8)
            };
            var btnSelect = new Button { Text = "اختيار", Width = 90 };
            var btnCancel = new Button { Text = "إلغاء", Width = 90, DialogResult = DialogResult.Cancel };
            btnSelect.Click += (_, _) => SelectCurrent();
            footer.Controls.Add(btnSelect);
            footer.Controls.Add(btnCancel);

            Controls.Add(dgvItems);
            Controls.Add(footer);
            Controls.Add(searchPanel);
            Controls.Add(header);

            txtSearch.TextChanged += (_, _) => ApplySearch();
            dgvItems.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0)
                    SelectCurrent();
            };
            dgvItems.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F9)
                {
                    e.Handled = true;
                    SelectCurrent();
                }
            };
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.F9 || e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    SelectCurrent();
                }
            };
            Shown += (_, _) =>
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
            };
        }

        private void ApplySearch()
        {
            string term = txtSearch.Text.Trim();
            dgvItems.DataSource = string.IsNullOrWhiteSpace(term)
                ? _items
                : _items.Where(x =>
                    x.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void SelectCurrent()
        {
            if (dgvItems.CurrentRow?.DataBoundItem is not ReferenceLookupItem item)
                return;

            SelectedId = item.Id;
            SelectedCode = item.Code;
            SelectedName = item.Name;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    /// <summary>
    /// صف مبسط يعرضه نموذج البحث الموحد.
    /// </summary>
    public sealed class ReferenceLookupItem
    {
        public string Id { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}