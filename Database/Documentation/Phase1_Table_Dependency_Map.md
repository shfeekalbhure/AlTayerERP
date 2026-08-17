# خريطة اعتماد جداول المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** تدقيق مكتمل للمصادر الحالية، وليست DDL تنفيذية.

## 1. ترتيب الإنشاء المقترح

### المستوى 0 — الجذور والمرجعيات المستقلة

- `tenant_groups`
- `countries`
- `system_permissions`
- `system_screens`
- `roles`
- `voucher_types`
- `voucher_statuses`
- `payment_methods`
- `branch_types`

### المستوى 1 — الجغرافيا والهيكل المؤسسي

- `governorates` ← `countries.Country_ID`
- `cities` ← `countries.Country_ID`, `governorates.Governorate_ID`
- `companies` ← `tenant_groups.Group_ID`
- `tenant_branches` ← `companies.Company_ID`, parent branch، والجغرافيا
- `fiscal_years` ← `companies.Company_ID`
- `currencies` ← `companies.Company_ID`

### المستوى 2 — الأمن والصلاحيات

- `users` ← `companies`, `tenant_branches`, `roles`
- `role_permissions` ← `roles`, `system_screens`
- `user_permissions` ← `users`
- `login_attempts` ← `users` اختياريًا
- `refresh_tokens` ← `users`, `companies`, `tenant_branches`, `fiscal_years`

### المستوى 3 — التهيئة والإعدادات

- `system_settings` ← الشركة/الفرع/السنة حسب Scope
- `fiscal_periods` ← `companies`, `tenant_branches`, `fiscal_years`
- `exchange_rates` ← `companies`, `currencies`
- `numbering_settings` ← الشركة/الفرع/السنة
- `numbering_counters` ← نطاق نوع المستند والشركة والفرع والسنة
- `account_code_settings` ← `companies`

### المستوى 4 — الأدلة والموارد المالية

- `account_categories` ← `companies`
- `chart_of_accounts` ← `companies`, parent account, account category
- `cost_centers` ← `companies`, parent cost center
- `cash_boxes` ← `companies`, `tenant_branches`, `chart_of_accounts`, العملات
- `bank_accounts` ← `companies`, الحساب العام/العملة بحسب القرار
- `parties` ← `companies`, حساب رقابي اختياري

### المستوى 5 — السياسات والاعتمادات

- `financial_limits` ← الشركة/الفرع/نوع المستند
- `financial_limit_movements` ← `financial_limits`, `users`
- `approval_requests` ← الشركة/الفرع/المستند/المستخدم

### المستوى 6 — المحرك المالي والتشغيلي

- `financial_voucher_headers` ← الفرع، السنة، النوع، الحالة، العملة، الطرف، الحساب النقدي/البنكي
- `financial_voucher_details` ← رأس السند، الحساب، مركز التكلفة، العملة
- `journal_entry_headers` ← الفرع، السنة، السند المصدر
- `journal_entry_details` ← رأس القيد، الحساب، مركز التكلفة، العملة
- `document_allocations` ← رأس السند والمستند المصدر
- `document_links` ← مستند أصل ومستند تابع
- `payment_requests` ← الشركة، الفرع، السنة، الطرف، طريقة السداد، السند الناتج
- `payment_request_lines` ← طلب الصرف، الحساب، مركز التكلفة، العملة
- `payment_request_attachments` ← طلب الصرف

### المستوى 7 — الرقابة

- `audit_logs` ← هوية المستخدم/الفرع كـFK أو Snapshot حسب القرار
- `voucher_action_logs` ← السند والمستخدم
- `database_alignment_findings` ← جدول تشخيصي تاريخي فقط، لا يدخل Baseline

## 2. تصنيف الجداول حسب المصدر

### جداول مغطاة فعليًا بـMigration الحالية

- `journal_entry_headers`
- `journal_entry_details`

### جداول EF/DbSet غير مغطاة بـMigration إنشاء

جميع DbSets الأخرى وعددها 39، ومنها الهيكل المؤسسي، الأمن، الإعدادات، الأدلة، السندات، الطلبات والتدقيق.

### جداول SQL-only أو خارج DbSets الحالية

- `countries`
- `governorates`
- `cities`
- `branch_types`
- `account_categories` موجود Entity لكن لا يظهر DbSet صريحًا في الجرد الأصلي؛ يُستخدم عبر `Set<AccountCategory>()`
- `database_alignment_findings` تشخيصي فقط

### جداول تعتمد عليها المنظومة وقت التشغيل دون Migration شاملة

- `tenant_groups`, `companies`, `tenant_branches`
- `users`, `roles`, `role_permissions`, `system_screens`
- `currencies`, `chart_of_accounts`, `cost_centers`, `cash_boxes`, `bank_accounts`
- `financial_voucher_headers/details`
- `payment_requests/lines/attachments`
- جداول الجغرافيا وأنواع الفروع

