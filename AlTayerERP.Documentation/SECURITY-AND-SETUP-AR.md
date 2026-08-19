# دليل الأمان والتجهيز — AlTayerERP

**الحالة:** baseline تجهيز وإطلاق داخلي

هذا الدليل يحدد الحد الأدنى لتشغيل واجهة API بأمان، ويمنع وضع بيانات اعتماد قاعدة البيانات داخل Git. يعتمد المشروع على **.NET 8** و**MySQL**، بينما يعمل تطبيق سطح المكتب على **Windows Forms**.

## إعداد قاعدة البيانات محليًا

يجب إنشاء مستخدم قاعدة بيانات مخصص للتطبيق بدل استخدام المستخدم `root`، مع منحه الصلاحيات اللازمة على قاعدة بيانات المشروع فقط. لا تُنسخ كلمة المرور إلى `appsettings.json` أو إلى أي ملف يتم رفعه للمستودع.

يمكن ضبط نص الاتصال محليًا باستخدام User Secrets من مجلد المشروع:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=altayer_erp_db;Uid=altayer_app;Pwd=ضع_كلمة_المرور_هنا;AllowUserVariables=True;"
```

أو يمكن تمريره من متغير بيئة عند التشغيل:

```bash
export ConnectionStrings__DefaultConnection="Server=127.0.0.1;Port=3306;Database=altayer_erp_db;Uid=altayer_app;Pwd=ضع_كلمة_المرور_هنا;AllowUserVariables=True;"
```

في Windows PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=altayer_erp_db;Uid=altayer_app;Pwd=ضع_كلمة_المرور_هنا;AllowUserVariables=True;"
```

## سياسة CORS

لا يسمح API بأي أصل افتراضيًا. هذا مناسب لتطبيق Windows Forms الذي لا يعتمد على CORS. إذا أضيفت واجهة ويب، يجب إدراج عناوينها صراحة في إعداد البيئة:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://erp.example.com"
    ]
  }
}
```

يجب عدم استخدام `*` في بيئة اختبار مشتركة أو إنتاجية، كما ينبغي مراجعة الأصول المسموح بها عند كل نشر.

## التشغيل والتحقق

بعد ضبط نص الاتصال، ينفذ المطور الأوامر الآتية من جذر المستودع:

```bash
dotnet restore AlTayerERP.API/AlTayerERP.API.csproj
dotnet restore AlTayerERP.Tests/AlTayerERP.Tests.csproj
dotnet build AlTayerERP.API/AlTayerERP.API.csproj --configuration Release
dotnet test AlTayerERP.Tests/AlTayerERP.Tests.csproj --configuration Release
```

وعلى Windows يمكن تنفيذ بناء الحل الكامل أو بناء سطح المكتب منفردًا:

```powershell
dotnet restore AlTayerERP.sln
dotnet build AlTayerERP.Desktop/AlTayerERP.Desktop.csproj --configuration Release
```

يفحص endpoint الصحة حالة API وقابلية اتصال قاعدة البيانات:

```text
GET /api/health
```

الاستجابة الناجحة تكون بحالة HTTP `200` وتحتوي على `api: ready` و`database: ready`. أما تعذر الاتصال بقاعدة البيانات فيعيد API حالة `503`.

## ملاحظات النشر

يجب تدوير أي كلمة مرور كانت مستخدمة سابقًا في المستودع، حتى لو كانت مخصصة للتطوير، لأن حذفها من الملف الحالي لا يزيلها من تاريخ Git. كما يجب حفظ أسرار بيئة الاختبار والإنتاج في مدير أسرار أو متغيرات محمية في منصة CI/CD.

يحتوي المستودع على ملفات SQL وEF Core Migrations؛ لذلك يجب تحديد إصدار قاعدة البيانات المستهدف قبل كل نشر وتوثيق نتيجة الترحيل والنسخ الاحتياطي والاسترجاع في سجل الإصدار.
