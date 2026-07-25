using AlTayerERP.Desktop.Common;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>التخطيط الموحد المعتمد لشاشة الشركات دون تغيير منطق الحفظ والـAPI.</summary>
public partial class CompanyForm
{
    private bool _approvedLayoutApplied;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_approvedLayoutApplied) return;
        _approvedLayoutApplied = true;
        ApplyApprovedCompanyLayout();
        BeginInvoke(new Action(ApplyCompanyResponsiveSizing));
        Resize += (_, _) => ApplyCompanyResponsiveSizing();
    }

    private void ApplyApprovedCompanyLayout()
    {
        SuspendLayout();
        Controls.Clear();
        Text = "الشركات";
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        MinimumSize = Size.Empty;

        btnSaveCompany.Text = "حفظ";
        btnDelete.Text = "إيقاف";
        btnApprove.Text = "إعادة تفعيل";
        btnUnApprove.Visible = false;
        btnImport.Visible = false;
        btnExport.Visible = false;
        btnPreview.Visible = false;

        foreach (var button in new[] { btnNew, btnSaveCompany, btnEdit, btnDelete, btnApprove, btnRefresh, btnSearch, btnPrint, btnClose })
        {
            button.AutoSize = false;
            button.Width = 104;
            button.Height = 36;
            button.Margin = new Padding(3);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(205, 217, 232);
        }
        btnSaveCompany.BackColor = Color.FromArgb(14, 93, 216);
        btnSaveCompany.ForeColor = Color.White;

        chkIsActive.Enabled = false;
        chkIsActive.Text = "نشطة";
        cmbGroups.DropDownStyle = ComboBoxStyle.DropDownList;

        var shell = new TableLayoutPanel
        {
            Name = "companyShell",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        shell.Controls.Add(new BrandHeaderControl("الشركات"), 0, 0);

        var toolbar = new FlowLayoutPanel
        {
            Name = "companyToolbar",
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.White,
            Padding = new Padding(3, 4, 3, 3)
        };
        toolbar.Controls.AddRange(new Control[] { btnNew, btnSaveCompany, btnEdit, btnDelete, btnApprove, btnRefresh, btnSearch, btnPrint, btnClose });
        shell.Controls.Add(toolbar, 0, 1);

        var editor = new TableLayoutPanel { Name = "companyEditor", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 6, 0, 6) };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        editor.Controls.Add(BuildCompanyIdentityCard(), 0, 0);
        editor.Controls.Add(BuildCompanyContactCard(), 1, 0);
        shell.Controls.Add(editor, 0, 2);

        var search = new TextBox { Name = "companySearch", Dock = DockStyle.Fill, PlaceholderText = "ابحث بكود الشركة أو الاسم أو الهاتف أو الرقم الضريبي…", Margin = new Padding(0, 4, 0, 4) };
        search.TextChanged += (_, _) =>
        {
            var q = search.Text.Trim();
            foreach (DataGridViewRow row in dgvCompanies.Rows)
                row.Visible = string.IsNullOrWhiteSpace(q) || row.Cells.Cast<DataGridViewCell>().Any(c => (c.Value?.ToString() ?? string.Empty).Contains(q, StringComparison.CurrentCultureIgnoreCase));
        };
        shell.Controls.Add(search, 0, 3);

        dgvCompanies.Dock = DockStyle.Fill;
        dgvCompanies.ReadOnly = true;
        dgvCompanies.AllowUserToAddRows = false;
        dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCompanies.MultiSelect = false;
        dgvCompanies.RowHeadersVisible = false;
        dgvCompanies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        shell.Controls.Add(Card("قائمة الشركات", dgvCompanies), 0, 4);

        shell.Controls.Add(new Label
        {
            Text = "ترتبط كل شركة بمجموعة تجارية واحدة، وتدار الحالة عبر الإيقاف وإعادة التفعيل مع التدقيق.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(55, 85, 130),
            AutoEllipsis = true
        }, 0, 5);

        Controls.Add(shell);
        ResumeLayout(true);
    }

    private void ApplyCompanyResponsiveSizing()
    {
        if (Controls.Find("companyShell", true).FirstOrDefault() is not TableLayoutPanel shell) return;
        var compact = ClientSize.Width < 1120 || ClientSize.Height < 720;
        shell.Padding = compact ? new Padding(6) : new Padding(10);
        shell.RowStyles[0].Height = compact ? 62 : 68;
        shell.RowStyles[1].Height = compact ? 42 : 46;
        shell.RowStyles[2].Height = compact ? 224 : 250;
        shell.RowStyles[3].Height = compact ? 36 : 40;

        if (Controls.Find("companyEditor", true).FirstOrDefault() is TableLayoutPanel editor)
        {
            editor.ColumnStyles[0].Width = compact ? 52 : 54;
            editor.ColumnStyles[1].Width = compact ? 48 : 46;
        }
    }

    private Control BuildCompanyIdentityCard()
    {
        var table = FormTable(4);
        AddField(table, 0, "المجموعة التجارية *", cmbGroups, "كود الشركة *", txtCompanyPrefix);
        AddField(table, 1, "اسم الشركة بالعربية *", txtCompanyNameAr, "اسم الشركة بالإنجليزية", txtCompanyNameEn);
        AddField(table, 2, "الرقم الضريبي", txtTaxNumber, "الحالة", chkIsActive);
        return Card("الهوية القانونية", table);
    }

    private Control BuildCompanyContactCard()
    {
        picCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
        picCompanyLogo.Dock = DockStyle.Fill;
        var logoButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 34, FlowDirection = FlowDirection.RightToLeft };
        btnBrowseLogo.Text = "اختيار الشعار";
        btnRemoveLogo.Text = "إزالة الشعار";
        logoButtons.Controls.AddRange(new Control[] { btnBrowseLogo, btnRemoveLogo });

        var table = FormTable(4);
        AddSingle(table, 0, "الهاتف", txtPhone);
        AddSingle(table, 1, "البريد الإلكتروني", txtEmail);
        AddSingle(table, 2, "العنوان", txtAddress);
        var logo = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        logo.Controls.Add(picCompanyLogo);
        logo.Controls.Add(logoButtons);
        table.Controls.Add(logo, 0, 3);
        table.SetColumnSpan(logo, 4);
        return Card("الاتصال والشعار", table);
    }

    private static TableLayoutPanel FormTable(int rows)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = rows, Padding = new Padding(8) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < rows; i++) t.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
        return t;
    }

    private static void AddField(TableLayoutPanel t, int row, string c1, Control x1, string c2, Control x2)
    {
        t.Controls.Add(Caption(c1), 0, row); t.Controls.Add(Input(x1), 1, row);
        t.Controls.Add(Caption(c2), 2, row); t.Controls.Add(Input(x2), 3, row);
    }

    private static void AddSingle(TableLayoutPanel t, int row, string caption, Control input)
    {
        t.Controls.Add(Caption(caption), 0, row); t.Controls.Add(Input(input), 1, row); t.SetColumnSpan(input, 3);
    }

    private static Control Input(Control c) { c.Dock = DockStyle.Fill; c.Margin = new Padding(3, 5, 3, 5); return c; }
    private static Label Caption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, AutoEllipsis = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 58, 92) };
    private static Panel Card(string title, Control body)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(6) };
        body.Dock = DockStyle.Fill;
        p.Controls.Add(body);
        p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) });
        return p;
    }
}
