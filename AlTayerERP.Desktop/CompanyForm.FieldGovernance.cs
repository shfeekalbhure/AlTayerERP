namespace AlTayerERP.Desktop;

/// <summary>
/// حوكمة حقول وأوامر شاشة الشركات دون تغيير بنية قاعدة البيانات.
/// </summary>
public partial class CompanyForm
{
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Company_ID هو كود الشركة المولد مركزياً ويعرض في الجدول للقراءة فقط.
        if (dgvCompanies.Columns.Contains("Company_ID"))
            dgvCompanies.Columns["Company_ID"].HeaderText = "كود الشركة";

        // Company_Prefix بادئة ترقيم وليست كوداً مستقلاً للشركة.
        label4.Text = "بادئة الترقيم";
        if (dgvCompanies.Columns.Contains("Company_Prefix"))
            dgvCompanies.Columns["Company_Prefix"].HeaderText = "بادئة الترقيم";

        // الحالة للعرض فقط؛ تغييرها يتم حصراً عبر الإيقاف وإعادة التفعيل المدققين.
        chkIsActive.Text = "الحالة: نشطة";
        chkIsActive.Enabled = false;
        chkIsActive.TabStop = false;

        // إخفاء أوامر غير منفذة فعلياً حتى لا توهم المستخدم بوجود وظيفة مكتملة.
        btnPreview.Visible = false;
        btnImport.Visible = false;
        btnPrint.Visible = false;
        btnUnApprove.Visible = false;

        // تحديث حالة أزرار الإجراءات بعد اختيار أي صف.
        dgvCompanies.CellClick += (_, _) => UpdateCompanyActionButtons();
        UpdateCompanyActionButtons();
    }

    private void UpdateCompanyActionButtons()
    {
        if (dgvCompanies.CurrentRow is null || dgvCompanies.CurrentRow.Cells.Count < 8)
        {
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
            btnApprove.Enabled = false;
            return;
        }

        var statusText = dgvCompanies.CurrentRow.Cells[7].Value?.ToString();
        var isActive = string.Equals(statusText, "نشط", StringComparison.OrdinalIgnoreCase);

        chkIsActive.Checked = isActive;
        chkIsActive.Text = isActive ? "الحالة: نشطة" : "الحالة: موقوفة";
        btnEdit.Enabled = isActive;
        btnDelete.Enabled = isActive;
        btnApprove.Enabled = !isActive;
    }
}
