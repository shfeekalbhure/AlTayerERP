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
6. Seeders: `SystemScreenCatalogSeeder`, `VoucherReferenceDataSeeder`؛ كلاهما بيانات فقط.

## 4. جرد DDL وقت تشغيل API

### ما لا يوجد

- لا يوجد `Database.EnsureCreated()`.
- لا يوجد `Database.Migrate()` أو `MigrateAsync()`.
- لا يوجد استدعاء `Update-Database` برمجي.
- لا يوجد SchemaInitializer عام مسجل لتأسيس المخطط.

### ما يوجد

`PaymentRequestSchemaInitializer.EnsureCreatedAsync()` ينفذ SQL خامًا لإنشاء `payment_requests`, `payment_request_lines`, `payment_request_attachments`. لكن `Program.cs` الحالي لا يسجله في DI ولا يستدعيه؛ لذلك لا يغير المخطط في مسار التشغيل الحالي، مع بقائه مصدر DDL موازٍ يجب إلغاؤه في المرحلة الثانية.

Seeders تعمل فقط عند `DatabaseBootstrap:EnableReferenceDataSeeding` وتضيف/تحدّث بيانات مرجعية، ولا تنشئ جداول.

## 5. حالة AppDbContext والـSnapshot

- 42 Entity مقابل Migration واحدة.
- `TenantGroup` يحتوي خصائص يتجاهلها Fluent API.
- `AccountCategory` Entity وSQL-only دون DbSet/Migration.
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

## 7. Collation

### الوضع الحالي

- أغلب سكربتات المرحلة الأولى التي تحدد Collation تستخدم `utf8mb4_unicode_ci`.
- Migration القيود تستخدم `utf8mb4` دون Collation صريحة.
- بعض السكربتات لا تحدد Charset/Collation.
- لا يوجد `UseCollation` موحد في EF.

### مقارنة رسمية

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

- إصدار MySQL في التطوير: غير موثق.
- إصدار MySQL في الاختبار: غير موثق.
- إصدار الإنتاج المتوقع: MySQL 8 كهدف معماري، لكنه غير مثبت بتقرير بيئة.
- وجود MariaDB: غير مثبت أو منفي رسميًا.

### التوصية الواحدة

**`utf8mb4_unicode_ci` — تحتاج اعتماد المالك.**

السبب: لا يمكن حاليًا ضمان أن كل البيئات MySQL 8 فقط أو نفي MariaDB، وهي تطابق معظم السكربتات الحالية وتقلل مخاطر نقل البيانات القديمة. يمكن تقييم `utf8mb4_0900_ai_ci` لاحقًا بعد توحيد البيئات وتوثيقها.

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

## 10. القرارات المطلوبة من المالك

1. صيغة `Group_ID` و`Company_ID`.
2. اعتماد `Branch_ID INT` بعد فحص البيانات.
3. اعتماد النصية وأطوال `Account_ID`, `Cost_Center_ID`, `Cash_Box_ID`, `Party_ID`.
4. سياسة حقول التدقيق.
5. اعتماد `utf8mb4_unicode_ci`.
6. اعتماد Migrations كمصدر وحيد للمخطط.
7. مصير Triggers ومنطق الحماية.
8. عزل SQL القديم تحت Legacy بعد إنشاء Baseline.

## 11. التأكيد

لم تُنشأ قاعدة جديدة، ولم تُشغّل Migration، ولم يُنفذ SQL، ولم يتغير AppDbContext أو Entity أو Connection String أو القاعدة القديمة، ولم يتم الدمج إلى `master`.