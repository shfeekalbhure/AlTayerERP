# تقرير P0 النهائي المصحح — تحقق الـCommits والعقود قبل التنفيذ

**التاريخ:** 30-07-2026  
**المستودع:** `shfeekalbhure/AlTayerERP`  
**فرع التكامل:** `agent/mobile-payment-request-clean-db-integration`  
**رأس التقرير السابق/قاعدة التحليل:** `02332fbc0ef4952f7066db1e6ac4c8c5d8c4e2fa`  
**طبيعة العمل:** تقرير Markdown فقط. لم يُنفذ Cherry-pick أو Merge أو SQL أو Migration أو تعديل قاعدة بيانات.

## 1. القيود والملفات المحمية

| الفئة | المسار/القاعدة | القرار |
|---|---|---|
| Baseline EF | `AlTayerERP.Infrastructure/Data/AppDbContext.cs` و`Phase1BaselineModelConfiguration.cs` و`AppDbContextDesignTimeFactory.cs` | لا استبدال أو تعديل |
| Migrations | `AlTayerERP.Infrastructure/Migrations/**` | لا إضافة/حذف/نقل |
| النموذج المحمي | `AlTayerERP.Core/Entities/Phase1ReferenceEntities.cs` و`TenantBranch.cs` و`FinancialVoucherHeader.cs` | لا استبدال أو تعديل |
| الترقيم | `AlTayerERP.API/Services/NumberGeneratorService.cs` | لا تعديل |
| SQL وDDL | `Database/**`، أي SQL، وأي Runtime DDL أو `PaymentRequestSchemaInitializer` | استبعاد كامل |
| CI | `.github/workflows/build-verification.yml` و`phase1-baseline-generation.yml` | لا تعديل |
| UAT المعتمدة | `Tests/Mobile_Clean_Db_Payment_Request_UAT.md` | محمية من التعديل |

## 2. نتيجة التحقق المصححة للشاشات

**مصدر الشاشات:** `agent/unified-phase1-screens` عند `446ab03a17ad2a4f4bff60f15e4dbd1bd7031f40`.

### جرد الـCommits والتبعيات الفعلية

| الترتيب | SHA | الرسالة | الملفات الفعلية ونوعها | يلمس ملفًا محميًا؟ | التبعية الفعلية | القرار |
|---:|---|---|---|---|---|---|
| 1 | `a98701910ee5551a6a087181829a27a9f0d7e794` | `feat(ui): add unified required-field styling service` | **إضافة** `AlTayerERP.Desktop/Common/RequiredFieldStyleService.cs` | لا | أصل الخدمة؛ لا يعتمد على BaseForm | نقل كامل أولًا |
| 2 | `97f8b706a504ecf2bd7c00a8c007fa7d53972a7e` | `feat(ui): expose required-field styling from base form` | **تعديل** `AlTayerERP.Desktop/Common/BaseForm.cs` | لا | يعتمد على وجود `RequiredFieldStyleService.cs` | نقل كامل بعد `a987…` |
| 3 | `32b12098e2e96c85f7ece7a81d68abadd01d3e49` | `feat(branches): apply unified required-field validation` | **تعديل** `AlTayerERP.Desktop/BranchForm.cs` | لا | يستدعي `ApplyRequiredFieldStyle` و`ValidateRequiredField`؛ لذلك يعتمد على الخدمة ثم BaseForm | نقل كامل بعد `a987…` و`97f8…` |
| 4 | `ecbcf23dfdaa4d1c71eda639a1cb419b882ba917` | `feat(branches): add cascading geographic selectors` | **تعديل** `AlTayerERP.Desktop/BranchForm.cs` | لا | يشارك الملف مع `32b…`؛ يلزم تطبيقه بعده أو حل التعارض معه يدويًا | تطبيق يدوي جزئي |
| 5 | `2d89a99f3223cd30edcfcf7a91f939bf0541f930` | `fix(ui): keep disabled input fields visually distinct` | **تعديل** `AlTayerERP.Desktop/Common/BaseForm.cs` | لا | يكمل التعديل البصري في BaseForm | نقل كامل بعد `97f8…` |
| 6 | `49f22092977584667f1467a613ccf7b03bc8056c` | `test(branches): document required-field acceptance checks` | **تعديل** `Tests/BranchForm_RequiredFields_Acceptance.md` | لا | توثيق اختبارات التغييرات السابقة | نقل كامل أخيرًا |
| 7 | `446ab03a17ad2a4f4bff60f15e4dbd1bd7031f40` | `fix(desktop): configure error provider per field` | **تعديل** `AlTayerERP.Desktop/Common/RequiredFieldStyleService.cs` | لا | إصلاح فوق الخدمة المضافة في `a987…`: ينقل محاذاة وأبعاد أيقونة الخطأ من إعداد عام إلى إعداد مستقل لكل حقل | نقل كامل بعد `a987…`؛ لا ينقل قبله |

