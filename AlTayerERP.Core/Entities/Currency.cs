using System;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.Core.Entities
{
    /// <summary>
    /// يمثل تعريف العملة وأسعار الصرف الخاصة بها.
    /// </summary>
    public class Currency
    {
        [Key]
        public int Currency_ID { get; set; }

        public string Company_ID { get; set; } = string.Empty;

        public string Currency_Code { get; set; } = string.Empty;

        public string Currency_Name_AR { get; set; } = string.Empty;

        public string? Currency_Name_EN { get; set; }

        public string? Currency_Symbol { get; set; }

        public int Decimal_Places { get; set; }

        /// <summary>
        /// سعر الصرف الحالي المعتمد.
        /// </summary>
        public decimal Exchange_Rate { get; set; }

        /// <summary>
        /// أقل سعر صرف مسموح به.
        /// </summary>
        public decimal? Min_Exchange_Rate { get; set; }

        /// <summary>
        /// أعلى سعر صرف مسموح به.
        /// </summary>
        public decimal? Max_Exchange_Rate { get; set; }

        /// <summary>
        /// هل العملة هي العملة المحلية للشركة؟
        /// </summary>
        public bool Is_Local_Currency { get; set; }

        /*
         * نترك الحقل لأنه موجود حاليًا في قاعدة البيانات،
         * لكننا لم نعد نعتمد عليه في النظام.
         */
        public bool Is_Default { get; set; }

        public bool Is_Active { get; set; }

        public string? Notes { get; set; }

        public string? Created_By { get; set; }

        public DateTime? Created_At { get; set; }

        public string? Updated_By { get; set; }

        public DateTime? Updated_At { get; set; }
    }
}