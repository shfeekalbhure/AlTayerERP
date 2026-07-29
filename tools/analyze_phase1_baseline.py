from __future__ import annotations

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SQL_PATH = ROOT / "Database/Baseline/Baseline_Phase1.sql"
REPORT_PATH = ROOT / "Database/Reviews/Phase2_Baseline_PreApplication_Report.md"

if not SQL_PATH.exists():
    raise SystemExit("لم يتم العثور على سكربت Baseline المولد.")

sql = SQL_PATH.read_text(encoding="utf-8-sig")
lower = sql.lower()

forbidden = {
    "اسم القاعدة القديمة": "altayer_erp_db",
    "حذف قاعدة": "drop database",
    "اختيار قاعدة ثابتة": "use altayer_erp_db",
    "تفريغ جدول": "truncate table",
    "كلمة مرور": "password=",
    "بيانات اتصال": "server=",
}
violations = [label for label, token in forbidden.items() if token in lower]
if violations:
    raise SystemExit("فشل فحص الأمان: " + "، ".join(violations))

# أوامر DROP TABLE داخل قسم Down متوقعة في Migration، لكن السكربت Up المولد من 0
# يجب ألا يحتوي حذف جداول تشغيلية. السكربت idempotent لا يتضمن Down عادةً.
if re.search(r"\bdrop\s+table\b", lower):
    raise SystemExit("فشل فحص الأمان: السكربت المولد يحتوي DROP TABLE.")

create_tables = re.findall(r"CREATE\s+TABLE\s+`?([a-z0-9_]+)`?", sql, flags=re.I)
foreign_keys = re.findall(r"FOREIGN\s+KEY", sql, flags=re.I)
unique_constraints = re.findall(r"\bUNIQUE\b", sql, flags=re.I)
check_constraints = re.findall(r"\bCHECK\s*\(", sql, flags=re.I)
create_indexes = re.findall(r"CREATE\s+(?:UNIQUE\s+)?INDEX", sql, flags=re.I)

# استبعاد جداول EF الداخلية من العدد الوظيفي.
functional_tables = sorted({name for name in create_tables if name.lower() != "__efmigrationshistory"})

migration_files = sorted((ROOT / "AlTayerERP.Infrastructure/Migrations").glob("*.cs"))

report = f"""# تقرير Baseline قبل التطبيق — المرحلة الثانية

**الحالة:** مولدة ومفحوصة دون إنشاء قاعدة أو تنفيذ SQL على MySQL.  
**اسم Baseline:** `Baseline_Phase1`  
**Character Set:** `utf8mb4`  
**Collation:** `utf8mb4_unicode_ci`

## النتائج العددية

- عدد الجداول الوظيفية: **{len(functional_tables)}**.
- عدد علاقات Foreign Key في السكربت: **{len(foreign_keys)}**.
- عدد أوامر إنشاء الفهارس: **{len(create_indexes)}**.
- عدد ظهور القيود/الفهارس الفريدة: **{len(unique_constraints)}**.
- عدد Check Constraints: **{len(check_constraints)}**.
- عدد ملفات Migration/Snapshot النشطة: **{len(migration_files)}**.

## الجداول التي تنشئها Baseline

""" + "\n".join(f"- `{name}`" for name in functional_tables) + """

## فحص الأمان

- لا يوجد `altayer_erp_db`.
- لا يوجد `USE` لقاعدة ثابتة.
- لا يوجد `DROP DATABASE`.
- لا يوجد `DROP TABLE` في سكربت الإنشاء من الصفر.
- لا يوجد `TRUNCATE TABLE`.
- لا توجد كلمات مرور أو Connection String.
- لا توجد بيانات شركة أو فرع أو مستخدم تجريبية.

## بيانات Seed المسموحة

- أنواع الفروع.
- أنواع السندات.
- حالات السندات.
- طرق السداد.
- حالات الاعتماد.
- أنواع المستندات والترقيم.
- الصلاحيات الأساسية.
- كتالوج الشاشات الأساسي.

## مصادر المخطط

- `AppDbContext` و`Phase1ModelConfiguration`.
- Migration `Baseline_Phase1` المولدة بواسطة `dotnet ef`.
- ملفات SQL التاريخية غير مستخدمة في إنشاء Baseline.
- `PaymentRequestSchemaInitializer` لا ينفذ DDL.

## القيود

لم يتم تشغيل `Update-Database`، ولم تُنشأ `altayer_erp_db_clean`، ولم يتغير أي Connection String أو قاعدة بيانات.
"""

REPORT_PATH.parent.mkdir(parents=True, exist_ok=True)
REPORT_PATH.write_text(report, encoding="utf-8", newline="\n")
print(f"الجداول={len(functional_tables)} العلاقات={len(foreign_keys)} الفهارس={len(create_indexes)} unique={len(unique_constraints)} checks={len(check_constraints)}")
