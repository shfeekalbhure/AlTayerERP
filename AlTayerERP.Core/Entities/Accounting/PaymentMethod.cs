using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities.Accounting
{
    /// <summary>
    /// يمثل طرق السداد المستخدمة في السندات المالية.
    /// أمثلة:
    /// نقدي - شيك - تحويل بنكي - بطاقة - آجل - دفعة من الحساب.
    /// </summary>
    [Table("payment_methods")]
    public class PaymentMethod
    {
        #region أولًا: المفتاح الرئيسي

        /// <summary>
        /// المعرف الرقمي الفريد لطريقة السداد.
        /// </summary>
        [Key]
        [Column("Payment_Method_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Payment_Method_ID { get; set; }

        #endregion

        #region ثانيًا: بيانات طريقة السداد

        /// <summary>
        /// الكود البرمجي المختصر لطريقة السداد.
        /// أمثلة:
        /// CASH - CHEQUE - BANK_TRANSFER.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("Payment_Method_Code")]
        public string Payment_Method_Code { get; set; } = string.Empty;

        /// <summary>
        /// اسم طريقة السداد باللغة العربية.
        /// </summary>
        [Required]
        [MaxLength(150)]
        [Column("Payment_Method_Name_AR")]
        public string Payment_Method_Name_AR { get; set; } = string.Empty;

        /// <summary>
        /// اسم طريقة السداد باللغة الإنجليزية.
        /// </summary>
        [MaxLength(150)]
        [Column("Payment_Method_Name_EN")]
        public string? Payment_Method_Name_EN { get; set; }

        #endregion

        #region ثالثًا: خصائص طريقة السداد

        /// <summary>
        /// هل تتطلب طريقة السداد إدخال رقم مرجع؟
        /// مثل رقم الشيك أو رقم التحويل البنكي.
        /// </summary>
        [Column("Requires_Reference")]
        public bool Requires_Reference { get; set; }

        /// <summary>
        /// هل تتطلب طريقة السداد إدخال تاريخ مرجع؟
        /// مثل تاريخ الشيك.
        /// </summary>
        [Column("Requires_Reference_Date")]
        public bool Requires_Reference_Date { get; set; }

        /// <summary>
        /// هل تمثل طريقة السداد عملية نقدية مباشرة؟
        /// </summary>
        [Column("Is_Cash")]
        public bool Is_Cash { get; set; }

        /// <summary>
        /// هل تمثل طريقة السداد عملية بنكية؟
        /// </summary>
        [Column("Is_Bank")]
        public bool Is_Bank { get; set; }

        #endregion

        #region رابعًا: الحالة والترتيب

        /// <summary>
        /// يحدد هل طريقة السداد نشطة ومتاحة للاستخدام.
        /// </summary>
        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        /// <summary>
        /// ترتيب ظهور طريقة السداد في القوائم المنسدلة.
        /// </summary>
        [Column("Sort_Order")]
        public int Sort_Order { get; set; }

        #endregion
    }
}