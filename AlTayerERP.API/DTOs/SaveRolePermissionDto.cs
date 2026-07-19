namespace AlTayerERP.API.DTOs
{
    public class SaveRolePermissionDto
    {
        public int Role_ID { get; set; }

        public int Screen_ID { get; set; }

        public bool Can_View { get; set; }

        public bool Can_Add { get; set; }

        public bool Can_Edit { get; set; }

        public bool Can_Delete { get; set; }

        public bool Can_Print { get; set; }

        public bool Can_Export { get; set; }

        public bool Can_Import { get; set; }

        public bool Can_Approve { get; set; }

        public bool Can_UnApprove { get; set; }
    }
}