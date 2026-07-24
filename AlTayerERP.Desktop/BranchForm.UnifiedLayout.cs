using System.Drawing;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace AlTayerERP.Desktop;

/// <summary>التخطيط الموحد لشاشة الفروع وربط العملة ونوع الفرع بالقوائم المرجعية.</summary>
public partial class BranchForm
{
    private readonly ComboBox cmbUnifiedCurrency = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtUnifiedSearch = new() { PlaceholderText = "ابحث بكود الفرع أو الاسم أو الهاتف أو المدينة…" };
    private bool _unifiedBranchLayoutApplied;

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_unifiedBranchLayoutApplied) return;
        _unifiedBranchLayoutApplied = true;
        ApplyUnifiedBranchLayout();

        btnSaveBranch.Click -= btnSaveBranch_Click;
        btnEdit.Click -= btnEdit_Click;
        btnSaveBranch.Click += async (_, _) => await SaveBranchUnifiedAsync(false);
        btnEdit.Click += async (_, _) => await SaveBranchUnifiedAsync(true);
        cmbCompanies.SelectedValueChanged += async (_, _) => await LoadUnifiedReferenceDataAsync();
        dgvBranches.SelectionChanged += (_, _) => BindUnifiedCurrency();
        await LoadUnifiedReferenceDataAsync();
    }

    private void ApplyUnifiedBranchLayout()
    {
        SuspendLayout();
        Controls.Clear();
        Text = "الفروع";
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        MinimumSize = new Size(1180, 740);
        Width = 1440;
        Height = 880;

        btnSaveBranch.Text = "حفظ";
        btnDelete.Text = "إيقاف";
        btnApprove.Text = "إعادة تفعيل";
        btnUnApprove.Visible = false;
        btnImport.Visible = false;
        btnExport.Visible = false;
        btnPreview.Visible = false;
        pnlTopBar.Visible = false;
        cmbStatus.Enabled = false;
        cmbCompanies.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBranchType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbParentBranch.DropDownStyle = ComboBoxStyle.DropDownList;

        foreach (var button in new[] { btnNew, btnSaveBranch, btnEdit, btnDelete, btnApprove, btnRefresh, btnSearch, btnPrint, btnClose })
        {
            button.Width = 112; button.Height = 34; button.Margin = new Padding(4);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(205, 217, 232);
        }
        btnSaveBranch.BackColor = Color.FromArgb(14, 93, 216);
        btnSaveBranch.ForeColor = Color.White;

        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16) };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 340));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        shell.Controls.Add(UnifiedTitle("الفروع"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.White, Padding = new Padding(4, 7, 4, 5) };
        toolbar.Controls.AddRange(new Control[] { btnNew, btnSaveBranch, btnEdit, btnDelete, btnApprove, btnRefresh, btnSearch, btnPrint, btnClose });
        shell.Controls.Add(toolbar, 0, 1);

        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 8, 0, 8) };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        editor.Controls.Add(BuildUnifiedBranchIdentity(), 0, 0);
        editor.Controls.Add(BuildUnifiedBranchContact(), 1, 0);
        shell.Controls.Add(editor, 0, 2);

        txtUnifiedSearch.Dock = DockStyle.Fill;
        txtUnifiedSearch.Margin = new Padding(0, 5, 0, 5);
        txtUnifiedSearch.TextChanged += (_, _) => UnifiedFilterBranches();
        shell.Controls.Add(txtUnifiedSearch, 0, 3);

        dgvBranches.Dock = DockStyle.Fill;
        dgvBranches.ReadOnly = true;
        dgvBranches.AllowUserToAddRows = false;
        dgvBranches.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvBranches.MultiSelect = false;
        dgvBranches.RowHeadersVisible = false;
        dgvBranches.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        shell.Controls.Add(UnifiedCard("قائمة الفروع", dgvBranches), 0, 4);
        shell.Controls.Add(new Label { Text = "يرتبط الفرع بشركة ونوع فرع وعملة نشطة، ولا تتغير حالته إلا بإجراء مدقق.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(55, 85, 130) }, 0, 5);
        Controls.Add(shell);
        ResumeLayout(true);
    }

    private Control BuildUnifiedBranchIdentity()
    {
        var table = UnifiedFormTable(6);
        UnifiedAddRow(table, 0, "الشركة التابعة *", cmbCompanies, "كود الفرع", txtBranchCode);
        UnifiedAddRow(table, 1, "اسم الفرع بالعربية *", txtBranchNameAr, "اسم الفرع بالإنجليزية", txtBranchNameEn);
        UnifiedAddRow(table, 2, "نوع الفرع *", cmbBranchType, "الفرع الأب", cmbParentBranch);
        UnifiedAddRow(table, 3, "العملة الافتراضية *", cmbUnifiedCurrency, "الحالة", cmbStatus);
        UnifiedAddRow(table, 4, "العنوان التفصيلي", txtLocation, "المسؤول", cmbManager);
        var options = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        options.Controls.AddRange(new Control[] { chkAllowCredit, chkAllowPercentage });
        table.Controls.Add(UnifiedCaption("خيارات التشغيل"), 0, 5); table.Controls.Add(options, 1, 5); table.SetColumnSpan(options, 3);
        return UnifiedCard("بيانات الفرع", table);
    }

    private Control BuildUnifiedBranchContact()
    {
        var table = UnifiedFormTable(6);
        UnifiedAddSingle(table, 0, "الهاتف", txtPhone);
        UnifiedAddSingle(table, 1, "الجوال", txtMobile);
        UnifiedAddSingle(table, 2, "البريد الإلكتروني", txtEmail);
        UnifiedAddSingle(table, 3, "الموقع الإلكتروني", txtWebsite);
        txtNotes.Multiline = true;
        UnifiedAddSingle(table, 4, "ملاحظات", txtNotes);
        return UnifiedCard("الاتصال والملاحظات", table);
    }

    private async Task LoadUnifiedReferenceDataAsync()
    {
        var companyId = cmbCompanies.SelectedValue?.ToString();
        if (string.IsNullOrWhiteSpace(companyId)) return;
        try
        {
            var result = await _client.GetFromJsonAsync<BranchReferenceLookups>($"{_baseUrl}branch-reference-lookups?companyId={Uri.EscapeDataString(companyId)}");
            var selectedType = cmbBranchType.Text;
            cmbBranchType.DataSource = result?.BranchTypes ?? new();
            cmbBranchType.DisplayMember = nameof(BranchTypeLookup.Branch_Type_Name_AR);
            cmbBranchType.ValueMember = nameof(BranchTypeLookup.Branch_Type_ID);
            if (!string.IsNullOrWhiteSpace(selectedType)) cmbBranchType.Text = selectedType;

            cmbUnifiedCurrency.DataSource = result?.Currencies ?? new();
            cmbUnifiedCurrency.DisplayMember = nameof(CurrencyLookup.Display_Name);
            cmbUnifiedCurrency.ValueMember = nameof(CurrencyLookup.Currency_ID);
            var current = _branchesList.FirstOrDefault(x => x.Branch_ID == _selectedBranchId);
            if (current is not null && current.Currency_ID > 0) cmbUnifiedCurrency.SelectedValue = current.Currency_ID;
            else
            {
                var preferred = result?.Currencies.FirstOrDefault(x => x.Is_Default) ?? result?.Currencies.FirstOrDefault(x => x.Is_Local_Currency);
                if (preferred is not null) cmbUnifiedCurrency.SelectedValue = preferred.Currency_ID;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("تعذر تحميل أنواع الفروع والعملات.\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task SaveBranchUnifiedAsync(bool edit)
    {
        if (cmbCompanies.SelectedValue is null || string.IsNullOrWhiteSpace(txtBranchNameAr.Text))
        {
            MessageBox.Show("الشركة واسم الفرع بالعربية حقول مطلوبة."); return;
        }
        if (cmbBranchType.SelectedItem is not BranchTypeLookup type)
        {
            MessageBox.Show("اختر نوع الفرع من القائمة المرجعية."); return;
        }
        if (!int.TryParse(cmbUnifiedCurrency.SelectedValue?.ToString(), out var currencyId) || currencyId <= 0)
        {
            MessageBox.Show("اختر العملة الافتراضية للفرع."); return;
        }
        if (edit && _selectedBranchId <= 0) { MessageBox.Show("اختر فرعاً من الجدول أولاً."); return; }

        var dto = new
        {
            Company_ID = cmbCompanies.SelectedValue.ToString(),
            Branch_Code = txtBranchCode.Text.Trim(),
            Branch_Name = txtBranchNameAr.Text.Trim(),
            Branch_Name_EN = txtBranchNameEn.Text.Trim(),
            Address = txtLocation.Text.Trim(),
            Branch_Type = type.Branch_Type_Name_AR,
            Parent_Branch_ID = cmbParentBranch.SelectedValue is null ? (int?)null : Convert.ToInt32(cmbParentBranch.SelectedValue),
            Manager_Name = cmbManager.Text.Trim(), Phone = txtPhone.Text.Trim(), Mobile = txtMobile.Text.Trim(),
            Website = txtWebsite.Text.Trim(), Email = txtEmail.Text.Trim(), Notes = txtNotes.Text.Trim(),
            Allow_Credit = chkAllowCredit.Checked, Allow_Percentage = chkAllowPercentage.Checked,
            Is_Active = true, Currency_ID = currencyId
        };
        var response = edit
            ? await _client.PutAsJsonAsync($"{_baseUrl}Branches/{_selectedBranchId}", dto)
            : await _client.PostAsJsonAsync($"{_baseUrl}Branches", dto);
        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync(), edit ? "تعذر التعديل" : "تعذر الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        await LoadBranchesAsync(cmbCompanies.SelectedValue?.ToString());
        MessageBox.Show(edit ? "تم تعديل بيانات الفرع بنجاح." : "تم حفظ الفرع بنجاح.");
    }

    private void BindUnifiedCurrency()
    {
        var row = _branchesList.FirstOrDefault(x => x.Branch_ID == _selectedBranchId);
        if (row is not null && row.Currency_ID > 0) cmbUnifiedCurrency.SelectedValue = row.Currency_ID;
    }

    private void UnifiedFilterBranches()
    {
        var q = txtUnifiedSearch.Text.Trim();
        foreach (DataGridViewRow row in dgvBranches.Rows)
            row.Visible = string.IsNullOrWhiteSpace(q) || row.Cells.Cast<DataGridViewCell>().Any(c => (c.Value?.ToString() ?? string.Empty).Contains(q, StringComparison.CurrentCultureIgnoreCase));
    }

    private static Panel UnifiedTitle(string title) => new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(8, 55, 112), Controls = { new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 14F, FontStyle.Bold) } } };
    private static Panel UnifiedCard(string title, Control body) { var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(8) }; body.Dock = DockStyle.Fill; p.Controls.Add(body); p.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112) }); return p; }
    private static TableLayoutPanel UnifiedFormTable(int rows) { var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = rows, Padding = new Padding(12) }; t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); for (var i = 0; i < rows; i++) t.RowStyles.Add(new RowStyle(SizeType.Absolute, 48)); return t; }
    private static void UnifiedAddRow(TableLayoutPanel t, int row, string c1, Control x1, string c2, Control x2) { t.Controls.Add(UnifiedCaption(c1), 0, row); t.Controls.Add(UnifiedInput(x1), 1, row); t.Controls.Add(UnifiedCaption(c2), 2, row); t.Controls.Add(UnifiedInput(x2), 3, row); }
    private static void UnifiedAddSingle(TableLayoutPanel t, int row, string c, Control x) { t.Controls.Add(UnifiedCaption(c), 0, row); t.Controls.Add(UnifiedInput(x), 1, row); t.SetColumnSpan(x, 3); }
    private static Label UnifiedCaption(string text) => new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 58, 92) };
    private static Control UnifiedInput(Control c) { c.Dock = DockStyle.Fill; c.Margin = new Padding(4, 7, 4, 7); return c; }

    private sealed class BranchReferenceLookups { public List<BranchTypeLookup> BranchTypes { get; set; } = new(); public List<CurrencyLookup> Currencies { get; set; } = new(); }
    private sealed class BranchTypeLookup { public int Branch_Type_ID { get; set; } public string Branch_Type_Code { get; set; } = string.Empty; public string Branch_Type_Name_AR { get; set; } = string.Empty; public override string ToString() => Branch_Type_Name_AR; }
    private sealed class CurrencyLookup { public int Currency_ID { get; set; } public string Currency_Code { get; set; } = string.Empty; public string Currency_Name_AR { get; set; } = string.Empty; public bool Is_Local_Currency { get; set; } public bool Is_Default { get; set; } public string Display_Name => $"{Currency_Code} - {Currency_Name_AR}"; }
}
