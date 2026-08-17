# دليل المطور — AlTayerERP

## الهدف

يوضح هذا الدليل طريقة إعداد مشروع **الطائر AlTayerERP** وتشغيله محليًا والتحقق من جودة التغييرات. يعتمد الحل على .NET 8، ويتكون من API، وCore، وInfrastructure، وواجهة Windows Forms، ومشروع اختبارات وحدة.

## المتطلبات

يلزم تثبيت **.NET 8 SDK**، وMySQL متوافق مع إعدادات المشروع، وبيئة Windows عند بناء مشروع سطح المكتب `AlTayerERP.Desktop`. يمكن بناء API وCore وInfrastructure والاختبارات على Linux أو Windows.

## إعداد الاتصال بقاعدة البيانات

لا يحتوي المستودع على كلمة مرور أو نص اتصال فعلي. استخدم إحدى الطريقتين الآتيتين، مع تفضيل User Secrets أو متغيرات البيئة في بيئات CI والنشر.

### متغير البيئة

```bash
export ConnectionStrings__DefaultConnection='Server=localhost;Port=3306;Database=altayer_erp_db;Uid=altayer_app;Pwd=CHANGE_ME;AllowUserVariables=True;'
export Cors__AllowedOrigins__0='http://localhost:3000'
```

على Windows PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Port=3306;Database=altayer_erp_db;Uid=altayer_app;Pwd=CHANGE_ME;AllowUserVariables=True;'
$env:Cors__AllowedOrigins__0 = 'http://localhost:3000'
```

يمكن نسخ `AlTayerERP.API/appsettings.example.json` كمرجع فقط. يجب عدم وضع كلمة مرور حقيقية في `appsettings.json` أو رفع ملف إعداد محلي إلى Git.

## الاستعادة والبناء

من جذر المستودع نفّذ:

```bash
dotnet restore AlTayerERP.slnx
dotnet build AlTayerERP.API/AlTayerERP.API.csproj --configuration Release
dotnet build AlTayerERP.Tests/AlTayerERP.Tests.csproj --configuration Release
```

يتطلب بناء Windows Forms تشغيل الأمر على Windows:

```powershell
dotnet restore AlTayerERP.slnx
dotnet build AlTayerERP.Desktop/AlTayerERP.Desktop.csproj --configuration Release
```

## الاختبارات

لتشغيل اختبارات الوحدة:

```bash
dotnet test AlTayerERP.Tests/AlTayerERP.Tests.csproj --configuration Release
```

تغطي الاختبارات الحالية دورة إنشاء جلسة الخادم، والتحقق من رمز الجلسة، ورفض الرمز الفارغ أو المجهول، وإبطال الجلسة عند تسجيل الخروج. يجب توسيعها تدريجيًا لتشمل الصلاحيات، ودورة اعتماد وترحيل السندات، وقواعد الحسابات وقاعدة البيانات.

## تشغيل API

بعد ضبط `ConnectionStrings__DefaultConnection` وتشغيل قاعدة البيانات:

```bash
dotnet run --project AlTayerERP.API/AlTayerERP.API.csproj
```

نقطة الفحص العامة هي:

```text
GET /api/health
```

تُرجع النقطة نجاحًا فقط عندما تكون API قادرة على الاتصال بقاعدة البيانات. أما جميع واجهات العمل، باستثناء نقاط الدخول والقوائم العامة المحدودة، فتتطلب رأس الجلسة `X-Session-Token`.

## سياسة CORS

تُقرأ الأصول المسموحة من `Cors:AllowedOrigins`. عندما تكون القائمة فارغة لا تُسمح طلبات المتصفح العابرة للأصول، وهو الوضع الآمن الافتراضي. تطبيق Windows Forms لا يعتمد على CORS، لذلك لا يلزم فتح السياسة من أجل تشغيله.

## التكامل المستمر

يحتوي المستودع على سير عمل `.github/workflows/dotnet-ci.yml`. يشغّل سير العمل استعادة وبناء API واختبارات الوحدة على Ubuntu، ويبني مشروع Windows Forms على Windows. لا يحتاج بناء CI إلى أسرار قاعدة البيانات ما دام لا يشغّل Seeder أو التطبيق نفسه.

## قواعد التسليم

قبل فتح طلب دمج يجب تشغيل البناء والاختبارات محليًا، ومراجعة `git diff` للتأكد من عدم وجود أسرار أو ملفات اتصال محلية، وتوثيق أي تغيير في قاعدة البيانات أو الصلاحيات أو سير عمل السندات. لا يُعد المشروع جاهزًا للإنتاج إلا بعد نجاح بناء Windows، واختبارات التكامل بقاعدة بيانات نظيفة، ومراجعة إعدادات النشر والأسرار.
