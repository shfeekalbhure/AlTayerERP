using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// استدعاء المجلد المركزي لخدمات الـ API
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class FiscalYearForm : Form
    {
// إنشاء كائن ثابت لـ HttpClient للتعامل مع اتصالات الشبكة والـ API
private readonly HttpClient _client = ApiService.Client;
    // الرابط الأساسي الثابت الذي يشير إلى موقع الـ API الخاص بالفروع على السيرفر المحلي
    private readonly string _baseUrl = ApiService.BaseUrl;

    // متغير لتخزين رقم السنة المالية المحددة من الجدول (0 يعني وضع إضافة جديد)
    private int _selectedFiscalYearId = 0;

        // قائمة لتخزين البيانات مؤقتاً في ذاكرة البرنامج (كاش) لتسريع عمليات البحث المحلي
        private List<FiscalYearModel> _fiscalYearsCache = new List<FiscalYearModel>();

        // أدوات معالجة الطباعة والمعاينة برمجياً
        private PrintDocument printDocument = new PrintDocument();
        private PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();

        public FiscalYearForm()
        {
            InitializeComponent();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
// إعداد أعمدة وخصائص جدول عرض البيانات
            SetupFiscalYearsGrid();

            // ربط أحداث الشاشة والأزرار برمجياً لضمان عملها وتوافقها مع الـ Designer
            this.Load += FiscalYearForm_Load;
            dgvFiscalYears.CellClick += dgvFiscalYears_CellClick;
            btnNew.Click += btnNew_Click;
            btnDelete.Click += btnDelete_Click;
            btnRefresh.Click += btnRefresh_Click;
            btnClose.Click += btnClose_Click;
            btnPrint.Click += btnPrint_Click;

            // ربط حدث زر المعاينة قبل الطباعة بأمان
            btnEdit.Click += btnEdit_Click; // زر التعديل المسمى btnEdit في الـ Designer

            // ربط محرك الرسم الخاص بالطباعة
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        // ======================================================
        // حدث تحميل الشاشة (Form Load)
        // ======================================================
        private async void FiscalYearForm_Load(object sender, EventArgs e)
        {
            // تهيئة خيارات قائمة الحالة وتحديد الخيار الافتراضي "نشط"
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("نشط");
            cmbStatus.Items.Add("موقوف");
            cmbStatus.Text = "نشط";

            // جلب البيانات فور فتح الشاشة من الـ API
            await LoadFiscalYearsAsync();
        }

        // ======================================================
        // دالة تهيئة وإعداد جدول عرض البيانات (DataGridView)
        // ======================================================




          private void SetupFiscalYearsGrid()
        {
            // 1. تفريغ أي أعمدة أو صفوف قديمة لتجنب التداخل
            dgvFiscalYears.Columns.Clear();
            dgvFiscalYears.Rows.Clear();

            // 2. ضبط خصائص حماية الجدول ومظهره
            dgvFiscalYears.AllowUserToAddRows = false;
            dgvFiscalYears.ReadOnly = true;
            dgvFiscalYears.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiscalYears.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 3. البناء الصريح والآمن للأعمدة (الاسم البرمجي أولاً ثم نص الترويسة العربي)
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fiscal_Year_ID", HeaderText = "رقم السنة" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Year_Name", HeaderText = "اسم السنة المالية" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Start_Date", HeaderText = "تاريخ البداية" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "End_Date", HeaderText = "تاريخ النهاية" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Is_Default", HeaderText = "افتراضية" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Is_Closed", HeaderText = "مقفلة" });
            dgvFiscalYears.Columns.Add(new DataGridViewTextBoxColumn { Name = "Is_Active", HeaderText = "الحالة" });
          }

        // ======================================================
        // دالة جلب البيانات من الـ API بشكل غير متزامن (Async Task)
        // ======================================================
        private async Task LoadFiscalYearsAsync()
        {
            try
            {
                var years = await _client.GetFromJsonAsync<List<FiscalYearModel>>($"{_baseUrl}FiscalYears");

                dgvFiscalYears.Rows.Clear();
                _fiscalYearsCache.Clear();

                if (years == null || !years.Any())
                    return;

                // تخزين البيانات في الذاكرة المؤقتة (Cache)
                _fiscalYearsCache = years;

                // عرض البيانات داخل الجدول
                DisplayYearsInGrid(years);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // دالة مساعدة لتعبئة الصفوف داخل الـ DataGridView
        // ======================================================
        private void DisplayYearsInGrid(List<FiscalYearModel> list)
        {
            dgvFiscalYears.Rows.Clear();
            foreach (var year in list)
            {
                dgvFiscalYears.Rows.Add(
                    year.Fiscal_Year_ID,
                    year.Year_Name,
                    year.Start_Date.ToShortDateString(),
                    year.End_Date.ToShortDateString(),
                    year.Is_Default ? "نعم" : "لا",
                    year.Is_Closed ? "نعم" : "لا",
                    year.Is_Active ? "نشط" : "موقوف"
                );
            }
        }

        // ======================================================
        // حدث النقر على صف في الجدول لنقل البيانات إلى حقول الإدخال
        // ======================================================
        private void dgvFiscalYears_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                DataGridViewRow row = dgvFiscalYears.Rows[e.RowIndex];

                // الاحتفاظ برقم معرف السنة المالية المحددة للتعديل أو الحذف
                _selectedFiscalYearId = Convert.ToInt32(row.Cells["Fiscal_Year_ID"].Value);

                // توزيع قيم الصف المحددة على أدوات الواجهة
                txtYearName.Text = row.Cells["Year_Name"].Value.ToString();
                dtpStartDate.Value = Convert.ToDateTime(row.Cells["Start_Date"].Value);
                dtpEndDate.Value = Convert.ToDateTime(row.Cells["End_Date"].Value);
                chkIsDefault.Checked = row.Cells["Is_Default"].Value.ToString() == "نعم";
                chkIsClosed.Checked = row.Cells["Is_Closed"].Value.ToString() == "نعم";
                cmbStatus.Text = row.Cells["Is_Active"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء جلب بيانات الصف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ======================================================
        // حدث زر "جديد" لتفريغ الحقول وتهيئة الواجهة للإضافة
        // ======================================================
        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearFormFields();
            txtYearName.Focus();
        }

        // ======================================================
        // حدث زر "حفظ" لإضافة سنة مالية جديدة بالكامل للسيرفر
        // ======================================================
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (_selectedFiscalYearId > 0)
            {
                MessageBox.Show("الشاشة في وضع التعديل حالياً. اضغط على زر 'جديد' للإضافة أو زر 'تعديل' لحفظ التغييرات الحالية.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtYearName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم السنة المالية.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearName.Focus();
                return;
            }

            var request = new CreateFiscalYearRequest
            {
                Company_ID = "COMP001",
                Year_Name = txtYearName.Text.Trim(),
                Start_Date = dtpStartDate.Value,
                End_Date = dtpEndDate.Value,
                Is_Default = chkIsDefault.Checked,
                Is_Closed = chkIsClosed.Checked,
                Is_Active = cmbStatus.Text == "نشط"
            };

            try
            {
                var response = await _client.PostAsJsonAsync($"{_baseUrl}FiscalYears", request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حفظ السنة المالية بنجاح.", "نجاح العملية", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFormFields();
                    await LoadFiscalYearsAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحفظ: {errorContent}", "خطأ من الخادم", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الاتصال بالخادم: {ex.Message}", "خطأ غير متوقع", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // حدث زر "تعديل" لحفظ تعديلات سنة مالية محددة مسبقاً (btnEdit)
        // ======================================================
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedFiscalYearId == 0)
            {
                MessageBox.Show("يرجى اختيار سنة مالية من الجدول أولاً لتعديلها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtYearName.Text))
            {
                MessageBox.Show("اسم السنة المالية لا يمكن أن يكون فارغاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearName.Focus();
                return;
            }

            var request = new CreateFiscalYearRequest // استخدام الـ CreateDto الموحد بالسيرفر منعاً للتكرار
            {
                Company_ID = "COMP001",
                Year_Name = txtYearName.Text.Trim(),
                Start_Date = dtpStartDate.Value,
                End_Date = dtpEndDate.Value,
                Is_Default = chkIsDefault.Checked,
                Is_Closed = chkIsClosed.Checked,
                Is_Active = cmbStatus.Text == "نشط"
            };

            try
            {
                var response = await _client.PutAsJsonAsync($"{_baseUrl}FiscalYears/{_selectedFiscalYearId}", request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم تحديث بيانات السنة المالية بنجاح.", "تم التعديل", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFormFields();
                    await LoadFiscalYearsAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل التعديل: {errorContent}", "خطأ من الخادم", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تعديل البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // حدث زر "حذف" لإزالة السنة المالية المحددة
        // ======================================================
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedFiscalYearId == 0)
            {
                MessageBox.Show("يرجى اختيار سنة مالية من الجدول لحذفها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"هل أنت متأكد من رغبتك في حذف السنة المالية المحددة؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;

            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}FiscalYears/{_selectedFiscalYearId}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حذف السنة المالية بنجاح.", "تم الحذف", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFormFields();
                    await LoadFiscalYearsAsync();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحذف: {errorContent}", "خطأ من الخادم", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء عملية الحذف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ======================================================
        // حدث زر "بحث" (مربوط بـ btnSearch_Click_1 المعتمد بالـ Designer)
        // ======================================================
        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            // يعتمد البحث الذكي المحلي على حقل txtYearName نظراً لعدم وجود حقل بحث مستقل بالـ Designer
            string searchKey = txtYearName.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchKey))
            {
                MessageBox.Show("يرجى كتابة جزء من اسم السنة في حقل (السنة المالية) للبحث عنها.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisplayYearsInGrid(_fiscalYearsCache);
                return;
            }

            var filteredList = _fiscalYearsCache.Where(x => x.Year_Name.ToLower().Contains(searchKey) ||
                                                            x.Fiscal_Year_ID.ToString().Contains(searchKey)).ToList();

            DisplayYearsInGrid(filteredList);
        }

        // ======================================================
        // حدث زر "تحديث" لإعادة قراءة البيانات الطازجة من قاعدة البيانات
        // ======================================================
        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await LoadFiscalYearsAsync();
        }

        // ======================================================
        // حدث زر "إغلاق" لإنهاء الشاشة والعودة للقائمة الرئيسية
        // ======================================================
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ======================================================
        // 🖨️ حدث المعاينة قبل الطباعة الذكي (مربوط برمجياً بدالة الطباعة)
        // ======================================================
        public void OpenPrintPreview()
        {
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.WindowState = FormWindowState.Maximized;
            printPreviewDialog.ShowDialog();
        }

        // ======================================================
        // 🖨️ حدث زر الطباعة المباشرة (btnPrint)
        // ======================================================
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        // ======================================================
        // 🎨 محرك رسم صفحة التقرير للطباعة (PrintPage)
        // ======================================================
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font titleFont = new Font("Arial", 18, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font dataFont = new Font("Arial", 10, FontStyle.Regular);

            Brush blackBrush = Brushes.Black;
            Pen grayPen = new Pen(Color.LightGray, 1);

            int yPosition = 50;
            e.Graphics.DrawString("شركة الطاير السعيد للنقل والخدمات", headerFont, blackBrush, new PointF(550, yPosition));
            yPosition += 30;
            e.Graphics.DrawString("تقرير السنوات المالية المسجلة بالنظام", titleFont, blackBrush, new PointF(250, yPosition));
            yPosition += 30;
            e.Graphics.DrawString($"تاريخ استخراج التقرير: {DateTime.Now.ToShortDateString()}", dataFont, blackBrush, new PointF(50, yPosition));
            yPosition += 40;

            e.Graphics.DrawLine(new Pen(Color.Black, 2), 50, yPosition, 750, yPosition);
            yPosition += 20;

            int[] columnWidths = { 80, 150, 110, 110, 80, 80, 80 };
            string[] headers = { "رقم السنة", "اسم السنة المالية", "تاريخ البدء", "تاريخ الانتهاء", "افتراضية", "مقفلة", "الحالة" };

            int xPosition = 50;
            for (int i = 0; i < headers.Length; i++)
            {
                e.Graphics.DrawString(headers[i], headerFont, blackBrush, new PointF(xPosition, yPosition));
                xPosition += columnWidths[i];
            }

            yPosition += 25;
            e.Graphics.DrawLine(new Pen(Color.Black, 1), 50, yPosition, 750, yPosition);
            yPosition += 10;

            foreach (DataGridViewRow row in dgvFiscalYears.Rows)
            {
                if (yPosition > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = false;
                    break;
                }

                xPosition = 50;
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    string cellValue = row.Cells[i].Value?.ToString() ?? "";
                    e.Graphics.DrawString(cellValue, dataFont, blackBrush, new PointF(xPosition, yPosition));
                    xPosition += columnWidths[i];
                }

                yPosition += 25;
                e.Graphics.DrawLine(grayPen, 50, yPosition, 750, yPosition);
                yPosition += 5;
            }
        }

        // ======================================================
        // دالة مساعدة لتنظيف أدوات الإدخال وتصفير الـ ID للعودة لوضع الإضافة
        // ======================================================
        private void ClearFormFields()
        {
            _selectedFiscalYearId = 0;
            txtYearName.Clear();
            dtpStartDate.Value = DateTime.Now;
            dtpEndDate.Value = DateTime.Now.AddYears(1);
            chkIsDefault.Checked = false;
            chkIsClosed.Checked = false;
            cmbStatus.Text = "نشط";
        }

        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel3_Paint(object sender, PaintEventArgs e) { }


        private void btnPreview_Click(object sender, EventArgs e)
        {
            OpenPrintPreview();
        }
       
    }

    // ======================================================
    // كائنات الـ Models والـ Requests (DTOs) في جانب العميل
    // ======================================================
    public class CreateFiscalYearRequest
    {
        public string Company_ID { get; set; } = string.Empty;
        public string Year_Name { get; set; } = string.Empty;
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public bool Is_Default { get; set; }
        public bool Is_Closed { get; set; }
        public bool Is_Active { get; set; }
    }

    public class FiscalYearModel
    {
        public int Fiscal_Year_ID { get; set; }
        public string Company_ID { get; set; } = string.Empty;
        public string Year_Name { get; set; } = string.Empty;
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public bool Is_Default { get; set; }
        public bool Is_Closed { get; set; }
        public bool Is_Active { get; set; }
    }
}