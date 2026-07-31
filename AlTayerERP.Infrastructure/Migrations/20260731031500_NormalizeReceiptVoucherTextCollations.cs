using System;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlTayerERP.Infrastructure.Migrations;

/// <summary>
/// يوحّد ترميز الحقول النصية في مسار حفظ سندات القبض والصرف.
/// قاعدة المرحلة الأولى تعتمد utf8mb4_unicode_ci؛ وهذه الهجرة تصلح قواعد
/// أنشئت سابقاً بإعداد MySQL 8 الافتراضي utf8mb4_0900_ai_ci.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260731031500_NormalizeReceiptVoucherTextCollations")]
public partial class NormalizeReceiptVoucherTextCollations : Migration
{
    private const string CharacterSet = "utf8mb4";
    private const string Collation = "utf8mb4_unicode_ci";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // العمليات التالية تخص قاعدة الاختبار فقط. تعطيل فحص المفاتيح مؤقتاً
        // يسمح بتحويل جانبي العلاقة النصية من دون إسقاط أي بيانات أو قيود.
        migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 0;");

        ConvertTable(migrationBuilder, "companies");
        ConvertTable(migrationBuilder, "tenant_branches");
        ConvertTable(migrationBuilder, "fiscal_years");
        ConvertTable(migrationBuilder, "currencies");
        ConvertTable(migrationBuilder, "chart_of_accounts");
        ConvertTable(migrationBuilder, "cash_boxes");
        ConvertTable(migrationBuilder, "bank_accounts");
        ConvertTable(migrationBuilder, "numbering_settings");
        ConvertTable(migrationBuilder, "financial_voucher_headers");
        ConvertTable(migrationBuilder, "financial_voucher_details");

        migrationBuilder.Sql("SET FOREIGN_KEY_CHECKS = 1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "هذه الهجرة تصلح حالة Collation مختلطة ولا تعيد قاعدة البيانات إلى حالة غير متجانسة.");
    }

    private static void ConvertTable(MigrationBuilder migrationBuilder, string table)
    {
        migrationBuilder.Sql(
            $"ALTER TABLE `{table}` CONVERT TO CHARACTER SET {CharacterSet} COLLATE {Collation};");
    }
}
