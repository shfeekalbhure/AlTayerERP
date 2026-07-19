using System.ComponentModel.DataAnnotations;

namespace AlTayerERP.API.DTOs
{
    public class CreateCostCenterDto
    {
        [Required]
        public string Company_ID { get; set; } = "";

        public string? Parent_Cost_Center_ID { get; set; }

        // أصبح اختياري لأن النظام سيولده تلقائياً
        public string? Center_Code { get; set; }

        [Required]
        public string Center_Name_AR { get; set; } = "";

        public string? Center_Name_EN { get; set; }

        public int Center_Level { get; set; } = 1;

        public bool Is_Postable { get; set; } = true;

        public bool Is_Active { get; set; } = true;

        public string? Notes { get; set; }

        public string? Created_By { get; set; }

        public string? Updated_By { get; set; }
    }
}