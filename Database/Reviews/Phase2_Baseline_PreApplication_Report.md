# تقرير Baseline قبل التطبيق — المرحلة الثانية

**الحالة:** مولدة ومفحوصة دون إنشاء قاعدة أو تنفيذ SQL على MySQL.  
**اسم Baseline:** `Baseline_Phase1`  
**Character Set:** `utf8mb4`  
**Collation:** `utf8mb4_unicode_ci`

## النتائج العددية

- عدد الجداول الوظيفية: **48**.
- عدد علاقات Foreign Key في السكربت: **81**.
- عدد أوامر إنشاء الفهارس: **115**.
- عدد ظهور القيود/الفهارس الفريدة: **43**.
- عدد Check Constraints: **17**.
- عدد ملفات Migration/Snapshot النشطة: **3**.

## ملاحظة تحقق العدد

- العدد الفعلي المستخرج من أوامر `CREATE TABLE` هو **48** جدولًا وظيفيًا.
- العدد السابق **47** كان خطأً حسابيًا في التقرير؛ قائمة الجداول وسكربت Baseline السابقان كانا يحتويان بالفعل **48** جدولًا وظيفيًا.

## الجداول التي تنشئها Baseline

- `account_categories`
- `account_code_settings`
- `approval_requests`
- `approval_statuses`
- `audit_logs`
- `bank_accounts`
- `branch_types`
- `cash_boxes`
- `chart_of_accounts`
- `cities`
- `companies`
- `cost_centers`
- `countries`
- `currencies`
- `document_allocations`
- `document_links`
- `exchange_rates`
- `financial_limit_movements`
- `financial_limits`
- `financial_voucher_details`
- `financial_voucher_headers`
- `fiscal_periods`
- `fiscal_years`
- `governorates`
- `journal_entry_details`
- `journal_entry_headers`
- `login_attempts`
- `numbering_counters`
- `numbering_document_types`
- `numbering_settings`
- `parties`
- `payment_methods`
- `payment_request_attachments`
- `payment_request_lines`
- `payment_requests`
- `refresh_tokens`
- `role_permissions`
- `roles`
- `system_permissions`
- `system_screens`
- `system_settings`
- `tenant_branches`
- `tenant_groups`
- `user_permissions`
- `users`
- `voucher_action_logs`
- `voucher_statuses`
- `voucher_types`

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