### إثباتات الخدمة

- **إضافة الخدمة:** `a987019…` هو commit الذي يضيف ملف `RequiredFieldStyleService.cs` كاملًا.
- **استخدام الخدمة في الفروع:** `32b120…` هو commit الذي يستدعي داخل `BranchForm`:
  `ApplyRequiredFieldStyle(cmbCompanies, txtBranchNameAr, cmbBranchType, cmbCity)`
  ويستبدل تحقق الحقول بـ`ValidateRequiredField(...)`.
- **طبيعة 446ab03:** ليس إضافة للخدمة؛ هو إصلاح لاحق لإعداد `ErrorProvider` لكل حقل داخل الخدمة الموجودة بالفعل.
- لذلك **لا يجوز نقل BaseForm (`97f8…` أو`2d89…`) قبل `a987…`**.

### ترتيب الدفعة الأولى النهائي

`a987…` → `97f8…` → `2d89…` → `32b…` → `446ab…` → تطبيق يدوي محدود لـ`ecbc…` في `BranchForm` → `49f2…`.

## 3. نتيجة التحقق المصححة لطلب الصرف والجوال

**المصدر:** `agent/mobile-payment-request-completion` عند `9393e213483dc14a1e8fe4b1d6ff647ffca6ec35`.

| الترتيب | SHA | الرسالة | الملفات الفعلية ونوعها | يلمس ملفًا محميًا؟ | التبعية/الأثر | قرار النقل |
|---:|---|---|---|---|---|---|
| 1 | `088b5d1d8edcc10d1abce228dca54413251571b5` | carry existing timestamp concurrency token | **تعديل** `Mobile.Office/DTOs/PaymentRequestDtos.cs` | لا | أساس DTO لتزامن التعديل | متوقف مع الدفعة الثانية بسبب فجوة العقد أدناه |
| 2 | `cc593a8d76a47f696d394ee9a5cf95dadd04bf00` | normalize mobile concurrency timestamp | **تعديل** `Mobile.Office/DTOs/PaymentRequestDtos.cs` | لا | يعتمد على 088b | متوقف مع الدفعة الثانية |
| 3 | `8f09663506e3eb6e997c4d526fc881b89750274a` | enforce duties transitions and voucher permission | **تعديل** `API/Controllers/PaymentRequestsController.cs` | لا | يتبعه إغلاق فجوات SoD في 9a81؛ الملف يحتوي تعارض `Branch_ID` محتمل مع Baseline | تطبيق يدوي لاحقًا فقط |
| 4 | `9a81a3b10cfbdc4ba888c559c8cda4d06c12ef87` | close SOD and timestamp concurrency gaps | **تعديل** `API/Controllers/PaymentRequestsController.cs` | لا | يكمل 8f09؛ لا ينقل منفردًا | تطبيق يدوي لاحقًا فقط |
| 5 | `05102c90195c3de61561abe7a1cbdca86309ac59` | add optional line reference field | **تعديل** `Mobile.Office/NewPaymentRequestPage.xaml` | لا | يضيف حقل إدخال المرجع الذي يستخدمه 6748 | متوقف مع الدفعة الثانية |
| 6 | `2cf205a03909937e95657b5e7f8e311dfba661cc` | centralize 401/403/409 handling | **تعديل** `Mobile.Office/Services/PaymentRequestService.cs` | لا | يعرّف `PaymentRequestSessionExpiredException` و`PaymentRequestForbiddenException` و`PaymentRequestConflictException`؛ يعتمد عليه 6748 و9393 | نقل كامل **بعد رفع حظر العقد** |
| 7 | `6748f3e57db5589ee500683574e3397f6d2a0523` | persist line reference and concurrency timestamp | **تعديل** `Mobile.Office/NewPaymentRequestPage.xaml.cs` | لا | يقرأ/يحفظ `Reference_No` ويرسل `Expected_Last_Modified_At`؛ يعتمد على `0510…` لعنصر الإدخال وعلى `2cf205…` لاستثناء التعارض | نقل كامل **بعد 0510 و2cf205 ورفع حظر العقد** |
| 8 | `39cebb8ecd4eee87b73ddbc4ebee61ef710f271c` | reload state after denied or conflicting actions | **تعديل** `Mobile.Office/PaymentRequestDetailsPage.xaml.cs` | لا | يعتمد على استثناءات 2cf205 واستجابات API | نقل كامل بعد 2cf205، بعد رفع الحظر |
| 9 | `7f2dedfd2f1683e42905b8808c488e91d1a18eb1` | expand functional acceptance matrix | **تعديل** `Tests/Part31_Vouchers_And_PaymentRequest_Acceptance.md` | لا | توثيق فقط | نقل كامل لاحقًا |
| 10 | `d4fafcc21f5d84f23d5238a0544fa9e0bbb8e928` | cover reviewer separation and timestamp precision | **تعديل** `Tests/Part31_Vouchers_And_PaymentRequest_Acceptance.md` | لا | يكمل 7f2d | نقل كامل بعد 7f2d لاحقًا |
| 11 | `9393e213483dc14a1e8fe4b1d6ff647ffca6ec35` | localize missing voucher numbering errors | **تعديل** `Mobile.Office/Services/PaymentRequestService.cs` | لا | يعتمد على بنية المعالجة المركزية في 2cf205 | نقل كامل بعد 2cf205، بعد رفع الحظر |

