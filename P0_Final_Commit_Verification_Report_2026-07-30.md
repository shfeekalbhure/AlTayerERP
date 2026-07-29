# تقرير التحقق النهائي للـCommits المقترحة — P0

**التاريخ:** 30-07-2026  
**المستودع:** `shfeekalbhure/AlTayerERP`  
**نوع التقرير:** تحقق مسبق فقط. لا Cherry-pick ولا Merge ولا تعديل برمجي.  
**فرع التكامل المثبت:** `agent/mobile-payment-request-clean-db-integration`  
**SHA المثبت:** `0bf446513b20b14e5846206612de04ba02db78fe`  
**حالة الرأس:** يضيف فقط `Tests/Mobile_Clean_Db_Payment_Request_UAT.md`؛ لا يثبت نجاح أي حالة UAT قبل تنفيذها محليًا.

## 1. القيود الحاكمة

- ممنوع SQL أو Migration أو تعديل أي قاعدة بيانات.
- ممنوع Force Push أو الدمج إلى `master` أو إغلاق/دمج PR #14.
- تبقى الـBaseline المرجع الأساسي ولا تُستبدل بملفات الفروع الأخرى.
- القرار أدناه **قرار قابلية دمج تقني فقط**؛ لا يعني تنفيذ Cherry-pick أو اعتماد وظيفي.

## 2. الملفات المحمية

| الفئة | الملفات/المسارات المحمية | سياسة P0 |
|---|---|---|
| نموذج الـBaseline | `AlTayerERP.Infrastructure/Data/AppDbContext.cs`، `AlTayerERP.Infrastructure/Data/Phase1BaselineModelConfiguration.cs` | لا استبدال ولا تعديل من فروع الشاشات أو الجوال |
| Migrations | `AlTayerERP.Infrastructure/Migrations/**` | لا نقل أو إنشاء أو حذف |
| الترقيم | `AlTayerERP.API/Services/NumberGeneratorService.cs` | لا تعديل؛ التحقق من استهلاكه فقط |
| SQL/قاعدة البيانات | `Database/**/*.sql`، `Database/Baseline/**`، وأي DDL وقت التشغيل | استبعاد كامل |
| Runtime DDL | `AlTayerERP.API/Services/PaymentRequestSchemaInitializer.cs` وأي استدعاء له | لا ينقل ولا يعاد تفعيله |
| UAT المعتمدة | `Tests/Mobile_Clean_Db_Payment_Request_UAT.md` | محمي من التعديل في P0 |

## 3. نتائج التحقق العامة

- جميع الـCommits التي أمكن جلبها أدناه تعدل ملفًا واحدًا فقط لكل commit، ولا يطابق أي منها ملفًا محميًا.
- لا توجد إضافة أو حذف ملفات في المعلومات المتاحة؛ النوع الفعلي لكل ملف أدناه هو **تعديل**.
- تكرار التعديل في الملف نفسه يفرض ترتيبًا محددًا، ولا يصح نقل commit اللاحق قبل سابقه.
- توجد خمسة معرّفات مختصرة وردت في المقارنة الأولية ولا يمكن حلها الآن إلى SHA كامل من GitHub: `a987…`، `32b1…`، `446a…`، `6748…`، `2cf2…`. قرارها **استبعاد** حتى يقدَّم SHA كامل أو يتوفر مرجع بعيد قابل للقراءة.

## 4. الدفعة الأولى — الشاشات

**المصدر:** `agent/unified-phase1-screens`  
**رأس المصدر المرجعي:** `446ab03a17ad2a4f4bff60f15e4dbd1bd7031f40`

