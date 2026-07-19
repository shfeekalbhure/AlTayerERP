// استدعاء مكتبات النظام الأساسية للتعامل مع المدخلات والمخرجات والواجهات
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
// استدعاء مكتبة الشبكة والاتصالات للتعرف على HttpClient
using System.Net.Http;
using System.Net.Http.Json; // مكتبة مضافة لقراءة الـ Json مباشرة
using System.Threading.Tasks;
// استدعاء المجلد المركزي لخدمات الـ API
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    // تعريف كلاس الشاشة الرئيسية (FrmMain)
    public partial class FrmMain : Form
    {
        // إنشاء كائن ثابت لـ HttpClient للتعامل مع اتصالات الشبكة والـ API من الملف المركزي
        private readonly HttpClient _client = ApiService.Client;

        // الرابط الأساسي الثابت الذي يشير إلى موقع الـ API من الملف المركزي
        private readonly string _baseUrl = ApiService.BaseUrl;

        // ==========================================
        // 1. دالة البناء (Constructor)
        // ==========================================
        public FrmMain()
        {
            InitializeComponent();

            // وضع حالة الانتظار وجاري التحميل للقيم عند بداية إقلاع الشاشة
            lblCompanyName.Text = "جاري التحميل...";
            lblCurrentBranch.Text = "الفرع\n" + CurrentSession.Branch_ID;
            lblFiscalYear.Text = "جاري التحميل...";
            lblCurrentUser.Text = "المستخدم\n" + CurrentSession.Username;

            // استدعاء الدالة غير المتزامنة لجلب الأسماء الحقيقية من الـ API
            _ = LoadSessionDetails();

            // بناء القائمة الشجرية للأقسام
            BuildMainMenu();

            // فحص الصلاحيات لإخفاء القوائم غير المصرح بها
            if (!CurrentSession.Is_System_Admin)
            {
                // هنا سنخفي القوائم حسب الصلاحيات في الخطوة القادمة.
            }

            // فك الارتباط القديم للحدث ثم إعادة ربطه لمنع فتح الشاشة مرتين عند الضغط المزدوج
            tvMainMenu.NodeMouseDoubleClick -= tvMainMenu_NodeMouseDoubleClick;
            tvMainMenu.NodeMouseDoubleClick += tvMainMenu_NodeMouseDoubleClick;
        }

        // ==========================================
        // دالة جلب تفاصيل الجلسة والأسماء الحقيقية من الـ API
        // ==========================================
        private async Task LoadSessionDetails()
        {
            try
            {
                // تم تمرير معرّف السنة المالية أيضاً للسيرفر ليقوم بجلب الاسم المقابل لها
                string url = $"{_baseUrl}Branches/GetSessionInfo?companyId={CurrentSession.Company_ID}&branchId={CurrentSession.Branch_ID}&yearId={CurrentSession.Year_ID}";

                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // قراءة النتيجة المسترجعة ديناميكياً شاملة الاسم الجديد للسنوات المالية
                    var data = await response.Content.ReadFromJsonAsync<SessionInfoResponse>();
                    if (data != null)
                    {
                        // تحديث الليبلات بالأسماء الحقيقية المسترجعة من قاعدة البيانات وعرض اسم السنة بدلاً من الرقم
                        lblCompanyName.Text = "شركة\n" + data.Company_Name_AR;
                        lblCurrentBranch.Text = "الفرع\n" + data.Branch_Name;
                        lblFiscalYear.Text = "السنة المالية\n" + data.Year_Name;
                    }
                }
            }
            catch (Exception ex)
            {
                // في حال حدوث أي خطأ في الاتصال، تظهر المعرفات الاحتياطية للأمان بدلاً من جملة الانتظار
                lblCompanyName.Text = "شركة\n" + CurrentSession.Company_ID;
                lblFiscalYear.Text = "السنة المالية\n" + CurrentSession.Year_ID;
                MessageBox.Show($"خطأ أثناء جلب بيانات الشركة والفرع: {ex.Message}", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // كلاس داخلي مساعد معدل لاستقبال حقل Year_Name من السيرفر
        private class SessionInfoResponse
        {
            public string Company_ID { get; set; } = "";
            public string Company_Name_AR { get; set; } = "";
            public int Branch_ID { get; set; }
            public string Branch_Name { get; set; } = "";
            public string Year_Name { get; set; } = "";
        }

        // ==========================================
        // 2. دالة بناء القائمة الرئيسية الشجرية (TreeView)
        // ==========================================
        private void BuildMainMenu()
        {
            // مسح أي عناصر سابقة داخل القائمة الشجرية لتجنب التكرار
            tvMainMenu.Nodes.Clear();

            // إنشاء العقدة الرئيسية الأولى "الإدارة العامة" وتعبئة فروعها
            TreeNode adminNode = new TreeNode("الإدارة العامة");
            adminNode.Nodes.Add("Companies", "الشركات");
            adminNode.Nodes.Add("Branches", "الفروع");
            adminNode.Nodes.Add("FiscalYears", "السنوات المالية");
            adminNode.Nodes.Add("Users", "المستخدمون والصلاحيات");
            adminNode.Nodes.Add("Roles", "الأدوار");
            adminNode.Nodes.Add("NumberingSettings", "إعدادات الترقيم");

            // إنشاء العقدة الرئيسية الثانية "الحسابات" وتعبئة فروعها
            TreeNode accountingNode = new TreeNode("الحسابات");
            accountingNode.Nodes.Add("ChartOfAccounts", "الدليل المحاسبي");
            accountingNode.Nodes.Add("Currencies", "العملات");
            accountingNode.Nodes.Add("CostCenters", "مراكز التكلفة");
            accountingNode.Nodes.Add("CashBoxes", "الصناديق");
            accountingNode.Nodes.Add("ReceiptVoucher", "سند القبض");
            accountingNode.Nodes.Add("PaymentVoucher", "سند الصرف");
       //     accountingNode.Nodes.Add("PaymentVoucher", "سند الصرف");


            // إضافة العقد الرئيسية بالترتيب الهرمي إلى أداة الـ TreeView
            tvMainMenu.Nodes.Add(adminNode);
            tvMainMenu.Nodes.Add(accountingNode);


            // جعل القائمة ممتدة تلقائياً لتظهر الفروع مباشرة للمستخدم
            tvMainMenu.ExpandAll();
        }

        // ==========================================
        // 3. حدث فتح الشاشات عند النقر المزدوج على الشجرة
        // ==========================================
        private void tvMainMenu_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // استخدام جملة switch لفحص الاسم البرمجي (e.Node.Name) للعقدة المحددة
            switch (e.Node.Name)
            {
                case "Companies":
                    new CompanyForm().ShowDialog();
                    break;

                case "Branches":
                    new BranchForm().ShowDialog();
                    break;

                case "FiscalYears":
                    new FiscalYearForm().ShowDialog();
                    break;

                case "Users":
                    new FrmUsers().ShowDialog();
                    break;

                case "Roles":
                    new FrmRoles().ShowDialog();
                    break;

                case "NumberingSettings":
                    new FrmNumberingSettings().ShowDialog();
                    break;

                case "ChartOfAccounts":
                    new FrmChartOfAccounts().ShowDialog();
                    break;
                    //العملة
                 case "Currencies":
                    new FrmCurrencies().ShowDialog();
                    break;
                    //مركز التكلفة
                case "CostCenters":
                    new FrmCostCenters().ShowDialog();
                    break;

                case "CashBoxes":
                    new FrmCashBoxes().ShowDialog();
                    break;

                case "ReceiptVoucher":
                    new FrmReceiptVoucher().ShowDialog();
                    break;

                case "PaymentVoucher":
                    new FrmPaymentVoucher().ShowDialog();
                    break;
      //          case "PaymentVoucher":
       //             new FrmPaymentVoucher().ShowDialog();
      //              break;


            }
        }

        // ==========================================
        // 4. أحداث الأزرار وعناصر الواجهة الأخرى 
        // ==========================================
        private void btnUsers_Click(object sender, EventArgs e)
        {
            FrmUsers frm = new FrmUsers();
            frm.ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e) { }
        private void btnAccountingCenter_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void lblStatusTime_Click(object sender, EventArgs e) { }
        private void tvMainMenu_AfterSelect(object sender, TreeViewEventArgs e) { }
        private void flpMenu_Paint(object sender, PaintEventArgs e) { }
        private void pnlWorkspace_Paint(object sender, PaintEventArgs e) { }

        private void tvMainMenu_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }
    }
}