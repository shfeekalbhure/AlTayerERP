# خريطة اعتماد جداول المرحلة الأولى

**الحالة:** مسودة تدقيق، وليست DDL نهائية.

## 1. التسلسل المقترح لإنشاء الجداول

### المستوى 0 — جداول مستقلة أو جذرية

- `tenant_groups`
- `system_permissions`
- `system_screens`
- `roles`
- `voucher_types`
- `voucher_statuses`
- `payment_methods`

### المستوى 1 — الهيكل المؤسسي

- `companies` ← `tenant_groups.Group_ID`
- `tenant_branches` ← `companies.Company_ID`
- `fiscal_years` ← `companies.Company_ID`
- `currencies` ← `companies.Company_ID`

### المستوى 2 — الأمن والصلاحيات

- `users` ← الشركة/الفرع عند اعتماد العلاقة النهائية
- `role_permissions` ← `roles`, `system_screens`, `system_permissions`
- `user_permissions` ← `users`, `system_screens`, `system_permissions`
- `login_attempts` ← `users` اختيارياً
- `refresh_tokens` ← `users`

### المستوى 3 — التهيئة المالية

- `system_settings` ← الشركة/الفرع/السنة حسب Scope
- `fiscal_periods` ← `companies`, `tenant_branches`, `fiscal_years`
- `exchange_rates` ← `companies`, `currencies`
- `numbering_settings` ← الشركة/الفرع/السنة
- `numbering_counters` ← `numbering_settings`
- `account_code_settings` ← `companies`

### المستوى 4 — الأدلة والموارد المالية

- `chart_of_accounts` ← `companies`, parent account, currency عند الحاجة
- `cost_centers` ← `companies`, parent cost center
- `cash_boxes` ← `companies`, `tenant_branches`, `chart_of_accounts`, `currencies`
- `bank_accounts` ← `companies`, `chart_of_accounts`, `currencies`
- `parties` ← `companies`, حساب رقابي اختياري

### المستوى 5 — السياسات والاعتمادات

- `financial_policies` ← الشركة/الفرع/نوع المستند
- `financial_policy_movements` ← `financial_policies`, `users`
- `approval_requests` ← الشركة/الفرع/المستند/المستخدم

### المستوى 6 — المحرك المالي

- `financial_voucher_headers` ← الشركة، الفرع، السنة، النوع، الحالة، العملة، الطرف، الصندوق/البنك
- `financial_voucher_details` ← رأس السند، الحساب، مركز التكلفة، العملة
- `journal_entry_headers` ← الفرع، السنة، الحالة، السند المصدر
- `journal_entry_details` ← رأس القيد، الحساب، مركز التكلفة، العملة
- `document_allocations` ← المستند المصدر والمستند المسدد
- `document_links` ← مستند أصل ومستند تابع
- `payment_requests` ← الشركة، الفرع، السنة، العملة، الطرف، الحالة
- `payment_request_lines` ← طلب الصرف، الحساب، مركز التكلفة
- `payment_request_attachments` ← طلب الصرف

### المستوى 7 — الرقابة

- `audit_logs` ← المستخدم/الفرع منطقياً؛ يفضّل FK nullable أو snapshot حسب القرار
- `voucher_action_logs` ← السند، المستخدم

## 2. العلاقات الحرجة المقترحة

| الجدول الابن | المفتاح | الجدول الأب | Delete Behavior المقترح |
|---|---|---|---|
| companies | Group_ID | tenant_groups | Restrict |
| tenant_branches | Company_ID | companies | Restrict |
| fiscal_years | Company_ID | companies | Restrict |
| fiscal_periods | Fiscal_Year_ID | fiscal_years | Restrict |
| role_permissions | Role_ID | roles | Cascade |
| role_permissions | Screen_ID | system_screens | Restrict |
| user_permissions | User_ID | users | Cascade |
| chart_of_accounts | Parent_Account_ID | chart_of_accounts | Restrict |
| cost_centers | Parent_Cost_Center_ID | cost_centers | Restrict |
| cash_boxes | Branch_ID | tenant_branches | Restrict |
| bank_accounts | Company_ID | companies | Restrict |
| financial_voucher_details | Voucher_ID | financial_voucher_headers | Cascade |
| journal_entry_details | Journal_Entry_ID | journal_entry_headers | Cascade |
| payment_request_lines | Payment_Request_ID | payment_requests | Cascade |
| payment_request_attachments | Payment_Request_ID | payment_requests | Cascade |
| audit_logs | User_ID | users | SetNull أو بدون FK حسب قرار الاحتفاظ التاريخي |

## 3. دوائر اعتماد يجب تجنبها

1. `financial_voucher_headers` ↔ `journal_entry_headers` عند وجود Source/Posted Entry؛ يجب جعل أحد الطرفين nullable وإضافة العلاقة بعد إنشاء الجدولين.
2. `tenant_groups.Main_Company_ID` ↔ `companies.Group_ID`؛ هذه دائرة مؤسسية. القرار المقترح: عدم تخزين Main Company في Baseline الأولى أو إضافة FK بعد إنشاء الجدولين مع nullable.
3. علاقات الإلغاء والعكس داخل `journal_entry_headers` ذاتية؛ تستخدم `Restrict`.
4. Parent account وParent cost center علاقات ذاتية؛ تستخدم `Restrict`.

## 4. الجداول المرجعية التي يجب Seedها بعد المخطط

- `system_permissions`
- `system_screens`
- `voucher_types`
- `voucher_statuses`
- `payment_methods`
- أنواع الفروع عند اعتماد Entity/جدول نهائي

## 5. ترتيب التشغيل المقترح للـBaseline

1. الجذور والمرجعيات.
2. الهيكل المؤسسي.
3. الأمن والصلاحيات.
4. التهيئة المالية.
5. الأدلة والموارد المالية.
6. السياسات والاعتمادات.
7. المحرك المالي.
8. الرقابة.
9. الفهارس الإضافية والـFK الدائرية المؤجلة.
10. Seed Data.

## 6. ملاحظات

- الخريطة لا تعتمد أي نوع مفتاح نهائي بعد.
- العلاقات النصية تحتاج تطابق charset/collation والطول قبل FK.
- لا توجد Views أو Triggers أو Stored Procedures أو Functions مثبتة من بحث الكود الحالي.
- لم يُنفذ أي SQL.
