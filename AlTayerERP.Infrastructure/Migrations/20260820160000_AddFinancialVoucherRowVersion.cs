using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlTayerERP.Infrastructure.Migrations
{
    /// <summary>
    /// يضيف رمز تزامن لرأس السند المالي. يطبق على MySQL تجريبية أولاً فقط
    /// وفق نموذج القبول الميداني؛ لا يعد أمراً لتعديل قاعدة الإنتاج مباشرة.
    /// </summary>
    public partial class AddFinancialVoucherRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                table: "financial_voucher_headers",
                type: "char(36)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE `financial_voucher_headers` SET `RowVersion` = UUID() WHERE `RowVersion` IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "RowVersion",
                table: "financial_voucher_headers",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "financial_voucher_headers");
        }
    }
}