| الترتيب | SHA | الرسالة | الملفات المتغيرة فعليًا | محمي؟ | التبعيات/الملاحظات | قرار التنفيذ |
|---:|---|---|---|---|---|---|
| 1 | `97f8b706a504ecf2bd7c00a8c007fa7d53972a7e` | expose required-field styling from base form | تعديل `AlTayerERP.Desktop/Common/BaseForm.cs` | لا | أساس لتطبيق أسلوب الحقول المطلوبة | Cherry-pick كامل |
| 2 | `2d89a99f3223cd30edcfcf7a91f939bf0541f930` | keep disabled input fields visually distinct | تعديل `AlTayerERP.Desktop/Common/BaseForm.cs` | لا | يعتمد عمليًا على استمرار تغييرات BaseForm من الصف السابق | Cherry-pick كامل بعد 97f8 |
| 3 | `ecbcf23dfdaa4d1c71eda639a1cb419b882ba917` | add cascading geographic selectors | تعديل `AlTayerERP.Desktop/BranchForm.cs` | لا | يحتاج اختبار تحميل القوائم من API الحالي؛ لا يغير النموذج | تطبيق يدوي جزئي: تُنقل منطق الواجهة فقط بعد مراجعة الـAPI الحالي |
| 4 | `49f22092977584667f1467a613ccf7b03bc8056c` | document required-field acceptance checks | تعديل `Tests/BranchForm_RequiredFields_Acceptance.md` | لا | يستند إلى 97f8 و2d89 | Cherry-pick كامل بعدهما |
| 5 | `a987…` | غير قابل للتحقق | غير متاح | غير معلوم | SHA غير قابل للحل | استبعاد |
| 6 | `32b1…` | غير قابل للتحقق | غير متاح | غير معلوم | SHA غير قابل للحل | استبعاد |
| 7 | `446a…` | غير قابل للتحقق | غير متاح | غير معلوم | لا يجوز استبداله بالرأس `446ab…` لأنه commit مختلف | استبعاد |
| 8 | `446ab03a17ad2a4f4bff60f15e4dbd1bd7031f40` | configure error provider per field | تعديل `AlTayerERP.Desktop/Common/RequiredFieldStyleService.cs` | لا | يعتمد على وجود خدمة أسلوب الحقول في قاعدة التكامل؛ يلزم دمج يدوي إذا لم تكن موجودة | تطبيق يدوي جزئي |

### الترتيب النهائي المقترح للدفعة الأولى

`97f8…` → `2d89…` → `49f2…`، ثم تطبيق يدوي محدود لمنطق `ecbc…`، ثم تطبيق يدوي محدود لـ`446ab…` بعد التحقق من وجود `RequiredFieldStyleService` في فرع التكامل.  
لا تدخل المعرفات غير القابلة للتحقق في أي دفعة.

## 5. الدفعة الثانية — طلب الصرف والجوال

**المصدر:** `agent/mobile-payment-request-completion`  
**رأس المصدر المرجعي:** `9393e213483dc14a1e8fe4b1d6ff647ffca6ec35`

| الترتيب | SHA | الرسالة | الملفات المتغيرة فعليًا | محمي؟ | التبعيات/الملاحظات | قرار التنفيذ |
|---:|---|---|---|---|---|---|
| 1 | `088b5d1d8edcc10d1abce228dca54413251571b5` | carry existing timestamp concurrency token | تعديل `AlTayerERP.Mobile.Office/DTOs/PaymentRequestDtos.cs` | لا | أساس لـ timestamp concurrency في الجوال | Cherry-pick كامل |
| 2 | `cc593a8d76a47f696d394ee9a5cf95dadd04bf00` | normalize mobile concurrency timestamp | تعديل `AlTayerERP.Mobile.Office/DTOs/PaymentRequestDtos.cs` | لا | يعتمد على 088b؛ لا ينقل قبله | Cherry-pick كامل بعد 088b |
| 3 | `8f09663506e3eb6e997c4d526fc881b89750274a` | enforce duties transitions and voucher permission | تعديل `AlTayerERP.API/Controllers/PaymentRequestsController.cs` | لا | يجب أن يتبعه إصلاح SoD/Concurrency في 9a81 | Cherry-pick كامل ضمن زوج متسلسل |
| 4 | `9a81a3b10cfbdc4ba888c559c8cda4d06c12ef87` | close SOD and timestamp concurrency gaps | تعديل `AlTayerERP.API/Controllers/PaymentRequestsController.cs` | لا | يعتمد على 8f09 ويكمله | Cherry-pick كامل بعد 8f09 |
| 5 | `05102c90195c3de61561abe7a1cbdca86309ac59` | add optional line reference field | تعديل `AlTayerERP.Mobile.Office/NewPaymentRequestPage.xaml` | لا | لا يظهر من هذا commit وحده DTO/API المقابل؛ يتحقق يدويًا من التوافق قبل النقل | تطبيق يدوي جزئي |
| 6 | `39cebb8ecd4eee87b73ddbc4ebee61ef710f271c` | reload state after denied or conflicting actions | تعديل `AlTayerERP.Mobile.Office/PaymentRequestDetailsPage.xaml.cs` | لا | يفترض رسائل 401/403/409 من API الحالي | Cherry-pick كامل بعد تحقق API |
| 7 | `7f2dedfd2f1683e42905b8808c488e91d1a18eb1` | expand functional acceptance matrix | تعديل `Tests/Part31_Vouchers_And_PaymentRequest_Acceptance.md` | لا | توثيق داعم، لا يغير UAT المحمية | Cherry-pick كامل |
| 8 | `d4fafcc21f5d84f23d5238a0544fa9e0bbb8e928` | cover reviewer separation and timestamp precision | تعديل `Tests/Part31_Vouchers_And_PaymentRequest_Acceptance.md` | لا | يعتمد على محتوى 7f2d | Cherry-pick كامل بعد 7f2d |
| 9 | `9393e213483dc14a1e8fe4b1d6ff647ffca6ec35` | localize missing voucher numbering errors | تعديل `AlTayerERP.Mobile.Office/Services/PaymentRequestService.cs` | لا | مستقل؛ لا يغير `NumberGeneratorService` المحمي | Cherry-pick كامل |
| 10 | `6748…` | غير قابل للتحقق | غير متاح | غير معلوم | SHA غير قابل للحل | استبعاد |
| 11 | `2cf2…` | غير قابل للتحقق | غير متاح | غير معلوم | SHA غير قابل للحل | استبعاد |

