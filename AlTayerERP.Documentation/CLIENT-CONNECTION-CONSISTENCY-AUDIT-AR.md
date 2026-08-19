# تقرير التدقيق العميق لتوحيد نقاط الاتصال والحقول

**المشروع:** AlTayerERP  
**النطاق:** تطبيق `Mobile.Office`، تطبيق `Desktop`، طبقة `API`، و`MySQL`  
**الغرض:** التحقق مما إذا كان العميلان يستخدمان نفس الحقول والمسارات ومصدر البيانات، وكشف أي اختلافات قد تجعل المنسدلات أو عمليات الحفظ تعرض بيانات مختلفة.  
**الحالة:** تدقيق ساكن (Static Audit) مبني على الكود الحالي، وليس بديلًا عن اختبار تكاملي بقاعدة MySQL فعلية.

## 1. الخلاصة التنفيذية

النتيجة الأساسية هي أن **الجوال وDesktop لا يتصلان بقاعدة MySQL مباشرة**. كلاهما يتصل بطبقة `AlTayerERP.API` عبر HTTP، والـAPI يحقن `AppDbContext` واحدًا ويستخدم `ConnectionStrings:DefaultConnection` للوصول إلى MySQL. لذلك، إذا كان العميلان موجَّهين إلى **نفس نسخة API**، فهما يقرآن ويكتبان في قاعدة البيانات نفسها.[1] [2] [3]

لكن التوحيد ليس كاملًا على مستوى العقد البرمجي. فالمصدر النهائي للبيانات مركزي، بينما **المسارات وDTOs وحقول JSON وشروط التصفية متعددة**. الجوال يستخدم Controllers مخصصة تحت `api/mobile/...` لمنسدلات السندات والتقارير وبعض الوظائف، في حين يستخدم Desktop مسارات عامة مثل `FinancialVoucherLookups` و`FinancialVoucher/accounts`. كلا المسارين يقرآن من جداول محاسبية واحدة، لكنهما قد يعيدان قوائم مختلفة بسبب اختلاف الشروط أو طريقة تشكيل الحقول.[4] [5]

> **الحكم العملي:** لا توجد حاليًا قرينة على أن الجوال مربوط بقاعدة مختلفة من داخل الكود، لكن توجد قرينة مؤكدة على وجود **عقود متعددة لنفس البيانات**. كما أن الجوال يمكنه اختيار عنوان Wi‑Fi مختلف؛ فإذا كان هذا العنوان يشير إلى خادم API آخر، فقد تكون قاعدة البيانات مختلفة تبعًا لإعداد ذلك الخادم. لهذا لا ينبغي اعتماد عبارة «قاعدة واحدة دائمًا» إلا بعد تثبيت عنوان API وقراءة `databaseName` من `/api/health` في العميلين.[6]

## 2. خريطة الاتصال الفعلية

| الطبقة | Desktop | Mobile.Office | النتيجة |
|---|---|---|---|
| الاتصال الأول | `http://localhost:5021/api/` | USB: `http://127.0.0.1:5021/`، أو Wi‑Fi: `http://<host>:<port>/` | غالبًا نفس جهاز التطوير عند استخدام المنفذ 5021، لكن العنوانين قابلان للاختلاف فعليًا |
| عميل HTTP | `ApiService.Client` مشترك | `HttpClient` واحد مسجل Singleton | لا يوجد عميل MySQL داخل أي تطبيق |
| فحص الجاهزية | يعتمد على الوصول إلى API | يفحص `GET api/health` ويتحقق من `api=ready` و`database=ready` | نقطة التحقق المركزية هي API وليس قاعدة البيانات من العميل |
| التوثيق | `Authorization: Bearer` و`X-Session-Token` | Bearer/جلسة التطبيق وفق استجابة الدخول | الخادم يتحقق من الجلسة والنطاق في `AppDbContext` نفسه |
| قاعدة البيانات | غير مرئية للعميل؛ يحددها API | غير مرئية للعميل؛ يحددها API الذي تم اختياره | نفس القاعدة فقط إذا كانت نسخة API نفسها تستخدم نفس ConnectionString |

ملف Desktop يثبت أن العميل يستخدم عنوان API مركزيًا ولا يحتوي على ConnectionString أو كود اتصال MySQL.[2] وملف إعداد الجوال يثبت أن USB وWi‑Fi مجرد طريقتين لاختيار عنوان HTTP، وأن اختبار الاتصال يمر عبر `/api/health`.[6]

