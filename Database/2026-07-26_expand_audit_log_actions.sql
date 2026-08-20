-- توافق آمن لسجل التدقيق المركزي في altayer_erp_db فقط.
-- يسمح بكل رمز عملية مكتوب بأحرف إنجليزية كبيرة، مثل INSERT وCREATE وAPPROVE.
-- لا يحذف أي سجل تدقيق ولا يغير بياناته.

SET @schema_name = DATABASE();
SET @constraint_exists = (
    SELECT COUNT(*)
    FROM information_schema.table_constraints
    WHERE constraint_schema = @schema_name
      AND table_name = 'audit_logs'
      AND constraint_name = 'CHK_Audit_Logs_Action'
      AND constraint_type = 'CHECK'
);

SET @drop_constraint_sql = IF(
    @constraint_exists > 0,
    'ALTER TABLE audit_logs DROP CHECK CHK_Audit_Logs_Action',
    'SELECT 1'
);
PREPARE audit_action_drop FROM @drop_constraint_sql;
EXECUTE audit_action_drop;
DEALLOCATE PREPARE audit_action_drop;

ALTER TABLE audit_logs
    ADD CONSTRAINT CHK_Audit_Logs_Action
    CHECK (
        CHAR_LENGTH(Action_Type) BETWEEN 1 AND 64
        AND Action_Type = UPPER(Action_Type)
    );
