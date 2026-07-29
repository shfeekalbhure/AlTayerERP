# مراجعة الأمن والصلاحيات

## منفذ

PBKDF2 بكود مناسب، JWT مع Session server-side، Refresh Tokens في النموذج، تدقيق، وصلاحيات شاشات. مسارات API العامة محصورة في Middleware.

## مخاطر مثبتة

| ID | الخطر | الدليل |
|---|---|---|
| AT-006 | الجلسات في ذاكرة عملية واحدة؛ إعادة التشغيل أو التوسع الأفقي يكسرها. | `ServerSessionService.cs:14-15` |
| AT-007 | لا Rate Limiting ظاهر على Login أو API. | `Program.cs` بلا `AddRateLimiter/UseRateLimiter` |
| AT-003 | تطبيق الجوال يسمح Cleartext HTTP. | `AndroidManifest.xml` |
| AT-004 | Desktop يستخدم HTTP محليًا وإرسالًا توافقياً لـ`X-Session-Token`. | `Services.cs:18,35-37` |

يلزم اختبار اختراق محدود لـ401/403، تغيير الدور، إبطال الجلسة، brute force، ورفع المرفقات قبل الجزم بالجاهزية.
