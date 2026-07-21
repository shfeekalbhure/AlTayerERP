using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Json;
// استدعاء المجلد المركزي لخدمات الـ API
using AlTayerERP.Desktop.Services;

namespace AlTayerERP.Desktop
{
    public partial class Form1 : Form
    {
        
        // إنشاء كائن ثابت لـ HttpClient للتعامل مع اتصالات الشبكة والـ API من الملف المركزي
        private readonly HttpClient _client = ApiService.Client;
        // الرابط الأساسي الثابت الذي يشير إلى موقع الـ API من الملف المركزي
        private readonly string _baseUrl = ApiService.BaseUrl;

        public Form1()
        {
            InitializeComponent();

            // تطبيق المظهر العربي الموحد دون تغيير منطق الشاشة.
            ArabicErpFormStyle.Apply(this);

            // ربط الأحداث برمجياً لضمان العمل السلس
            this.Load += Form1_Load;
            btnSaveGroup.Click += btnSaveGroup_Click;
        }

        // ======================================================
        // [حدث] زر الحفظ - إرسال بيانات المجموعة التجارية للـ API بالربط المركزي
        // ======================================================
        private async void btnSaveGroup_Click(object sender, EventArgs e)
        {
            // 1. التحقق من إدخال البيانات الأساسية
            if (string.IsNullOrWhiteSpace(txtGroupNameAr.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المجموعة التجاري بالعربي!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. تجهيز كائن البيانات (Object) لإرساله
            var groupData = new
            {
                Group_Name_AR = txtGroupNameAr.Text.Trim(),
                Group_Name_EN = txtGroupNameEn.Text.Trim()
            };

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // 3. [تعديل التحسين]: استخدام الـ _client والـ _baseUrl المركزيين بدلاً من إنشائهم يدوياً هنا
                HttpResponseMessage response = await _client.PostAsJsonAsync($"{_baseUrl}TenantGroups", groupData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم حفظ وتأسيس المجموعة التجارية بنجاح في السيرفر الرئيسي!", "نجاح العملية", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // تنظيف الحقول بعد الحفظ
                    txtGroupNameAr.Clear();
                    txtGroupNameEn.Clear();
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"فشل الحفظ. تفاصيل السيرفر: {errorDetails}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"تعذر الاتصال بالـ API المركزي. تفاصيل الخطأ: {ex.Message}", "خطأ اتصال", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}