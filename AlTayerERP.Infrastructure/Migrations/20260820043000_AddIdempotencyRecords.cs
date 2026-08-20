using System;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlTayerERP.Infrastructure.Migrations
{
    /// <summary>إضافة سجل دائم لمفاتيح عدم التكرار للعمليات المالية الحساسة.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260820043000_AddIdempotencyRecords")]
    public partial class AddIdempotencyRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    Idempotency_Record_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Operation = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Idempotency_Key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Company_ID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Branch_ID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Fiscal_Year_ID = table.Column<int>(type: "int", nullable: false),
                    User_ID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Request_Fingerprint = table.Column<string>(type: "char(64)", nullable: false),
                    Status = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Resource_ID = table.Column<long>(type: "bigint", nullable: true),
                    Resource_No = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Completed_At = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_records", x => x.Idempotency_Record_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "UQ_Idempotency_Record_Scope_Key",
                table: "idempotency_records",
                columns: new[]
                {
                    "Operation", "Idempotency_Key", "Company_ID", "Branch_ID", "Fiscal_Year_ID", "User_ID"
                },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Idempotency_Record_Status_Created",
                table: "idempotency_records",
                columns: new[] { "Status", "Created_At" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "idempotency_records");
        }
    }
}
