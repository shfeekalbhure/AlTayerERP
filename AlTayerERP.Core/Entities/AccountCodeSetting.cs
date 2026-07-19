using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlTayerERP.Core.Entities
{
    // ======================================================
    // إعدادات طريقة ترقيم الحسابات لكل شركة
    // مثال:
    // المستوى 1 طوله 1
    // المستوى 2 طوله 1
    // المستوى 3 طوله 1
    // المستوى 4 طوله 2 أو 3 أو 4 حسب رغبة الشركة
    // ======================================================
    [Table("account_code_settings")]
    public class AccountCodeSetting
    {
        [Key]
        [Column("Setting_ID")]
        public int Setting_ID { get; set; }

        [Column("Company_ID")]
        public string Company_ID { get; set; } = string.Empty;

        [Column("Level_No")]
        public int Level_No { get; set; }

        [Column("Segment_Length")]
        public int Segment_Length { get; set; }


        // الرقم الذي يبدأ منه الترقيم في هذا المستوى
        [Column("Start_Number")]
        public int Start_Number { get; set; } = 1;

        // حرف الإكمال، غالبًا صفر
        [Column("Padding_Char")]
        public string Padding_Char { get; set; } = "0";

        // هل الرقم يعتمد على الحساب الأب
        [Column("Parent_Based")]
        public bool Parent_Based { get; set; } = true;

        // هل يتم توليد الرقم تلقائيًا
        [Column("Auto_Generate")]
        public bool Auto_Generate { get; set; } = true;

        // هل يسمح بترقيم يدوي في هذا المستوى
        [Column("Allow_Manual_Code")]
        public bool Allow_Manual_Code { get; set; } = false;

        // أقصى عدد للأرقام في هذا المستوى
        // يستخدم للتحقق من تجاوز الحد
        [Column("Max_Serial")]
        public int Max_Serial { get; set; } = 9999;

        [Column("Is_Active")]
        public bool Is_Active { get; set; } = true;

        [Column("Created_At")]
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}