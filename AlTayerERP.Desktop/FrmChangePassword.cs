using System.Net;
using System.Net.Http.Json;

namespace AlTayerERP.Desktop;

/// <summary>
/// شاشة مستقلة للمستخدم الحالي فقط. التحقق الفعلي وسياسة القوة وإبطال الجلسات
/// ينفذها API؛ التحقق المحلي هنا لتحسين تجربة الاستخدام لا ليكون حد أمان.
/// </summary>
public sealed class FrmChangePassword : Form
{
    private readonly TextBox _currentPassword = PasswordBox();
    private readonly TextBox _newPassword = PasswordBox();
    private readonly TextBox _confirmPassword = PasswordBox();
    private readonly Label _policyHint = new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        Text = "12 حرفاً على الأقل، وتتضمن حرفاً كبيراً وصغيراً ورقماً ورمزاً خاصاً. لا تستخدم اسم الدخول أو الاسم.",
        ForeColor = Color.FromArgb(75, 85, 99),
        TextAlign = ContentAlignment.MiddleRight
    };
    private readonly Button _saveButton = new() { Name = "btnSave", Text = "تغيير كلمة المرور", AutoSize = true };
    private readonly Button _closeButton = new() { Name = "btnClose", Text = "إغلاق", AutoSize = true };
    private bool _saving;

    public FrmChangePassword()
    {
        Text = "تغيير كلمة المرور";
        Name = "FrmChangePassword";
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(580, 390);
        Size = new Size(640, 430);

        var header = new Label
        {
            Dock = DockStyle.Top,
            Height = 62,
            Text = "تغيير كلمة المرور",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(24, 74, 119),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(22, 0, 22, 0)
        };

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 22, 28, 12),
            ColumnCount = 2,
            RowCount = 5
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        AddField(fields, 0, "كلمة المرور الحالية", _currentPassword);
        AddField(fields, 1, "كلمة المرور الجديدة", _newPassword);
        AddField(fields, 2, "تأكيد كلمة المرور", _confirmPassword);
        fields.Controls.Add(_policyHint, 0, 3);
        fields.SetColumnSpan(_policyHint, 2);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 6, 0, 0)
        };
        actions.Controls.Add(_saveButton);
        actions.Controls.Add(_closeButton);
        fields.Controls.Add(actions, 0, 4);
        fields.SetColumnSpan(actions, 2);

        Controls.Add(fields);
        Controls.Add(header);
        AcceptButton = _saveButton;
        CancelButton = _closeButton;

        _saveButton.Click += async (_, _) => await ChangePasswordAsync();
        _closeButton.Click += (_, _) => Close();
        Shown += (_, _) => _currentPassword.Focus();
        ArabicErpFormStyle.Apply(this, "تغيير آمن لكلمة مرور المستخدم الحالي");
    }

    private static TextBox PasswordBox() => new()
    {
        Dock = DockStyle.Fill,
        UseSystemPasswordChar = true,
        Margin = new Padding(3, 5, 3, 5)
    };

    private static void AddField(TableLayoutPanel target, int row, string caption, Control input)
    {
        target.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        }, 0, row);
        target.Controls.Add(input, 1, row);
    }

    private async Task ChangePasswordAsync()
    {
        if (_saving)
            return;

        if (string.IsNullOrWhiteSpace(_currentPassword.Text) ||
            string.IsNullOrWhiteSpace(_newPassword.Text) ||
            string.IsNullOrWhiteSpace(_confirmPassword.Text))
        {
            ShowValidation("أدخل كلمة المرور الحالية والجديدة والتأكيد.");
            return;
        }
        if (!string.Equals(_newPassword.Text, _confirmPassword.Text, StringComparison.Ordinal))
        {
            ShowValidation("كلمة المرور الجديدة وتأكيدها غير متطابقين.");
            _confirmPassword.Focus();
            return;
        }

        _saving = true;
        _saveButton.Enabled = false;
        try
        {
            var request = new ChangePasswordRequest
            {
                Current_Password = _currentPassword.Text,
                New_Password = _newPassword.Text,
                Confirm_Password = _confirmPassword.Text
            };
            using var response = await ApiService.Client.PostAsJsonAsync("Auth/ChangePassword", request);
            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorAsync(response);
                ShowValidation(message);
                return;
            }

            ClearSensitiveFields();
            MessageBox.Show("تم تغيير كلمة المرور بنجاح. تم إنهاء الجلسات الأخرى للمستخدم، وتبقى جلستك الحالية فعالة.",
                "تم", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (HttpRequestException)
        {
            ShowValidation("تعذر الاتصال بالخادم. لم يتم تغيير كلمة المرور.");
        }
        finally
        {
            _saving = false;
            if (!IsDisposed) _saveButton.Enabled = true;
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ApiError>();
            if (!string.IsNullOrWhiteSpace(payload?.Message)) return payload.Message;
        }
        catch (NotSupportedException) { }
        catch (System.Text.Json.JsonException) { }

        return response.StatusCode == HttpStatusCode.Forbidden
            ? "ليس لديك صلاحية تغيير كلمة المرور."
            : "تعذر تغيير كلمة المرور. تحقق من البيانات وحاول مجدداً.";
    }

    private void ShowValidation(string message) =>
        MessageBox.Show(message, "تغيير كلمة المرور", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void ClearSensitiveFields()
    {
        _currentPassword.Clear();
        _newPassword.Clear();
        _confirmPassword.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) ClearSensitiveFields();
        base.Dispose(disposing);
    }

    private sealed class ChangePasswordRequest
    {
        public string Current_Password { get; set; } = string.Empty;
        public string New_Password { get; set; } = string.Empty;
        public string Confirm_Password { get; set; } = string.Empty;
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
