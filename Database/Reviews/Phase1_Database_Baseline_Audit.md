# تدقيق Baseline قاعدة بيانات المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA نقطة البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** **مرحلة التدقيق الأولى مكتملة توثيقيًا، ومتوقفة بانتظار اعتماد المالك قبل المرحلة الثانية.**

## 1. الملخص التنفيذي

لا توجد Baseline قادرة على إنشاء قاعدة المرحلة الأولى كاملة. النموذج الحالي يحتوي **42 Entity مستمرة**، منها **41 DbSet** معلنة في `AppDbContext` وكيان `AccountCategory` مستخدم عبر `Set<AccountCategory>()` دون DbSet صريح. توجد Migration تنفيذية واحدة فقط تنشئ جدولي القيود، بينما يعتمد بقية المخطط على **28 ملف SQL** تاريخيًا ومصدر DDL برمجي غير مفعّل لطلبات الصرف.

## 2. الأعداد النهائية

- Entities المستمرة في `AlTayerERP.Core`: **42**.
- DbSets المعلنة في `AppDbContext`: **41**.
- Entity بلا DbSet صريح: **1 — `AccountCategory`**.
- Migrations التنفيذية: **1 — `20260716223222_AddJournalEntryTables`**.
- ملفات Migration المساندة: Designer واحد وSnapshot واحد.
- ملفات SQL داخل `Database` وما تحته: **28**.
- الجداول التي تنشئها Migration الحالية: **2**.
- قاموس البيانات: **46 تعريف جدول/كيان تشغيلي موثق من 46 = 100%**؛ جدول التشخيص `database_alignment_findings` موثق في التصنيف والخريطة ومستبعد صراحة من Baseline.

## 3. مصادر تعريف أو تغيير المخطط

1. `AppDbContext` وData Annotations وFluent API — نموذج تعريفي، لا ينفذ DDL وحده.
2. Migration `AddJournalEntryTables` — تنشئ `journal_entry_headers`, `journal_entry_details`.
3. 28 ملف SQL — مخططات جزئية، ترقيات، إصلاحات، Seeds، Triggers وتشخيصات كتابية.
4. `PaymentRequestSchemaInitializer` — يحتوي `CREATE TABLE IF NOT EXISTS` لثلاثة جداول عبر `ExecuteSqlRawAsync`.
5. SQL خام في Controllers/Services للقراءة أو الاستعلامات الخاصة؛ لم يثبت مصدر DDL دائم آخر.
6. `SystemScreenCatalogSeeder` و`VoucherReferenceDataSeeder` — DML مرجعي فقط.

## 4. جرد DDL وقت تشغيل API

### غير موجود

- `Database.EnsureCreated()`.
- `Database.Migrate()` أو `MigrateAsync()`.
- استدعاء برمجي لـ`Update-Database`.
- SchemaInitializer عام مسجل لتأسيس المخطط.

### موجود ولكن غير مفعّل حاليًا

`PaymentRequestSchemaInitializer.EnsureCreatedAsync()` ينفذ SQL خامًا لإنشاء:

- `payment_requests`.
- `payment_request_lines`.
- `payment_request_attachments`.

لكن `Program.cs` الحالي لا يسجله في DI ولا يستدعيه، لذلك لا يغير المخطط في مسار الإقلاع الحالي. يبقى مصدر DDL موازٍ يجب إزالته أو تحويله إلى فاحص قراءة فقط بعد تغطية الجداول بـMigration في المرحلة الثانية.

### Seeders

تشغيل Seeders مشروط بـ`DatabaseBootstrap:EnableReferenceDataSeeding`. وظيفتها إدخال/تحديث بيانات مرجعية فقط، ولا تنشئ مخططًا.

## 5. حالة AppDbContext والـSnapshot

- 42 Entity مقابل Migration واحدة.
- `TenantGroup` يحتوي خصائص يتجاهلها Fluent API.
- `AccountCategory` Entity دون DbSet صريح أو Migration، وDDL/Seed عبر SQL.
- لا توجد سياسة عامة للـCharset/Collation.
- نصوص كثيرة بلا `MaxLength` وتظهر `longtext` في Snapshot.
- علاقات FK غير مكتملة.
- Snapshot لا يمثل تاريخ إنشاء صالحًا للمخطط الكامل.

