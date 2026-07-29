# قاموس بيانات المرحلة الأولى — مسودة تدقيق مكتملة

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**النطاق:** 41 `DbSet` + 5 جداول SQL-only/Runtime = **46 تعريف جدول**.  
**التغطية:** 100% على مستوى الكيان/الجدول والمفتاح والمصدر والحالة؛ الأطوال/Defaults غير المعرفة في الكود موسومة صراحةً **غير محدد** ولا يتم تخمينها.

## 1. مفتاح القراءة

- `R`: Required/غير nullable.
- `O`: Optional/nullable.
- `D=`: Default موثق.
- `P(p,s)`: Precision/Scale.
- **المصدر:** EF، Migration، SQL، Runtime DDL.
- **النطاق:** System / Group / Company / Branch / FiscalYear.

## 2. الهيكل المؤسسي والجغرافيا

| الكيان/الاسم العربي | الجدول والنطاق | PK | الحقول الحالية المختصرة | FKs/Indexes/Checks | المصدر والتغطية والقرار |
|---|---|---|---|---|---|
| TenantGroup / المجموعة التجارية | `tenant_groups` / Group | `Group_ID string`؛ الطول مختلف 36/50 | `Group_Code string R`; `Group_Name_AR string R`; `Group_Name_EN string O`; `Is_Default bool D=0`; `Show_In_Login bool D=1`; `Show_In_Tree bool D=1`; `Notes O`; `Created_At`; حقول تدقيق/حالة؛ خصائص موسعة كثيرة Ignored | Unique Group_Code في SQL؛ Parent/MainCompany تاريخيان بلا FK | EF جزئي + SQL؛ لا Migration. القرار: نموذج نهائي واحد وطول ID موحد |
| Company / الشركة | `companies` / Group,Company | `Company_ID string` | `Group_ID string R`; Name_AR R؛ Name_EN حاليًا غير nullable في Entity رغم بيانات قديمة؛ Prefix/Activity/Tax/Phone/Mobile/Email/Address O؛ Logo binary O؛ audit/status | Index Group+Active؛ FK Group غير مثبت | EF + SQL ترقيات؛ لا Migration. تثبيت nullability والأطوال |
| TenantBranch / الفرع | `tenant_branches` / Company,Branch | `Branch_ID int` | `Company_ID string R`; `Branch_Code string`; Names؛ Address/Type/Parent/contacts/manager/notes O؛ AllowCredit/Percentage bool؛ Currency_ID int D=1؛ geography IDs عبر SQL؛ audit/status | Parent self FK مقترح Restrict؛ indexes geography؛ Branch_ID Drift | EF + SQL؛ لا Migration |
| FiscalYear / السنة المالية | `fiscal_years` / Company,FiscalYear | `Fiscal_Year_ID int` | `Company_ID string R`; `Year_Name string R`; Start/End R؛ Is_Default/Closed/Active bool؛ Created/Updated | Unique Company+Year مقترح؛ date check | EF فقط/قاعدة موجودة؛ لا Migration |
| Country / الدولة | `countries` / System | `Country_ID int AI` | Code varchar10 R؛ Name_AR 150 R؛ Name_EN 150 O؛ ISO2 2 O؛ ISO3 3 O؛ Phone/Currency Code 10 O؛ Nationality 150 O؛ Sort int D=0؛ Active D=1؛ Notes 500 O؛ dates | Unique Code/Arabic Name؛ لا Unique ISO في SQL الحالي | SQL-only؛ API يستخدم raw queries/DTOs؛ لا Entity/DbSet/Migration |
| Governorate / المحافظة | `governorates` / System | `Governorate_ID int AI` | Country_ID int R؛ Code20 R؛ Names150؛ Sort/Active/Notes/dates | FK Country Restrict؛ Unique country+code/name | SQL-only |
| City / المدينة | `cities` / System | `City_ID int AI` | Country_ID/Governorate_ID int R؛ Code20؛ Names150؛ Postal20 O؛ Sort/Active/Notes/dates | FK Country/Governorate Restrict؛ Unique governorate+code/name | SQL-only |
| BranchType / نوع الفرع | `branch_types` / System | `Branch_Type_ID int AI` | Code30 R؛ Names100؛ Sort D=0؛ Active D=1؛ dates | Unique Code؛ index active+sort | SQL-only + Seed |

## 3. الأمن والصلاحيات