## 3. العلاقات وسياسة الحذف المقترحة

| الجدول الابن | المفتاح | الجدول الأب | سياسة الحذف المقترحة |
|---|---|---|---|
| `companies` | `Group_ID` | `tenant_groups` | Restrict |
| `governorates` | `Country_ID` | `countries` | Restrict |
| `cities` | `Country_ID` | `countries` | Restrict |
| `cities` | `Governorate_ID` | `governorates` | Restrict |
| `tenant_branches` | `Company_ID` | `companies` | Restrict |
| `tenant_branches` | `Parent_Branch_ID` | `tenant_branches` | Restrict |
| `tenant_branches` | geography IDs | الجغرافيا | Restrict |
| `users` | `Role_ID` | `roles` | Restrict |
| `users` | `Branch_ID` | `tenant_branches` | Restrict |
| `role_permissions` | `Role_ID` | `roles` | Cascade |
| `role_permissions` | `Screen_ID` | `system_screens` | Restrict |
| `user_permissions` | `User_ID` | `users` | Cascade |
| `refresh_tokens` | `User_ID` | `users` | Cascade أو حذف صريح عند إلغاء المستخدم |
| `fiscal_periods` | `Fiscal_Year_ID` | `fiscal_years` | Restrict |
| `chart_of_accounts` | `Parent_Account_ID` | `chart_of_accounts` | Restrict |
| `cost_centers` | `Parent_Cost_Center_ID` | `cost_centers` | Restrict |
| `cash_boxes` | `Branch_ID` | `tenant_branches` | Restrict |
| `cash_boxes` | `Account_ID` | `chart_of_accounts` | Restrict |
| `parties` | `Account_ID` | `chart_of_accounts` | SetNull/Restrict حسب الحساب الرقابي |
| `financial_voucher_details` | `Voucher_ID` | `financial_voucher_headers` | Cascade للمسودة فقط؛ الحذف المادي ممنوع بعد الترحيل |
| `journal_entry_details` | `Journal_Entry_ID` | `journal_entry_headers` | Cascade |
| `voucher_action_logs` | `Voucher_ID` | `financial_voucher_headers` | Restrict |
| `document_allocations` | `Voucher_ID` | `financial_voucher_headers` | Cascade قبل الترحيل/Restrict بعده منطقياً |
| `payment_request_lines` | `Payment_Request_ID` | `payment_requests` | Cascade |
| `payment_request_attachments` | `Payment_Request_ID` | `payment_requests` | Cascade منطقي؛ الملفات الخارجية تنظف صراحة |
| `audit_logs` | `User_ID` | `users` | SetNull أو بدون FK مع Snapshot |

## 4. العلاقات الدائرية

1. `financial_voucher_headers` ↔ `journal_entry_headers` عبر السند المصدر والقيد الناتج؛ أحد المفتاحين يبقى nullable وتضاف العلاقة الثانية بعد إنشاء الجدولين.
2. `tenant_groups.Main_Company_ID` ↔ `companies.Group_ID`; الخاصية متجاهلة حاليًا وتحتاج قرارًا قبل Baseline.
3. العلاقات الذاتية في الفروع والحسابات ومراكز التكلفة والقيود العكسية؛ تستخدم `Restrict`.
4. روابط المستندات متعددة الأنواع (`document_links`) لا يمكن فرض FK واحدة مباشرة؛ تحتاج جدول أنواع/علاقات محددة أو تحقق تطبيقي.

## 5. Views وTriggers وProcedures وFunctions

- **Triggers مثبتة:**
  - `trg_financial_voucher_details_account_policy_bi`
  - `trg_financial_voucher_details_account_policy_bu`
  موجودة في سكربت حماية دليل الحسابات، وليست ممثلة في EF/Migrations.
- **Procedures مؤقتة:** عدة سكربتات تنشئ Procedures مساعدة ثم تحذفها؛ ليست كائنات دائمة مقصودة.
- **Views:** لا توجد Views مثبتة في الملفات المجرودة.
- **Functions:** لا توجد Functions دائمة مثبتة.

## 6. ترتيب Baseline المقترح بعد الاعتماد

1. الجذور والمرجعيات.
2. الجغرافيا والهيكل المؤسسي.
3. الأمن والصلاحيات.
4. التهيئة المالية.
5. الأدلة والموارد.
6. السياسات والاعتمادات.
7. المحرك المالي والتشغيلي.
8. الرقابة.
9. العلاقات الدائرية والـFK المؤجلة.
10. Seed Data.
11. أي Trigger معتمد صراحةً؛ والأفضل نقل قواعد الأعمال إلى API ما لم توجد ضرورة قاعدة بيانات موثقة.

## 7. التأكيد

الخريطة لا تعتمد أنواع المفاتيح المفتوحة ولا تنفذ DDL. لم يُنفذ SQL ولم تتغير القاعدة القديمة.