## 3. مطابقة الدخول والمنسدلات الأولية

### 3.1 الشركات والفروع والسنوات

| الوظيفة | Desktop | Mobile.Office | مصدر البيانات |
|---|---|---|---|
| قائمة الشركات | `GET api/Branches/GetCompaniesLookup` | خدمة المصادقة تستخدم قائمة الشركات نفسها في التدفق الحالي | جدول `Companies` عبر API |
| الفروع | `GET api/Branches/GetActiveBranchesLookup?companyId=...` | `POST api/Auth/LoginOptions` يعيد الفروع بعد التحقق من المستخدم | جدول `Tenant_Branches` |
| السنوات | `GET api/FiscalYears/Lookup?companyId=...` | `POST api/Auth/LoginOptions` يعيد السنوات المفتوحة | جدول `Fiscal_Years` |
| الدخول النهائي | `POST api/Auth/Login` | `POST api/Auth/Login` | `Users`, `Roles`, نطاق الشركة/الفرع/السنة، والجلسة |

المساران لا يستخدمان نفس شكل الاستجابة. Desktop يحمل الفروع والسنوات منفصلين، بينما الجوال يحصل عليهما ضمن `LoginOptionsResponseDto` الذي يحتوي `User_ID`, `Full_Name`, `Company_ID`, `Branches`, و`Years`.[7] مع ذلك، المصدر الجدولي هو نفسه: `Tenant_Branches` و`Fiscal_Years`.

### 3.2 الفرق الوظيفي المؤكد

هناك اختلاف يجب اعتباره **فجوة اتساق** وليس مجرد اختلاف تسمية:

* مسار `LoginOptions` في الجوال يقيد المستخدم غير مدير النظام بفرعه فقط من خلال الشرط `x.Branch_ID == user.Branch_ID`.
* Desktop يحمل الفروع النشطة للشركة من مسار مستقل قبل الدخول، وبالتالي قد يعرض فروعًا أكثر للمستخدم قبل أن يرفض الخادم الدخول أو يطبق نطاقه النهائي.
* قائمة الشركات في `LoginOptionsController` تربط `Companies` بـ`Tenant_Groups` وتطبق `Group.Is_Active` و`Group.Show_In_Login`. أما مسار `Branches/GetCompaniesLookup` المستخدم في Desktop/التدفق الحالي للجوال فلا يثبت من جرده أنه يطبق القيد نفسه.

إذًا، قد يرى العميلان **قوائم شركات أو فروع مختلفة** رغم أنهما يقرآن من الجداول نفسها. القرار المطلوب هنا قرار عمل: هل يسمح للمستخدم باختيار أي فرع قبل الدخول، أم يجب أن تظهر له فروعته المصرح بها فقط؟ لا أوصي بتغيير هذا السلوك تلقائيًا ضمن إصلاحات الأمان أو قبل اعتماد السياسة.

## 4. مطابقة منسدلات السندات

### 4.1 الحسابات ومراكز التكلفة والعملات والأطراف

| البيانات | عقد الجوال | عقد Desktop | المصدر الجدولي | ملاحظة الاتساق |
|---|---|---|---|---|
| الحسابات | `id`, `displayName` | `Account_ID`, `Account_Code`, `Account_Name_AR` | `Chart_Of_Accounts` | الجوال يشترط نشطًا وقابلًا للترحيل وغير تجميعي؛ Desktop المقروء لا يطبق شرط `Is_Summary_Account` في نفس المسار |
| مراكز التكلفة | `id`, `displayName` | `Cost_Center_ID`, `Cost_Center_Code`, `Cost_Center_Name_AR` | `Cost_Centers` | الجوال يطبق `Is_Postable`؛ Desktop المقروء يركز على `Is_Active`، ما قد ينتج قائمة أوسع |
| العملات | `id`, `displayName`, `exchangeRate`, `isLocal`, `isDefault` | `Currency_ID`, `Currency_Code`, `Currency_Name_AR`, `Exchange_Rate`, `Is_Default`, `Is_Local` | `Currencies` | نفس المعنى العام، لكن الاسم والشكل مختلفان، والجوال يفرض سعر 1 للعملة المحلية |
| الأطراف | `id`, `displayName`, `name` | `Party_ID`, `Party_Code`, `Party_Name_AR` | `Parties` | نفس المصدر، شكل JSON مختلف |
| طرق السداد | `id`, `displayName` | `Payment_Method_ID`, `Payment_Method_Code`, `Payment_Method_Name_AR` | `Payment_Methods` | الجوال لا يعرض الكود في العقد الحالي |
| الفترات المفتوحة | `startDate`, `endDate` | ليست ضمن `FinancialVoucherLookups` المقروء | `Fiscal_Periods` | الجوال يربطها بالفرع والسنة في الجلسة؛ Desktop يحتاج مسارًا آخر عند الحاجة |

