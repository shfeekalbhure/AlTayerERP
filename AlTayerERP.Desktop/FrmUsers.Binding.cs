using System;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AlTayerERP.Desktop
{
    public partial class FrmUsers
    {
        /// <summary>
        /// يزامن مربع الحالة المرئي مع قيمة الحساب النشط التي يرسلها الطلب إلى API.
        /// </summary>
        private void chkIsActive_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isBinding) return;
            cmbStatus.Text = chkIsActive.Checked ? "نشط" : "موقوف";
        }

        /// <summary>
        /// يزامن تغيير القائمة مع مربع الحساب النشط عند الإدخال أو التعديل.
        /// </summary>
        private void cmbStatus_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isBinding) return;
            chkIsActive.Checked = cmbStatus.Text == "نشط";
        }

        /// <summary>
        /// يجلب بطاقة تدقيق المستخدم المحدد؛ لا تُحسب القيم في سطح المكتب.
        /// </summary>
        private async Task LoadUserAuditInfoAsync(int userId)
        {
            try
            {
                var audit = await _client.GetFromJsonAsync<UserAuditInfoModel>($"{_baseUrl}Users/{userId}/audit-info");
                if (audit == null) return;

                _lblCreatedBy.Text = $"أنشئ بواسطة: {audit.Created_By}";
                _lblCreatedAt.Text = $"تاريخ الإنشاء: {FormatAuditDate(audit.Created_At)}";
                _lblUpdatedBy.Text = $"عُدّل بواسطة: {audit.Updated_By}";
                _lblUpdatedAt.Text = $"تاريخ التعديل: {FormatAuditDate(audit.Updated_At)}";
                _lblEditCount.Text = $"عدد مرات التعديل: {audit.Edit_Count}";
                _lblPrintCount.Text = $"عدد مرات الطباعة: {audit.Print_Count}";
            }
            catch
            {
                // يبقى التذييل آمناً وواضحاً إذا تعذر الاتصال، دون إظهار رسالة مزعجة لكل اختيار.
                ClearUserAuditInfo();
            }
        }

        /// <summary>
        /// يسجل عملية طباعة بيانات المستخدم المحدد ثم ينعش عداد الطباعة في التذييل.
        /// </summary>
        private async Task RegisterUserPrintAsync(int userId)
        {
            try
            {
                var response = await _client.PostAsync($"{_baseUrl}Users/{userId}/print", null);
                if (response.IsSuccessStatusCode)
                    await LoadUserAuditInfoAsync(userId);
            }
            catch
            {
                // فشل التسجيل لا يمنع المستخدم من فتح معاينة الطباعة.
            }
        }

        /// <summary>يعيد بطاقة التدقيق إلى القيم المحايدة عند إنشاء سجل جديد.</summary>
        private void ClearUserAuditInfo()
        {
            _lblCreatedBy.Text = "أنشئ بواسطة: —";
            _lblCreatedAt.Text = "تاريخ الإنشاء: —";
            _lblUpdatedBy.Text = "عُدّل بواسطة: —";
            _lblUpdatedAt.Text = "تاريخ التعديل: —";
            // السجل الجديد لم يُعدّل أو يُطبع بعد، لذلك تظهر العدادات صفراً.
            _lblEditCount.Text = "عدد مرات التعديل: 0";
            _lblPrintCount.Text = "عدد مرات الطباعة: 0";
        }

        /// <summary>صياغة موحدة للتواريخ العربية داخل بطاقة التدقيق.</summary>
        private static string FormatAuditDate(DateTime? value) =>
            value?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—";
    }

    /// <summary>طلب سطح المكتب المطابق لعقد CreateUserDto في API.</summary>
    public sealed class UserSaveRequest
    {
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Role_ID { get; set; }
        public string User_Code { get; set; } = string.Empty;
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public bool Must_Change_Password { get; set; }
        public bool Is_Active { get; set; }
    }

    /// <summary>قيم تدقيق المستخدم المقروءة من API ولا تقبل التعديل من الواجهة.</summary>
    public sealed class UserAuditInfoModel
    {
        public string Created_By { get; set; } = "—";
        public DateTime? Created_At { get; set; }
        public string Updated_By { get; set; } = "—";
        public DateTime? Updated_At { get; set; }
        public int Edit_Count { get; set; }
        public int Print_Count { get; set; }
    }
}
