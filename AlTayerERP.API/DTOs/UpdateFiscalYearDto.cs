namespace AlTayerERP.API.DTOs
{
    public class UpdateFiscalYearDto
    {
        public int Fiscal_Year_ID { get; set; }

        public string Company_ID { get; set; } = string.Empty;

        public string Year_Name { get; set; } = string.Empty;

        public DateTime Start_Date { get; set; }

        public DateTime End_Date { get; set; }

        public bool Is_Default { get; set; }

        public bool Is_Closed { get; set; }

        public bool Is_Active { get; set; }
    }
}