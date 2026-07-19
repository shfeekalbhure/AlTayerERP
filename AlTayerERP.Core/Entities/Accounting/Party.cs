using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// يمثل الأطراف المالية في النظام.
    /// (عميل - مورد - موظف - مندوب - وكيل - جهة حكومية ...).
    /// </summary>
    [Table("parties")]
    public class Party
    {
        #region المفتاح الرئيسي

        /// <summary>
        /// معرف الطرف.
        /// </summary>
        [Key]
        [Column("Party_ID")]
        [MaxLength(50)]
        public string Party_ID { get; set; } = string.Empty;

        #endregion

        #region بيانات الشركة

        /// <summary>
        /// الشركة المالكة للطرف.
        /// </summary>
        [Required]
        [Column("Company_ID")]
        [MaxLength(50)]
        public string Company_ID { get; set; } = string.Empty;

        #endregion

        #region البيانات الأساسية

        /// <summary>
        /// كود الطرف.
        /// </summary>
        [Required]
        [Column("Party_Code")]
        [MaxLength(50)]
        public string Party_Code { get; set; } = string.Empty;

        /// <summary>
        /// الاسم العربي.
        /// </summary>
        [Required]
        [Column("Party_Name_AR")]
        [MaxLength(200)]
        public string Party_Name_AR { get; set; } = string.Empty;

        /// <summary>
        /// الاسم الإنجليزي.
        /// </summary>
        [Column("Party_Name_EN")]
        [MaxLength(200)]
        public string? Party_Name_EN { get; set; }

        /// <summary>
        /// نوع الطرف.
        /// </summary>
        [Required]
        [Column("Party_Type")]
        [MaxLength(50)]
        public string Party_Type { get; set; } = string.Empty;

        #endregion

        #region بيانات الاتصال

        /// <summary>
        /// رقم الجوال.
        /// </summary>
        [Column("Mobile_No")]
        [MaxLength(30)]
        public string? Mobile_No { get; set; }

        /// <summary>
        /// رقم الهاتف.
        /// </summary>
        [Column("Phone_No")]
        [MaxLength(30)]
        public string? Phone_No { get; set; }

        /// <summary>
        /// رقم الهوية.
        /// </summary>
        [Column("Identity_No")]
        [MaxLength(100)]
        public string? Identity_No { get; set; }

        /// <summary>
        /// الرقم الضريبي.
        /// </summary>
        [Column("Tax_No")]
        [MaxLength(100)]
        public string? Tax_No { get; set; }

        /// <summary>
        /// العنوان.
        /// </summary>
        [Column("Address")]
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// المدينة.
        /// </summary>
        [Column("City_Name")]
        [MaxLength(150)]
        public string? City_Name { get; set; }

        #endregion

        #region الربط المالي

        /// <summary>
        /// الحساب المالي المرتبط بالطرف.
        /// </summary>
        [Column("Account_ID")]
        [MaxLength(50)]
        public string? Account_ID { get; set; }

        /// <summary>
        /// الحد الائتماني.
        /// </summary>
        [Column("Credit_Limit")]
        public decimal Credit_Limit { get; set; }

        #endregion

        #region الحالة

        /// <summary>
        /// هل الطرف فعال؟
        /// </summary>
        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        /// <summary>
        /// ملاحظات.
        /// </summary>
        [Column("Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        #endregion

        #region بيانات النظام

        /// <summary>
        /// تاريخ الإنشاء.
        /// </summary>
        [Column("Created_At")]
        public DateTime Created_At { get; set; }

        /// <summary>
        /// المستخدم الذي أنشأ السجل.
        /// </summary>
        [Column("Created_By")]
        [MaxLength(50)]
        public string? Created_By { get; set; }

        /// <summary>
        /// تاريخ آخر تعديل.
        /// </summary>
        [Column("Updated_At")]
        public DateTime? Updated_At { get; set; }

        /// <summary>
        /// المستخدم الذي عدل السجل.
        /// </summary>
        [Column("Updated_By")]
        [MaxLength(50)]
        public string? Updated_By { get; set; }

        #endregion
    }
}