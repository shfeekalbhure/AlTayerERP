# مراجعة البنية

## الحالة

منفذ: فصل مشاريع Core/Infrastructure/API/Desktop/Mobile.Office. API وInfrastructure على .NET 8، وMobile.Office على .NET 10 Android. لا توجد تبعية دائرية ظاهرة من ملفات csproj.

## ملاحظات مثبتة

| الحالة | النتيجة | الدليل |
|---|---|---|
| منفذ جزئياً | فصل الطبقات موجود، لكن Desktop يملك عميل HTTP ثابتًا وإعداد اتصال ثابتًا. | `AlTayerERP.Desktop/Services.cs:15-25` |
| منفذ جزئياً | Workflow واحد يغطي API/Desktop/Mobile، لكنه لا يختبر ولا يبني/يفحص Driver أو Customer. | `.github/workflows/build-verification.yml` |
| غير موجود | مشروع Driver ومشروع Customer. | `AlTayerERP.slnx` يحوي خمسة مشاريع فقط |
| منفذ لكنه غير مختبر | Baseline نظيفة موجودة في PR #16 لا في رأس PR #11. | PR #16 و`Migrations/20260729201352_Baseline_Phase1.cs` |

العيوب ذات الصلة: AT-001، AT-003، AT-004، AT-014، AT-015، AT-016.
