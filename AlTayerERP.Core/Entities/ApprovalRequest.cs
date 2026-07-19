using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    // حالات طلب الاعتماد
    public enum ApprovalStatus
    {
        Pending,      // بانتظار الاعتماد
        UnderReview,  // تحت المراجعة
        Approved,     // معتمد
        Rejected,     // مرفوض
        Returned,     // معاد للتعديل
        Canceled      // ملغي
    }

    [Table("approval_requests")]
    public class ApprovalRequest
    {
        [Key]
        [Column("Approval_ID")]
        public int Approval_ID { get; set; }

        [Required]
        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Required]
        [Column("Request_Type")]
        public string Request_Type { get; set; } = string.Empty;

        [Column("Reference_Type")]
        public string? Reference_Type { get; set; }

        [Column("Reference_ID")]
        public string? Reference_ID { get; set; }

        [Column("Entity_Type")]
        public string? Entity_Type { get; set; }

        [Column("Entity_ID")]
        public string? Entity_ID { get; set; }

        [Column("Currency_Code")]
        public string? Currency_Code { get; set; }

        [Column("Amount", TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        [Column("Reason")]
        public string? Reason { get; set; }

        [Column("Status")]
        public string Status { get; set; } = ApprovalStatus.Pending.ToString();

        [Column("Requested_By")]
        public string? Requested_By { get; set; }

        [Column("Requested_At")]
        public DateTime Requested_At { get; set; } = DateTime.UtcNow;

        [Column("Approved_By")]
        public string? Approved_By { get; set; }

        [Column("Approved_At")]
        public DateTime? Approved_At { get; set; }

        [Column("Approval_Notes")]
        public string? Approval_Notes { get; set; }
    }
}