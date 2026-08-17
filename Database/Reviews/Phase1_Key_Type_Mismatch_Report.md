# تقرير اختلاف أنواع المفاتيح — المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**الحالة:** توثيق فقط؛ لا تغيير لأي نوع ولا تنفيذ Migration أو SQL.

## 1. الأنواع الفعلية المؤكدة

| المفتاح | النوع في Entity | مصادر أخرى | الحالة والتوصية المقترحة |
|---|---|---|---|
| `Group_ID` | `string` | `tenant_groups` و`companies.Group_ID` نصيان؛ سكربتات تستخدم `VARCHAR(36/50)` | نصي حاليًا؛ تثبيت صيغة وطول واحد قبل FK |
| `Company_ID` | `string` | أغلب SQL يستخدم `VARCHAR(50)` | نصي متسق غالبًا، لكن FKs غير مكتملة |
| `Branch_ID` | `int` في `TenantBranch`, `User`, `PaymentRequest` | `string/VARCHAR(50)` في `FinancialVoucherHeader`, `JournalEntryHeader` وMigration القيود | اختلاف حرج؛ راجع تقرير Branch المستقل |
| `Fiscal_Year_ID` | `int` | `int?` في السند/القيد و`int` في طلب الصرف | اختلاف nullability لا النوع |
| `Currency_ID` | `int` | بعض الجداول تخزن `Currency_Code` فقط مثل الصندوق والبنك | اعتماد ID للعلاقات مع Code كقيمة أعمال |
| `User_ID` | `int` | حقول `Created_By`, `Posted_By`, `Reviewed_By` كثيرة منها `string/VARCHAR(50)` | اختلاف حرج بين FK رقمية وSnapshot نصي |
| `Role_ID` | `int` | مستخدم في `users.Role_ID` | متوافق؛ يحتاج FK وUnique للربط |
| `Account_ID` | `string` | Migration القيود وطلبات الصرف `VARCHAR(50)` | متسق نصيًا؛ يجب تثبيت الطول والـCollation وفصل الكود عن المعرف |
| `Cost_Center_ID` | `string` | Migration/طلبات الصرف `VARCHAR(50)` | متسق نصيًا؛ يحتاج FK فعلية وطول موحد |
| `Cash_Box_ID` | `string` GUID | API routes وDesktop تتعامل معه كنص | صحح: ليس `int`; مقترح إبقاؤه نصيًا إن اعتمد GUID |
| `Bank_Account_ID` | `int` | SQL `INT AUTO_INCREMENT` | متوافق |
| `Party_ID` | `string` | السند وطلب الصرف يستخدمان `string?`/`VARCHAR(50)` | صحح: نصي وليس int/long |
| `Voucher_ID` | `long` | `BIGINT` | متوافق |
| `Payment_Request_ID` | `long` | SQL وSchemaInitializer `BIGINT` | صحح: `long` متوافق |
| `Journal_Entry_ID` | `long` | Migration/SQL `BIGINT` | متوافق |

## 2. التأثير حسب المفتاح

### Group_ID
- **الجداول:** `tenant_groups`, `companies`، وخصائص الأب/الشركة الرئيسية التاريخية.
- **API/Desktop:** إدارة المجموعات والشركات تستخدم النص مباشرة.
- **Mobile:** لا يظهر كمعرف إدخال مباشر حاليًا.
- **المراجع النصية:** GUID/`VARCHAR(36/50)` مختلطة الأطوال.
- **المقترح:** إبقاؤه نصيًا بطول ثابت يحدده قرار الهوية.
- **أثر التغيير:** تغيير الطول/الصيغة يؤثر على الشركات والفهارس وCollation.
- **Migration بيانات مستقبلية:** نعم عند تغيير الصيغة أو الطول.

### Company_ID
- **الجداول:** معظم الجداول المؤسسية والمالية والأمنية.
- **APIs:** كل Scoping تقريبًا يعتمد النص.
- **Desktop/Mobile:** محفوظ في الجلسة ويرسل ضمن السياق.
- **المقترح:** نصي موحد `VARCHAR(50)` أو `CHAR(36)` بعد اعتماد صيغة الهوية.
- **Migration بيانات مستقبلية:** نعم إذا تم التحويل إلى رقم أو تغيير صيغة GUID.

