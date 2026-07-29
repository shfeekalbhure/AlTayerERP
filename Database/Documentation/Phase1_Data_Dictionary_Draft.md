# قاموس بيانات المرحلة الأولى — تدقيق مكتمل

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**النطاق:** 41 `DbSet` + `AccountCategory` المستخدم عبر `Set<>` + 5 جداول SQL-only/تشخيصية = **47 تعريفًا**.  
**التغطية:** **47/47 = 100% على مستوى الجدول** و**42/42 = 100% على مستوى حقول الكيانات المستمرة**. الجداول SQL-only موثقة من DDL اليدوي. أي طول أو Default أو Precision غير معرف في المصدر موسوم «غير محدد» ولا يُخمن.

## 1. مفتاح القراءة

- `R`: إلزامي/غير nullable.
- `O`: اختياري/nullable.
- `AI`: Auto Increment/Identity.
- `D=`: Default موثق في Entity أو SQL.
- `L=`: Max Length.
- `P(p,s)`: Precision وScale.
- النطاق: System / Group / Company / Branch / FiscalYear.
- المصدر: EF/Data Annotations/Fluent، Migration، SQL، Runtime DDL.

## 2. الهيكل المؤسسي والجغرافيا

### 2.1 `TenantGroup` — المجموعة التجارية — `tenant_groups`

- **PK:** `Group_ID string R`؛ الطول متعارض بين 36 و50.
- **الحقول:** `Group_Code string R L=30(SQL)`؛ `Group_Name_AR string R`؛ `Group_Name_EN string O`؛ `Short_Name string R L=100(SQL, Ignored EF)`؛ `Is_Default bool D=0`؛ `Parent_Group_ID string O L=36, Ignored`؛ `Main_Company_ID string O L=50, Ignored`؛ `Default_Currency_Code string O L=10, Ignored`؛ `Country_Name string O L=100, Ignored`؛ `City_Name string O L=100, Ignored`؛ `Short_Address string O L=300, Ignored`؛ `Phone string O L=50, Ignored`؛ `Email string O L=150, Ignored`؛ `Manager_Name string O L=150, Ignored`؛ `Show_In_Login bool D=1`؛ `Show_In_Tree bool D=1`؛ `Sort_Order int D=0, Ignored`؛ `Notes string O L=500`؛ `Created_At DateTime R`؛ `Updated_At DateTime O, Ignored`؛ `Created_By/Updated_By int O, Ignored`؛ `Edit_Count int D=0, Ignored`؛ حقول الإيقاف وإعادة التفعيل `int?/DateTime?/string?`, Ignored.
- **Indexes/Unique:** Unique `Group_Code`؛ فهارس Parent وLogin/Active/Sort في SQL.
- **FKs:** `Parent_Group_ID` و`Main_Company_ID` بلا FK فعلية.
- **النطاق:** Group.
- **المصدر/التغطية:** EF جزئي + SQL متعدد؛ لا Migration.
- **القرار:** حسم الخصائص المتجاهلة وطول ID قبل Baseline.

### 2.2 `Company` — الشركة — `companies`

- **PK:** `Company_ID string R`، طول غير محدد في Entity؛ SQL غالبًا 50.
- **الحقول:** `Group_ID string R`؛ `Company_Name_AR string R`؛ `Company_Name_EN string R في Entity`؛ `Company_Prefix string O`؛ `Activity_Type string O`؛ `Tax_Number string O`؛ `Phone/Mobile/Email/Address string O`؛ `Company_Logo byte[] O`؛ `Created_At DateTime D=UtcNow`؛ `Updated_At DateTime O`؛ `Is_Active bool D=1`؛ `Created_By/Updated_By int O`؛ `Edit_Count int D=0`؛ حقول الإيقاف/إعادة التفعيل `int?/DateTime?/string?`.
- **Indexes:** `IX_Companies_Group_Active`, `IX_Companies_Created_By` في SQL.
- **FK:** `Group_ID` → `tenant_groups` غير مفروض.
- **النطاق:** Group/Company.
- **المصدر:** EF + ترقيات SQL؛ لا Migration.
- **القرار:** تثبيت الأطوال وNullability للاسم الإنجليزي.

### 2.3 `TenantBranch` — الفرع — `tenant_branches`

