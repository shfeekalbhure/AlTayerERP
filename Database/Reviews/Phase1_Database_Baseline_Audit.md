# تدقيق Baseline قاعدة بيانات المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA نقطة البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** تدقيق فقط؛ لا Migration جديدة، لا SQL منفذ، لا تغيير اتصال، ولا تعديل للقاعدة القديمة.

## 1. الملخص التنفيذي

لا توجد Baseline قادرة على إنشاء قاعدة المرحلة الأولى كاملة. النموذج الحالي يحتوي **42 Entity مستمرة**، منها **41 DbSet** معلنة في `AppDbContext` وكيان `AccountCategory` مستخدم عبر `Set<AccountCategory>()` دون DbSet صريح. توجد Migration تنفيذية واحدة فقط تنشئ جدولي القيود، بينما يعتمد بقية المخطط على 28 سكربت SQL تاريخيًا ومصدر DDL برمجي غير مفعّل لطلبات الصرف.

## 2. الأعداد النهائية

- Entities المستمرة في `AlTayerERP.Core`: **42**.
- DbSets المعلنة في `AppDbContext`: **41**.
- Entity بلا DbSet صريح: **1 — `AccountCategory`**.
- Migrations التنفيذية: **1 — `20260716223222_AddJournalEntryTables`**.
- ملفات Migration المساندة: Designer واحد وSnapshot واحد.
- ملفات SQL داخل `Database` وما تحته: **28**.
- الجداول التي تنشئها Migration الحالية: **2**.
- الجداول/Entities خارج تغطية Migration الحالية: **40 Entity/Table على الأقل**، إضافة إلى جداول SQL-only.

## 3. جميع مصادر DDL المكتشفة

1. `AppDbContext` وData Annotations وFluent API — نموذج تعريفي، لا ينفذ DDL وحده.
2. Migration `AddJournalEntryTables` — تنشئ `journal_entry_headers`, `journal_entry_details`.
3. 28 ملف SQL — مخططات جزئية، ترقيات، إصلاحات، Seeds، Triggers وتشخيصات كتابية.
4. `PaymentRequestSchemaInitializer` — يحتوي `CREATE TABLE IF NOT EXISTS` لثلاثة جداول عبر `ExecuteSqlRawAsync`.
5. SQL خام في Controllers/Services لقراءة مرجعية أو استعلامات خاصة؛ لم يثبت مصدر DDL آخر دائم غير initializer المذكور.
6. Seeders:
   - `SystemScreenCatalogSeeder`.
   - `VoucherReferenceDataSeeder`.
   كلاهما يضيف/يحدث بيانات فقط ولا ينشئ مخططًا.

## 4. جرد DDL وقت تشغيل API

### ما لا يوجد

- لا يوجد `Database.EnsureCreated()`.
- لا يوجد `Database.Migrate()` أو `MigrateAsync()`.
- لا يوجد استدعاء `Update-Database` برمجي.
- لا يوجد SchemaInitializer عام مسجل لتأسيس المخطط.

### ما يوجد

`PaymentRequestSchemaInitializer.EnsureCreatedAsync()` ينفذ SQL خامًا لإنشاء:

- `payment_requests`.
- `payment_request_lines`.
- `payment_request_attachments`.

لكن `Program.cs` الحالي لا يسجله في DI ولا يستدعيه عند الإقلاع؛ لذلك **لا يغير المخطط في مسار التشغيل الحالي**. وجوده يبقى مصدر DDL موازٍ وخطر Drift يجب إزالته أو تحويله إلى Migration في المرحلة الثانية.

### Seeders

تشغيل Seeders مشروط بـ`DatabaseBootstrap:EnableReferenceDataSeeding`. وظيفتها بيانات مرجعية فقط، ويجب أن تفشل بوضوح إذا كان المخطط غير موجود بدل إنشائه.

## 5. حالة AppDbContext والـSnapshot

### نقاط إيجابية

- أسماء معظم الجداول والمفاتيح الأساسية معرفة.
- بعض العلاقات والفهارس والـPrecision معرفة للسندات والقيود وطلبات الصرف.
- وحدات النظام مفصولة منطقيًا.

### Drift يمنع Baseline

- 42 Entity مقابل Migration واحدة.
- `TenantGroup` يحتوي خصائص كثيرة يتجاهلها Fluent API.
- `AccountCategory` موجود كEntity وSQL-only دون DbSet/Migration.
- لا توجد سياسة عامة للـCharset/Collation.
- نصوص كثيرة بلا `MaxLength` وتظهر `longtext` في Snapshot.
- علاقات FK غير مكتملة.
- Snapshot لا يمثل مصدرًا تاريخيًا صالحًا لإنشاء كل المخطط.

## 6. إثبات فجوة Migrations

### تنشئها Migration الحالية

- `journal_entry_headers`.
- `journal_entry_details`.

### موجودة في EF/Snapshot أو DbSets ولا تنشئها Migration

- الهيكل المؤسسي: `tenant_groups`, `companies`, `tenant_branches`, `fiscal_years`.
- الأمن: `users`, `roles`, `role_permissions`, `user_permissions`, `system_permissions`, `system_screens`, `login_attempts`, `refresh_tokens`.
- الإعدادات: `system_settings`, `fiscal_periods`, `exchange_rates`, `numbering_settings`, `numbering_counters`.
- السياسات: `financial_limits`, `financial_limit_movements`, `approval_requests`.
- الأدلة: `account_code_settings`, `chart_of_accounts`, `cost_centers`, `cash_boxes`, `currencies`, `bank_accounts`, `account_categories`.
- المحرك المالي: `financial_voucher_headers`, `financial_voucher_details`, `document_allocations`, `document_links`, `audit_logs`, `voucher_action_logs`.
- المرجعيات: `voucher_types`, `voucher_statuses`, `payment_methods`, `parties`.
- طلبات الصرف: `payment_requests`, `payment_request_lines`, `payment_request_attachments`.

