import type { PaymentStatus } from "@/lib/erp-types";

export type PaymentSubmissionPlan = {
  status: PaymentStatus;
  sendToServer: boolean;
  requiresNumberingSetup: boolean;
  message: string;
};

/**
 * Prevents duplicate submissions when the API cannot reserve a central
 * PAYMENT_REQUEST number. A local draft remains available for the operator.
 */
export function planPaymentSubmission(sendRequested: boolean, numberingReady: boolean): PaymentSubmissionPlan {
  if (!sendRequested) {
    return {
      status: "DRAFT",
      sendToServer: false,
      requiresNumberingSetup: false,
      message: "تم حفظ الطلب كمسودة محلية.",
    };
  }

  if (!numberingReady) {
    return {
      status: "DRAFT",
      sendToServer: false,
      requiresNumberingSetup: true,
      message: "يلزم وجود إعداد ترقيم نشط لنوع المستند PAYMENT_REQUEST قبل الإرسال.",
    };
  }

  return {
    status: "PENDING_REVIEW",
    sendToServer: true,
    requiresNumberingSetup: false,
    message: "تم إرسال طلب الصرف لمسار المراجعة.",
  };
}

export function isConnectionConfigurationValid(host: string, port: string): boolean {
  return host.trim().length > 0 && /^\d{2,5}$/.test(port.trim());
}
