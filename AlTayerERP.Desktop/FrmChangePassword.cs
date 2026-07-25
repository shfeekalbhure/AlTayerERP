using System.Net.Http.Json;
using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop;

/// <summary>شاشة الخدمة الذاتية لتغيير كلمة مرور المستخدم صاحب الجلسة الحالية فقط.</summary>
public sealed class FrmChangePassword : BaseForm
{
    private readonly TextBox _currentPassword = PasswordInput();
    private readonly TextBox _newPassword = PasswordInput();
    private readonly TextBox _confirmPassword = PasswordInput();
    private readonly CheckBox _showPasswords = new() { Text = "إظهار كلمات المرور", AutoSize = true };
    private readonly Button _save;

    public FrmChangePassword()
    {
        Text = "تغيير كلمة المرور";
        Width = 690;
        Height = 500;
        MinimumSize = new Size(620, 440);
        StartPosition = FormStartPosition.CenterParent;
        ApplyBaseFormStyle();

        _save = CreateButton("تغيير كلمة المرور", Color.FromArgb(15, 103, 208));
        _save.Click += async (_, _) => await ChangePasswordAsync();
        Build();
        KeyDown += HandleKeys;
    }

    private void Build()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(244, 247, 251)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        shell.Controls.Add(new BrandHeaderControl("تغيير كلمة المرور"), 0, 0);
        shell.Controls.Add(BuildFormCard(), 0, 1);
        shell.Controls.Add(BuildActions(), 0, 2);
        shell.Controls.Add(new Label
        {
            Text = "لن تُعرض كلمة المرور أو تُسجل في التدقيق. الحد الأدنى 8 أحرف ويتضمن حرفاً ورقماً.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(71, 85, 105),
            Font = new Font("Segoe UI", 8.7F),
            Padding = new Padding(8, 0, 8, 0)
        }, 0, 3);

        Controls.Add(shell);
        _currentPassword.Focus();
    }

    private Control BuildFormCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(18, 14, 18, 14)
        };

        var heading = new Label
        {
            Text = $"المستخدم الحالي: {CurrentSession.Full_Name}",
            Dock = DockStyle.Top,
            Height = 34,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 55, 112)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(0, 8, 0, 0),
            RightToLeft = RightToLeft.Yes
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 3; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 53));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        AddField(fields, 0, "كلمة المرور الحالية *", _currentPassword);
        AddField(fields, 1, "كلمة المرور الجديدة *", _newPassword);
        AddField(fields, 2, "تأكيد كلمة المرور الجديدة *", _confirmPassword);
        _showPasswords.CheckedChanged += (_, _) =>
        {
            var hide = !_showPasswords.Checked;
            _currentPassword.UseSystemPasswordChar = hide;
            _newPassword.UseSystemPasswordChar = hide;
            _confirmPassword.UseSystemPasswordChar = hide;
        };
        fields.Controls.Add(_showPasswords, 1, 3);

        card.Controls.Add(fields);
        card.Controls.Add(heading);
        return card;
    }

    private Control BuildActions()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(2, 7, 2, 2),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        var close = CreateButton("إغلاق", Color.FromArgb(91, 101, 116));
        close.Click += (_, _) => Close();
        actions.Controls.Add(_save);
        actions.Controls.Add(close);
        return actions;
    }

    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(_currentPassword.Text) ||
            string.IsNullOrWhiteSpace(_newPassword.Text) ||
            string.IsNullOrWhiteSpace(_confirmPassword.Text))
        {
            MessageBox.Show("أدخل كلمة المرور الحالية والجديدة وتأكيدها.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_newPassword.Text != _confirmPassword.Text)
        {
            MessageBox.Show("تأكيد كلمة المرور الجديدة غير مطابق.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _confirmPassword.Focus();
            _confirmPassword.SelectAll();
            return;
        }

        _save.Enabled = false;
        try
        {
            var response = await ApiService.Client.PostAsJsonAsync("Auth/change-password", new
            {
                Current_Password = _currentPassword.Text,
                New_Password = _newPassword.Text,
                Confirm_New_Password = _confirmPassword.Text
            });

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                MessageBox.Show(string.IsNullOrWhiteSpace(message) ? "تعذر تغيير كلمة المرور." : message,
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentPassword.Clear();
            _newPassword.Clear();
            _confirmPassword.Clear();
            MessageBox.Show("تم تغيير كلمة المرور بنجاح.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            AppExceptionHandler.HandleUiException(ex, "تغيير كلمة المرور");
        }
        finally
        {
            if (!IsDisposed) _save.Enabled = true;
        }
    }

    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F2) { _ = ChangePasswordAsync(); e.SuppressKeyPress = true; }
        if (e.KeyCode == Keys.Escape) { Close(); e.SuppressKeyPress = true; }
    }

    private static void AddField(TableLayoutPanel table, int row, string caption, Control input)
    {
        table.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9.3F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 58, 92)
        }, 0, row);
        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(5, 7, 5, 7);
        table.Controls.Add(input, 1, row);
    }

    private static TextBox PasswordInput() => new() { UseSystemPasswordChar = true, BorderStyle = BorderStyle.FixedSingle };

    private static Button CreateButton(string text, Color color) => new()
    {
        Text = text,
        Width = 155,
        Height = 34,
        Margin = new Padding(4, 0, 4, 0),
        FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = 0 },
        BackColor = color,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        UseVisualStyleBackColor = false
    };
}
