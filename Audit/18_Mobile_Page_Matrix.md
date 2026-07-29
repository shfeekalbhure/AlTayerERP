# Mobile Page Matrix — P0

| الصفحة | Endpoint/DTO | صلاحية/اتصال | الرسائل و401/403/409 | UAT |
|---|---|---|---|---|
| MainPage | Auth/LoginOptions DTOs | session + HTTP localhost | login عربي؛ HTTPS غير جاهز | Android/network |
| PaymentRequestsPage | `/api/payment-requests` | PaymentRequest View | completion يعالج status responses | clean DB |
| NewPaymentRequestPage | POST/PUT PaymentRequestDto | Add/Edit | timestamp in completion | save/update conflict |
| PaymentRequestDetailsPage | transitions/create voucher | review/approve/payment voucher | reload after 401/403/409 في completion | SOD/second voucher |
| PaymentVoucherDetailsPage | Mobile voucher APIs | Authorize | API messages جزئية | PDF/attachment |

فشل الاتصال العام بالإنجليزية غير قابل للحسم من القراءة؛ يلزم جهاز Android دون اتصال وخادم غير متاح.
