-- Apply to the AlTayerERP database before deploying the updated API.
-- Use the chart's actual account ID type and collation for the foreign key.
SET @branch_account_type = (
    SELECT CONCAT(COLUMN_TYPE, ' CHARACTER SET ', CHARACTER_SET_NAME, ' COLLATE ', COLLATION_NAME)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'chart_of_accounts'
      AND COLUMN_NAME = 'Account_ID'
);

SET @branch_office_ddl = IF(
    EXISTS (
        SELECT 1 FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = 'tenant_branches'
          AND COLUMN_NAME = 'Analytical_Account_ID'
    ),
    'SELECT ''Transport office settings already installed'' AS Result',
    CONCAT(
        'ALTER TABLE tenant_branches ',
        'ADD COLUMN Country VARCHAR(100) NULL, ',
        'ADD COLUMN Governorate VARCHAR(100) NULL, ',
        'ADD COLUMN District VARCHAR(100) NULL, ',
        'ADD COLUMN Locality VARCHAR(100) NULL, ',
        'ADD COLUMN Credit_Limit DECIMAL(18,6) NULL, ',
        'ADD COLUMN Allow_Commission TINYINT(1) NOT NULL DEFAULT 0, ',
        'ADD COLUMN Passenger_Commission_Method VARCHAR(12) NULL, ',
        'ADD COLUMN Passenger_Commission_Value DECIMAL(18,6) NULL, ',
        'ADD COLUMN Outbound_Commission_Method VARCHAR(12) NULL, ',
        'ADD COLUMN Outbound_Commission_Value DECIMAL(18,6) NULL, ',
        'ADD COLUMN Inbound_Commission_Method VARCHAR(12) NULL, ',
        'ADD COLUMN Inbound_Commission_Value DECIMAL(18,6) NULL, ',
        'ADD COLUMN Analytical_Account_ID ', @branch_account_type, ' NULL, ',
        'ADD UNIQUE INDEX IX_tenant_branches_Analytical_Account_ID (Analytical_Account_ID), ',
        'ADD CONSTRAINT FK_tenant_branches_chart_of_accounts_Analytical_Account_ID ',
        'FOREIGN KEY (Analytical_Account_ID) REFERENCES chart_of_accounts (Account_ID) ON DELETE RESTRICT'
    )
);
PREPARE branch_office_stmt FROM @branch_office_ddl;
EXECUTE branch_office_stmt;
DEALLOCATE PREPARE branch_office_stmt;
