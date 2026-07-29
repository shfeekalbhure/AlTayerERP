# تقرير انحراف مخطط قاعدة البيانات

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** تدقيق فقط؛ لا Migration ولا SQL منفذ ولا تعديل قاعدة.

## 1. نطاق المقارنة

تمت المقارنة بين:

- 42 Entity مستمرة في `AlTayerERP.Core`.
- 41 `DbSet` معلنة و`AccountCategory` المستخدم عبر `Set<AccountCategory>()`.
- `OnModelCreating` وData Annotations.
- `AppDbContextModelSnapshot`.
- Migration التنفيذية الوحيدة.
- 28 ملف SQL.
- SQL الخام وSchemaInitializer وSeeders.
- الاستخدام في API وDesktop وMobile.

## 2. الانحرافات الرئيسية المؤكدة

### 2.1 فجوة Migrations

- Migration تنفيذية واحدة فقط: `AddJournalEntryTables`.
- تنشئ جدولين فقط: `journal_entry_headers`, `journal_entry_details`.
- 39 DbSet أخرى لا تملك Migration إنشاء.
- `AccountCategory` لا يملك DbSet صريحًا ولا Migration.
- الجغرافيا و`branch_types` جداول SQL-only.

**النتيجة:** لا يمكن إنشاء قاعدة النظام من Migrations الحالية.

### 2.2 اختلاف النموذج عن Snapshot

- Snapshot يحتوي نماذج أوسع من Migration ولكنه غير متزامن مع النموذج الحالي.
- نصوص كثيرة تظهر `longtext` لغياب أطوال صريحة.
- لا توجد Collation عامة.
- لا يجوز توليد Migration جديدة مباشرة من Snapshot الحالي.

### 2.3 `TenantGroup`

- Entity يحتوي خصائص موسعة وتدقيقًا وحالة.
- Fluent API يتجاهل عددًا كبيرًا منها.
- عدة سكربتات تضيف أو تعدل هذه الخصائص تاريخيًا.

**القرار المطلوب:** نموذج نهائي واحد يحدد ما هو مستمر وما هو ViewModel/NotMapped.

### 2.4 أنواع المفاتيح

- `Branch_ID`: `int` في المرجع والجلسة والطلبات، و`string/varchar(50)` في السندات والقيود.
- `User_ID`: `int` بينما حقول التدقيق والترحيل كثيرة منها نصية.
- IDs النصية الأخرى تحتاج طولًا وCollation موحدين قبل FK.

### 2.5 مصادر DDL المكررة

- القيود: Migration + Restore SQL.
- طلبات الصرف: SQL + `PaymentRequestSchemaInitializer`.
- الحسابات البنكية: ملف مستقل + compatibility + alignment.
- الإعدادات الأربعة: ملفات مستقلة + compatibility.

### 2.6 SQL-only وTriggers

- SQL-only: `countries`, `governorates`, `cities`, `branch_types`.
- `account_categories`: Entity بلا DbSet صريح وDDL/Seed عبر SQL.
- Triggerان لحماية تفاصيل السند موجودان في SQL ولا يمثلان في EF/Migrations.
- جدول `database_alignment_findings` تشخيصي كتابي ولا يجب أن يدخل Baseline.

### 2.7 Collation

- ملفات متعددة تستخدم `utf8mb4_unicode_ci`.
- Migration القيود تحدد `utf8mb4` فقط.
- بعض الملفات لا تحدد Collation.
- لا يوجد `UseCollation` في EF.

## 3. Drift حسب الوحدة

| الوحدة | EF | Migration | SQL/Runtime | مستوى Drift |
|---|---|---|---|---|
| الهيكل المؤسسي | موجود | لا | ترقيات متعددة | مرتفع |
| الجغرافيا | لا DbSets | لا | SQL-only | مرتفع |
| المستخدمون والأمن | موجود | لا | SQL أمن + Seeders | مرتفع |
| الترقيم والإعدادات | موجود | لا | ملفات متكررة | مرتفع |
| دليل الحسابات | موجود | لا | SQL + Triggers + Seed | مرتفع |
| الصناديق والبنوك | موجود | لا | SQL متكرر | مرتفع |
| السندات المالية | موجود | لا | ترقيات تاريخية | مرتفع |
| القيود اليومية | موجود | نعم | Restore مكرر | متوسط/متعارض |
| طلبات الصرف | موجود | لا | SQL + Runtime Initializer | مرتفع/متعارض |
| التدقيق والرقابة | موجود | لا | Check/SQL تاريخي | مرتفع |

