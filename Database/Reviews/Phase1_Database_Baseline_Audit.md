# تدقيق Baseline قاعدة بيانات المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA نقطة البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**حالة المرحلة:** تدقيق فقط؛ لا Migration جديدة، لا SQL منفذ، لا تغيير اتصال، ولا تعديل للقاعدة القديمة.

## 1. الملخص التنفيذي

المنظومة الحالية لا تملك Baseline قابلة لإنشاء قاعدة المرحلة الأولى كاملة من الصفر. `AppDbContext` يحتوي 41 `DbSet`، بينما مجلد Migrations يحتوي Migration تنفيذية واحدة فقط (`AddJournalEntryTables`) تنشئ جدولي `journal_entry_headers` و`journal_entry_details`. بقية المخطط يعتمد على قاعدة موجودة مسبقاً وسكربتات SQL تراكمية وتهيئة مرجعية اختيارية وقت تشغيل API.

## 2. الأرقام المؤكدة من الكود

- عدد `DbSet` في `AppDbContext`: **41**.
- عدد Migrations التنفيذية: **1**.
- ملفات Migration المساندة: Designer واحد و`AppDbContextModelSnapshot` واحد.
- عدد Entities المستمرة المتوقع من DbSets: **41**؛ توجد خصائص إضافية `Ignored/NotMapped` لا تدخل المخطط.
- ملفات SQL المؤكدة عبر بحث المحتوى: **9 على الأقل**. لا يُدّعى أن هذا هو العدد النهائي لأن موصل GitHub الحالي لا يتيح سرداً شجرياً كاملاً للمجلد؛ يجب تثبيت العدد النهائي في المرحلة الثانية عبر clone/read-only tree export قبل اعتماد Baseline.

## 3. مصادر تعريف أو تغيير المخطط

1. `AlTayerERP.Infrastructure/Data/AppDbContext.cs`.
2. `AlTayerERP.Infrastructure/Migrations/20260716223222_AddJournalEntryTables.cs`.
3. `AppDbContextModelSnapshot.cs`، وهو غير ممثل لكل DbSets الحالية.
4. سكربتات `Database/*.sql` و`Database/Scripts/*.sql`.
5. SQL نصي عبر `DbConnection/CreateCommand/CommandText` داخل بعض Controllers/Services.
6. Seeders وقت التشغيل:
   - `SystemScreenCatalogSeeder`.
   - `VoucherReferenceDataSeeder`.
7. `Program.cs` لا يستدعي `EnsureCreated` أو `Migrate` أو `Update-Database` آلياً.

## 4. حالة AppDbContext

### نقاط إيجابية

- أسماء الجداول الرئيسية محددة في Fluent API.
- يوجد فصل واضح لوحدات الهيكل الإداري، الأمن، الإعدادات، المحاسبة والسندات.
- بعض العلاقات والفهارس والـprecision معرفة في Fluent API.

### نقاط ضعف تمنع Baseline

- 41 DbSet مقابل Migration واحدة فقط.
- `TenantGroup` يحتوي خصائص كثيرة يتجاهلها Fluent API لأنها لا توجد في جدول المرحلة الحالية، ما يثبت drift بين نموذج المجال والقاعدة.
- لا توجد سياسة عامة موحدة لـcharset/collation في `OnModelCreating`.
- أطوال نصوص كثيرة تعتمد على Convention أو تظهر `longtext` في Snapshot.
- العلاقات الخارجية ليست معرفة بصورة كاملة لكل المفاتيح المرجعية.
- بعض الجداول تُقرأ أو تُنشأ/تُرقع بواسطة SQL يدوي بدل Migration.

## 5. حالة Migrations

Migration الوحيدة تنشئ:

- `journal_entry_headers`.
- `journal_entry_details`.

وتستخدم:

- `Journal_Entry_ID`: `bigint`.
- `Branch_ID`: `varchar(50)`.
- `Fiscal_Year_ID`: `int?`.
- `Account_ID`: `varchar(50)`.
- `Cost_Center_ID`: `varchar(50)?`.
- `Currency_ID`: `int`.
- مبالغ `decimal(18,2)` وسعر صرف `decimal(18,6)`.
- Cascade من تفاصيل القيد إلى رأس القيد.

لا تنشئ Migration الحالية الجداول المؤسسية أو الأمنية أو المرجعية أو معظم جداول المحرك المالي.

## 6. التشغيل وقت إقلاع API

`Program.cs`:

- يحمل Connection String من User Secrets أو Environment أو `appsettings.Local.json` الاختياري.
- لا يعرض الأسرار في الملفات المتتبعة.
- لا ينفذ `Database.Migrate()` أو `EnsureCreated()`.
- يمكنه تشغيل Seeder للشاشات ومراجع السندات عند تفعيل `DatabaseBootstrap:EnableReferenceDataSeeding`.
- Seeders قد تعدل بيانات مرجعية، لكنها ليست مصدراً مقبولاً لإنشاء المخطط.

