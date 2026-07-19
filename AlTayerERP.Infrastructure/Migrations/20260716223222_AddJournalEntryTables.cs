using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlTayerERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryTables : Migration
    {
        /// <inheritdoc />
        /// 
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            #region إنشاء جدول رأس القيود المحاسبية

            migrationBuilder.CreateTable(
                name: "journal_entry_headers",
                columns: table => new
                {
                    Journal_Entry_ID = table.Column<long>(
                            type: "bigint",
                            nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    Entry_No = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: false),

                    Entry_Type = table.Column<byte>(
                        type: "tinyint unsigned",
                        nullable: false),

                    Entry_Status_ID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Branch_ID = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: false),

                    Fiscal_Year_ID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    Entry_Date = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false),

                    Transaction_Date = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false),

                    Source_System = table.Column<string>(
                        type: "varchar(30)",
                        maxLength: 30,
                        nullable: false),

                    Is_System_Generated = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Source_Voucher_ID = table.Column<long>(
                        type: "bigint",
                        nullable: true),

                    Source_Document_Type = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Source_Document_No = table.Column<string>(
                        type: "varchar(100)",
                        maxLength: 100,
                        nullable: true),

                    Description = table.Column<string>(
                        type: "varchar(500)",
                        maxLength: 500,
                        nullable: true),

                    Notes = table.Column<string>(
                        type: "varchar(1000)",
                        maxLength: 1000,
                        nullable: true),

                    Total_Debit = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Total_Credit = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Is_Posted = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Posted_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Posted_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true),

                    Is_Reversal = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Original_Journal_Entry_ID = table.Column<long>(
                        type: "bigint",
                        nullable: true),

                    Is_Reversed = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Reversal_Journal_Entry_ID = table.Column<long>(
                        type: "bigint",
                        nullable: true),

                    Reversal_Reason = table.Column<string>(
                        type: "varchar(500)",
                        maxLength: 500,
                        nullable: true),

                    Reversed_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Reversed_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true),

                    Is_Cancelled = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Cancellation_Reason = table.Column<string>(
                        type: "varchar(500)",
                        maxLength: 500,
                        nullable: true),

                    Cancelled_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Cancelled_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true),

                    Is_Active = table.Column<bool>(
                        type: "tinyint(1)",
                        nullable: false),

                    Created_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Created_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false),

                    Updated_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Updated_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_journal_entry_headers",
                        x => x.Journal_Entry_ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            #endregion

            #region إنشاء جدول تفاصيل القيود المحاسبية

            migrationBuilder.CreateTable(
                name: "journal_entry_details",
                columns: table => new
                {
                    Journal_Entry_Detail_ID = table.Column<long>(
                            type: "bigint",
                            nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    Journal_Entry_ID = table.Column<long>(
                        type: "bigint",
                        nullable: false),

                    Line_No = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Account_ID = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: false),

                    Description = table.Column<string>(
                        type: "varchar(500)",
                        maxLength: 500,
                        nullable: true),

                    Cost_Center_ID = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Project_ID = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Currency_ID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Exchange_Rate = table.Column<decimal>(
                        type: "decimal(18,6)",
                        precision: 18,
                        scale: 6,
                        nullable: false),

                    Foreign_Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Local_Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Debit_Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Credit_Amount = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false),

                    Reference_Type = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Reference_No = table.Column<string>(
                        type: "varchar(100)",
                        maxLength: 100,
                        nullable: true),

                    Reference_Name = table.Column<string>(
                        type: "varchar(250)",
                        maxLength: 250,
                        nullable: true),

                    Reference_Date = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true),

                    Source_Voucher_Detail_ID = table.Column<long>(
                        type: "bigint",
                        nullable: true),

                    Line_Type = table.Column<byte>(
                        type: "tinyint unsigned",
                        nullable: false),

                    Notes = table.Column<string>(
                        type: "varchar(500)",
                        maxLength: 500,
                        nullable: true),

                    Created_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Created_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false),

                    Updated_By = table.Column<string>(
                        type: "varchar(50)",
                        maxLength: 50,
                        nullable: true),

                    Updated_At = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_journal_entry_details",
                        x => x.Journal_Entry_Detail_ID);

                    table.ForeignKey(
                        name: "FK_journal_entry_details_journal_entry_headers_Journal_Entry_ID",
                        column: x => x.Journal_Entry_ID,
                        principalTable: "journal_entry_headers",
                        principalColumn: "Journal_Entry_ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            #endregion

            #region إنشاء الفهارس

            migrationBuilder.CreateIndex(
                name: "UQ_Journal_Entry_No",
                table: "journal_entry_headers",
                column: "Entry_No",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Entry_Date",
                table: "journal_entry_headers",
                column: "Entry_Date");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Entry_Source_Voucher",
                table: "journal_entry_headers",
                column: "Source_Voucher_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_Entry_Details_Header",
                table: "journal_entry_details",
                column: "Journal_Entry_ID");

            migrationBuilder.CreateIndex(
                name: "UQ_Journal_Entry_Detail_Line",
                table: "journal_entry_details",
                columns: new[]
                {
            "Journal_Entry_ID",
            "Line_No"
                },
                unique: true);

            #endregion
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "journal_entry_details");

            migrationBuilder.DropTable(
                name: "journal_entry_headers");
        }
       
    }
}
