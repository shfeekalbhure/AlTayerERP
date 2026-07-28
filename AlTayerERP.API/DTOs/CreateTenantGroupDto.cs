using System.Text.Json.Serialization;

namespace AlTayerERP.API.DTOs
{
    /// <summary>حقول المجموعة التجارية المعتمدة للمرحلة الحالية.</summary>
    public sealed class CreateTenantGroupDto
    {
        public string Group_Code { get; set; } = string.Empty;
        public string Group_Name_AR { get; set; } = string.Empty;
        public string? Group_Name_EN { get; set; }
        public bool Is_Default { get; set; }
        public bool Show_In_Login { get; set; }

        /// <summary>
        /// حقل توافق داخلي فقط. لا يُقبل من العميل ولا يظهر في عقدة الإدخال؛
        /// ظهور الشاشات في الشجرة تحكمه الصلاحيات وكتالوج الشاشات.
        /// </summary>
        [JsonIgnore]
        public bool Show_In_Tree => true;

        public string? Notes { get; set; }
    }
}