### إثبات 6748 و2cf205

- `6748…` يحفظ فعليًا `Reference_No` من `LineReferenceEntry` داخل سطر الطلب، ويعيد عرضه عند التعديل، ويرسل `Expected_Last_Modified_At = _editingRequest?.LastModifiedAt` عند الحفظ.
- `2cf205…` يوحّد فعليًا المعالجة: **401** يمسح الجلسة ويعيد المستخدم للدخول، **403** يرمي استثناء صلاحية عربيًا، و**409** يرمي `PaymentRequestConflictException` برسالة تعارض عربية.
- استبعاد `2cf205…` يمنع بناء `6748…` و`39ce…` و`9393…` بصورة سليمة لأن الاستثناءات المركزية غير موجودة، ويجعل UAT لمسارات 401/403/409 والتعارض غير قابل للتنفيذ.
- استبعاد `6748…` يترك UAT الخاصة بمرجع سطر الطلب وتزامن الحفظ غير محققة، حتى لو وُجد حقل الإدخال في `0510…`.

### ترتيب الدفعة الثانية النهائي — مشروط بالحظر

بعد معالجة فجوة العقد بقرار مستقل فقط:

`0510…` → `2cf205…` → `6748…` → `9393…` → `088b…` → `cc59…` → تطبيق يدوي لـ`8f09…` ثم `9a81…` → `39ce…` → `7f2d…` → `d4fa…`.

**الحالة الآن: لا يُسمح بتنفيذ أي عنصر من هذه الدفعة.**

