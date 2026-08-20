using System;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlTayerERP.Infrastructure.Migrations
{
    /// <summary>إضافة رمز التزامن التفاؤلي لطلبات الصرف القائمة والجديدة.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260821000000_AddPaymentRequestRowVersion")]
    public partial class AddPaymentRequestRowVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RowVersion",
                table: "payment_requests",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty);

            // تُهيأ الصفوف القائمة بمعرّف فريد قبل بدء تطبيق فحص التزامن عليها.
            migrationBuilder.Sql(
                "UPDATE payment_requests SET RowVersion = UUID() " +
                "WHERE RowVersion = '00000000-0000-0000-0000-000000000000';");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "payment_requests");
        }
    }
}