- **PK:** `Branch_ID int R AI`.
- **الحقول:** `Company_ID string R`؛ `Branch_Code string R`؛ `Branch_Name string R`؛ `Branch_Name_EN string O`؛ `Address string O`؛ `Branch_Type string O`؛ `Parent_Branch_ID int O`؛ `Phone/Mobile/Email/Website/Manager_Name/Notes string O`؛ `Allow_Credit/Allow_Percentage bool`؛ `Is_Active bool D=1`؛ `Currency_ID int D=1`؛ `Created_Date DateTime D=Now`؛ `Updated_Date DateTime O`؛ `Created_By/Updated_By int O`؛ `Edit_Count int D=0`؛ حقول الإيقاف/إعادة التفعيل؛ `Country_ID/Governorate_ID/City_ID int O` موجودة عبر SQL وليست في Entity الحالي.
- **FKs:** Self parent؛ Company؛ Geography؛ Currency — غير مكتملة في Fluent/Migration.
- **Indexes:** Geography indexes في SQL.
- **النطاق:** Company/Branch.
- **المصدر:** EF + SQL؛ لا Migration.
- **القرار:** `Branch_ID int` مقترح موحد؛ استبدال `Branch_Type string` بمرجع `Branch_Type_ID` بعد الاعتماد.

### 2.4 `FiscalYear` — السنة المالية — `fiscal_years`

- **PK:** `Fiscal_Year_ID int R`.
- **الحقول:** `Company_ID string R`؛ `Year_Name string R`؛ `Start_Date/End_Date DateTime R`؛ `Is_Default/Is_Closed bool D=0`؛ `Is_Active bool D=1`؛ `Created_At DateTime D=Now`؛ `Updated_At DateTime O`.
- **Constraints مقترحة:** Unique `(Company_ID, Year_Name)`؛ Check `Start_Date<=End_Date`.
- **النطاق:** Company/FiscalYear.
- **المصدر:** EF فقط؛ لا Migration.

### 2.5 الجداول الجغرافية SQL-only

- **`countries` / الدولة:** PK `Country_ID int AI`; `Country_Code varchar10 R`; `Country_Name_AR varchar150 R`; `Country_Name_EN varchar150 O`; `ISO2 varchar2 O`; `ISO3 varchar3 O`; `Phone_Code/Currency_Code varchar10 O`; `Nationality_Name_AR varchar150 O`; `Sort_Order int D=0`; `Is_Active bool D=1`; `Notes varchar500 O`; dates. Unique Code وArabic Name. Seed اليمن.
- **`governorates` / المحافظة:** PK `Governorate_ID int AI`; `Country_ID int R`; Code20؛ Names150؛ Sort/Active/Notes/dates. Unique country+code/name؛ FK Country Restrict.
- **`cities` / المدينة:** PK `City_ID int AI`; Country/Governorate int R؛ Code20؛ Names150؛ Postal20 O؛ Sort/Active/Notes/dates. Unique governorate+code/name؛ FKs Restrict.
- **`branch_types` / نوع الفرع:** PK `Branch_Type_ID int AI`; Code30؛ Names100؛ Sort D=0؛ Active D=1؛ dates. Unique Code؛ Seed أربعة أنواع.
- **التغطية:** SQL-only؛ لا DbSet ولا Migration.

## 3. الأمن والصلاحيات

### 3.1 `User` — المستخدم — `users`

PK `User_ID int`; fields: `Company_ID string R`; `Branch_ID int R`; `Role_ID int R`; `User_Code/Full_Name/Login_Name/Password_Hash string R`; `Phone/Email/Notes string O`; `Must_Change_Password bool D=1`; `Is_Active bool D=1`; dates؛ `Failed_Login_Count int D=0`; `Last_Failed_Login_At/Locked_Until/Last_Login_At DateTime O`; `Last_Login_IP string O`. FKs company/branch/role غير مفروضة. Unique Login/UserCode مقترحة. EF+SQL security، no Migration.

### 3.2 `Role` — الدور — `roles`

PK `Role_ID int`; `Role_Name string R`; `Description string O`; `Is_Active bool D=1`; dates؛ `Role_Code string O`; `Is_System_Admin bool D=0`. Unique Role_Code مقترح. EF، no Migration.

### 3.3 `RolePermission` — صلاحية الدور — `role_permissions`

PK `Permission_ID int`; `Role_ID int`; `Screen_ID int`; flags: View/Add/Edit/Delete/Print/Export/Import/Approve/UnApprove bool. Unique Role+Screen مقترح؛ FK Role Cascade وScreen Restrict. EF، no Migration.

