using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("companies")]
    public class Company
    {
        [Key]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Group_ID")]
        public string Group_ID { get; set; } = string.Empty;

        [Column("Company_Name_AR")]
        [Required]
        public string Company_Name_AR { get; set; } = string.Empty;

        [Column("Company_Name_EN")]
        public string Company_Name_EN { get; set; } = string.Empty;

        // رمز مختصر للشركة يستخدم في الترقيم
        [Column("Company_Prefix")]
        public string? Company_Prefix { get; set; }

        // جعلناه Nullable لأن الحقل قد يكون فارغاً بالداتابيز للشركات القديمة
        [Column("Activity_Type")]
        public string? Activity_Type { get; set; }

        // جعلناه Nullable لتجنب خطأ الـ DBNull إذا لم يُدخل رقم ضريبي
        [Column("Tax_Number")]
        public string? Tax_Number { get; set; }

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        // ======================================================
        // رقم الهاتف (جعلناه يقبل Null)
        // ======================================================
        [Column("Phone")]
        public string? Phone { get; set; }

        // ======================================================
        // رقم الجوال (جعلناه يقبل Null)
        // ======================================================
        [Column("Mobile")]
        public string? Mobile { get; set; }

        // ======================================================
        // البريد الإلكتروني (جعلناه يقبل Null)
        // ======================================================
        [Column("Email")]
        public string? Email { get; set; }

        // ======================================================
        // عنوان الشركة (جعلناه يقبل Null)
        // ======================================================
        [Column("Address")]
        public string? Address { get; set; }

        // ======================================================
        // شعار الشركة
        // ======================================================
        [Column("Company_Logo")]
        public byte[]? Company_Logo { get; set; }

        // ======================================================
        // تاريخ آخر تعديل
        // ======================================================
        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        /// <summary>المستخدم المنشئ Created_By من جلسة الخادم.</summary>
        [Column("Created_By")]
        public int? Created_By { get; set; }

        /// <summary>آخر مستخدم عدّل السجل Updated_By من جلسة الخادم.</summary>
        [Column("Updated_By")]
        public int? Updated_By { get; set; }

        /// <summary>عدد التعديلات الناجحة Edit_Count ولا يستقبل من العميل.</summary>
        [Column("Edit_Count")]
        public int Edit_Count { get; set; }
    }
}