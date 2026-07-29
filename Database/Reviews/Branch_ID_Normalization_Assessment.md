# تقييم توحيد `Branch_ID` — المرحلة الأولى

**فرع التدقيق:** `agent/phase1-clean-database-baseline`  
**SHA نقطة البداية:** `4c40992a9c62ece3033bc7f652ceef2817cfe616`  
**الحالة:** تحليل فقط؛ لا تغيير أنواع، لا Migration، ولا SQL منفذ.

## 1. الملخص التنفيذي

`tenant_branches.Branch_ID` معرف في كيان الفرع والجلسة والمستخدم وطلبات الصرف كـ`int`، بينما رأس السند المالي ورأس القيد المحاسبي وMigration القيود يستخدمونه كـ`string`/`VARCHAR(50)`. هذا Drift مؤكد يمنع إنشاء Foreign Key مباشرة من السندات والقيود إلى `tenant_branches`، ويؤدي إلى استخدام `CAST(... AS UNSIGNED)` في SQL تاريخي.

**التوصية المقترحة، وتحتاج اعتماد المالك:** اعتماد `Branch_ID INT` كمفتاح داخلي موحد، مع إبقاء `Branch_Code VARCHAR` ككود أعمال مستقل.

## 2. مواضع الاستخدام المؤكدة

| المصدر/الجدول | النوع | Nullable | الملاحظة |
|---|---|---:|---|
| `TenantBranch.Branch_ID` / `tenant_branches` | `int` | لا | المفتاح الأساسي المرجعي الحالي |
| `TenantBranch.Parent_Branch_ID` | `int?` | نعم | علاقة ذاتية |
| `User.Branch_ID` / `users` | `int` | لا | نطاق المستخدم |
| `CashBox.Branch_ID` / `cash_boxes` | `int` | لا | نطاق الصندوق |
| `FiscalPeriod.Branch_ID` / `fiscal_periods` | `int` | لا | نطاق الفترة |
| `SystemSetting.Branch_ID` / `system_settings` | `int` | لا/قيمة 0 للنظام | نطاق الإعداد |
| `NumberingSetting.Branch_ID` | `int?` أو نطاق اختياري بحسب الكيان الحالي | نعم | نطاق الترقيم |
| `NumberingCounter.Branch_ID` | `int` بعد التطبيع إلى 0 | لا وظيفيًا | نطاق العداد |
| `PaymentRequest.Branch_ID` / `payment_requests` | `int` | لا | SQL وSchemaInitializer متوافقان |
| `PaymentRequestAttachment.Branch_ID` | `int` | لا | نطاق المرفق |
| `LoginAttempt.Branch_ID` | `int?` | نعم | Snapshot لمحاولة الدخول |
| `RefreshToken.Branch_ID` | `int` | لا | نطاق الجلسة |
| `FinancialVoucherHeader.Branch_ID` | `string` | لا | Drift مؤكد |
| `JournalEntryHeader.Branch_ID` | `string` | لا | Migration: `varchar(50)` |
| `AuditLog.Branch_ID` | `string?` | نعم | Snapshot تاريخي لا FK حاليًا |
| جلسة الخادم `ServerSession.Branch_ID` | `int` | لا | المصدر المعتاد للطلبات الجديدة |
| Desktop `CurrentSession.Branch_ID` | `int` | لا | يرسل أرقامًا |
| Mobile DTOs/Session | عدد صحيح في مسارات التشغيل الحديثة | غالبًا لا | يحتاج تثبيت عقد API في المرحلة الثانية |

## 3. هل القيم رقمية فعليًا؟

### ما يثبته الكود

- السجلات الجديدة لطلبات الصرف والصناديق والمستخدمين تأخذ `Branch_ID` من جلسة خادم رقمية.
- السندات المالية والقيود تخزن `Branch_ID` كنص، لكن المسارات الحديثة غالبًا تحول `Session.Branch_ID` إلى نص، لذلك القيم الجديدة المتوقعة رقمية شكلاً.
- سكربت حماية دليل الحسابات يربط السند بالفرع عبر:
  `tenant_branches.Branch_ID = CAST(financial_voucher_headers.Branch_ID AS UNSIGNED)`، ما يؤكد أن التصميم يفترض رقمًا مخزنًا كنص.

### ما لا يمكن إثباته دون قراءة قاعدة البيانات

- لا يمكن تأكيد عدم وجود قيم قديمة مثل `BR-01`, `ADEN`, فراغات أو نصوص غير رقمية.
- لا يمكن تأكيد عدم وجود أصفار بادئة مثل `001` أو قيم سالبة/صفرية.
- لم يُنفذ استعلام التحقق الموجود في `database_alignment_preflight.sql` التزامًا بمنع SQL على القاعدة القديمة.

**الحالة:** قابلية تحويل كل البيانات الحالية إلى `INT` **غير مثبتة بعد**، وليست فشلًا مؤكدًا.

## 4. العلاقات غير الممكنة حاليًا