### 4.2 الصناديق والبنوك ومصادر سندات القبض/الصرف

الجوال لا يعرض الصندوق كصف مستقل فقط؛ بل يبني مصدرًا مركبًا من `Cash_Boxes` و`Chart_Of_Accounts`، ويستبعد الصندوق الذي لا يملك حسابًا نشطًا قابلًا للحركة. كما يضيف البنوك من `Bank_Accounts` ويربطها بحساب الأستاذ العام، ثم يعيد قائمة موحدة تحتوي `accountId`, `sourceType`, و`displayName`.[4]

Desktop يعيد الصناديق في `FinancialVoucherLookups` كحقول منفصلة مثل `Cash_Box_ID`, `Cash_Box_Code`, `Cash_Box_Name`, `Account_ID`, و`Branch_ID`، ولا يدمج البنوك في نفس العقد المقروء. هذه ليست قاعدة بيانات أخرى؛ إنها **قاعدة عرض وتشكيل مختلفة**.

### 4.3 نوع السند وحالته

الجوال يطلب نوعًا محددًا مثل `RECEIPT` أو `PAYMENT` ويجلب حالة `DRAFT`، بينما Desktop يجلب الأنواع والحالات النشطة ثم يرشح النوع `JOURNAL` داخل شاشة القيد. هذا يعني أن العميلين لا يستخدمان نفس سيناريو المنسدلات حتى لو كانت جداول `Voucher_Types` و`Voucher_Statuses` واحدة.

## 5. مطابقة حقول الحفظ

الجوال يستخدم `CreateMobileVoucherDto` و`UpdateMobileVoucherDto`. حقول الرأس الأساسية هي `Voucher_Type_ID`, `Voucher_Status_ID`, `Branch_ID`, `Fiscal_Year_ID`, `Voucher_Date`, `Transaction_Date`, `Cash_Account_ID`, `Party_ID`, `Payment_Method_ID`, `Currency_ID`, `Exchange_Rate`, `Amount`, `Foreign_Total`, و`Local_Total`. أما تفاصيل السند فتحتوي `Account_ID`, `Cost_Center_ID`, `Currency_ID`, `Exchange_Rate`, `Foreign_Amount`, `Local_Amount`, `Debit_Amount`, و`Credit_Amount`.[8]

Desktop يبني تفاصيل القيد من شجرة الحسابات التي يحصل عليها عبر `FinancialVoucher/accounts`، ويستخدم مسارات `FinancialVoucher` للحفظ وسير العمل. لذلك فإن **مفاتيح الحفظ الأساسية مشتركة أو متقاربة**، لكن DTO الجوال صريح ومسمى للعميل المحمول، في حين يعتمد Desktop على نماذج محلية مثل `LookupResponse` و`AccountRow` ونماذج الحفظ داخل ملفات الشاشة.

لا يجوز اعتبار تطابق الاسم كافيًا؛ فالمطابقة النهائية تحتاج اختبارًا يرسل نفس السند المتوازن من العميلين إلى بيئة MySQL اختبارية، ثم يقارن `Voucher_ID`, حالة السند، تفاصيل المدين/الدائن، ومراجع الشركة والفرع والسنة.

## 6. هل المنسدلات تأتي من مكان واحد؟

الإجابة الدقيقة هي: **من مصدر قاعدة مركزي واحد لكل نسخة API، ولكن عبر أماكن API متعددة**.

