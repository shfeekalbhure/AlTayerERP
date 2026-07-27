namespace AlTayerERP.Desktop.Models
{
    /// <summary>
    /// نموذج الصندوق المستخدم في شاشة سطح المكتب واستجابات API.
    /// Account_ID يمثل حساب الصناديق الأب المختار من المنسدلة.
    /// Linked_Account_ID يمثل الحساب الفرعي المرتبط بالصندوق بعد إنشائه.
    /// </summary>
    public class CashBoxModel
    {
        public string Cash_Box_ID { get; set; } = "";
        public string Company_ID { get; set; } = "";
        public int Branch_ID { get; set; }

        public string Account_ID { get; set; } = "";
        public string Linked_Account_ID { get; set; } = "";
        public string Account_Name_AR { get; set; } = "";

        public string Currency_Code { get; set; } = "";
        public string CashBox_Code { get; set; } = "";
        public string Box_Name_AR { get; set; } = "";
        public string? Box_Name_EN { get; set; }

        public decimal Opening_Balance { get; set; }
        public decimal Max_Limit { get; set; }
        public decimal Min_Limit { get; set; }
        public bool Is_Active { get; set; }
        public string? Notes { get; set; }

        public string? Created_By { get; set; }
        public System.DateTime Created_At { get; set; }
        public string? Updated_By { get; set; }
        public System.DateTime? Updated_At { get; set; }

        // خصائص توافقية مع أسماء الشاشة الحالية.
        public string ID
        {
            get => Cash_Box_ID;
            set => Cash_Box_ID = value;
        }

        public string Code
        {
            get => CashBox_Code;
            set => CashBox_Code = value;
        }

        public string NameAR
        {
            get => Box_Name_AR;
            set => Box_Name_AR = value;
        }

        public string? NameEN
        {
            get => Box_Name_EN;
            set => Box_Name_EN = value;
        }

        public bool IsActive
        {
            get => Is_Active;
            set => Is_Active = value;
        }
    }
}
