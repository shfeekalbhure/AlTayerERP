namespace AlTayerERP.Desktop.Models
{
    public class CostCenterModel
    {
        public string Cost_Center_ID { get; set; } = "";

        public string Company_ID { get; set; } = "";

        public string? Parent_Cost_Center_ID { get; set; }

        public string Center_Code { get; set; } = "";

        public string Center_Name_AR { get; set; } = "";

        public string? Center_Name_EN { get; set; }

        public int Center_Level { get; set; }

        public bool Is_Postable { get; set; }

        public bool Is_Active { get; set; }

        public string? Notes { get; set; }

        public string? Created_By { get; set; }

        public System.DateTime Created_At { get; set; }

        public string? Updated_By { get; set; }

        public System.DateTime? Updated_At { get; set; }

        public string Id
        {
            get => Cost_Center_ID;
            set => Cost_Center_ID = value;
        }

        public string? ParentId
        {
            get => Parent_Cost_Center_ID;
            set => Parent_Cost_Center_ID = value;
        }

        public string CompanyId
        {
            get => Company_ID;
            set => Company_ID = value;
        }

        public string Code
        {
            get => Center_Code;
            set => Center_Code = value;
        }

        public string Name
        {
            get => Center_Name_AR;
            set => Center_Name_AR = value;
        }

        public int Level
        {
            get => Center_Level;
            set => Center_Level = value;
        }

        public bool IsActive
        {
            get => Is_Active;
            set => Is_Active = value;
        }
    }
}