## 7. Collation

### الوضع الحالي

- بعض سكربتات SQL تعتمد `utf8mb4_unicode_ci`.
- Migration القيود تضع `utf8mb4` فقط دون Collation صريحة.
- Snapshot لا يثبت Collation عامة موحدة.
- لا يوجد دليل كودي أن كل الجداول والأعمدة النصية متطابقة.

### المقارنة

| المعيار | `utf8mb4_0900_ai_ci` | `utf8mb4_unicode_ci` |
|---|---|---|
| MySQL 8 | أصلي ومفضل | مدعوم |
| MySQL 5.7 | غير مدعوم | مدعوم |
| MariaDB | غير متوافق عادة | أوسع توافقاً |
| العربية | جيد جداً | جيد |
| دقة Unicode | أحدث Unicode Collation Algorithm | أقدم |
| أثر السكربتات الحالية | يحتاج تعديل جميع تعريفات Collation | يطابق عدداً من السكربتات الحالية |
| نقل بيانات قديمة | يحتاج فحص اختلاف المقارنات والفهارس | أقل تغييراً |

### القرار المقترح غير المنفذ

- إن ثبت أن كل البيئات MySQL 8 فقط: التوصية الرسمية `utf8mb4_0900_ai_ci`.
- إن وجدت MariaDB أو MySQL أقدم: التوصية `utf8mb4_unicode_ci`.
- القرار معلق حتى توثيق إصدارات كل بيئات التطوير والإنتاج.

## 8. الجداول غير القابلة للإنشاء من Migrations الحالية

كل DbSets التالية باستثناء جدولي القيود لا تملك Migration تنفيذية تغطي إنشاءها بالكامل، ومنها:

- `tenant_groups`, `companies`, `tenant_branches`, `fiscal_years`.
- `users`, `roles`, `role_permissions`, `user_permissions`, `system_permissions`, `system_screens`.
- `login_attempts`, `refresh_tokens`.
- `system_settings`, `fiscal_periods`, `exchange_rates`.
- `numbering_settings`, `numbering_counters`.
- `financial_policies`, `financial_policy_movements`, `approval_requests`.
- `account_code_settings`, `chart_of_accounts`, `cost_centers`, `cash_boxes`, `currencies`, `bank_accounts`.
- `financial_voucher_headers`, `financial_voucher_details`, `document_allocations`, `document_links`.
- `audit_logs`, `voucher_action_logs`, `voucher_types`, `voucher_statuses`, `payment_methods`, `parties`.
- `payment_requests`, `payment_request_lines`, `payment_request_attachments`.

## 9. المخاطر

1. إنشاء قاعدة من Migration الحالية ينتج قاعدة ناقصة.
2. تشغيل السكربتات التاريخية بترتيب خاطئ قد ينتج drift مختلفاً بين الأجهزة.
3. اختلاف أنواع المفاتيح النصية والعددية يمنع علاقات FK سليمة.
4. اختلاف Collation في مفاتيح نصية قد يسبب أخطاء مقارنة أو FK.
5. Snapshot غير متزامن يجعل توليد Migration جديدة مباشرة خطراً.
6. Seeders قد تفشل إذا لم يكن المخطط المرجعي موجوداً أولاً.

## 10. الخطة المقترحة للمرحلة الثانية — بعد الاعتماد فقط

1. تثبيت قائمة Entities والجداول النهائية.
2. اعتماد أنواع المفاتيح.
3. اعتماد Collation بعد توثيق البيئات.
4. استكمال Fluent API: الأطوال، precision، defaults، indexes، unique، FK، delete behavior.
5. فصل Seed Data عن DDL.
6. إنشاء Baseline واحدة في فرع التدقيق بعد حذف تاريخ Migrations من نسخة العمل فقط، دون المساس بالفرع الأصلي.
7. توليد SQL idempotent للمراجعة، دون تنفيذه.
8. بعد اعتماد SQL فقط: إنشاء قاعدة جديدة وتطبيق Baseline واختبارها.

## 11. القرارات المطلوبة من المالك

- اعتماد نوع `Group_ID` و`Company_ID` النهائي: نصي أم عددي.
- اعتماد نوع `Branch_ID` النهائي؛ حالياً يظهر `int` في كيانات متعددة و`varchar(50)` في Migration القيود.
- اعتماد نوع `Account_ID` و`Cost_Center_ID`: نصي أم رقمي.
- تحديد ما إذا كانت جميع البيئات MySQL 8 حصراً.
- اعتماد Collation النهائية.
- تحديد السكربتات التاريخية التي تعد مرجعاً وظيفياً فقط ولا تدخل Baseline.

## 12. تأكيد القيود

- لم تُنشأ `altayer_erp_db_clean`.
- لم تُشغّل Migration.
- لم يُنفذ SQL.
- لم يتغير أي Connection String.
- لم تتغير القاعدة القديمة.
- لم يتم الدمج إلى `master`.
