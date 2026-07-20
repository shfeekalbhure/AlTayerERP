-- إصلاح دورة مراجعة سندات القبض في سجل حركات السندات.
-- السبب: القيد القديم لا يسمح بالقيمتين REVIEW و RETURN_CORRECTION.
-- الملف آمن لإعادة التشغيل؛ يحذف القيد الحالي إن وجد ثم يعيد إنشاءه بالقيم المعتمدة في النظام.

SET @ddl = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.TABLE_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = DATABASE()
          AND TABLE_NAME = 'voucher_action_logs'
          AND CONSTRAINT_NAME = 'CHK_Voucher_Action_Logs_Action'
          AND CONSTRAINT_TYPE = 'CHECK'
    ),
    'ALTER TABLE voucher_action_logs DROP CHECK CHK_Voucher_Action_Logs_Action',
    'SELECT 1'
);

PREPARE stmt FROM @ddl;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE voucher_action_logs
    ADD CONSTRAINT CHK_Voucher_Action_Logs_Action
    CHECK (
        Action_Type IN (
            'CREATE',
            'UPDATE',
            'DELETE',
            'REVIEW',
            'RETURN_CORRECTION',
            'APPROVE',
            'REJECT',
            'REQUEST_REVISION',
            'CANCEL_APPROVAL',
            'POST',
            'UNPOST',
            'PRINT',
            'EXPORT',
            'IMPORT',
            'ATTACHMENT_ADD',
            'ATTACHMENT_DELETE',
            'UNDO',
            'RESTORE'
        )
    );

-- فحص القيد بعد التنفيذ.
SELECT
    CONSTRAINT_NAME,
    CHECK_CLAUSE
FROM information_schema.CHECK_CONSTRAINTS
WHERE CONSTRAINT_SCHEMA = DATABASE()
  AND CONSTRAINT_NAME = 'CHK_Voucher_Action_Logs_Action';
