-- المراجع الجغرافية: الدولة ← المحافظة ← المدينة
-- MySQL 8 / آمن لإعادة التشغيل

CREATE TABLE IF NOT EXISTS countries (
    Country_ID INT NOT NULL AUTO_INCREMENT,
    Country_Code VARCHAR(10) NOT NULL,
    Country_Name_AR VARCHAR(150) NOT NULL,
    Country_Name_EN VARCHAR(150) NULL,
    ISO2 VARCHAR(2) NULL,
    ISO3 VARCHAR(3) NULL,
    Phone_Code VARCHAR(10) NULL,
    Currency_Code VARCHAR(10) NULL,
    Nationality_Name_AR VARCHAR(150) NULL,
    Sort_Order INT NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Notes VARCHAR(500) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Country_ID),
    UNIQUE KEY UX_Countries_Code (Country_Code),
    UNIQUE KEY UX_Countries_Name_AR (Country_Name_AR)
);

CREATE TABLE IF NOT EXISTS governorates (
    Governorate_ID INT NOT NULL AUTO_INCREMENT,
    Country_ID INT NOT NULL,
    Governorate_Code VARCHAR(20) NOT NULL,
    Governorate_Name_AR VARCHAR(150) NOT NULL,
    Governorate_Name_EN VARCHAR(150) NULL,
    Sort_Order INT NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Notes VARCHAR(500) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (Governorate_ID),
    UNIQUE KEY UX_Governorates_Country_Code (Country_ID, Governorate_Code),
    UNIQUE KEY UX_Governorates_Country_Name (Country_ID, Governorate_Name_AR),
    CONSTRAINT FK_Governorates_Countries FOREIGN KEY (Country_ID)
        REFERENCES countries (Country_ID) ON UPDATE CASCADE ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS cities (
    City_ID INT NOT NULL AUTO_INCREMENT,
    Country_ID INT NOT NULL,
    Governorate_ID INT NOT NULL,
    City_Code VARCHAR(20) NOT NULL,
    City_Name_AR VARCHAR(150) NOT NULL,
    City_Name_EN VARCHAR(150) NULL,
    Postal_Code VARCHAR(20) NULL,
    Sort_Order INT NOT NULL DEFAULT 0,
    Is_Active TINYINT(1) NOT NULL DEFAULT 1,
    Notes VARCHAR(500) NULL,
    Created_At DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Updated_At DATETIME NULL,
    PRIMARY KEY (City_ID),
    UNIQUE KEY UX_Cities_Governorate_Code (Governorate_ID, City_Code),
    UNIQUE KEY UX_Cities_Governorate_Name (Governorate_ID, City_Name_AR),
    CONSTRAINT FK_Cities_Countries FOREIGN KEY (Country_ID)
        REFERENCES countries (Country_ID) ON UPDATE CASCADE ON DELETE RESTRICT,
    CONSTRAINT FK_Cities_Governorates FOREIGN KEY (Governorate_ID)
        REFERENCES governorates (Governorate_ID) ON UPDATE CASCADE ON DELETE RESTRICT
);

INSERT INTO countries (Country_Code, Country_Name_AR, Country_Name_EN, ISO2, ISO3, Phone_Code, Currency_Code, Nationality_Name_AR, Sort_Order)
SELECT 'YE', 'اليمن', 'Yemen', 'YE', 'YEM', '+967', 'YER', 'يمني', 1
WHERE NOT EXISTS (SELECT 1 FROM countries WHERE Country_Code = 'YE');