## 6. إثبات فجوة Migrations

### تنشئها Migration الحالية

- `journal_entry_headers`.
- `journal_entry_details`.

### لا تنشئها Migration الحالية

- الهيكل المؤسسي: `tenant_groups`, `companies`, `tenant_branches`, `fiscal_years`.
- الأمن: `users`, `roles`, `role_permissions`, `user_permissions`, `system_permissions`, `system_screens`, `login_attempts`, `refresh_tokens`.
- الإعدادات: `system_settings`, `fiscal_periods`, `exchange_rates`, `numbering_settings`, `numbering_counters`.
- السياسات: `financial_limits`, `financial_limit_movements`, `approval_requests`.
- الأدلة: `account_code_settings`, `chart_of_accounts`, `cost_centers`, `cash_boxes`, `currencies`, `bank_accounts`, `account_categories`.
- المحرك المالي: `financial_voucher_headers`, `financial_voucher_details`, `document_allocations`, `document_links`, `audit_logs`, `voucher_action_logs`.
- المرجعيات: `voucher_types`, `voucher_statuses`, `payment_methods`, `parties`.
- طلبات الصرف: `payment_requests`, `payment_request_lines`, `payment_request_attachments`.

### SQL-only أو غير مغطاة صراحة في EF Model

- `countries`, `governorates`, `cities`, `branch_types`.
- `database_alignment_findings` — تشخيص تاريخي لا يدخل Baseline.
- Triggerا حماية تفاصيل السند في سكربت دليل الحسابات.

## 7. نتيجة تحليل `Branch_ID`

- المرجع المؤسسي والجلسة والمستخدم وطلبات الصرف تستخدم `INT`.
- السندات المالية والقيود تستخدم `string/VARCHAR(50)`.
- هذا يمنع FK مباشرة ويؤدي إلى استخدام `CAST(... AS UNSIGNED)`.
- لا يمكن إثبات خلو القاعدة القديمة من قيم غير رقمية دون قراءة بيانات، ولم يُنفذ أي استعلام عليها.
- **التوصية المقترحة:** توحيد `Branch_ID` إلى `INT` مع إبقاء `Branch_Code` نصيًا، وحالتها تحتاج اعتماد المالك وفحص بيانات مستقبلًا على نسخة آمنة.

## 8. Collation

### الوضع الحالي

- أغلب السكربتات التي تحدد Collation تستخدم `utf8mb4_unicode_ci`.
- Migration القيود تستخدم `utf8mb4` دون Collation صريحة.
- بعض السكربتات لا تحدد Charset/Collation.
- لا يوجد `UseCollation` موحد في EF.

### المقارنة الرسمية

| المعيار | `utf8mb4_0900_ai_ci` | `utf8mb4_unicode_ci` |
|---|---|---|
| MySQL 8 | أصلي وأحدث | مدعوم |
| MySQL 5.7 | غير مدعوم | مدعوم |
| MariaDB | غير متوافق عادة | أوسع توافقًا |
| العربية | قواعد Unicode أحدث | دعم جيد ومستقر |
| الفهارس | يحتاج إعادة بناء عند النقل | أقل تغييرًا للوضع الحالي |
| البيانات القديمة | احتمال اختلاف مساواة/ترتيب أكبر | أقل مخاطرة في النقل الحالي |
| الأدوات الحالية | مناسب عند ضمان MySQL 8 فقط | أكثر توافقًا مع بيئات غير موثقة |

### حالة البيئات

- إصدار MySQL في التطوير: **غير موثق**.
- إصدار MySQL في الاختبار: **غير موثق**.
- الإنتاج المتوقع: MySQL 8 كهدف معماري، لكنه غير مثبت بتقرير بيئة.
- وجود MariaDB: غير مثبت أو منفي رسميًا.

### التوصية الواحدة

**`utf8mb4_unicode_ci` — تحتاج اعتماد المالك.**

