-- CE-RGA-03: رمز تزامن تفاؤلي لرأس السند المالي.
-- يطبق على نسخة MySQL تجريبية بعد نسخة احتياطية. لا يطبق مباشرة على الإنتاج.
-- يعاد تشغيله بأمان ما دام العمود غير موجود، ثم يملأ القيم القديمة ويمنع NULL.

SET @schema_name = DATABASE();
SET @column_exists = (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'financial_voucher_headers'
      AND COLUMN_NAME = 'RowVersion'
);
SET @add_column_sql = IF(
    @column_exists = 0,
    'ALTER TABLE `financial_voucher_headers` ADD COLUMN `RowVersion` CHAR(36) NULL',
    'SELECT 1'
);
PREPARE add_column_statement FROM @add_column_sql;
EXECUTE add_column_statement;
DEALLOCATE PREPARE add_column_statement;

UPDATE `financial_voucher_headers`
SET `RowVersion` = UUID()
WHERE `RowVersion` IS NULL OR `RowVersion` = '';

ALTER TABLE `financial_voucher_headers`
    MODIFY COLUMN `RowVersion` CHAR(36) NOT NULL;
