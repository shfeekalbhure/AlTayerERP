# مسودة قاموس بيانات المرحلة الأولى

**الحالة:** مسودة تدقيق مبنية على `AppDbContext` والكيانات وMigration الحالية وملفات SQL المؤكدة. لا تعتمد للتنفيذ قبل مراجعة الحقول تفصيلياً واعتماد أنواع المفاتيح.

## 1. اصطلاحات

- **Snapshot:** هل ظهر الكيان في Snapshot الحالي بصورة مثبتة.
- **Migration:** هل توجد Migration تنفيذية تنشئ الجدول.
- **SQL:** هل وجد تعريف/تعديل يدوي مؤكد.
- **Runtime:** هل API ينشئ الجدول؟ الإجابة العامة: لا؛ لا يوجد `EnsureCreated/Migrate`.
- `؟` تعني أن التفصيل يحتاج استخراجاً نهائياً من الملف/بيئة القراءة قبل Baseline.

## 2. جدول الكيانات والجداول

| # | Entity | Table | PK / النوع الحالي | أهم FKs / الأنواع | Required / Optional وقيود الحقول | Snapshot | Migration | SQL يدوي | Runtime DDL | Drift والقرار المقترح |
|---:|---|---|---|---|---|---|---|---|---|---|
| 1 | TenantGroup | tenant_groups | Group_ID `string` | Parent_Group_ID `string?`, Main_Company_ID `string?` | Group_Code/Name_AR required؛ Name_EN/Notes optional؛ خصائص كثيرة Ignored | غير مكتمل | لا | نعم | لا | حسم نموذج المجموعة والخصائص المتجاهلة |
| 2 | Company | companies | Company_ID `string` | Group_ID `string` | الاسم العربي required؛ النصوص الاختيارية تحتاج أطوال موحدة | غير مثبت | لا | نعم/تاريخي | لا | تثبيت طول Company_ID وFK إلى المجموعة |
| 3 | TenantBranch | tenant_branches | Branch_ID `int` | Company_ID `string`, Parent_Branch_ID `int?`, Currency_ID `int?`, geography IDs | الاسم/الشركة required؛ الموقع حسب القرار | غير مثبت | لا | نعم | لا | تعارض Branch_ID مع Migration القيود النصي |
| 4 | FiscalYear | fiscal_years | Fiscal_Year_ID `int` | Company_ID `string` | التواريخ required؛ الإغلاق اختياري | غير مثبت | لا | تاريخي | لا | إضافة FK وUnique للسنة داخل الشركة |
| 5 | User | users | User_ID `int` | Role_ID `int`, Company/Branch حسب النموذج | Login/FullName/PasswordHash required | غير مثبت | لا | تاريخي | لا | توحيد حقول التدقيق إلى int? |
| 6 | Role | roles | Role_ID `int` | — | الاسم/الكود required | غير مثبت | لا | تاريخي | لا | Unique للكود والاسم ضمن النطاق |
| 7 | RolePermission | role_permissions | Permission_ID أو مركب `int`؟ | Role_ID, Screen_ID, System_Permission_ID `int` | flags required | غير مثبت | لا | تاريخي | لا | اعتماد PK وUnique مركب |
| 8 | UserPermission | user_permissions | User_Permission_ID `int`؟ | User_ID, Screen_ID, Permission_ID | optional override fields | غير مثبت | لا | غير مثبت | لا | تحديد precedence وUnique |
| 9 | SystemPermission | system_permissions | Permission_ID `int` | — | Code/Name required | غير مثبت | لا | Seed/تاريخي | لا | Seed ثابت وUnique Code |
| 10 | SystemScreen | system_screens | Screen_ID `int` | Module_ID `int?` | Screen_Code/Name required | غير مثبت | لا | Seeder | لا | Seed مستقل؛ Unique Screen_Code |
| 11 | LoginAttempt | login_attempts | Login_Attempt_ID `long/int` | User_ID `int?` | Login_Name/Attempted_At required | موجود في Fluent | لا | لا | لا | indexes موجودة؛ لا تحفظ أسرار |
| 12 | RefreshToken | refresh_tokens | Refresh_Token_ID `long/int` | User_ID `int` | TokenHash required؛ dates required | موجود في Fluent | لا | لا | لا | Unique hash وindex user/expiry |
| 13 | SystemSetting | system_settings | Setting_ID `int` | Company_ID `string`, Branch_ID `int`, Fiscal_Year_ID `int` | key/value/scope required؛ Effective_Date optional | غير مثبت | لا | نعم | لا | SQL يستخدم unicode_ci؛ اعتماد Scope/Unique |
| 14 | FiscalPeriod | fiscal_periods | Fiscal_Period_ID `int` | Company_ID string, Branch_ID int, Fiscal_Year_ID int | dates/code/name required؛ close fields optional | غير مثبت | لا | نعم | لا | Unique scope+code وفحص عدم تداخل الفترات |
| 15 | ExchangeRate | exchange_rates | Exchange_Rate_ID `int` | Company_ID string, Currency_ID/Code | rate decimal(18,6)؛ min/max optional | غير مثبت | لا | نعم | لا | اعتماد Currency_ID بدلاً من Code للعلاقة |
| 16 | NumberingSetting | numbering_settings | Numbering_ID `int` | company/branch/year | document/reset fields required | غير مثبت | لا | تاريخي | لا | Unique للنطاق ونوع المستند |
| 17 | NumberingCounter | numbering_counters | Counter_ID `long/int` | Numbering_ID `int` | Last_Number required | غير مثبت | لا | غير مثبت | لا | concurrency token/unique scope |
| 18 | FinancialPolicy | financial_policies | Policy_ID `int/long` | company/branch/user refs | limits decimal؛ dates optional | غير مثبت | لا | تاريخي | لا | precision وscope يحتاجان اعتماداً |
| 19 | FinancialPolicyMovement | financial_policy_movements | Movement_ID `long`؟ | Policy_ID, User_ID | old/new values; action date | غير مثبت | لا | غير مثبت | لا | audit append-only |
| 20 | ApprovalRequest | approval_requests | Approval_Request_ID `long/int` | user/branch/document refs | status/amount/dates | غير مثبت | لا | تاريخي | لا | document polymorphism يحتاج قراراً |
| 21 | AccountCodeSetting | account_code_settings | Setting_ID `int` | Company_ID `string` | lengths/numbers required | يظهر في Snapshot | لا | تاريخي | لا | Snapshot يستخدم longtext لبعض النصوص |
| 22 | ChartOfAccount | chart_of_accounts | Account_ID غير محسوم (`string/int`) | Company_ID, Parent_Account_ID, Currency_ID | Account_Code/Name required؛ balances decimal | غير مثبت | لا | متفرق | لا | فصل ID عن Account_Code وحسم النوع |
| 23 | CostCenter | cost_centers | Cost_Center_ID غير محسوم | Company_ID, Parent_Cost_Center_ID | code/name required | غير مثبت | لا | متفرق | لا | فصل ID عن Code وحسم النوع |
| 24 | CashBox | cash_boxes | Cash_Box_ID غالباً `int` | Company_ID, Branch_ID, Currency_ID, Account_ID | code/name required؛ limits decimal | غير مثبت | لا | Controller قد يحتوي SQL | لا | FK وتعارض SQL النصي يحتاجان توحيداً |
| 25 | Currency | currencies | Currency_ID `int` | Company_ID `string` | code/name required؛ rates/flags | غير مثبت | لا | تاريخي | لا | Unique company+code |
| 26 | BankAccount | bank_accounts | Bank_Account_ID `int` | Company_ID, Currency_ID/Code, GL Account | Account_No required؛ IBAN optional | غير مثبت | لا | نعم | لا | SQL يستخدم Currency_Code؛ اعتماد FK ID |
| 27 | FinancialVoucherHeader | financial_voucher_headers | Voucher_ID `long` | company/branch/year/type/status/currency/party/cash/bank | amounts decimal(18,2)؛ dates/status required | غير مثبت | لا | نعم تاريخي | لا | تجميع عدة سكربتات في نموذج واحد |
| 28 | FinancialVoucherDetail | financial_voucher_details | Voucher_Detail_ID `long` | Voucher_ID, Account_ID, Cost_Center_ID, Currency_ID | debit/credit decimal(18,2), rate(18,6) | غير مثبت | لا | تاريخي | لا | أنواع Account/Cost Center غير محسومة |
| 29 | JournalEntryHeader | journal_entry_headers | Journal_Entry_ID `long` | Branch_ID string في Migration؛ FiscalYear int?؛ Voucher long? | Entry_No varchar50 unique؛ amounts 18,2 | غير كامل | نعم | Restore SQL | لا | Migration وSQL مصدران؛ Branch/User types متعارضة |
| 30 | JournalEntryDetail | journal_entry_details | Journal_Entry_Detail_ID `long` | JournalEntry long, Account string, CostCenter string?, Currency int | amounts 18,2؛ rate 18,6 | غير كامل | نعم | Restore SQL | لا | Cascade مثبت؛ بقية FKs غير معرفة |
| 31 | DocumentAllocation | document_allocations | Allocation_ID `long` | source/target voucher refs | amount decimal؛ dates | غير مثبت | لا | غير مثبت | لا | اعتماد FK ونطاق العملات |
| 32 | DocumentLink | document_links | Link_ID `long` | source/target document refs | document types/numbers | غير مثبت | لا | غير مثبت | لا | polymorphic links بلا FK مباشرة |
| 33 | AuditLog | audit_logs | Audit_ID `long` | User_ID/Branch_ID مخزنة نصياً حالياً | Old/New JSON optional؛ action required | يظهر جزئياً Snapshot | لا | غير مثبت | لا | قرار FK أم snapshot تاريخي |
| 34 | VoucherActionLog | voucher_action_logs | Action_Log_ID `long` | Voucher_ID long, User_ID | action/date required | غير مثبت | لا | تاريخي | لا | append-only وFK voucher |
| 35 | VoucherType | voucher_types | Voucher_Type_ID `int` | — | code/name required؛ Sort_Order | غير مثبت | لا | نعم/Seed | لا | Unique code؛ فصل Seed |
| 36 | VoucherStatus | voucher_statuses | Voucher_Status_ID `int` | — | code/name required؛ Sort_Order | غير مثبت | لا | نعم/Seed | لا | Unique code؛ workflow constraints |
| 37 | PaymentMethod | payment_methods | Payment_Method_ID `int` | — | code/name required؛ Sort_Order | غير مثبت | لا | نعم/Seed | لا | Unique code؛ flags capability |
| 38 | Party | parties | Party_ID غير محسوم | Company_ID, Account_ID | name/type required؛ contact optional | غير مثبت | لا | متفرق | لا | حسم PK وعلاقة الحساب الرقابي |
| 39 | PaymentRequest | payment_requests | Payment_Request_ID غير محسوم | company/branch/year/currency/party/status | totals decimal؛ dates/status required | غير مثبت | لا | غير مثبت | لا | لا مصدر إنشاء موحد |
| 40 | PaymentRequestLine | payment_request_lines | Line_ID `long/int` | Payment_Request_ID, Account_ID, Cost_Center_ID | amount decimal； description optional | غير مثبت | لا | غير مثبت | لا | Cascade من الطلب |
| 41 | PaymentRequestAttachment | payment_request_attachments | Attachment_ID `long/int` | Payment_Request_ID | path/name/type required؛ binary/path decision | غير مثبت | لا | غير مثبت | لا | تخزين ملفات خارج DB مفضل مع metadata |

