# AlTayerERP Mobile Office — Expo

تطبيق جوال عربي باتجاه **RTL** مبني بـ Expo وReact Native، ويقدّم واجهة تشغيلية متصلة بخادم AlTayerERP لعمليات الدخول، طلبات الصرف، سندات القبض، وطلبات الاعتماد.

## التشغيل المحلي

يتطلب التشغيل Node.js 22 وpnpm 9 أو أحدث. بعد نسخ المستودع، ثبّت التبعيات ثم شغّل فحوصات الجودة قبل فتح التطبيق:

```bash
cd AlTayerERP.Mobile.Office.Expo
pnpm install --frozen-lockfile
pnpm check
pnpm test
pnpm lint
pnpm dev
```

## إعداد الاتصال

لا يتضمن المشروع عنواناً ثابتاً لخادم المؤسسة. من شاشة **إعداد الاتصال** داخل التطبيق، أدخل عنوان خادم API والمنفذ المستخدمين في شبكة المؤسسة؛ المثال التشغيلي الحالي هو `172.16.5.82:5021` وقد يتغير مع DHCP.

يستخدم التطبيق المسارات التالية المتوافقة مع خادم AlTayerERP: `api/Auth/LoginCompanies` و`api/Auth/LoginOptions` و`api/Auth/Login` و`api/payment-requests` و`api/FinancialVoucher` و`api/approval-requests` و`api/health`.

## قواعد التشغيل المالي

يرسل التطبيق مفتاح `Idempotency-Key` عند إنشاء طلب صرف أو سند قبض لتقليل أثر الإرسال المكرر. يحتفظ رمز الجلسة في التخزين الآمن للجهاز. لا يتصل التطبيق مباشرة بقاعدة البيانات ولا ينفذ أي تعديل عليها.

عند ظهور خطأ ترقيم `PAYMENT_REQUEST`، لا تُعدل أرقام الطلبات من الجوال. راجع التقرير التشغيلي العربي في `docs/OPERATING-READINESS-AR.md` ونفّذ فحص قاعدة البيانات بإشراف مسؤول معتمد.

## النطاق والمحتوى

المجلد `AlTayerERP.Mobile.Office.Expo` مستقل عن تطبيق MAUI القائم `AlTayerERP.Mobile.Office`. لا توجد ملفات بيئة محلية أو مخازن حزَم أو بيانات اعتماد ضمن هذا المصدر.
