-- Phase 1 geography foundation: apply after verified backup.
CREATE TABLE IF NOT EXISTS countries (
  Country_ID BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  Country_Code CHAR(3) NOT NULL,
  Country_Code2 CHAR(2) NULL,
  Country_Name_AR VARCHAR(150) NOT NULL,
  Country_Name_EN VARCHAR(150) NULL,
  Phone_Code VARCHAR(12) NULL,
  Sort_Order INT NOT NULL DEFAULT 0,
  Is_Active TINYINT(1) NOT NULL DEFAULT 1,
  Created_At DATETIME(6) NOT NULL,
  Updated_At DATETIME(6) NULL,
  PRIMARY KEY (Country_ID),
  UNIQUE KEY UX_Countries_Code (Country_Code),
  UNIQUE KEY UX_Countries_Name_AR (Country_Name_AR)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS governorates (
  Governorate_ID BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  Country_ID BIGINT UNSIGNED NOT NULL,
  Governorate_Code VARCHAR(20) NOT NULL,
  Governorate_Name_AR VARCHAR(150) NOT NULL,
  Governorate_Name_EN VARCHAR(150) NULL,
  Sort_Order INT NOT NULL DEFAULT 0,
  Is_Active TINYINT(1) NOT NULL DEFAULT 1,
  Created_At DATETIME(6) NOT NULL,
  Updated_At DATETIME(6) NULL,
  PRIMARY KEY (Governorate_ID),
  UNIQUE KEY UX_Gov_Country_Code (Country_ID, Governorate_Code),
  UNIQUE KEY UX_Gov_Country_Name (Country_ID, Governorate_Name_AR),
  CONSTRAINT FK_Gov_Country FOREIGN KEY (Country_ID) REFERENCES countries(Country_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
CREATE TABLE IF NOT EXISTS cities (
  City_ID BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  Governorate_ID BIGINT UNSIGNED NOT NULL,
  City_Code VARCHAR(20) NOT NULL,
  City_Name_AR VARCHAR(150) NOT NULL,
  City_Name_EN VARCHAR(150) NULL,
  Postal_Code VARCHAR(20) NULL,
  Latitude DECIMAL(10,7) NULL,
  Longitude DECIMAL(10,7) NULL,
  Sort_Order INT NOT NULL DEFAULT 0,
  Is_Active TINYINT(1) NOT NULL DEFAULT 1,
  Created_At DATETIME(6) NOT NULL,
  Updated_At DATETIME(6) NULL,
  PRIMARY KEY (City_ID),
  UNIQUE KEY UX_City_Gov_Code (Governorate_ID, City_Code),
  UNIQUE KEY UX_City_Gov_Name (Governorate_ID, City_Name_AR),
  CONSTRAINT FK_City_Gov FOREIGN KEY (Governorate_ID) REFERENCES governorates(Governorate_ID) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;