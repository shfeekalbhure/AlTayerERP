using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>
/// معالج أول تشغيل. يجمع بيانات المنشأة المتغيرة فقط؛ API ينشئ القوائم والترقيم والإعدادات الثابتة تلقائياً.
/// </summary>
public sealed class FrmInitialSetup : Form
{
    private readonly TextBox _group = new();
    private readonly TextBox _company = new();
    private readonly TextBox _prefix = new();
    private readonly TextBox _branch = new();
    private readonly NumericUpDown _year = new() { Minimum = 2020, Maximum = 2100, Value = DateTime.Today.Year };
    private readonly TextBox _adminName = new();
    private readonly TextBox _login = new();
    private readonly TextBox _password = new() { UseSystemPasswordChar = true };
    private readonly Button _save = new() { Text = "إنشاء النظام", AutoSize = false, Width = 150, Height = 36 };

    public FrmInitialSetup()
    {
        Text = "معالج التهيئة الأولية";
        StartPosition = FormStartPosition.CenterParent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
        Font = new Font("Segoe UI", 10F); Width = 620; Height = 570;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 11 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        layout.Controls.Add(new Label { Text = "بيانات المنشأة لأول تشغيل", Font = new Font("Segoe UI", 13F, FontStyle.Bold), AutoSize = true }, 0, 0);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 0), 2);
        Add(layout, 1, "اسم المجموعة التجارية *", _group);
        Add(layout, 2, "اسم الشركة *", _company);
        Add(layout, 3, "رمز الشركة للترقيم", _prefix);
        Add(layout, 4, "اسم الفرع الرئيسي *", _branch);
        Add(layout, 5, "السنة المالية", _year);
        Add(layout, 6, "اسم مدير النظام", _adminName);
        Add(layout, 7, "اسم الدخول *", _login);
        Add(layout, 8, "كلمة المرور *", _password);
        var note = new Label { Text = "ستُنشأ تلقائياً: أنواع الفروع، الصلاحيات، أنواع وحالات السندات، طرق السداد، إعدادات الترقيم، والإعدادات الافتراضية.", AutoSize = true, MaximumSize = new Size(500, 0), ForeColor = Color.FromArgb(70, 80, 95) };
        layout.Controls.Add(note, 0, 9); layout.SetColumnSpan(note, 2);
        var cancel = new Button { Text = "إلغاء", Width = 100, Height = 36 }; cancel.Click += (_, _) => Close();
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill }; buttons.Controls.Add(_save); buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 0, 10); layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout); _save.Click += async (_, _) => await SaveAsync();
    }

    private static void Add(TableLayoutPanel layout, int row, string caption, Control input)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight }, 0, row);
        input.Dock = DockStyle.Fill; layout.Controls.Add(input, 1, row);
    }

    private async Task SaveAsync()
    {
        if (new[] { _group.Text, _company.Text, _branch.Text, _login.Text, _password.Text }.Any(string.IsNullOrWhiteSpace))
        { MessageBox.Show("أدخل كل الحقول التي عليها علامة *.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (_password.Text.Length < 8) { MessageBox.Show("كلمة المرور يجب ألا تقل عن 8 أحرف.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        _save.Enabled = false; UseWaitCursor = true;
        try
        {
            var response = await ApiService.Client.PostAsJsonAsync("InitialSetup", new
            {
                Group_Name_AR = _group.Text.Trim(), Company_Name_AR = _company.Text.Trim(), Company_Prefix = _prefix.Text.Trim(),
                Branch_Name = _branch.Text.Trim(), Fiscal_Year = (int)_year.Value,
                Admin_Full_Name = _adminName.Text.Trim(), Admin_Login_Name = _login.Text.Trim(), Admin_Password = _password.Text
            });
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
            MessageBox.Show("اكتملت التهيئة. سجّل الدخول الآن باسم مدير النظام الذي أنشأته.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { MessageBox.Show("تعذر إنشاء التهيئة.\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; _save.Enabled = true; }
    }
}