| الكيان | الجدول/PK | الحقول | العلاقات/القيود | المصدر والقرار |
|---|---|---|---|---|
| User | `users`; `User_ID int` | Company_ID string; Branch_ID int; Role_ID int; codes/names/login/password hash strings؛ phone/email/notes O؛ lockout/date fields | FK company/branch/role مطلوبة؛ Unique Login_Name/User_Code مقترحة | EF + SQL login security؛ لا Migration |
| Role | `roles`; `Role_ID int` | Role_Name string؛ Description O؛ Active D=1؛ dates؛ Role_Code O؛ Is_System_Admin bool | Unique Role_Code مقترح | EF؛ لا Migration |
| RolePermission | `role_permissions`; `Permission_ID int` | Role_ID int؛ Screen_ID int؛ Can_View/Add/Edit/Delete/Print/Export/Import/Approve/UnApprove bool | Unique Role+Screen؛ Cascade role، Restrict screen | EF؛ لا Migration |
| UserPermission | `user_permissions`; `Permission_ID int` | User_ID int؛ Permission_Category/Name strings؛ flags | Unique User+Category+Name | EF؛ النموذج لا يطابق RolePermission؛ قرار معماري مطلوب |
| SystemPermission | `system_permissions`; `Permission_ID int` | Code max100 R؛ Name max200 R؛ Type max50 R؛ Module max100 O؛ Active D=1؛ Sort D=0؛ Created | Unique Code | EF + Seeder؛ لا Migration |
| SystemScreen | `system_screens`; `Screen_ID int` | Screen_Code/Name/Module strings؛ Active D=1؛ Sort؛ Created | Unique Screen_Code | EF + Seeder؛ لا Migration |
| LoginAttempt | `login_attempts`; `Login_Attempt_ID bigint` | User_ID int O؛ Login_Name100؛ Company50 O؛ Branch int O؛ Year int O؛ Attempted datetime6 D=current؛ success bool؛ reason100/IP64/agent512/device128/session64/lockout O | indexes login+date, user+date, session | EF + SQL؛ no Migration |
| RefreshToken | `refresh_tokens`; `Refresh_Token_ID bigint` | Token_Hash char64 R؛ Session64؛ User int؛ Company50؛ Branch int؛ Year int؛ Device128؛ dates؛ revoke fields | Unique hash؛ indexes user+expiry/session | EF + SQL؛ no Migration |

## 4. الإعدادات والترقيم والسياسات

| الكيان | الجدول/PK | الحقول الرئيسية | Constraints | المصدر/القرار |
|---|---|---|---|---|
| SystemSetting | `system_settings`; `Setting_ID int AI` | Key100, Name200, Value text, Scope20, Company50 D='', Branch int D=0, Year int D=0, Effective date O, Description text, Active D=1, dates | Unique key+scope+company+branch+year | EF + SQL؛ no Migration |
| FiscalPeriod | `fiscal_periods`; `Fiscal_Period_ID int AI` | Company50, Branch int, Year int, Code50, Name150, Start/End date, Closed bool, close fields O, Active, dates | Unique scope+code؛ overlap check مقترح | EF + SQL |
| ExchangeRate | `exchange_rates`; `Exchange_Rate_ID int AI` | Company50؛ Currency_Code20 في SQL؛ Rate_Date؛ Exchange_Rate P(18,6)؛ Min/Max O؛ Default/Active؛ Notes500؛ dates | Unique company+currency+date | EF + SQL؛ قرار Currency_ID vs Code |
| NumberingSetting | `numbering_settings`; `Numbering_ID int` | Document_Type, Prefix, Digits, Reset_Type, Last_Number, Use flags, Active | حقول نطاق company/branch/year غير موجودة في Entity الحالي رغم استخدامها API/SQL | EF ناقص + SQL تاريخي |
| NumberingCounter | `numbering_counters`; `Counter_ID int` | Document_Type string؛ Company_ID string O؛ Branch_ID int O؛ Year int O؛ Last int؛ dates | Unique document+company+branch+year؛ D ''/0 بعد التطبيع | EF + SQL؛ no Migration |
| FinancialPolicy | `financial_limits`; `Limit_ID` حسب Entity الحالي | scope/type/limits/status/dates حسب Entity؛ أطوال غير محددة في Fluent | يحتاج precision/FKs/checks | EF؛ no Migration |
| FinancialPolicyMovement | `financial_limit_movements`; `Movement_ID` | policy/user/action/old-new/date؛ التفاصيل من Entity، أطوال غير محددة | append-only؛ FK policy/user | EF؛ no Migration |
| ApprovalRequest | `approval_requests`; `Approval_ID` | document polymorphic, scope, user, status, amount/dates | FKs غير مكتملة؛ check workflow مطلوب | EF؛ no Migration |

