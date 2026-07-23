-- المرحلة الأولى / دورة حياة السندات (Voucher Lifecycle)
-- Migration: 20260724_AddVoucherReversalFields.sql
-- الغرض: حفظ حالة العكس والقيد العكسي داخل رأس السند المالي.
-- لا يحذف هذا التحديث أي سند أو قيد سابق.

ALTER TABLE financial_voucher_headers
    -- هل تم عكس السند بقيد عكسي؟
    ADD COLUMN Is_Reversed TINYINT(1) NOT NULL DEFAULT 0 AFTER Unpost_Reason,

    -- رقم القيد العكسي الناتج من عملية العكس.
    ADD COLUMN Reversal_Journal_Entry_ID BIGINT NULL AFTER Is_Reversed,

    -- معرف المستخدم الذي نفذ العكس.
    ADD COLUMN Reversed_By VARCHAR(50) NULL AFTER Reversal_Journal_Entry_ID,

    -- تاريخ ووقت تنفيذ العكس.
    ADD COLUMN Reversed_At DATETIME(6) NULL AFTER Reversed_By,

    -- السبب الإلزامي الذي أدخله المستخدم للعكس.
    ADD COLUMN Reversal_Reason VARCHAR(500) NULL AFTER Reversed_At,

    -- فهارس لتسريع البحث والتقارير وحماية الربط.
    ADD INDEX IX_FinancialVoucher_Reversed (Is_Reversed),
    ADD INDEX IX_FinancialVoucher_ReversalJournal (Reversal_Journal_Entry_ID),

    -- يمنع حذف القيد العكسي إذا كان مرتبطاً بسند مالي.
    ADD CONSTRAINT FK_FinancialVoucher_ReversalJournal
        FOREIGN KEY (Reversal_Journal_Entry_ID)
        REFERENCES journal_entry_headers(Journal_Entry_ID)
        ON DELETE RESTRICT;