السبب: لا يمكن ضمان أن كل البيئات MySQL 8 فقط أو نفي MariaDB، وهي تطابق غالبية السكربتات الحالية وتقلل مخاطر نقل البيانات القديمة. يمكن تقييم `utf8mb4_0900_ai_ci` لاحقًا بعد توحيد البيئات وتوثيقها.

## 9. التقارير المكتملة

1. `Database/Reviews/Phase1_Database_Baseline_Audit.md`.
2. `Database/Reviews/Database_Schema_Drift_Report.md`.
3. `Database/Reviews/Phase1_Key_Type_Mismatch_Report.md`.
4. `Database/Reviews/Phase1_SQL_Scripts_Classification.md`.
5. `Database/Reviews/Branch_ID_Normalization_Assessment.md`.
6. `Database/Documentation/Phase1_Table_Dependency_Map.md`.
7. `Database/Documentation/Phase1_Data_Dictionary_Draft.md`.
8. `Database/Documentation/Database_Decision_Log.md`.

## 10. القرارات التي تحتاج اعتماد المالك

1. صيغة وطول `Group_ID` و`Company_ID`.
2. اعتماد `Branch_ID INT` بعد فحص القيم القديمة.
3. اعتماد النصية وأطوال `Account_ID`, `Cost_Center_ID`, `Cash_Box_ID`, `Party_ID`.
4. سياسة IDs المرجعية: رقم داخلي وكود أعمال منفصل.
5. سياسة حقول التدقيق: FK رقمية + Snapshot.
6. اعتماد `utf8mb4_unicode_ci`.
7. اعتماد `snake_case lowercase` لأسماء الجداول والأعمدة.
8. سياسة UTC والتواريخ المحاسبية.
9. سياسة الحذف والإيقاف المنطقي.
10. Migrations كمصدر وحيد للمخطط.
11. مصير SQL التاريخي وMigrations القديمة.
12. إلغاء Runtime DDL ومصير `PaymentRequestSchemaInitializer`.
13. نطاق Seeders والبيانات المرجعية.
14. مصير Triggers وقواعد الحماية داخل قاعدة البيانات.

## 11. خطة المرحلة الثانية المقترحة — غير منفذة

1. اعتماد القرارات المفتوحة.
2. تثبيت النموذج النهائي لكل Entity والجداول SQL-only المطلوبة.
3. استكمال Fluent API لكل الأعمدة والعلاقات والقيود.
4. إزالة مصادر DDL وقت التشغيل من المسار النهائي.
5. فصل Seed Data.
6. إنشاء Migration Baseline واحدة على الفرع المعزول بعد موافقة المالك.
7. توليد SQL للمراجعة دون تنفيذ.
8. بعد اعتماد مستقل: إنشاء قاعدة جديدة واختبار إعادة الإنشاء مرتين.

## 12. شروط الإغلاق المتحققة

- العدد النهائي للـEntities مثبت: **42**.
- العدد النهائي للـDbSets مثبت: **41**.
- العدد النهائي للـMigrations مثبت: **1**.
- العدد النهائي لملفات SQL مثبت: **28**.
- جميع التقارير المطلوبة موجودة ومكتملة.
- قاموس البيانات يغطي 100% من التعريفات التشغيلية المجرودة.
- خريطة العلاقات والسياسات مكتملة توثيقيًا.
- اختلافات المفاتيح مصححة.
- `Branch_ID` محلل في تقرير مستقل.
- توصية Collation موحدة ومعلقة لاعتماد المالك.
- كل ملفات SQL مصنفة.
- القرارات المفتوحة محددة.
- لم تحدث تغييرات برمجية أو قاعدة بيانات.

## 13. التأكيد النهائي

- لم تُنشأ `altayer_erp_db_clean`.
- لم تُشغّل أو تُنشأ Migration.
- لم يُنفذ SQL.
- لم يتغير `AppDbContext` أو أي Entity.
- لم يتغير أي Connection String.
- لم تُعدل القاعدة القديمة `altayer_erp_db`.
- لم يتم الدمج إلى `master`.

**توقفت المرحلة عند هذا التقرير بانتظار اعتماد المالك قبل بدء المرحلة الثانية.**