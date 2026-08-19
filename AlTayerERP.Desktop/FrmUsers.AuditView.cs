using System;
using System.Drawing;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

public partial class FrmUsers
{
    private readonly Label _lblCreatedByValue = CreateAuditValueLabel();
    private readonly Label _lblCreatedAtValue = CreateAuditValueLabel();
    private readonly Label _lblUpdatedByValue = CreateAuditValueLabel();
    private readonly Label _lblUpdatedAtValue = CreateAuditValueLabel();
    private bool _auditViewInitialized;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_auditViewInitialized) return;
        _auditViewInitialized = true;

        BuildUserAuditPanel();
        dgvUsers.SelectionChanged += async (_, _) => await RefreshSelectedUserAuditAsync();
        btnNew.Click += (_, _) => ClearUserAuditValues();
    }

    private void BuildUserAuditPanel()
    {
        _grpUserAudit.Controls.Clear();
        _grpUserAudit.Text = "بيانات الإنشاء والتعديل";
        _grpUserAudit.Padding = new Padding(10, 5, 10, 7);
        _grpUserAudit.BackColor = Color.White;

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RightToLeft = RightToLeft.Yes,
            ColumnCount = 8,
            RowCount = 1,
            Padding = new Padding(2)
        };

        for (var i = 0; i < 4; i++)
        {
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        }

        AddAuditField(panel, 0, "أنشئ بواسطة", _lblCreatedByValue);
        AddAuditField(panel, 2, "تاريخ الإنشاء", _lblCreatedAtValue);
        AddAuditField(panel, 4, "عُدّل بواسطة", _lblUpdatedByValue);
        AddAuditField(panel, 6, "تاريخ التعديل", _lblUpdatedAtValue);

        _grpUserAudit.Controls.Add(panel);
        ClearUserAuditValues();
    }

    private static void AddAuditField(TableLayoutPanel panel, int column, string caption, Label valueLabel)
    {
        var captionLabel = new Label
        {
            Text = caption + ":",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Font = new Font("Tahoma", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(8, 49, 92),
            Margin = new Padding(8, 5, 4, 3)
        };

        valueLabel.Dock = DockStyle.Fill;
        panel.Controls.Add(captionLabel, column, 0);
        panel.Controls.Add(valueLabel, column + 1, 0);
    }

    private static Label CreateAuditValueLabel() => new()
    {
        Text = "—",
        AutoEllipsis = true,
        TextAlign = ContentAlignment.MiddleRight,
        Font = new Font("Tahoma", 8.5F),
        ForeColor = Color.FromArgb(51, 65, 85),
        Margin = new Padding(4, 3, 10, 3)
    };

    private async Task RefreshSelectedUserAuditAsync()
    {
        if (_selectedUserId <= 0)
        {
            ClearUserAuditValues();
            return;
        }

        try
        {
            var audit = await _client.GetFromJsonAsync<UserAuditSummary>($"{_baseUrl}Users/{_selectedUserId}/audit");
            if (audit == null)
            {
                ClearUserAuditValues();
                return;
            }

            _lblCreatedByValue.Text = string.IsNullOrWhiteSpace(audit.Created_By) ? "—" : audit.Created_By;
            _lblCreatedAtValue.Text = FormatDate(audit.Created_At);
            _lblUpdatedByValue.Text = string.IsNullOrWhiteSpace(audit.Updated_By) ? "—" : audit.Updated_By;
            _lblUpdatedAtValue.Text = FormatDate(audit.Updated_At);
        }
        catch
        {
            _lblCreatedByValue.Text = "غير متاح";
            _lblCreatedAtValue.Text = "غير متاح";
            _lblUpdatedByValue.Text = "غير متاح";
            _lblUpdatedAtValue.Text = "غير متاح";
        }
    }

    private void ClearUserAuditValues()
    {
        _lblCreatedByValue.Text = "—";
        _lblCreatedAtValue.Text = "—";
        _lblUpdatedByValue.Text = "—";
        _lblUpdatedAtValue.Text = "—";
    }

    private static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "—";

    private sealed class UserAuditSummary
    {
        public string Created_By { get; set; } = string.Empty;
        public DateTime? Created_At { get; set; }
        public string Updated_By { get; set; } = string.Empty;
        public DateTime? Updated_At { get; set; }
    }
}