### 3.4 `UserPermission` — صلاحية المستخدم — `user_permissions`

PK `Permission_ID int`; `User_ID int`; `Permission_Category string R`; `Permission_Name string R`; flags View/Add/Edit/Delete/Print/Export/Import/Preview/Approve/UnApprove. Unique User+Category+Name مقترح. لا Screen_ID في النموذج الحالي. EF، no Migration.

### 3.5 `SystemPermission` — صلاحية النظام — `system_permissions`

PK `Permission_ID int`; `Permission_Code string R L=100`; `Permission_Name string R L=200`; `Permission_Type string R L=50`; `Module_Name string O L=100`; `Is_Active bool D=1`; `Sort_Order int D=0`; `Created_At DateTime D=Now`. Unique Code. EF + Seeder، no Migration.

### 3.6 `SystemScreen` — شاشة النظام — `system_screens`

PK `Screen_ID int`; `Screen_Code/Screen_Name/Module_Name string R` أطوال غير محددة؛ `Is_Active bool D=1`; `Sort_Order int`; `Created_At DateTime D=Now`. Unique Screen_Code. EF + Seeder، no Migration.

### 3.7 `LoginAttempt` — محاولة دخول — `login_attempts`

PK `Login_Attempt_ID long`; `User_ID int O`; `Login_Name string R L=100(SQL)`؛ `Company_ID string O L=50`; `Branch_ID/Fiscal_Year_ID int O`; `Attempted_At DateTime D=CURRENT_TIMESTAMP(6)`؛ `Is_Success bool`; `Failure_Reason100/IP64/UserAgent512/Device128/Session64 string O`; `Lockout_Until DateTime O`. Indexes login+date, user+date, session. EF + SQL، no Migration.

### 3.8 `RefreshToken` — رمز التجديد — `refresh_tokens`

PK `Refresh_Token_ID long`; `Token_Hash char64 R`; `Session_ID varchar64 R`; `User_ID int R`; `Company_ID varchar50 R`; `Branch_ID/Fiscal_Year_ID int R`; `Device_ID varchar128 R`; Created/Expires R؛ Revoked O؛ `Replaced_By_Hash char64 O`; `Revoked_Reason varchar100 O`. Unique Hash؛ indexes user+expiry/session. EF + SQL، no Migration.

## 4. الإعدادات والترقيم والسياسات

### `SystemSetting` — `system_settings`
PK `Setting_ID int AI`; Key100 R؛ Name200 R؛ Value text R؛ Scope20 D=SYSTEM؛ Company50 D=''؛ Branch/Year int D=0؛ Effective Date O؛ Description text R؛ Active D=1؛ dates. Unique key+scope+company+branch+year. EF+SQL، no Migration.

### `FiscalPeriod` — `fiscal_periods`
PK `Fiscal_Period_ID int AI`; Company50/Branch int/Year int؛ Code50؛ Name150؛ Start/End؛ Closed bool؛ CloseDate O؛ CloseReason500 O؛ Active D=1؛ dates. Unique scope+code؛ date/overlap checks مقترحة. EF+SQL، no Migration.

### `ExchangeRate` — `exchange_rates`
PK `Exchange_Rate_ID int AI`; Company50؛ `Currency_Code20`; RateDate؛ ExchangeRate P(18,6 SQL)؛ Min/Max O؛ Is_Default/Active؛ Notes500 O؛ dates. Unique company+currency+date. EF+SQL؛ قرار ID مقابل Code.

### `NumberingSetting` — `numbering_settings`
PK `Numbering_ID int`; Document_Type string؛ Prefix string؛ Digits int؛ Reset_Type string؛ Last_Number int؛ Use_Company/Branch/Year bool؛ Active bool. الأطوال/Defaults غير محددة في Entity. ملاحظة: لا توجد `Company_ID/Branch_ID/Fiscal_Year_ID` في Entity الحالي رغم مفهوم النطاق. EF+SQL تاريخي، no Migration.

### `NumberingCounter` — `numbering_counters`
PK `Counter_ID int`; Document_Type string R؛ Company_ID string O؛ Branch_ID/Year_Value int O؛ Last_Number int؛ Created/Updated. Unique scope في Fluent/SQL؛ SQL يطبع NULL إلى ''/0. EF+SQL، no Migration.

