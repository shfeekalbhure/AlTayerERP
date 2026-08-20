# إعداد الاتصال المحلي الآمن

لا تضع كلمة مرور قاعدة البيانات داخل appsettings.json أو أي ملف متتبع في Git.

1. انسخ appsettings.Example.json إلى appsettings.Local.json داخل مشروع AlTayerERP.API.
2. عدّل YOUR_USER وYOUR_PASSWORD باسم مستخدم MySQL وكلمة المرور المحلية لديك.
3. لا تضف appsettings.Local.json إلى Git؛ الملف مهمل بالفعل في .gitignore.
4. لتفعيل إدخال البيانات المرجعية عند تهيئة محكومة فقط، اجعل DatabaseBootstrap:EnableReferenceDataSeeding = true. القيمة الافتراضية false، لذلك لا يضيف تشغيل API سجلات مرجعية تلقائياً.
5. تطبيق API لا ينشئ أو يعدّل جداول قاعدة البيانات عند بدء التشغيل. تغييرات المخطط تتم فقط عبر مراجعة Migration معتمدة.

قاعدة altayer_erp_db الحالية تبقى كما هي؛ لا ينفذ هذا الإعداد أي Migration أو SQL أو حذف للبيانات.