### SQL-only أو غير مغطاة صراحة في EF Model

- `countries`, `governorates`, `cities`.
- `branch_types`.
- `database_alignment_findings` — جدول تشخيصي تاريخي لا يجب أن يدخل Baseline.
- Triggers حماية تفاصيل السند في سكربت دليل الحسابات.

### يعتمد عليها النظام وقت التشغيل دون Migration

- جميع الجداول أعلاه تقريبًا، خصوصًا المستخدمين والجلسات والشركات والفروع والعملات والحسابات والسندات وطلبات الصرف.

## 7. Collation

### الوضع الحالي

- سكربتات كثيرة: `utf8mb4_unicode_ci`.
- Migration القيود: `utf8mb4` دون Collation صريحة.
- سكربتات أخرى لا تحدد Charset/Collation.
- لا يوجد `UseCollation` موحد في EF.

### المقارنة الرسمية

| المعيار | `utf8mb4_0900_ai_ci` | `utf8mb4_unicode_ci` |
|---|---|---|
| MySQL 8 | أصلي وأحدث | مدعوم |
| MySQL 5.7 | غير مدعوم | مدعوم |
| MariaDB | غير متوافق عادة | أوسع توافقًا |
| العربية | دعم جيد جدًا وقواعد Unicode أحدث | دعم جيد ومستقر |
| الفهارس | يتطلب إعادة بناء عند النقل من Collation أخرى | أقل تغييرًا للسكربتات الحالية |
| نقل البيانات القديمة | يحتاج فحص مساواة/ترتيب وفهارس | أقل مخاطرة مع الوضع الحالي |
| الأدوات الحالية | مناسب إذا كانت كلها MySQL 8 | أكثر تسامحًا مع بيئات مختلطة |

### الإصدارات

- جهاز التطوير: **غير موثق داخل المستودع**.
- بيئة الاختبار: **غير موثقة**.
- الإنتاج المتوقع: **MySQL 8 بحسب قرارات المشروع، لكنه غير مثبت بتقرير بيئة**.
- MariaDB: **لا يوجد دليل كودي على استخدامها، لكن عدم وجودها غير مثبت رسميًا**.

### التوصية الواحدة

**التوصية: `utf8mb4_0900_ai_ci`، حالتها: تحتاج اعتماد المالك.**

السبب: المشروع يصرح باستهداف MySQL 8، والقاعدة الجديدة فرصة لإنشاء مخطط موحد بخوارزمية Unicode أحدث. لا يعتمد القرار قبل توثيق أن التطوير والاختبار والإنتاج كلها MySQL 8 ولا تستخدم MariaDB.

## 8. أخطر اختلافات المفاتيح

- `Branch_ID`: `int` في المرجع والجلسة، نصي في السندات والقيود.
- `User_ID`: رقمي، بينما حقول تدقيق كثيرة نصية.
- `Group_ID`, `Company_ID`, `Account_ID`, `Cost_Center_ID`, `Cash_Box_ID`, `Party_ID`: نصية وتحتاج أطوالًا وCollation موحدة.
- `Voucher_ID`, `Payment_Request_ID`, `Journal_Entry_ID`: `long/BIGINT` متوافقة.

## 9. خطة Baseline المقترحة — غير منفذة

1. اعتماد القرارات المفتوحة.
2. تثبيت 42 Entity والجداول SQL-only المطلوبة.
3. استكمال Fluent API لكل الأعمدة والعلاقات والقيود.
4. إزالة مصادر DDL وقت التشغيل من المسار النهائي.
5. فصل Seed Data.
6. إنشاء Migration Baseline واحدة على الفرع المعزول بعد موافقة المالك.
7. توليد SQL للمراجعة دون تنفيذ.
8. بعد اعتماد مستقل: إنشاء قاعدة جديدة واختبار إعادة الإنشاء مرتين.

## 10. المخاطر

- توليد Migration من Snapshot الحالي قد ينتج حذفًا أو تغييرات غير مقصودة.
- تشغيل ملفات SQL التاريخية بترتيب مختلف ينتج مخططات مختلفة.
- DDL المكرر لطلبات الصرف والقيود يخلق Drift.
- تحويل Branch_ID أو User audit fields يحتاج ترحيل بيانات.
- Collation غير الموحدة تمنع FKs النصية أو تغير نتائج Unique.

## 11. القرارات المطلوبة من المالك

1. صيغة `Group_ID` و`Company_ID`.
2. اعتماد `Branch_ID INT` بعد فحص البيانات.
3. اعتماد النصية وأطوال `Account_ID`, `Cost_Center_ID`, `Cash_Box_ID`, `Party_ID`.
4. سياسة حقول التدقيق: FK رقمية + Snapshot أم نص فقط.
5. اعتماد `utf8mb4_0900_ai_ci` بعد توثيق البيئات.
6. اعتماد Migrations كمصدر وحيد للمخطط.
7. مصير Triggers ومنطق الحماية: قاعدة أم API.
8. عزل SQL القديم تحت Legacy بعد إنشاء Baseline.

## 12. التأكيد

- لم تُنشأ `altayer_erp_db_clean`.
- لم تُشغّل Migration.
- لم يُنفذ SQL.
- لم يتغير AppDbContext أو أي Entity.
- لم يتغير Connection String.
- لم تتغير القاعدة القديمة.
- لم يتم الدمج إلى `master`.