## 4. DDL وقت التشغيل

### غير موجود

- `Database.EnsureCreated()`.
- `Database.Migrate()` أو `MigrateAsync()`.
- استدعاء آلي لـUpdate-Database.

### موجود في الكود

`PaymentRequestSchemaInitializer.EnsureCreatedAsync()` يحتوي `CREATE TABLE IF NOT EXISTS` للجداول:

- `payment_requests`.
- `payment_request_lines`.
- `payment_request_attachments`.

وينفذه عبر `ExecuteSqlRawAsync`. لكنه **غير مسجل وغير مستدعى في `Program.cs` الحالي**، لذلك ليس نشطًا في مسار الإقلاع الحالي. مع ذلك هو مصدر DDL موازٍ يجب إلغاؤه أو تحويله إلى Migration بعد اعتماد المرحلة الثانية.

### Seeders

- `SystemScreenCatalogSeeder`.
- `VoucherReferenceDataSeeder`.

وظيفتهما DML مرجعي فقط، ولا تنشئان جداول.

## 5. إثبات فجوة Migrations

### تنشئها Migration

- `journal_entry_headers`.
- `journal_entry_details`.

### موجودة في EF/Snapshot ولا تنشئها Migration

- `tenant_groups`, `companies`, `tenant_branches`, `fiscal_years`.
- `users`, `roles`, `role_permissions`, `user_permissions`, `system_permissions`, `system_screens`, `login_attempts`, `refresh_tokens`.
- `system_settings`, `fiscal_periods`, `exchange_rates`, `numbering_settings`, `numbering_counters`.
- `financial_limits`, `financial_limit_movements`, `approval_requests`.
- `account_code_settings`, `chart_of_accounts`, `cost_centers`, `cash_boxes`, `currencies`, `bank_accounts`.
- `financial_voucher_headers`, `financial_voucher_details`, `document_allocations`, `document_links`, `audit_logs`, `voucher_action_logs`.
- `voucher_types`, `voucher_statuses`, `payment_methods`, `parties`.
- `payment_requests`, `payment_request_lines`, `payment_request_attachments`.

### SQL-only/خارج DbSets الصريحة

- `countries`, `governorates`, `cities`, `branch_types`.
- `account_categories` يستخدم عبر `Set<>` دون DbSet.
- `database_alignment_findings` تشخيصي مستبعد.

## 6. المخاطر

1. قاعدة منشأة من Migration الحالية ستكون ناقصة.
2. ترتيب تشغيل SQL التاريخي يغير النتيجة بين البيئات.
3. اختلاف المفاتيح يمنع FKs ويجبر CAST في الاستعلامات.
4. اختلاف Collation قد يفشل FK/Unique أو يغير نتائج المقارنة.
5. DDL وقت التشغيل يخلق مخططًا خارج سجل Migrations.
6. Triggers التاريخية قد تكرر قواعد موجودة في API.

## 7. إغلاق Drift المقترح — بعد اعتماد المرحلة الثانية فقط

1. اعتماد القرارات المفتوحة.
2. تثبيت Data Dictionary النهائي.
3. استكمال Fluent API.
4. جعل Migrations المصدر الوحيد للمخطط.
5. فصل Seed Data.
6. إزالة Runtime DDL.
7. توليد Baseline ومقارنتها بالقاموس دون تنفيذ.
8. اختبار إنشاء قاعدتين فارغتين متطابقتين بعد اعتماد مستقل.

## 8. التأكيد

التقرير توثيقي فقط. لم يُنفذ SQL، ولم تتغير Entities أو AppDbContext أو القاعدة القديمة.