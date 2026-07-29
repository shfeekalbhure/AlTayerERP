# سجل العيوب الرئيسي — محدث على أحدث الرؤوس

## قاعدة التقييم

«مؤكد» = موجود على رأس فرع حالي. «عولج في فرع لاحق» = إصلاح موجود في فرع آخر ولم يدخل التكامل. «يحتاج UAT» = لا يمكن إثباته بالقراءة أو Build فقط.

| ID | الحالة الحالية/الفرع | الخطورة | المسار والدليل | الأثر والحل | الأولوية واختبار القبول |
|---|---|---|---|---|---|
| AT-001 | **مؤكد**: integration=`a026…` يساوي baseline، والشاشات=`446ab…` منفصل | Critical | `AppDbContext`/`Baseline_Phase1`; الشاشات تستخدم جغرافيا وbranch types | لا يوجد رأس واحد يجمع EF النظيفة والشاشات؛ دمج انتقائي يحافظ على Baseline | P0؛ قاعدة فارغة ثم تحميل/حفظ المراجع من العملاء |
| AT-002 | **كود غير مستخدم** على الشاشات؛ غير موجود/غير مسجل في Baseline `a026…` | Medium | `API/Services/PaymentRequestSchemaInitializer.cs`; لا استدعاء في `Program.cs` | خطر عودة DDL موازٍ؛ إبقاؤه محظورًا أو حذفه في حزمة مستقلة | P1؛ بحث آلي يمنع DDL runtime |
| AT-003 | **مؤكد** على `446ab…` و`9393e…` | High | `Mobile.Office/MauiProgram.cs:22-27`, AndroidManifest | HTTP/127.0.0.1/cleartext خارج adb | P1؛ APK Release يتصل HTTPS فقط |
| AT-004 | **مؤكد** | High | `Desktop/Services.cs:18,35-37` | HTTP محلي وheader توافقية | P1؛ إعداد HTTPS لكل بيئة |
| AT-005 | **مؤكد** | High | workflow بلا `dotnet test`; لا Test csproj | لا دليل regression | P1؛ TRX من Unit/Integration |
| AT-006 | **مؤكد** | High | `ServerSessionService.cs:14-15` | الجلسات تضيع/تتباين مع restart/scale-out | P1؛ store دائم واختبار restart |
| AT-007 | **مؤكد** | High | `Program.cs` بلا RateLimiter | brute force/DoS | P1؛ 429 واختبار audit |
| AT-008 | **عولج في فرع لاحق فقط** `9393e…`; غير مثبت في integration `a026…` | High | `PaymentRequestsController` في completion: checks للمُنشئ/المراجع | فصل الواجبات غير جاهز في فرع التكامل | P1؛ A ينشئ، B يراجع، C يعتمد؛ A/B يفشلان |
| AT-009 | **مؤكد** | Medium | `PaymentRequestsController:85-129`, `NumberGeneratorService:43-86` | حجز رقم مستقل عن حفظ الطلب | P2؛ فشل الحفظ لا يغير العداد أو يسجل تسوية |
| AT-010 | **مؤكد** | Medium | `PaymentRequestsController:51-64` | حد 500 بلا pagination | P2؛ page/total/filter |
| AT-011 | **مؤكد على الشاشات**؛ يحتاج UAT بعد الدمج | Medium | `FrmReceiptVoucher.Actions.cs:102-108`; `FrmPaymentVoucher.cs:8-13` | نص قبض في مسار صرف | P2؛ فتح مرفقات الصرف يظهر «سند الصرف» |
| AT-012 | **مؤكد** | Medium | `API/Program.cs` لا ProblemDetails/global handler | تفاوت أخطاء 500 | P2؛ error id ورسالة آمنة |
| AT-013 | **مؤكد** (بديل AT-017 الملغى) | Medium | workflow يثبت build منفصل لا integration DB | لا gate للرأس الموحد | P2؛ CI ينشئ DB مؤقتة ويشغّل tests |
| AT-014 | **ميزة غير موجودة** | High | لا Entities/Controllers/Projects تشغيلية للنقل | نطاق نقل غير جاهز | P3؛ بوليصة→ترحيل→تسليم |
| AT-015 | **ميزة غير موجودة** | High | `AlTayerERP.slnx` بلا Driver/Customer | متطلبات ميدانية غير منفذة | P3؛ مشاريع وعقود/mزامنة |
| AT-016 | **مؤكد/غير قابل للتحقق تشغيليًا** | Medium | workflow واحد بلا release artifacts/rollback | نشر غير مضبوط | P4؛ version/artifact/rollback مجرب |

## التقييمات الخاصة المطلوبة

- `PAYMENT_REQUEST`: موجود في EF Baseline وMigration؛ SOD/concurrency و401/403/409 معالجان في completion فقط، ويحتاجان نقلًا واختبارًا على integration.
- `PAYMENT_VOUCHER`: إنشاء سند ثانٍ ممنوع في completion عبر `Payment_Voucher_ID` وTransaction؛ يحتاج UAT متزامن على integration.
- 409 الوهمي: completion يعيد تحميل حالة الطلب ويطبع الرسائل العربية؛ لم يثبت على integration.
- Connection failure الإنجليزية: خدمة الجوال في completion تعالج 401/403/409، لكن فشل الشبكة/connection يحتاج UAT Android؛ لا يدّعى علاجه.
