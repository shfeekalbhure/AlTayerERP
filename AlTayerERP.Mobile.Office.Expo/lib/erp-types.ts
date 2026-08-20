export type ConnectionState = "ready" | "warning" | "offline";
export type PaymentStatus = "DRAFT" | "PENDING_REVIEW" | "PENDING_APPROVAL" | "APPROVED" | "REJECTED" | "RETURNED";
export type ApprovalStatus = "PENDING" | "UNDER_REVIEW" | "APPROVED" | "REJECTED" | "RETURNED";

export type ConnectionSettings = {
  mode: "AUTO" | "WIFI" | "USB";
  host: string;
  port: string;
  state: ConnectionState;
  numberingReady: boolean;
  lastCheckedAt: string;
};

export type PaymentRequest = {
  id: string;
  requestNo: string;
  beneficiary: string;
  amount: number;
  date: string;
  status: PaymentStatus;
  description: string;
  lineCount: number;
};

export type ApprovalRequest = {
  id: string;
  reference: string;
  title: string;
  requester: string;
  amount: number;
  createdAt: string;
  status: ApprovalStatus;
  reason: string;
  type: "PAYMENT_REQUEST" | "RECEIPT_VOUCHER";
};

export type VoucherDraft = {
  id: string;
  voucherNo: string;
  party: string;
  amount: number;
  date: string;
  notes: string;
  requiresApproval: boolean;
  status: "DRAFT" | "PENDING_REVIEW";
};

export type DiagnosticEvent = {
  id: string;
  title: string;
  detail: string;
  type: "success" | "warning" | "error" | "info";
  timestamp: string;
};
