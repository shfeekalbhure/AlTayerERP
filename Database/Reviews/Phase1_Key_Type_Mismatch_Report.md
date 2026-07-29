# تقرير اختلاف أنواع المفاتيح — المرحلة الأولى

**مبدأ المرحلة:** توثيق فقط؛ لا تغيير لأي نوع.

## 1. الملخص

أكبر خطر قبل Baseline هو عدم ثبات أنواع المفاتيح بين C# وMigration وSQL والاستخدام داخل API. بعض المفاتيح نصية في كيانات، وبعضها رقمية في أجزاء أخرى، وتوجد حقول مستخدم/فرع محفوظة كنصوص في سجلات القيود والتدقيق.

## 2. جدول المفاتيح الحرجة

| المفتاح | النوع الغالب في C# | دليل Migration/SQL | موضع الاختلاف | القرار المقترح |
|---|---|---|---|---|
| `Group_ID` | `string` | SQL يستخدم `VARCHAR` | بعض DTOs/تاريخ قديم تعاملت معه كـint | يبقى نصياً إن كان GUID، مع طول ثابت موحد |
| `Company_ID` | `string` | SQL يستخدم `VARCHAR(50)` | بعض العلاقات لا تحمل FK فعلية | اعتماد نصي موحد مع FK لكل الجداول التابعة |
| `Branch_ID` | غالباً `int` | Migration القيود تستخدم `varchar(50)` | اختلاف مؤكد | اعتماد `int` للنظام الحالي أو توثيق سبب التحويل إلى string؛ لا خليط |
| `Fiscal_Year_ID` | `int` | Migration القيود `int?` | nullability تختلف حسب المستند | اعتماد `int` وإلزامه في المستندات المحاسبية بعد التهيئة |
| `Currency_ID` | `int` | Migration `int` | بعض SQL يستخدم `Currency_Code` بدلاً من ID | اعتماد ID للعلاقات وCode للعرض فقط |
| `User_ID` | `int` | حقول `Created_By/Posted_By` في Migration نصية | اختلاف مؤكد | اعتماد `int?` في الجداول التشغيلية، ونص فقط للهوية الخارجية إن وجدت |
| `Role_ID` | `int` | لا Migration تغطيه | DTOs متوافقة غالباً | اعتماد `int` مع FK |
| `Account_ID` | `string` في Migration القيود | كيانات الدليل قد تستخدم معرف/كود نصي | التباس بين المفتاح والكود | فصل `Account_ID` العددي عن `Account_Code` النصي أو اعتماد نصي رسمياً |
| `Cost_Center_ID` | يظهر نصياً في Migration | كيان مستقل قد يستخدم int/string حسب النسخة | اختلاف محتمل مرتفع | حسم نوع واحد وفصل الكود عن المعرف |
| `Cash_Box_ID` | غالباً `int` | SQL/Entity يجب مراجعته نهائياً | لا Migration شاملة | اعتماد `int` مبدئياً |
| `Bank_Account_ID` | `int` | SQL `INT AUTO_INCREMENT` | متوافق | تثبيت FK من السندات/طرق الدفع |
| `Party_ID` | غالباً `int/long` حسب الكيان | لا Baseline | يحتاج تثبيت | اعتماد نوع واحد قبل علاقات السندات |
| `Voucher_ID` | `long` | Migration/خدمات مالية تستخدم `bigint` | متوافق غالباً | تثبيت `long` |
| `Payment_Request_ID` | غير مثبت في Migration | Entity موجود | لا مصدر إنشاء موحد | مراجعة Entity/DTO ثم اعتماد `long` أو `int` |
| `Journal_Entry_ID` | `long` | Migration `bigint` | متوافق | تثبيت `long` |

## 3. اختلافات مؤكدة ذات أولوية قصوى

### 3.1 Branch_ID

Migration `AddJournalEntryTables` تعرف `Branch_ID` كـ`varchar(50)`، بينما سياق الجلسة وكيان الفروع وواجهات كثيرة تستخدم `int`. هذا يمنع FK مباشرة بين `journal_entry_headers` و`tenant_branches`.

### 3.2 User references

`Created_By`, `Updated_By`, `Posted_By`, `Cancelled_By`, `Reversed_By` في Migration القيود نصوص. `users.User_ID` في الكود عدد صحيح. يلزم تحويلها إلى `int?` أو تعريف هوية نصية مستقلة؛ الوضع المختلط غير مقبول في Baseline.

### 3.3 Account_ID وCost_Center_ID

Migration القيود تستخدم `varchar(50)`. يجب تحديد هل هذه قيم PK أم أكواد أعمال. إن كانت أكواداً، يجب إضافة PK رقمي مستقل أو توثيق أن الكود نفسه هو PK في كل النظام.

### 3.4 Company_ID وGroup_ID

كلاهما نصي حالياً. يجب تثبيت الطول (`char(36)` للـGUID أو `varchar(50)` لكود مولد) وتوحيد Collation قبل إنشاء FK نصية.

## 4. Nullability

- المفاتيح الخارجية التشغيلية مثل الشركة والفرع والسنة والعملة يجب أن تكون إلزامية بعد مرحلة المسودة، لكن بعض الجداول تسمح بـNULL.
- مفاتيح التدقيق (`Created_By`, `Updated_By`) يمكن أن تكون nullable عند ترحيل بيانات قديمة، مع FK `SET NULL`.
- `Parent_Branch_ID`, `Parent_Account_ID`, `Parent_Cost_Center_ID` يجب أن تكون nullable مع `Restrict` أو `NoAction` عند الحذف.

## 5. أثر الاختلاف على التطبيقات

- API يضطر للتحويل بين string وint.
- Desktop/Mobile قد يرسل قيمة نصية بينما Controller يتوقع رقمية.
- MySQL قد يرفض FK بسبب اختلاف النوع أو الطول أو Collation.
- الفهارس النصية أكبر وأبطأ من العددية في الجداول عالية الحركة.
- ترحيل البيانات لاحقاً يصبح أكثر خطراً إذا لم يُحسم النوع قبل Baseline.

## 6. قرارات تحتاج موافقة

1. هل `Group_ID` و`Company_ID` GUID نصية نهائياً؟
2. هل `Branch_ID` يجب أن يكون `int` في كل النظام؟
3. هل `Account_ID` و`Cost_Center_ID` معرفات أم أكواد أعمال؟
4. هل حقول المستخدم في التدقيق FK عددية أم snapshots نصية؟
5. نوع `Party_ID` و`Payment_Request_ID` النهائي.

## 7. حالة التقرير

لم يُعدّل أي Entity أو DTO أو Migration أو SQL.
