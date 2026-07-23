using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("role_permissions")]
    public class RolePermission
    {
        [Key]
        [Column("Permission_ID")]
        public int Permission_ID { get; set; }

        [Column("Role_ID")]
        public int Role_ID { get; set; }

        [Column("Screen_ID")]
        public int Screen_ID { get; set; }

        [Column("Can_View")]
        public bool Can_View { get; set; }

        [Column("Can_Add")]
        public bool Can_Add { get; set; }

        [Column("Can_Edit")]
        public bool Can_Edit { get; set; }

        [Column("Can_Delete")]
        public bool Can_Delete { get; set; }

        [Column("Can_Print")]
        public bool Can_Print { get; set; }

        [Column("Can_Export")]
        public bool Can_Export { get; set; }

        [Column("Can_Import")]
        public bool Can_Import { get; set; }

        [Column("Can_Approve")]
        public bool Can_Approve { get; set; }

        [Column("Can_UnApprove")]
        public bool Can_UnApprove { get; set; }

        // System_Permission_ID: الربط الدقيق مع كتالوج الصلاحيات الجديد.
        // يبقى Screen_ID للسجلات القديمة حتى اكتمال ترحيل شاشة الصلاحيات.
        [Column("System_Permission_ID")]
        public int? System_Permission_ID { get; set; }

        // Effect: Allow أو Deny، والمنع يتقدم على المنح عند التقييم.
        [Column("Effect")]
        public string Effect { get; set; } = "Allow";

        // Grant_Descendants: يمنح الأبناء عند السماح به صراحة فقط.
        [Column("Grant_Descendants")]
        public bool Grant_Descendants { get; set; }

        // حقول السريان تمنع استمرار صلاحية مؤقتة بعد انتهاء تفويضها.
        [Column("Effective_From")]
        public DateTime Effective_From { get; set; } = DateTime.UtcNow;

        [Column("Effective_To")]
        public DateTime? Effective_To { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;
    }
}