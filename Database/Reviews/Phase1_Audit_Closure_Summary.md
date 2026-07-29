# ملخص إغلاق مرحلة تدقيق Baseline — المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA نقطة البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`

## الحالة

مرحلة التدقيق الأولى مكتملة وثائقيًا وجاهزة لاعتماد المالك. لا تبدأ المرحلة الثانية قبل اعتماد صريح.

## الأعداد النهائية

- Entities المستمرة: **42**.
- DbSets الصريحة: **41**.
- Entity مستخدم عبر `Set<>` بلا DbSet صريح: **AccountCategory**.
- Migrations التنفيذية: **1**.
- ملفات SQL داخل `Database` ومجلداته: **28**.
- الجداول التي تنشئها Migration الحالية: **2**.
- تعريفات الجداول التشغيلية الموثقة: **46**.
- جدول تشخيصي تاريخي موثق ومستبعد: **1 — `database_alignment_findings`**.

## المخرجات المكتملة

1. `Database/Reviews/Phase1_Database_Baseline_Audit.md`
2. `Database/Reviews/Database_Schema_Drift_Report.md`
3. `Database/Reviews/Phase1_Key_Type_Mismatch_Report.md`
4. `Database/Reviews/Phase1_SQL_Scripts_Classification.md`
5. `Database/Reviews/Branch_ID_Normalization_Assessment.md`
6. `Database/Documentation/Phase1_Table_Dependency_Map.md`
7. `Database/Documentation/Phase1_Data_Dictionary_Draft.md`
8. `Database/Documentation/Database_Decision_Log.md`

## النتائج الحاكمة

- لا توجد Baseline مكتملة حاليًا.
- Migration الوحيدة تنشئ جدولي القيود فقط.
- `Branch_ID` مختلط بين `INT` و`VARCHAR(50)`؛ التوصية `INT` وتحتاج اعتماد المالك وفحص البيانات قبل أي ترحيل.
- يوجد Runtime DDL داخل `PaymentRequestSchemaInitializer` لكنه غير مسجل أو مستدعى في `Program.cs` الحالي.
- التوصية الوحيدة للـCollation: `utf8mb4_unicode_ci`، وتحتاج اعتماد المالك لأن إصدارات البيئات ووجود MariaDB غير موثقة نهائيًا.
- Migrations مقترحة كمصدر وحيد للمخطط بعد اعتماد المرحلة الثانية.

## القرارات التي تحتاج اعتماد المالك

1. صيغة وطول `Group_ID`.
2. صيغة وطول `Company_ID`.
3. توحيد `Branch_ID` إلى `INT` بعد فحص القيم القديمة.
4. تثبيت `Account_ID` نصيًا وطوله، أو اعتماد بديل رقمي.
5. تثبيت `Cost_Center_ID` نصيًا وطوله.
6. إبقاء `Cash_Box_ID` GUID نصيًا وتحديد `CHAR(36)` أو `VARCHAR(50)`.
7. إبقاء `Party_ID` نصيًا بطول 50.
8. اعتماد `utf8mb4_unicode_ci`.
9. اعتماد `snake_case` lowercase لأسماء الجداول.
10. اعتماد IDs رقمية للجداول المرجعية وCodes نصية منفصلة.
11. سياسة حقول التدقيق: FK رقمية nullable مع Snapshot اختياري.
12. تخزين أحداث النظام بتوقيت UTC في `DATETIME(6)`.
13. سياسة الحذف: Restrict/Soft Delete للرؤوس، Cascade للتفاصيل المسموح بها، Append-only للتدقيق.
14. جعل EF Migrations المصدر الوحيد للمخطط.
15. نقل SQL التاريخي لاحقًا إلى Legacy بعد استخراج المتطلبات، دون تشغيله على القاعدة النظيفة.
16. إزالة Runtime DDL من `PaymentRequestSchemaInitializer` بعد تغطيته بـMigration.
17. تشغيل Seeders بعد Migration فقط وبصورة idempotent.
18. تحديد مصير Triggerي حماية تفاصيل السند: قاعدة البيانات أم API.

## التحقق النهائي من نطاق التغيير

- الفرق من SHA البداية إلى رأس فرع التدقيق محصور في ملفات Markdown داخل `Database/Reviews` و`Database/Documentation`.
- لم يتغير أي ملف في Core أو Infrastructure أو API أو Desktop أو Mobile.
- لم يُضف أو يُعدل أي ملف SQL أو Migration.
- قاموس البيانات يغطي **47/47 تعريفًا = 100%** و**42/42 Entity = 100%**.

## القيود المؤكدة

- لم تُنشأ `altayer_erp_db_clean`.
- لم تُنشأ أو تُشغّل Migration.
- لم يُنفذ SQL.
- لم يتغير `AppDbContext` أو أي Entity.
- لم يتغير Connection String.
- لم تتغير قاعدة `altayer_erp_db`.
- لم يتم الدمج إلى `master`.

يتوقف العمل هنا وينتظر اعتماد المالك قبل المرحلة الثانية.