| السؤال | الإجابة |
|---|---|
| هل الجوال يقرأ MySQL مباشرة؟ | لا. يقرأ JSON من API فقط. |
| هل Desktop يقرأ MySQL مباشرة؟ | لا. يقرأ JSON من API فقط. |
| هل الشركات والفروع والسنوات من نفس الجداول؟ | نعم، في العموم `Companies`, `Tenant_Branches`, و`Fiscal_Years`. |
| هل كل منسدلة تستخدم نفس Endpoint؟ | لا. توجد Controllers عامة ومسارات `api/mobile/...` وعقود مختلفة. |
| هل كل Endpoint يطبق نفس التصفية؟ | لا. توجد فروقات مؤكدة في الحسابات، مراكز التكلفة، الفترات، الصناديق والبنوك، ونطاق الفرع. |
| هل يمكن أن تكون القاعدة مختلفة؟ | نعم، فقط إذا اختير عنوان API مختلف أو كان هناك أكثر من تشغيل API بنص اتصال مختلف. |

## 7. المخاطر التي تمنع اعتماد التوحيد الكامل

| الأولوية | الملاحظة | الأثر | هل تمنع دمج PR #20؟ |
|---|---|---|---|
| عالية | Desktop والجوال يختلفان في سياسة عرض الفروع قبل الدخول | قد يختار المستخدم فرعًا غير مسموح أو يرى خيارات مختلفة | تمنع اعتمادًا وظيفيًا كاملًا، ولا تثبت خللًا في قاعدة البيانات |
| عالية | الجوال وDesktop يستخدمان عقدي منسدلات مختلفين للحسابات ومراكز التكلفة | قد تظهر حسابات أو مراكز مختلفة لنفس الشركة | يجب اختبارها قبل اعتماد الاتساق المحاسبي |
| متوسطة | Desktop ثابت على `localhost` والجوال قابل للتبديل بين USB/Wi‑Fi | قد يعمل كل عميل على نسخة API مختلفة دون ملاحظة | يجب إظهار اسم قاعدة/بيئة API للمستخدم أو تثبيت العنوان |
| متوسطة | لا توجد طبقة DTO مشتركة بين Desktop والجوال للمنسدلات | زيادة احتمالات اختلاف أسماء الحقول والتصفية | لا تمنع البناء، لكنها تزيد تكلفة الصيانة |
| منخفضة | الصناديق والبنوك مدمجة في عقد الجوال فقط | اختلاف عرض، مع فلترة أكثر أمانًا في الجوال | تحتاج توثيقًا واختبارًا، لا تستلزم تغييرًا عاجلًا |

## 8. خطة التوحيد الموصى بها

### المرحلة الأولى: تثبيت نقطة الاتصال

ينبغي أن يعرض API في `/api/health` هوية البيئة واسم قاعدة البيانات، وأن يعرض العميلان هذه الهوية في شاشة التشخيص فقط أو في صفحة «حول النظام». يجب كذلك منع اعتماد بيئة الاختبار إذا لم يتطابق `databaseName` أو `environment` المتوقع. لا ينبغي وضع ConnectionString في أي عميل.

### المرحلة الثانية: اعتماد عقد Lookup موحد

ينبغي إنشاء عقد API موحد، مثل `VoucherReferenceLookupDto`، يثبت أسماء الحقول التالية: `id`, `code`, `name`, `displayName`, `isActive`, `isDefault`, مع حقول خاصة للعملة والفترة. ثم يستخدم Desktop والجوال نفس Endpoint الموثق، مع بقاء طبقة تحويل صغيرة داخل كل عميل للحفاظ على الواجهة الحالية دون تغيير منطق الأعمال.

### المرحلة الثالثة: توحيد سياسة النطاق والتصفية

يجب استخراج قواعد النطاق في خدمة واحدة على الخادم: الشركة من الجلسة، الفرع من الجلسة، السنة من الجلسة، الحساب القابل للترحيل، مركز التكلفة القابل للاستخدام، العملة النشطة، والفترة المفتوحة. بعد ذلك تستدعي مسارات Desktop وMobile نفس الخدمة بدل إعادة كتابة شروط `Where` في Controllers منفصلة.

### المرحلة الرابعة: توحيد الدخول

يجب اتخاذ قرار صريح حول سياسة اختيار الفروع. الخيار الأكثر أمانًا هو أن يعرض العميلان الفروع المصرح بها للمستخدم فقط، وأن يستخدم كلاهما عقد `LoginOptions`. إذا كان المطلوب السماح لمدير النظام باختيار عدة فروع، فيجب تطبيق السياسة نفسها في Desktop والجوال مع اختبار دور مدير النظام والمستخدم العادي.

