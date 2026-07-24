-- جزء 31: طلبات الصرف. آمن لإعادة التشغيل ولا يحذف بيانات أو جداول.
CREATE TABLE IF NOT EXISTS payment_requests (
 Payment_Request_ID BIGINT NOT NULL AUTO_INCREMENT,
 Company_ID VARCHAR(50) NOT NULL, Branch_ID INT NOT NULL, Fiscal_Year_ID INT NOT NULL,
 Request_No VARCHAR(50) NOT NULL, Request_Date DATETIME NOT NULL, Status VARCHAR(30) NOT NULL,
 Beneficiary_Name VARCHAR(200) NOT NULL, Party_ID VARCHAR(50) NULL, Payment_Method_ID INT NULL,
 Header_Reference_No VARCHAR(100) NULL, Description VARCHAR(500) NULL,
 Approved_Local_Total DECIMAL(19,4) NOT NULL DEFAULT 0, Payment_Voucher_ID BIGINT NULL,
 Review_Reason VARCHAR(500) NULL, Approval_Reason VARCHAR(500) NULL,
 Created_By VARCHAR(50) NOT NULL, Created_At DATETIME NOT NULL, Updated_By VARCHAR(50) NULL, Updated_At DATETIME NULL,
 PRIMARY KEY (Payment_Request_ID),
 UNIQUE KEY UQ_payment_requests_scope_no (Company_ID,Branch_ID,Fiscal_Year_ID,Request_No),
 KEY IX_payment_requests_scope_status (Company_ID,Branch_ID,Fiscal_Year_ID,Status)
) ENGINE=InnoDB;
CREATE TABLE IF NOT EXISTS payment_request_lines (
 Payment_Request_Line_ID BIGINT NOT NULL AUTO_INCREMENT, Payment_Request_ID BIGINT NOT NULL, Line_No INT NOT NULL,
 Account_ID VARCHAR(50) NOT NULL, Cost_Center_ID VARCHAR(50) NULL, Currency_ID INT NOT NULL,
 Exchange_Rate DECIMAL(19,8) NOT NULL, Foreign_Amount DECIMAL(19,4) NOT NULL DEFAULT 0, Local_Amount DECIMAL(19,4) NOT NULL,
 Reference_No VARCHAR(100) NULL, Description VARCHAR(500) NULL,
 PRIMARY KEY (Payment_Request_Line_ID), UNIQUE KEY UQ_payment_request_lines_no (Payment_Request_ID,Line_No),
 KEY IX_payment_request_lines_currency (Currency_ID)
) ENGINE=InnoDB;