## 5. الأدلة والموارد المالية

| الكيان | الجدول/PK | الحقول الرئيسية | Constraints | المصدر/القرار |
|---|---|---|---|---|
| AccountCategory / تصنيف الحساب | `account_categories`; `Category_ID string(50)` | Company50, Code50, Names150, Type30, Normal10, System/Active bool, Sort, audit strings/dates | Unique Company+Code | Entity يستخدم عبر `Set<>` + SQL-only DDL/Seed؛ إضافة DbSet/Fluent في المرحلة الثانية بعد الاعتماد |
| AccountCodeSetting | `account_code_settings`; `Setting_ID int` | Company_ID string؛ level/segment/start/max/padding/flags؛ Created/Active | Snapshot يحول نصوصًا إلى longtext لغياب الأطوال | EF/Snapshot؛ no Migration |
| ChartOfAccount | `chart_of_accounts`; `Account_ID string` | Company_ID string؛ Parent string O؛ Code/Name_AR/Type strings R؛ Name_EN O؛ Level int؛ flags؛ Currency_Code O؛ category/normal/path O؛ serial؛ audit؛ effective fields من SQL | self FK Restrict؛ Unique Company+Code؛ control account checks؛ Trigger SQL خارجي | EF + SQL؛ no Migration |
| CostCenter | `cost_centers`; `Cost_Center_ID string` | Company string؛ Parent O؛ Code/Name_AR R؛ Name_EN O؛ Level; Postable; Created; Active | self FK Restrict؛ Unique company+code | EF؛ no Migration |
| CashBox | `cash_boxes`; `Cash_Box_ID string GUID` | Company string; Branch int; Account_ID string; Currency_Code string; Code/Name R; Name_EN O; Opening/Min/Max decimals (precision غير محدد); Active; Notes; audit strings/dates | FK account/branch مقترحة Restrict؛ Unique branch+code/name | EF؛ no Migration |
| Currency | `currencies`; `Currency_ID int` | Company string; Code/Name_AR R; Name_EN/Symbol O; DecimalPlaces; rates decimals؛ local/default/active; notes/audit | Unique Company+Code؛ precision فقط Exchange_Rate 18,6 في Fluent | EF؛ no Migration |
| BankAccount | `bank_accounts`; `Bank_Account_ID int AI` | Company50; Code50; Names200; AccountNo100; IBAN64 O; Currency_Code20; GL100 O; BranchName200 O; Active; Notes; dates | Unique Company+AccountNo؛ index Company+Name | EF + 3 SQL sources؛ no Migration |
| Party | `parties`; `Party_ID string max50` | Company50; Code50; Names200; Type50; contacts/identity/tax/address/city O; Account_ID50 O; Credit_Limit decimal (Fluent 18,2); Active | Unique company+code؛ FK account optional | EF؛ no Migration |

## 6. المحرك المالي والتشغيلي