### المرحلة الخامسة: اختبارات المطابقة

يجب إضافة اختبارات تكاملية باستخدام قاعدة InMemory أو SQLite لاختبار شكل DTO والتصفية، ثم اختبار MySQL فعلي قبل إصدار Release Candidate. يجب أن تغطي الاختبارات شركة متعددة الفروع، مستخدمًا عاديًا، مدير نظام، حسابًا تجميعيًا، حسابًا قابلاً للترحيل، صندوقًا بلا حساب، عملة محلية، فترة مغلقة، وسندًا متوازنًا.

## 9. قرار الدمج بعد هذا التدقيق

**لا أوصي باعتبار PR #20 «مكتملًا وظيفيًا» اعتمادًا على نجاح CI وحده.** الإصلاحات الأمنية والبنائية في PR #20 يمكن مراجعتها، لكن صورة الجوال والتدقيق الحاليان كشفا أن اعتماد العميلين كمنظومة موحدة يحتاج تحققًا إضافيًا على API وMySQL.

في المقابل، لا يوجد من هذا التدقيق ما يبرر رفض PR #20 بسبب وجود قاعدة بيانات ثانية داخل الجوال أو Desktop؛ هذا غير موجود في الكود المقروء. القرار المنضبط هو:

1. إبقاء PR #20 دون دمج حتى تثبيت عنوان API المستخدم في الاختبار.
2. تشغيل `/api/health` من Desktop والجوال والتأكد من تطابق `databaseName` وبيئة التشغيل.
3. اختبار قوائم الشركات والفروع والسنوات بحساب عادي وحساب مدير نظام.
4. مقارنة عدد ومعرفات الحسابات والعملات ومراكز التكلفة والصناديق في العميلين.
5. بعد نجاح هذه المقارنة، يمكن دمج PR #20 دون دمج PRs قاعدة البيانات الكبيرة.
6. تنفيذ توحيد عقود المنسدلات في PR مستقل؛ لا يُخلط مع دمج حزمة التجهيز الحالية.

## 10. الملفات التي بُني عليها التدقيق

تمت مطابقة مسارات وحقول الملفات التالية داخل الفرع الحالي: `AlTayerERP.API/Program.cs`, `AlTayerERP.API/Controllers/LoginOptionsController.cs`, `AlTayerERP.API/Controllers/AuthController.cs`, `AlTayerERP.API/Controllers/MobileVoucherEntryReferencesController.cs`, `AlTayerERP.API/Controllers/FinancialVoucherLookupsController.cs`, `AlTayerERP.Mobile.Office/Services/ApiClientConfiguration.cs`, `AlTayerERP.Mobile.Office/Services/AuthenticationService.cs`, `AlTayerERP.Mobile.Office/DTOs/AuthDtos.cs`, `AlTayerERP.Mobile.Office/DTOs/VoucherEntryDtos.cs`, `AlTayerERP.Desktop/Services.cs`, `AlTayerERP.Desktop/FrmLogin.cs`, و`AlTayerERP.Desktop/FrmJournalVoucher.cs`.

## المراجع

[1]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.API/Program.cs "تهيئة API وAppDbContext وفحص الصحة"

[2]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.Desktop/Services.cs "عميل API المركزي في Desktop"

[3]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.API/Controllers/AuthController.cs "مسار الدخول والجلسة في API"

[4]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.API/Controllers/MobileVoucherEntryReferencesController.cs "منسدلات السندات الخاصة بالجوال"

[5]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.API/Controllers/FinancialVoucherLookupsController.cs "منسدلات السندات العامة المستخدمة في Desktop"

[6]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.Mobile.Office/Services/ApiClientConfiguration.cs "اختيار اتصال USB وWi-Fi في الجوال"

[7]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.API/Controllers/LoginOptionsController.cs "خيارات الدخول والفروع والسنوات للجوال"

[8]: https://github.com/shfeekalbhure/AlTayerERP/blob/chore/prepare-unified-phase1-screens/AlTayerERP.Mobile.Office/DTOs/VoucherEntryDtos.cs "DTOs إنشاء وتحديث السند في الجوال"


## 11. التنفيذ المنجز في حزمة التوحيد الحالية

