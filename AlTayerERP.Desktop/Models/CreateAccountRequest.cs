using System;

namespace AlTayerERP.Desktop.Models
{
    /// <summary>
    /// نموذج طلب إنشاء حساب مالي جديد يُستخدم في تطبيق السطح (Desktop Application) إرسال البيانات للـ API
    /// </summary>
    public class CreateAccountRequest
    {
        // --- البيانات الأساسية للهوية والتنظيم الشجري ---

        /// <summary>
        /// معرف الشركة الفرعي المرتبط بالحساب (مثال: FG-00001)
        /// </summary>
        public string Company_ID { get; set; } = string.Empty;

        /// <summary>
        /// معرف الحساب الأب (يكون فارغاً Null إذا كان الحساب رئيسياً في المستوى الأول)
        /// </summary>
        public string? Parent_Account_ID { get; set; }

        /// <summary>
        /// رقم أو كود الحساب المحاسبي (مثال: 1101001)
        /// </summary>
        public string Account_Code { get; set; } = string.Empty;

        /// <summary>
        /// اسم الحساب المالي باللغة العربية (حقل إجباري)
        /// </summary>
        public string Account_Name_AR { get; set; } = string.Empty;

        /// <summary>
        /// اسم الحساب المالي باللغة الإنجليزية (حقل اختياري)
        /// </summary>
        public string? Account_Name_EN { get; set; }


        // --- محددات الطبيعة المحاسبية والمستوى ---

        /// <summary>
        /// نوع الحساب الرئيسي (أصل، خصم، حقوق ملكية، إيراد، مصروف)
        /// </summary>
        public string Account_Type { get; set; } = string.Empty;

        /// <summary>
        /// المستوى الشجري للحساب بداخل الدليل (تبدأ من المستوى 1 للحسابات الرئيسية)
        /// </summary>
        public int Account_Level { get; set; }


        // --- محددات الحركة والنشاط ---

        /// <summary>
        /// هل الحساب قابل للترحيل؟ (True للحسابات الفرعية، False للحسابات الرئيسية الجامعة)
        /// </summary>
        public bool Is_Postable { get; set; } = true;

        /// <summary>
        /// رمز العملة الافتراضية للحساب (مثال: YER, SAR, USD)
        /// </summary>
        public string? Currency_Code { get; set; }

        /// <summary>
        /// حالة الحساب في النظام (True: نشط ومتاح للاستخدام، False: موقوف)
        /// </summary>
        public bool Is_Active { get; set; } = true;


        // --- الحقول الإضافية لضمان المطابقة الكاملة ---

        /// <summary>
        /// تصنيف الحساب الفرعي لمزيد من التخصيص (نقدية، بنك، عميل، مورد...)
        /// </summary>
        public string Account_Category { get; set; } = string.Empty;

        /// <summary>
        /// طبيعة الحركة المالية الافتراضية للحساب (مدين أو دائن)
        /// </summary>
        public string Normal_Balance { get; set; } = string.Empty;

        /// <summary>
        /// أي ملاحظات أو شروحات إضافية متعلقة بالحساب
        /// </summary>
        public string? Notes { get; set; }


        // --- الأسماء المحدثة لتطابق قاعدة البيانات والـ Entity والـ DTO تماماً ---

        /// <summary>
        /// السماح بإدخال أو تعديل كود الحساب يدوياً دون الاعتماد على التوليد الآلي للنظام
        /// </summary>
        public bool Allow_ManualEntry { get; set; }

        /// <summary>
        /// هل هذا حساب نظام أساسي؟ (حسابات محمية تولد وتستخدم تلقائياً من النظام ولا تحذف)
        /// </summary>
        public bool System_Account { get; set; }

        /// <summary>
        /// هل يتطلب الحساب ربطه بمركز تكلفة؟ (إلزامية تحديد مركز التكلفة عند إدخال المعاملة)
        /// </summary>
        public bool Requires_CostCenter { get; set; }


        // --- محددات الرقابة وصناديق الاختيار (CheckBoxes) المتطابقة مع الواجهة ---

        /// <summary>
        /// هل يتطلب الحساب تحديد طرف ثالث؟ (إلزامية تحديد العميل أو المورد أو الموظف عند القيد)
        /// </summary>
        public bool Requires_Party { get; set; }

        /// <summary>
        /// هل يتطلب الحساب ربطه بمشروع معين؟ (إلزامية تحديد المشروع عند التوجيه المالي)
        /// </summary>
        public bool Requires_Project { get; set; }

        /// <summary>
        /// هل الحساب تجميعي خلاصة؟ (يُستخدم لعرض وتجميع التقارير الإجمالية)
        /// </summary>
        public bool Is_Summary_Account { get; set; }

        /// <summary>
        /// تأثير الحساب في التقارير الختامية: هل يؤثر الحساب ويظهر في الميزانية العمومية؟
        /// </summary>
        public bool Affects_Balance_Sheet { get; set; }

        /// <summary>
        /// تأثير الحساب في التقارير الختامية: هل يؤثر الحساب ويظهر في قائمة الدخل؟
        /// </summary>
        public bool Affects_Income_Statement { get; set; }

        /// <summary>
        /// هل الحساب متعدد العملات؟ (السماح بمرور حركات مالية عليه بأكثر من عملة)
        /// </summary>
        public bool Multi_Currency { get; set; }


        // --- الحقول الإضافية الجديدة الموحدة في آخر الكلاس ---

        /// <summary>
        /// المسار الشجري الكامل للحساب المالي لسهولة الاستعلام والترتيب (مثال: 1/11/1101)
        /// </summary>
        public string? Account_Path { get; set; }

        /// <summary>
        /// الرقم التسلسلي التلقائي للحساب تحت الأب المباشر
        /// </summary>
        public int? Account_Serial { get; set; }

        /// <summary>
        /// معرف أو اسم المستخدم الذي قام بإنشاء الحساب
        /// </summary>
        public string? Created_By { get; set; }

        /// <summary>
        /// تاريخ ووقت آخر تعديل تم على الحساب
        /// </summary>
        public DateTime? Updated_At { get; set; }

        /// <summary>
        /// معرف أو اسم المستخدم الذي قام بآخر تعديل
        /// </summary>
        public string? Updated_By { get; set; }
    }
}