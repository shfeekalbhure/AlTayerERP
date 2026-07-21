using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Common
{
    /// <summary>
    /// عنصر موحّد يعرضه أي نموذج استعلام داخل النظام.
    /// </summary>
    public sealed class LookupDialogItem
    {
        public string Id { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string DisplayName => string.IsNullOrWhiteSpace(Code) ? Name : $"{Code} - {Name}";
    }

    /// <summary>
    /// شاشة استعلام عامة: البحث بالرقم أو الاسم ثم الاختيار بزر Enter أو النقر المزدوج.
    /// </summary>
    public class FrmReferenceLookup : Form
    {
        private readonly List<LookupDialogItem> _allItems;
        private readonly TextBox _txtSearch = new();
        private readonly DataGridView _grid = new();

        public LookupDialogItem? SelectedItem { get; private set; }

        protected FrmReferenceLookup(string title, IEnumerable<LookupDialogItem> items, string? initialSearch = null)
        {
            _allItems = (items ?? Enumerable.Empty<LookupDialogItem>())
                .OrderBy(x => x.Code)
                .ThenBy(x => x.Name)
                .ToList();

            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            Width = 760;
            Height = 500;
            MinimumSize = new Size(600, 380);
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            KeyPreview = true;

            var lblSearch = new Label
            {
                Text = "بحث بالرقم أو الاسم:",
                AutoSize = true,
                Location = new Point(610, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _txtSearch.Location = new Point(25, 17);
            _txtSearch.Width = 570;
            _txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtSearch.Text = initialSearch?.Trim() ?? string.Empty;

            _grid.Location = new Point(25, 55);
            _grid.Size = new Size(695, 340);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.ReadOnly = true;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.RowHeadersVisible = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCode",
                HeaderText = "الرقم",
                DataPropertyName = nameof(LookupDialogItem.Code),
                FillWeight = 35
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "الاسم",
                DataPropertyName = nameof(LookupDialogItem.Name),
                FillWeight = 65
            });

            var btnSelect = new Button
            {
                Text = "اختيار",
                DialogResult = DialogResult.None,
                Size = new Size(100, 30),
                Location = new Point(620, 410),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            var btnCancel = new Button
            {
                Text = "إلغاء",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 30),
                Location = new Point(510, 410),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            Controls.AddRange(new Control[] { lblSearch, _txtSearch, _grid, btnSelect, btnCancel });
            AcceptButton = btnSelect;
            CancelButton = btnCancel;

            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            _txtSearch.KeyDown += SearchKeyDown;
            _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) AcceptSelection(); };
            _grid.KeyDown += GridKeyDown;
            btnSelect.Click += (_, _) => AcceptSelection();
            KeyDown += FormKeyDown;

            ApplyFilter();
            Shown += (_, _) =>
            {
                _txtSearch.Focus();
                _txtSearch.SelectAll();
            };
        }

        private void ApplyFilter()
        {
            string term = _txtSearch.Text.Trim();
            IEnumerable<LookupDialogItem> result = _allItems;

            if (!string.IsNullOrWhiteSpace(term))
            {
                result = result.Where(item =>
                    item.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    item.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            _grid.DataSource = result.ToList();

            if (_grid.Rows.Count > 0)
            {
                _grid.ClearSelection();
                _grid.Rows[0].Selected = true;
                _grid.CurrentCell = _grid.Rows[0].Cells[0];
            }
        }

        private void AcceptSelection()
        {
            if (_grid.CurrentRow?.DataBoundItem is not LookupDialogItem item)
            {
                return;
            }

            SelectedItem = item;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void SearchKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && _grid.Rows.Count > 0)
            {
                _grid.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void GridKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AcceptSelection();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F9)
            {
                _txtSearch.Focus();
                _txtSearch.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FormKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }

    /// <summary>شاشة استعلام مراكز التكلفة.</summary>
    public sealed class FrmCostCenterLookup : FrmReferenceLookup
    {
        public FrmCostCenterLookup(IEnumerable<LookupDialogItem> items, string? initialSearch = null)
            : base("استعلام مراكز التكلفة", items, initialSearch)
        {
        }
    }

    /// <summary>شاشة استعلام الصناديق.</summary>
    public sealed class FrmCashBoxLookup : FrmReferenceLookup
    {
        public FrmCashBoxLookup(IEnumerable<LookupDialogItem> items, string? initialSearch = null)
            : base("استعلام الصناديق", items, initialSearch)
        {
        }
    }
}
