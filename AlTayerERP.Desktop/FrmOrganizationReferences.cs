using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlTayerERP.Desktop.Services;
using static AlTayerERP.Desktop.ReferenceFormUi;

namespace AlTayerERP.Desktop
{
    /// <summary>الشاشة الأولى في التسلسل: إدارة المجموعات التجارية.</summary>
    public sealed class FrmBusinessGroups : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = true };
        private readonly TextBox _arabic = new();
        private readonly TextBox _english = new();
        private readonly TextBox _code = new() { ReadOnly = true, TabStop = false };
        private readonly TextBox _notes = new();
        private readonly CheckBox _active = new() { Text = "نشطة", Checked = true, AutoSize = true };
        private string? _id;

        public FrmBusinessGroups()
        {
            Text = "إدارة المجموعات التجارية"; Width = 940; Height = 620; StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; Font = new Font("Segoe UI", 10F);
            var inputs = CreateInputs(("رقم المجموعة", _code), ("اسم المجموعة بالعربي *", _arabic), ("الاسم بالإنجليزي", _english), ("ملاحظات", _notes), ("الحالة", _active));
            var tools = CreateTools(("جديد", (_, _) => Clear()), ("حفظ", async (_, _) => await Save()), ("تفعيل/إيقاف", async (_, _) => await ChangeStatus()), ("حذف", async (_, _) => await Delete()), ("تحديث", async (_, _) => await LoadAsync()));
            Controls.Add(_grid); Controls.Add(inputs); Controls.Add(tools);
            _grid.Dock = DockStyle.Fill; _grid.SelectionChanged += (_, _) => SelectRow(); Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var rows = await _client.GetFromJsonAsync<List<BusinessGroupRow>>("TenantGroups?activeOnly=false") ?? new();
            _grid.DataSource = rows.OrderBy(x => x.Group_Name_AR).ToList();
            _grid.HideColumns("Group_ID", "Updated_At");
        }
        private void SelectRow()
        {
            if (_grid.CurrentRow?.DataBoundItem is not BusinessGroupRow r) return;
            _id = r.Group_ID; _code.Text = r.Group_Code; _arabic.Text = r.Group_Name_AR; _english.Text = r.Group_Name_EN; _notes.Text = r.Notes; _active.Checked = r.Is_Active;
        }
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(_arabic.Text)) { MessageBox.Show("اسم المجموعة بالعربي مطلوب."); return; }
            var body = new { Group_Name_AR = _arabic.Text.Trim(), Group_Name_EN = _english.Text.Trim(), Notes = _notes.Text.Trim() };
            var response = _id == null ? await _client.PostAsJsonAsync("TenantGroups", body) : await _client.PutAsJsonAsync($"TenantGroups/{_id}", body);
            await ShowResult(response, "تم حفظ المجموعة التجارية."); await LoadAsync(); Clear();
        }
        private async Task ChangeStatus()
        {
            if (_id == null) { MessageBox.Show("اختر مجموعة أولاً."); return; }
            var response = await _client.PatchAsJsonAsync($"TenantGroups/{_id}/status", !_active.Checked);
            await ShowResult(response, "تم تحديث حالة المجموعة."); await LoadAsync(); Clear();
        }
        private async Task Delete()
        {
            if (_id == null) { MessageBox.Show("اختر مجموعة أولاً."); return; }
            if (MessageBox.Show("هل تريد حذف المجموعة غير المرتبطة؟", "تأكيد", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await ShowResult(await _client.DeleteAsync($"TenantGroups/{_id}"), "تم حذف المجموعة."); await LoadAsync(); Clear();
        }
        private void Clear() { _id = null; _code.Clear(); _arabic.Clear(); _english.Clear(); _notes.Clear(); _active.Checked = true; _grid.ClearSelection(); _arabic.Focus(); }

        private sealed class BusinessGroupRow { public string Group_ID { get; set; } = ""; public string? Group_Code { get; set; } public string Group_Name_AR { get; set; } = ""; public string Group_Name_EN { get; set; } = ""; public string? Notes { get; set; } public bool Is_Active { get; set; } public DateTime Created_At { get; set; } public DateTime? Updated_At { get; set; } }
    }

    /// <summary>بيانات مرجعية مستقلة تغذي قائمة نوع الفرع في شاشة الفروع.</summary>
    public sealed class FrmBranchTypes : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoGenerateColumns = true };
        private readonly TextBox _code = new(); private readonly TextBox _arabic = new(); private readonly TextBox _english = new();
        private readonly NumericUpDown _sort = new() { Minimum = 0, Maximum = 9999, Width = 110 };
        private readonly TextBox _notes = new(); private readonly CheckBox _active = new() { Text = "نشط", Checked = true, AutoSize = true };
        private int? _id;

        public FrmBranchTypes()
        {
            Text = "إعدادات أنواع الفروع"; Width = 1100; Height = 650; StartPosition = FormStartPosition.CenterParent;
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; Font = new Font("Segoe UI", 10F);
            var inputs = CreateInputs(("رمز النوع *", _code), ("الاسم بالعربي *", _arabic), ("الاسم بالإنجليزي", _english), ("ترتيب الظهور", _sort), ("ملاحظات", _notes), ("الحالة", _active));
            var tools = CreateTools(("جديد", (_, _) => Clear()), ("حفظ", async (_, _) => await Save()), ("حذف", async (_, _) => await Delete()), ("تحديث", async (_, _) => await LoadAsync()));
            Controls.Add(_grid); Controls.Add(inputs); Controls.Add(tools);
            _grid.SelectionChanged += (_, _) => SelectRow(); Load += async (_, _) => await LoadAsync();
        }
        private async Task LoadAsync()
        {
            _grid.DataSource = await _client.GetFromJsonAsync<List<BranchTypeRow>>("BranchTypes?activeOnly=false") ?? new();
            _grid.HideColumns("Branch_Type_ID", "Created_At", "Updated_At");
        }
        private void SelectRow()
        {
            if (_grid.CurrentRow?.DataBoundItem is not BranchTypeRow r) return;
            _id = r.Branch_Type_ID; _code.Text = r.Branch_Type_Code; _arabic.Text = r.Branch_Type_Name_AR; _english.Text = r.Branch_Type_Name_EN; _sort.Value = r.Sort_Order; _notes.Text = r.Notes; _active.Checked = r.Is_Active;
        }
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(_code.Text) || string.IsNullOrWhiteSpace(_arabic.Text)) { MessageBox.Show("الرمز والاسم العربي مطلوبان."); return; }
            var body = new { Branch_Type_Code = _code.Text.Trim(), Branch_Type_Name_AR = _arabic.Text.Trim(), Branch_Type_Name_EN = _english.Text.Trim(), Sort_Order = (int)_sort.Value, Is_Active = _active.Checked, Notes = _notes.Text.Trim() };
            var response = _id == null ? await _client.PostAsJsonAsync("BranchTypes", body) : await _client.PutAsJsonAsync($"BranchTypes/{_id}", body);
            await ShowResult(response, "تم حفظ نوع الفرع."); await LoadAsync(); Clear();
        }
        private async Task Delete()
        {
            if (_id == null) { MessageBox.Show("اختر نوع فرع أولاً."); return; }
            if (MessageBox.Show("هل تريد حذف نوع الفرع؟", "تأكيد", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await ShowResult(await _client.DeleteAsync($"BranchTypes/{_id}"), "تم حذف نوع الفرع."); await LoadAsync(); Clear();
        }
        private void Clear() { _id = null; _code.Clear(); _arabic.Clear(); _english.Clear(); _notes.Clear(); _sort.Value = 0; _active.Checked = true; _grid.ClearSelection(); _code.Focus(); }
        private sealed class BranchTypeRow { public int Branch_Type_ID { get; set; } public string Branch_Type_Code { get; set; } = ""; public string Branch_Type_Name_AR { get; set; } = ""; public string? Branch_Type_Name_EN { get; set; } public int Sort_Order { get; set; } public bool Is_Active { get; set; } public string? Notes { get; set; } public DateTime Created_At { get; set; } public DateTime? Updated_At { get; set; } }
    }

    internal static class ReferenceFormUi
    {
        internal static Panel CreateInputs(params (string Caption, Control Control)[] fields)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 110, Padding = new Padding(12), FlowDirection = FlowDirection.RightToLeft, WrapContents = true, AutoScroll = true };
            foreach (var (caption, control) in fields)
            {
                control.Width = 170;
                var field = new Panel { Width = 190, Height = 68 };
                field.Controls.Add(control); control.Dock = DockStyle.Bottom;
                field.Controls.Add(new Label { Text = caption, Dock = DockStyle.Top, Height = 27, TextAlign = ContentAlignment.MiddleRight }); panel.Controls.Add(field);
            }
            return panel;
        }
        internal static FlowLayoutPanel CreateTools(params (string Text, EventHandler Action)[] buttons)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(12, 8, 12, 8), FlowDirection = FlowDirection.RightToLeft };
            foreach (var (text, action) in buttons) { var b = new Button { Text = text, Width = 110, Height = 32 }; b.Click += action; panel.Controls.Add(b); }
            return panel;
        }
        internal static void HideColumns(this DataGridView grid, params string[] names) { foreach (var name in names) if (grid.Columns.Contains(name)) grid.Columns[name].Visible = false; }
        internal static async Task ShowResult(HttpResponseMessage response, string success)
        {
            if (response.IsSuccessStatusCode) { MessageBox.Show(success, "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            MessageBox.Show(await response.Content.ReadAsStringAsync(), "تعذر التنفيذ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

}