### الترتيب النهائي المقترح للدفعة الثانية

1. `088b…` ثم `cc59…`.
2. `8f09…` ثم `9a81…`.
3. التحقق اليدوي من توافق `0510…` ثم تطبيقه جزئيًا إذا كان العقد موجودًا.
4. `39ce…` بعد التأكد من استجابات API الحالية.
5. `7f2d…` ثم `d4fa…` للتوثيق فقط.
6. `9393…`.

## 6. التبعيات المفقودة وقرارات الاستبعاد

| البند | الأثر | القرار |
|---|---|---|
| المعرفات غير القابلة للحل | لا يمكن جرد ملفاتها أو معرفة إن كانت تمس الملفات المحمية | استبعاد حتى SHA كامل قابل للجلب |
| `0510…` | لا يثبت بمفرده عقد الحقل المقابل في DTO/API | تطبيق يدوي فقط بعد فحص العقد الحالي |
| `ecbc…` | منطق BranchForm يجب أن يطابق API والـDTOs الموجودة في الـBaseline | تطبيق يدوي فقط؛ لا نقل أعمى |
| `446ab…` | يعتمد على وجود الخدمة/البنية الداعمة في Desktop | تطبيق يدوي فقط |
| `PaymentRequestSchemaInitializer` أو SQL تاريخي | مخالف لحدود P0 وBaseline | استبعاد صريح، حتى لو ظهر كتبعـية في فرع المصدر |

## 7. خطة الرجوع

1. قبل أي تنفيذ مستقبلي: أنشئ نقطة استعادة موثقة من الرأس `0bf446513b20b14e5846206612de04ba02db78fe`.
2. يكون كل commit/تطبيق يدوي في commit مستقل بعنوان P0 واضح.
3. عند فشل Build أو ثبوت مساس بملف محمي: أوقف الدفعة، واعكس commit الدفعة الأخير فقط بـ`git revert`؛ لا تستخدم Force Push ولا إعادة كتابة تاريخ الفرع.
4. لا تمس ملف UAT المعتمد أثناء الرجوع؛ يبقى مرجع اختبار غير منفذ.

## 8. خطة البناء والاختبار بعد اعتماد التنفيذ فقط

1. بناء API على SHA واحد.
2. بناء Desktop على SHA نفسه.
3. بناء Mobile Android على SHA نفسه.
4. فحص فرق الملفات للتأكد من غياب SQL وMigrations وRuntime DDL وتعديل الملفات المحمية.
5. تنفيذ UAT محليًا على Windows فقط، وتسجيل الأدلة لكل حالة في المصفوفة دون اعتبار أي حالة ناجحة مسبقًا.
6. لا PR merge ولا انتقال إلى `master` قبل اعتماد نتائج البناء وUAT وأي قرار لاحق من المالك.

## 9. القرار النهائي

**الدفعة الأولى:** تسمح فقط بنقل commits الشاشات الموثقة أعلاه، وفق الترتيب المحدد، مع حصر التطبيق اليدوي في `BranchForm` و`RequiredFieldStyleService`.  
**الدفعة الثانية:** تسمح بالترتيب المتسلسل المحدد لثبات التزامن وفصل الواجبات، وتستبعد كل SHA غير قابل للتحقق وأي Runtime DDL أو SQL.  
**الحالة الحالية:** تقرير فقط؛ لا Cherry-pick أو Merge تم أو يُسمح به ضمن هذا التقرير.