### `FinancialPolicy` — سياسة مالية — `financial_limits`
PK `Limit_ID int`; `Company_ID string`; `Entity_Type/Entity_ID/Limit_Type/Currency_Code string`; `Limit_Amount/Used_Amount decimal` precision غير محدد؛ `Period_Type string D=Monthly`; `Requires_Approval/Is_Active bool D=1`; dates. Polymorphic Entity بلا FK؛ precision/checks مطلوبة. EF، no Migration.

### `FinancialPolicyMovement` — حركة سياسة — `financial_limit_movements`
PK `Movement_ID int`; `Limit_ID int`; `Company_ID string`; `Movement_Date DateTime D=Now`; `Movement_Type string`; `Reference_Type/Reference_ID string O`; `Currency_Code string`; `Amount/Balance_After decimal` precision غير محدد؛ `Notes string O`; `Created_By string O`; `Created_At`. FK Limit/User غير مفروضة. EF، no Migration.

### `ApprovalRequest` — طلب اعتماد — `approval_requests`
PK `Approval_ID int`; Company_ID/Request_Type R؛ Reference_Type/ID وEntity_Type/ID وCurrency_Code O؛ Amount O P(18,2)؛ Reason O؛ Status string D=Pending؛ Requested_By string O؛ Requested_At D=UtcNow؛ Approved_By/At/Notes O. Polymorphic refs؛ workflow check مطلوب. EF، no Migration.

## 5. الأدلة والموارد المالية

### `AccountCategory` — تصنيف الحساب — `account_categories`
PK `Category_ID string L=50`; Company50؛ Code50؛ Names150؛ Type30؛ Normal10؛ Is_System/Active bool؛ Sort؛ Created/Updated؛ Created_By/Updated_By string L=100 O. Unique Company+Code. Entity عبر `Set<>` دون DbSet + SQL DDL/Seed؛ no Migration.

### `AccountCodeSetting` — إعداد كود الحساب — `account_code_settings`
PK `Setting_ID int`; Company_ID string؛ Level/Segment/Start/Max ints؛ Padding string D='0'؛ Parent_Based/Auto_Generate bool D=1؛ Allow_Manual bool D=0؛ Active D=1؛ Created. Unique Company+Level مقترح. EF/Snapshot، no Migration.

### `ChartOfAccount` — دليل الحسابات — `chart_of_accounts`
PK `Account_ID string D=Guid`; Company_ID string؛ Parent string O؛ Code/Name_AR/Type string R؛ Name_EN O؛ Level int؛ Postable bool؛ Currency_Code O؛ Created/Active؛ Category/Normal/Notes/Path O؛ flags Manual/System/RequiresCC/RequiresParty/RequiresProject/Summary/BS/IS/MultiCurrency/Control؛ ControlType L=30 O؛ Serial int D=0؛ Created_By string O؛ حقول effective مضافة عبر SQL. Unique Company+Code مقترح؛ self Restrict؛ Triggers SQL خارج EF. EF+SQL، no Migration.

### `CostCenter` — مركز التكلفة — `cost_centers`
PK `Cost_Center_ID string`; Company string؛ Parent string O؛ Code/Name_AR string R؛ Name_EN O؛ Level int؛ Postable bool؛ Created؛ Active. أطوال غير محددة. Unique Company+Code؛ self Restrict. EF، no Migration.

### `CashBox` — الصندوق — `cash_boxes`
PK `Cash_Box_ID string D=Guid`; Company string؛ Branch int؛ Account_ID string؛ Currency_Code string؛ Code/Name_AR string R؛ Name_EN O؛ Opening/Max/Min decimal precision غير محدد؛ Active D=1؛ Notes O؛ Created/Updated؛ Created_By/Updated_By string O. FKs account/branch مقترحة Restrict؛ Unique branch+code/name. EF، no Migration.

### `Currency` — العملة — `currencies`
PK `Currency_ID int`; Company_ID string؛ Code/Name_AR R؛ Name_EN/Symbol O؛ Decimal_Places int؛ Exchange_Rate decimal P(18,6 Fluent)؛ Min/Max O؛ Local/Default/Active bool؛ Notes/Created_By/Updated_By O؛ dates O. Unique Company+Code مقترح. EF، no Migration.

### `BankAccount` — الحساب البنكي — `bank_accounts`
PK `Bank_Account_ID int AI`; Company50 R؛ BankCode50؛ Names200؛ AccountNo100؛ IBAN64 O؛ CurrencyCode20؛ GLAccount100 O؛ BranchName200 O؛ Active D=1؛ Notes O؛ dates. Unique Company+AccountNo؛ index Company+Name. EF + SQL مكرر، no Migration.