تم تنفيذ الجزء الآمن من التوحيد دون تغيير منطق الترحيل أو مخطط قاعدة البيانات. أصبحت نقطة `GET /api/health` تعيد عقدًا typed ثابتًا يحتوي `api`, `database`, `environment`, و`databaseName`، مع عدم إرجاع ConnectionString أو كلمة المرور. كما أصبح Mobile.Office يحتفظ بهوية البيئة واسم قاعدة البيانات مع جلسة الاتصال، وأضيفت هذه البيانات إلى نتيجة التشخيص التطويري. وتم تحديث Desktop لقراءة هوية API وقاعدة البيانات من عقد health بدل الاكتفاء برسالة اتصال عامة.

تم كذلك توحيد شروط القوائم المشتركة في `FinancialVoucherLookupsController` مع قواعد مسار الجوال: الحسابات يجب أن تكون نشطة وقابلة للترحيل وغير تجميعية، مراكز التكلفة يجب أن تكون نشطة وقابلة للترحيل، الصناديق يجب أن تكون فعالة ومرتبطة بحساب أستاذ نشط وقابل للحركة، والعملات المحلية تعيد سعر صرف `1` مع ترتيب متسق. بقي عقد العرض مختلفًا عمدًا بين العميلين حتى لا تنكسر الواجهات الحالية؛ الاختلاف الآن في شكل JSON والتحويل داخل العميل، وليس في قواعد التصفية الأساسية للقوائم المشتركة.

أضيفت اختبارات عقد health التي تثبت حالة الجاهزية، حساسية/عدم حساسية حالة النص، عدم كشف اسم قاعدة البيانات عند غيابه، وأسماء JSON بصيغة camelCase. نتيجة الاختبارات الحالية هي **10/10 ناجحة**، وبناء API ناجح بـ**0 تحذيرات و0 أخطاء**.

## 12. حدود التحقق الحالية

تعذر بناء Desktop داخل بيئة Linux الحالية بسبب عدم توفر WindowsDesktop SDK، وتعذر بناء Mobile.Office محليًا بسبب عدم توفر Android workload/target platform في هذه البيئة. هذا لا يمثل فشلًا في كود التوحيد؛ فالمساران يجب إعادة بنائهما في Windows وAndroid CI كما هو معرف في GitHub Actions. كما لم يُنفذ اختبار MySQL فعلي بعد، ولذلك لا يزال تطابق `databaseName` بين العميلين بحاجة إلى تشغيل API وقاعدة اختبار حقيقية.

## 13. قرار ما قبل الدمج

حزمة التوحيد الحالية آمنة للمراجعة لأنها لا تغير schema ولا قواعد التوازن أو الترحيل، وتضيف تحققًا قابلًا للاختبار لهوية الخادم، وتضيق قوائم Desktop لتطابق شروط Mobile الأكثر أمانًا. قبل الدمج النهائي يجب مراجعة نتيجة CI الخاصة بـDesktop وAndroid، ثم تنفيذ اختبار قبول قصير على MySQL يقارن القوائم بعد تسجيل الدخول من العميلين. لا يُدمج `master` تلقائيًا، ولا تُضم تغييرات PRs الكبيرة الخاصة بقاعدة البيانات إلى هذه الحزمة.

## المراجع المحلية

[1]: ../AlTayerERP.API/Program.cs "تهيئة API ونقطة health وتسجيل AppDbContext"
[2]: ../AlTayerERP.Desktop/Services.cs "طبقة اتصال Desktop"
[3]: ../AlTayerERP.API/Contracts/ApiHealthResponse.cs "العقد المركزي لاستجابة health"
[4]: ../AlTayerERP.API/Controllers/MobileVoucherEntryReferencesController.cs "منسدلات السندات في Mobile.Office"
[5]: ../AlTayerERP.API/Controllers/FinancialVoucherLookupsController.cs "منسدلات السندات في Desktop"
[6]: ../AlTayerERP.Mobile.Office/Services/ApiClientConfiguration.cs "إعداد اتصال Mobile.Office وتشخيصه"
[7]: ../AlTayerERP.API/Controllers/LoginOptionsController.cs "خيارات الدخول والنطاق"
[8]: ../AlTayerERP.Mobile.Office/DTOs/VoucherEntryDtos.cs "DTOs السندات في Mobile.Office"
[9]: ../AlTayerERP.Tests/ApiHealthResponseTests.cs "اختبارات عقد health"

