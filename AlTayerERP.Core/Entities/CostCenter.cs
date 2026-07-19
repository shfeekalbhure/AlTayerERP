using System;
using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.Core.Entities
{
    public class CostCenter
    {
        [Key]
        public string Cost_Center_ID { get; set; } = "";
        public string Company_ID { get; set; } = "";
        public string? Parent_Cost_Center_ID { get; set; }
        public string Center_Code { get; set; } = "";
        public string Center_Name_AR { get; set; } = "";
        public string? Center_Name_EN { get; set; }
        public int Center_Level { get; set; }
        public bool Is_Postable { get; set; }
        public DateTime Created_At { get; set; }
        public bool Is_Active { get; set; }
    }
}