### Branch_ID
- **الجداول المتأثرة:** `tenant_branches`, `users`, `fiscal_periods`, `numbering_*`, `payment_requests`, `financial_voucher_headers`, `journal_entry_headers`, `audit_logs` وغيرها.
- **APIs:** الجلسة والفروع وطلبات الصرف رقمية؛ خدمات السند والقيد تستعمل نصوصًا وتحويلات.
- **Desktop/Mobile:** DTOs المرجعية غالبًا رقمية؛ سندات قد تعرضه كنص.
- **المقترح الأولي:** `int`، لكن القرار معلق حتى تقرير القيم الفعلية.
- **Migration بيانات مستقبلية:** نعم وبدرجة خطورة عالية.

### User_ID وحقول التدقيق
- **الجداول:** `users.User_ID int` مقابل `Created_By/Updated_By/Posted_By/Reviewed_By` نصية في عدد من الجداول.
- **APIs:** كثير من الخدمات تستخدم `session.User_ID.ToString()`.
- **Desktop/Mobile:** لا ينبغي أن يرسلا هوية التدقيق؛ الخادم يملؤها.
- **المقترح:** FK رقمية nullable + حقول Snapshot منفصلة للاسم/الهوية الخارجية عند الحاجة.
- **Migration بيانات مستقبلية:** نعم، مع تحليل القيم غير الرقمية أولًا.

### Account_ID وCost_Center_ID
- **الجداول:** الدليل، مراكز التكلفة، تفاصيل السندات والقيود وطلبات الصرف والصناديق والأطراف.
- **APIs/Desktop/Mobile:** جميعها تتعامل مع معرفات نصية.
- **المراجع:** `VARCHAR(50)` غالبًا، مع غياب FKs في كثير من المصادر.
- **المقترح:** إبقاؤهما نصيين مؤقتًا، وتثبيت `VARCHAR(50)` وCollation موحدة، وفصل `Account_Code/Center_Code` عن PK.
- **Migration بيانات مستقبلية:** فقط عند التحويل لرقمي؛ وإلا Migration قيود وفهارس فقط.

### Cash_Box_ID
- **الجداول/API:** كيان الصندوق ينشئ GUID نصيًا وController routes تستخدم `string id`.
- **Desktop:** `CashBoxModel` والخدمات تتعامل مع النص.
- **Mobile:** السندات تعتمد الحساب المرتبط أكثر من معرف الصندوق.
- **المقترح:** `CHAR(36)` أو `VARCHAR(50)` نصي موحد، لا `int`.
- **Migration بيانات مستقبلية:** نعم عند تغيير الطول أو التحويل العددي.

### Party_ID
- **الجداول:** `parties`, `financial_voucher_headers`, `payment_requests`.
- **APIs/Desktop/Mobile:** خيارات الطرف وقيم السند نصية.
- **المقترح:** نصي موحد بطول 50 مع FK nullable.
- **Migration بيانات مستقبلية:** لا لتثبيت النوع النصي، نعم لإضافة FK أو تغيير الصيغة.

### Payment_Request_ID
- **الجداول:** الرأس والخطوط والمرفقات.
- **API/Mobile/Desktop:** routes وDTOs تستخدم `long`.
- **المقترح:** تثبيت `BIGINT`.
- **Migration بيانات مستقبلية:** لا لتغيير النوع؛ نعم لإنشاء المخطط من Baseline.

## 3. العلاقات التي لا يمكن فرضها حاليًا

- `journal_entry_headers.Branch_ID` و`financial_voucher_headers.Branch_ID` إلى `tenant_branches.Branch_ID`.
- حقول المستخدم النصية إلى `users.User_ID`.
- علاقات `Currency_Code` إلى `currencies.Currency_ID`.
- بعض علاقات الحساب ومركز التكلفة بسبب غياب قيود موحدة رغم توافق النوع النصي.

## 4. القرارات المطلوبة

1. صيغة وطول `Group_ID` و`Company_ID`.
2. قرار `Branch_ID` بعد تقرير القيم الفعلية.
3. هل حقول التدقيق FKs رقمية أم Snapshots نصية أو كلاهما.
4. اعتماد `Account_ID`, `Cost_Center_ID`, `Cash_Box_ID`, `Party_ID` نصية نهائيًا وتحديد طولها.
5. تثبيت `Payment_Request_ID`, `Voucher_ID`, `Journal_Entry_ID` كـ`BIGINT`.

## 5. التأكيد

لم يُعدّل أي Entity أو DTO أو Migration أو قاعدة بيانات.