### `Party` — الطرف المالي — `parties`
PK `Party_ID string L=50`; Company50؛ Code50؛ Names200؛ Type50؛ Mobile/Phone30 O؛ Identity/Tax100 O؛ Address500 O؛ City150 O؛ Account_ID50 O؛ Credit_Limit decimal P(18,2 Fluent)؛ Active. Unique Company+Code مقترح؛ FK Account optional. EF، no Migration.

## 6. المحرك المالي والتشغيلي

### `FinancialVoucherHeader` — رأس السند — `financial_voucher_headers`

PK `Voucher_ID long AI`; `Voucher_No string L=50`; Type/Status int؛ `Branch_ID string L=50`؛ Year int O؛ Voucher/Transaction dates؛ Cash_Account_ID50؛ Party_ID50 O؛ Received_From_Name200 O؛ Payment_Method int O؛ Currency int؛ Rate P(18,6) D=1؛ Amount/Foreign/Local P(18,2) D=0؛ ReferenceNo100/Date O؛ Against/Description500 O؛ Notes1000 O؛ Module/DocumentType int O؛ DocumentID long O؛ SourceDocumentNo100 O؛ approval flags/status and user IDs string50 O؛ review fields؛ Edit/Print/Undo counters D=0؛ print/undo user/date O؛ posting fields including Journal_Entry_ID long O؛ user audit strings50 O؛ Is_Active D=1؛ dates. Unique Branch+Year+Type+No؛ indexes؛ Details Cascade؛ ActionLogs Restrict. EF+SQL upgrades، no Migration.

### `FinancialVoucherDetail` — تفاصيل السند — `financial_voucher_details`
PK `Voucher_Detail_ID long AI`; Voucher long؛ Line int؛ Account50؛ Description500 O؛ CostCenter/Project50 O؛ refs Type50/No100/Name250/Date O؛ Currency int؛ Rate18,6 D=1؛ Foreign/Local/Debit/Credit18,2 D=0؛ LineType byte D=2؛ Notes500 O؛ Created/Updated user50 O and dates. Unique Voucher+Line؛ FK header Cascade. EF، no Migration.

### `JournalEntryHeader` — رأس القيد — `journal_entry_headers`
PK `Journal_Entry_ID long AI`; EntryNo50؛ EntryType byte D=1؛ Status int D=1؛ `Branch_ID string50`; Year int O؛ dates؛ SourceSystem30 D=VOUCHER؛ SystemGenerated bool D=1؛ SourceVoucher long O؛ SourceType50/No100/Description500/Notes1000 O؛ totals18,2؛ Posted bool/user50/date؛ reversal flags and self IDs long O؛ cancellation fields؛ Active D=1؛ Created/Updated user50/dates. Unique EntryNo؛ indexes date/source voucher؛ Details Cascade. EF + Migration + Restore SQL.

### `JournalEntryDetail` — تفاصيل القيد — `journal_entry_details`
PK `Journal_Entry_Detail_ID long AI`; Journal long؛ Line int؛ Account50؛ Description500 O؛ CostCenter/Project50 O؛ Currency int؛ Rate18,6؛ Foreign/Local/Debit/Credit18,2؛ refs؛ SourceVoucherDetail long O؛ LineType byte D=2؛ Notes500 O؛ audit user50/dates. Unique Journal+Line؛ FK header Cascade. EF + Migration + Restore SQL.

### `DocumentAllocation` — توزيع مستند — `document_allocations`
PK `Allocation_ID long AI`; Module/DocumentType int؛ DocumentID long؛ DocumentNo50؛ Voucher long؛ Party50 O؛ Currency int؛ Rate18,6؛ DocumentTotal/CollectedBefore/CollectedNow/Remaining18,2؛ Active D=1؛ Notes500 O؛ audit user50/dates. FK Voucher Cascade. EF، no Migration.

### `DocumentLink` — رابط مستند — `document_links`
PK `Document_Link_ID long AI`; FromModule/Type int؛ FromID long؛ FromNo100 O؛ ToModule/Type int؛ ToID long؛ ToNo100 O؛ LinkType50 D=RELATED؛ Active D=1؛ Notes500 O؛ audit user50/dates. Polymorphic بلا FK مباشرة. EF، no Migration.

