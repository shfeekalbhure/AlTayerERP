import { describe, expect, it } from "vitest";

import { isConnectionConfigurationValid, planPaymentSubmission } from "../lib/erp-rules";

describe("payment request submission safeguards", () => {
  it("keeps a save-only action as a draft", () => {
    expect(planPaymentSubmission(false, false)).toMatchObject({
      status: "DRAFT",
      sendToServer: false,
      requiresNumberingSetup: false,
    });
  });

  it("blocks server submission when PAYMENT_REQUEST numbering is unavailable", () => {
    expect(planPaymentSubmission(true, false)).toMatchObject({
      status: "DRAFT",
      sendToServer: false,
      requiresNumberingSetup: true,
    });
  });

  it("allows a review submission only after numbering is confirmed", () => {
    expect(planPaymentSubmission(true, true)).toMatchObject({
      status: "PENDING_REVIEW",
      sendToServer: true,
      requiresNumberingSetup: false,
    });
  });
});

describe("connection configuration validation", () => {
  it("accepts an address and a valid API port", () => {
    expect(isConnectionConfigurationValid("172.16.5.82", "5021")).toBe(true);
  });

  it("rejects a missing address or malformed port", () => {
    expect(isConnectionConfigurationValid("", "5021")).toBe(false);
    expect(isConnectionConfigurationValid("172.16.5.82", "port")).toBe(false);
    expect(isConnectionConfigurationValid("172.16.5.82", "700000")).toBe(false);
  });
});