| الكيان | الجدول/PK | الحقول الرئيسية | Constraints | المصدر/التغطية |
|---|---|---|---|---|
| FinancialVoucherHeader | `financial_voucher_headers`; `Voucher_ID long AI` | VoucherNo50; Type/Status int; Branch_ID **string50**; Year int O; dates; CashAccount string50; Party string50 O; PaymentMethod int O; Currency int; Rate P(18,6); Amount/Foreign/Local P(18,2); refs/descriptions; approval/review/posting/reversal/audit fields | Unique Branch+Year+Type+No؛ indexes؛ details Cascade؛ action logs Restrict | EF + SQL upgrades؛ no Migration |
| FinancialVoucherDetail | `financial_voucher_details`; `Voucher_Detail_ID long` | Voucher long; Line int; Account string; CostCenter string O; Project string O; Currency int; Rate18,6; amounts18,2; refs/notes | Unique Voucher+Line؛ indexes؛ FK header Cascade | EF؛ no Migration |
| JournalEntryHeader | `journal_entry_headers`; `Journal_Entry_ID long AI` | EntryNo50; Type byte; Status int; Branch_ID **string50**; Year int O; dates; source fields; totals18,2; post/reversal/cancel/audit strings/dates | Unique EntryNo؛ indexes؛ details Cascade | EF + الوحيدة Migration + Restore SQL |
| JournalEntryDetail | `journal_entry_details`; `Journal_Entry_Detail_ID long AI` | Journal long; Line int; Account50; CostCenter50 O; Project50 O; Currency int; Rate18,6; amounts18,2; refs/notes/audit | Unique Journal+Line؛ FK header Cascade | EF + Migration + Restore SQL |
| DocumentAllocation | `document_allocations`; `Allocation_ID` | Voucher/source fields؛ Rate18,6؛ totals/collected/remaining18,2؛ dates | FK Voucher Cascade حسب Fluent | EF؛ no Migration |
| DocumentLink | `document_links`; `Document_Link_ID` | source/target document types and IDs؛ metadata حسب Entity | polymorphic، no direct FK | EF؛ no Migration |
| AuditLog | `audit_logs`; `Audit_ID long` | Table/Record/Action strings؛ User_ID/Branch_ID strings O؛ Module int O؛ date؛ Old/New json O؛ channel/device/ip/notes | Action check من SQL؛ indexes غير مكتملة | EF/Snapshot + SQL check؛ no Migration |
| VoucherActionLog | `voucher_action_logs`; `Voucher_Action_ID` | Voucher_ID؛ action/user/date/notes حسب Entity | FK Voucher Restrict؛ Check action عبر SQL | EF + SQL؛ no Migration |
| VoucherType | `voucher_types`; `Voucher_Type_ID int` | code/name/sort/active وغيرها حسب Entity | Unique Code؛ Seed | EF + SQL compatibility + Seeder |
| VoucherStatus | `voucher_statuses`; `Voucher_Status_ID int` | code/name/sort/active | Unique Code؛ workflow check مقترح | EF + SQL + Seeder |
| PaymentMethod | `payment_methods`; `Payment_Method_ID int` | code/name/sort/active/capabilities حسب Entity | Unique Code | EF + SQL + Seeder |
| PaymentRequest | `payment_requests`; `Payment_Request_ID long AI` | Company string; Branch int; Year int; No string; Date; Status; Beneficiary; Party string O; Method int O; refs/description; ApprovedTotal P(19,4); linked Voucher long O; reasons; audit strings/dates | Unique Company+Branch+Year+No؛ index scope+status | EF + SQL + Runtime SchemaInitializer؛ no Migration |
| PaymentRequestLine | `payment_request_lines`; `Payment_Request_Line_ID long AI` | Request long; Line int; Account string; CostCenter string O; Currency int; Rate P(19,8); Foreign/Local P(19,4); refs/description | Unique Request+Line؛ EF relationship Restrict حاليًا رغم SQL بلا FK | EF + SQL + Runtime DDL؛ no Migration |
| PaymentRequestAttachment | `payment_request_attachments`; `Payment_Request_Attachment_ID long AI` | Request long; Company string; Branch int; Year int; OriginalName260; StorageKey500; ContentType100; Size long; Active; CreatedBy string; CreatedAt | index Request+Active؛ FK غير موجود في SQL | EF + SQL + Runtime DDL؛ no Migration |

## 7. تغطية Snapshot/Migrations

- Migration تنشئ جدولين فقط: `journal_entry_headers`, `journal_entry_details`.
- Snapshot يحتوي نماذج أكثر من Migration لكنه غير متزامن مع 41 DbSet الحالية ولا يصلح كدليل إنشاء.
- 39 DbSet لا تملك Migration إنشاء.
- 5 جداول SQL-only/Runtime خارج DbSets الصريحة: geography الثلاثة، `branch_types`, `account_categories`؛ جدول التشخيص مستبعد من Baseline.

## 8. القرارات المقترحة العامة

- كل طول أو Default أو Check غير موثق يبقى «غير محدد» حتى المرحلة الثانية.
- توحيد IDs وCollation قبل FK النصية.
- Migrations تصبح المصدر الوحيد للمخطط.
- Seeders منفصلة ولا تنشئ Schema.
- Soft Delete/Restrict للجذور، Cascade فقط للتفاصيل التابعة قبل الترحيل.

لم يُعدل أي كيان أو قاعدة بيانات ولم يُنفذ SQL.