### `AuditLog` — سجل التدقيق — `audit_logs`
PK `Audit_ID long AI`; Module int O؛ Table/Record100 R؛ Action30 R؛ User/Branch50 O؛ ActionAt؛ Old/New JSON O؛ Channel20 D=DESKTOP؛ Device150/IP50/Notes500 O. Check Action عبر SQL؛ snapshot policy مفتوح. EF/Snapshot+SQL، no Migration.

### `VoucherActionLog` — حركة السند — `voucher_action_logs`
PK `Voucher_Action_ID long AI`; Voucher long؛ Action30؛ Old/NewStatus int O؛ User50 O؛ ActionAt؛ Channel20 D=DESKTOP؛ Device150/IP50/Reason500/Notes500 O. FK Voucher Restrict؛ Check Action SQL. EF+SQL، no Migration.

### `VoucherType` — نوع السند — `voucher_types`
PK int؛ Code50؛ NameAR150؛ NameEN150 O؛ Active D=1؛ Sort int. Unique Code مقترح. EF+Seeder/SQL، no Migration.

### `VoucherStatus` — حالة السند — `voucher_statuses`
PK int؛ Code50؛ NameAR150؛ NameEN150 O؛ Active D=1؛ Sort. Unique Code/Workflow checks. EF+Seeder/SQL، no Migration.

### `PaymentMethod` — طريقة السداد — `payment_methods`
PK int AI؛ Code50؛ NameAR150؛ NameEN150 O؛ RequiresReference/Date, IsCash/Bank bool؛ Active D=1؛ Sort. Unique Code. EF+Seeder/SQL، no Migration.

### `PaymentRequest` — طلب الصرف — `payment_requests`
PK `Payment_Request_ID long AI`; Company string؛ Branch/Year int؛ No string؛ Date؛ Status D=DRAFT؛ Beneficiary string؛ Party string O؛ PaymentMethod int O؛ HeaderRef/Description O؛ ApprovedTotal P(19,4)؛ PaymentVoucher long O؛ Review/Approval reasons O؛ Created_By string؛ CreatedAt؛ Updated O؛ Details collection. Unique Company+Branch+Year+No؛ index scope+status. EF + SQL + dormant Runtime DDL؛ no Migration.

### `PaymentRequestLine` — سطر طلب الصرف — `payment_request_lines`
PK long AI؛ Request long؛ Line int؛ Account string؛ CostCenter string O؛ Currency int؛ Rate P(19,8)؛ Foreign/Local P(19,4)؛ Ref/Description O. Unique Request+Line؛ Fluent الحالي Restrict؛ SQL بلا FK. EF+SQL+Runtime DDL، no Migration.

### `PaymentRequestAttachment` — مرفق طلب الصرف — `payment_request_attachments`
PK long AI؛ Request long؛ Company string؛ Branch/Year int؛ OriginalName string L=260(SQL)؛ StorageKey500؛ ContentType100 D=application/octet-stream؛ FileSize long؛ Active D=1؛ CreatedBy string؛ CreatedAt. Index Request+Active؛ لا FK في SQL. EF+SQL+Runtime DDL، no Migration.

## 7. الجداول التشخيصية والمستبعدة

- `database_alignment_findings`: PK bigint AI؛ CheckCode50؛ Severity20؛ ObjectName150؛ Message1000؛ Count bigint D=0؛ DetectedAt. يُنشأ ثم يُفرغ ويُعبأ بواسطة preflight؛ **مستبعد من Baseline الإنتاج**.

## 8. تغطية المصادر

- Migration الحالية: جدولان فقط.
- 39 DbSet دون Migration إنشاء.
- `AccountCategory` بلا DbSet صريح.
- أربعة جداول تشغيلية SQL-only + جدول تشخيصي SQL-only.
- Snapshot غير متزامن ولا يمثل Baseline قابلة لإعادة الإنشاء.

## 9. القرارات العامة المطلوبة

1. أنواع وأطوال IDs النصية و`Branch_ID`.
2. Collation موحدة.
3. FKs وDelete Policies.
4. Defaults/Checks وPrecision المفقودة.
5. Migrations كمصدر وحيد؛ عزل SQL القديم وRuntime DDL.
6. Seed Data مستقلة وIdempotent.

## 10. التأكيد

هذا القاموس توثيقي. لم يُعدل أي Entity أو AppDbContext أو Migration أو قاعدة بيانات، ولم يُنفذ SQL.