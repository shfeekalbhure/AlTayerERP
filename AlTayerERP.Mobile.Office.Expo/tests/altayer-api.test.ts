import { describe, expect, it } from "vitest";

import { AlTayerApiError, createIdempotencyKey, isPaymentNumberingError, safeApiMessage } from "../lib/altayer-api";

describe("AlTayer API error contract", () => {
  it("prioritizes a field validation message and preserves a safe correlation reference", () => {
    const error = new AlTayerApiError("تعذر الحفظ", 400, "api/FinancialVoucher", "VALIDATION", { Amount: ["المبلغ يجب أن يكون أكبر من صفر."] }, "corr-204");
    expect(safeApiMessage(error)).toBe("المبلغ يجب أن يكون أكبر من صفر. (مرجع المتابعة: corr-204)");
  });

  it("creates scoped idempotency keys for sensitive create operations", () => {
    const key = createIdempotencyKey("payment-request");
    expect(key).toMatch(/^payment-request-[a-z0-9]+-[a-z0-9]+$/);
  });

  it("translates payment-request numbering codes and flags them for diagnostics", () => {
    const error = new AlTayerApiError("Server error", 409, "api/payment-requests", "PAYMENT_REQUEST_NUMBERING_NOT_CONFIGURED");
    expect(safeApiMessage(error)).toContain("إعداد ترقيم نشط لطلبات الصرف");
    expect(isPaymentNumberingError(error)).toBe(true);
  });
});
