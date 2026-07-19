using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    [Table("role_permissions")]
    public class RolePermission
    {
        [Key]
        [Column("Permission_ID")]
        public int Permission_ID { get; set; }

        [Column("Role_ID")]
        public int Role_ID { get; set; }

        [Column("Screen_ID")]
        public int Screen_ID { get; set; }

        [Column("Can_View")]
        public bool Can_View { get; set; }

        [Column("Can_Add")]
        public bool Can_Add { get; set; }

        [Column("Can_Edit")]
        public bool Can_Edit { get; set; }

        [Column("Can_Delete")]
        public bool Can_Delete { get; set; }

        [Column("Can_Print")]
        public bool Can_Print { get; set; }

        [Column("Can_Export")]
        public bool Can_Export { get; set; }

        [Column("Can_Import")]
        public bool Can_Import { get; set; }

        [Column("Can_Approve")]
        public bool Can_Approve { get; set; }

        [Column("Can_UnApprove")]
        public bool Can_UnApprove { get; set; }
    }
}