لا يمكن إنشاء FK صحيحة قبل توحيد النوع في العلاقات التالية:

1. `financial_voucher_headers.Branch_ID` → `tenant_branches.Branch_ID`.
2. `journal_entry_headers.Branch_ID` → `tenant_branches.Branch_ID`.
3. أي `audit_logs.Branch_ID` يراد تحويله إلى FK؛ حاليًا هو Snapshot نصي.
4. روابط مستندات أو تقارير تعتمد على مقارنة Branch النصي بالرقمي.

العلاقات المتوافقة نوعيًا مبدئيًا:

- `users.Branch_ID`.
- `cash_boxes.Branch_ID`.
- `fiscal_periods.Branch_ID`.
- `payment_requests.Branch_ID`.
- `payment_request_attachments.Branch_ID`.
- `login_attempts.Branch_ID`.
- `refresh_tokens.Branch_ID`.

لكن وجود FK فعلي لا يزال غير مضمون لأن معظمها غير مغطى بـMigration.

## 5. أثر اعتماد `INT`

### المزايا

- يطابق المفتاح الأساسي الفعلي والجلسات وواجهات Desktop/Mobile الحديثة.
- يسمح بإنشاء FK وفهارس أصغر وأسرع.
- يلغي `CAST` في الاستعلامات وTriggers.
- يمنع إدخال رموز الفرع في حقل المعرف بالخطأ.
- يفصل الهوية الداخلية عن `Branch_Code` بوضوح.

### العيوب والمخاطر

- يحتاج فحص وترحيل القيم النصية في السندات والقيود والتدقيق.
- أي قيمة غير رقمية تحتاج خريطة تحويل يدوية.
- قد يلزم تعديل DTOs وعقود API وخدمات تقارير تعتمد string.
- تغيير أعمدة كبيرة التشغيل يتطلب خطة توقف/نسخ/تحقق مستقبلية.

## 6. أثر إبقاء `string`

### المزايا

- يتجنب تحويل السندات والقيود الحالية.
- يسمح بمفاتيح مستقبلية مركبة أو رمزية.

### العيوب

- يتطلب تحويل `tenant_branches.Branch_ID` وكل الجداول الرقمية إلى نص.
- يزيد حجم الفهارس والمفاتيح الخارجية.
- يحتاج Collation وطولًا متطابقين لكل FK نصية.
- يخلط المعرف الداخلي مع `Branch_Code`.
- يخالف غالبية الكود الحالي والجلسة وDesktop/Mobile.

## 7. القرار المقترح

**مقترح — يحتاج اعتماد المالك:** 

- `Branch_ID`: `INT` غير قابل لـNULL في الجداول التشغيلية، و`INT?` للعلاقات الاختيارية.
- `Parent_Branch_ID`: `INT?` مع `DeleteBehavior.Restrict`.
- `Branch_Code`: `VARCHAR(30)` أو طول يعتمد لاحقًا، Unique داخل الشركة.
- `audit_logs.Branch_ID`: قرار منفصل؛ إما `INT?` + Snapshot code/name، أو إبقاؤه نصيًا كسجل تاريخي بلا FK.

## 8. خطة ترحيل مستقبلية — غير منفذة

1. أخذ نسخة احتياطية من القاعدة القديمة عند بدء مرحلة الترحيل المعتمدة.
2. استخراج قائمة القيم المميزة لكل `Branch_ID` نصي.
3. تصنيف القيم إلى:
   - رقمية صحيحة.
   - رقمية بأصفار بادئة.
   - فارغة/NULL.
   - غير رقمية.
   - يتيمة لا تطابق فرعًا.
4. إنشاء جدول Mapping مؤقت في بيئة الترحيل الجديدة فقط، وليس القديمة قبل الاعتماد.
5. معالجة القيم غير الرقمية بموافقة أعمال موثقة.
6. إنشاء أعمدة `Branch_ID_New INT` في نسخة الترحيل فقط.
7. تعبئة الأعمدة والتحقق من عدد الصفوف والأيتام.
8. إنشاء FK والفهارس.
9. تبديل الأعمدة بعد نجاح اختبار API/Desktop/Mobile.
10. الاحتفاظ بتقرير Before/After وخطة رجوع.

## 9. APIs والتطبيقات المتأثرة

- API: السندات المالية، القيود، التقارير، الترحيل وإلغاء الترحيل، البحث، التدقيق.
- Desktop: سند القبض، سند الصرف، القيود، تقارير الأستاذ وميزان المراجعة.
- Mobile: سندات القبض والصرف وطلبات الصرف والبحث والتقارير.
- خدمات مشتركة: `FinancialVoucherService`, `VoucherPostingService`, `JournalEntryInquiryService`, DTOs المحاسبية.

## 10. الخلاصة

النوع المقترح هو `INT`، لكن لا يعتمد ولا ينفذ قبل قراءة القيم الحالية في نسخة آمنة أو تقرير قراءة معتمد من المالك. لم يتم تشغيل أي استعلام أو تغيير قاعدة بيانات في إعداد هذا التقرير.