## 3. Defaults وCheck Constraints العامة المقترحة

ليست مطبقة في هذه المرحلة، لكنها تحتاج قراراً في Baseline:

- `Is_Active DEFAULT 1`.
- `Created_At DEFAULT CURRENT_TIMESTAMP` أو ضبطها خادمياً بصورة موحدة.
- مبالغ السندات `>= 0`.
- Debit/Credit: لا يكونان موجبين معاً في السطر نفسه.
- Exchange_Rate `> 0`.
- Start_Date `<= End_Date`.
- ISO codes وأكواد العملات حسب قواعد التطبيق.
- Unique لكل أكواد الأعمال داخل نطاق الشركة/الفرع المناسب.

## 4. Delete Behavior المقترح

- الهيكل المؤسسي والحسابات: `Restrict` وعدم الحذف المادي.
- التفاصيل التابعة للرأس: `Cascade`.
- مراجع المستخدم في السجلات التاريخية: `SetNull` أو snapshot بلا FK بعد قرار المالك.
- العلاقات الذاتية: `Restrict`.

## 5. حالة المسودة

- لا تغير هذه الوثيقة أي كود أو قاعدة.
- الأطوال والـprecision النهائية يجب استخراجها من كل Entity/DTO وقرار الأعمال قبل Baseline.
- لا تعتبر القيم غير المثبتة قراراً نهائياً.