## 4. تحقق العقود وفجوة الـBaseline

تمت المقارنة بين Baseline `a026eb32633076601b1d73c0f77161f18129c2b6` ومصدر طلب الصرف `9393e213483dc14a1e8fe4b1d6ff647ffca6ec35`.

| العقد المطلوب | Baseline | مصدر طلب الصرف | النتيجة |
|---|---|---|---|
| `Reference_No` | موجود في Entity وDTO/API | موجود ويُربط في صفحة الإدخال عبر 6748 | متوافق بعد 0510+6748 |
| `Expected_Last_Modified_At` | غير موجود في DTO/API | موجود في DTO وAPI | فجوة في Baseline |
| `Last_Modified_At` | لا يوجد حقل صريح؛ يستخدم المصدر `Updated_At ?? Created_At` كـ`LastModifiedAt` | لا يوجد حقل Entity صريح أيضًا | فجوة/قرار معماري مطلوب |
| `Reviewed_By` | غير موجود | غير موجود كحقل Entity/DTO؛ المصدر يعتمد سجلات التدقيق لتحديد المراجع | فجوة إذا كان الحقل مطلوبًا كعقد بيانات صريح |
| `Approved_By` | غير موجود | غير موجود كحقل Entity/DTO | فجوة إذا كان الحقل مطلوبًا كعقد بيانات صريح |
| `Payment_Voucher_ID` | موجود | موجود ويُضبط عند إنشاء السند | متوافق |
| `PaymentVoucher.Add` | لا دليل في Baseline على فحص إنشاء السند | المصدر يفحص `Allow("PaymentVoucher", ScreenOperation.Add)` | متاح في المصدر، لكن لا ينقل قبل رفع الحظر |
| 401/403/409 | لا معالجة مركزية في خدمة الجوال | موجودة في 2cf205 | متاحة بعد 2cf205 فقط |

### قرار الحظر

لأن `Expected_Last_Modified_At` و`Last_Modified_At` و`Reviewed_By` و`Approved_By` مطلوبة صراحةً قبل السماح بالدفعة الثانية، وهي ليست عقود بيانات صريحة في الـBaseline، فإن **الدفعة الثانية متوقفة**.

لا يُعالج هذا الحظر في P0 عبر Migration أو SQL أو تعديل قاعدة بيانات. المطلوب لاحقًا تقرير أثر معماري مستقل يحدد: هل تكون هذه القيم حقولًا في النموذج، أم حقائق مشتقة ومقروءة من Audit Log، ثم يطلب موافقة منفصلة على أي تغيير.

## 5. خطة الرجوع والبناء بعد اعتماد لاحق

1. نقطة الرجوع الثابتة هي commit التقرير السابق `02332fbc0ef4952f7066db1e6ac4c8c5d8c4e2fa` قبل أي تنفيذ.
2. كل commit مستقبلي يُطبق منفردًا؛ عند الفشل يستخدم `git revert` للـcommit المعني فقط، ولا يستخدم Force Push أو إعادة كتابة التاريخ.
3. بعد كل دفعة معتمدة: Build API ثم Desktop ثم Mobile Android على SHA واحد.
4. فحص فرق الملفات: لا SQL ولا Migrations ولا Runtime DDL ولا ملف محمي.
5. UAT محلي على Windows فقط؛ لا تعتبر حالات المصفوفة ناجحة قبل الدليل المحلي.

## 6. القرار النهائي

- **الدفعة الأولى للشاشات:** ترتيبها مصحح ومسموح بها فقط عند بدء تنفيذ P0 بعد موافقة مستقلة.
- **الدفعة الثانية لطلب الصرف والجوال:** جميع عناصرها **متوقفة حاليًا** بسبب فجوة العقود المثبتة أعلاه.
- هذا التقرير لا ينفذ Cherry-pick أو Merge أو SQL أو Migration أو تعديل قاعدة بيانات.
