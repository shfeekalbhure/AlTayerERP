CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `approval_statuses` (
    `approval_status_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `approval_status_code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `approval_status_name_ar` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `approval_status_name_en` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_approval_statuses` PRIMARY KEY (`approval_status_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `branch_types` (
    `branch_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `branch_type_code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_type_name_ar` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_type_name_en` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_branch_types` PRIMARY KEY (`branch_type_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `countries` (
    `country_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `country_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `country_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `country_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `iso2` varchar(2) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `iso3` varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `phone_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `nationality_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_countries` PRIMARY KEY (`country_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `document_links` (
    `document_link_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `from_module_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `from_document_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `from_document_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `from_document_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `to_module_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `to_document_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `to_document_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `to_document_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `link_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_document_links` PRIMARY KEY (`document_link_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `numbering_counters` (
    `counter_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
    `branch_id` int COLLATE utf8mb4_unicode_ci NULL DEFAULT 0,
    `year_value` int COLLATE utf8mb4_unicode_ci NULL DEFAULT 0,
    `last_number` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_numbering_counters` PRIMARY KEY (`counter_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `numbering_document_types` (
    `numbering_document_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `document_type_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_type_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_type_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_numbering_document_types` PRIMARY KEY (`numbering_document_type_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `numbering_settings` (
    `numbering_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `prefix` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `digits_count` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `reset_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `last_number` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `use_company` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `use_branch` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `use_year` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_numbering_settings` PRIMARY KEY (`numbering_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `payment_methods` (
    `payment_method_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `payment_method_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `payment_method_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `payment_method_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `requires_reference` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `requires_reference_date` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_cash` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_bank` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_payment_methods` PRIMARY KEY (`payment_method_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `roles` (
    `role_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `role_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `role_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_system_admin` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_roles` PRIMARY KEY (`role_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `system_permissions` (
    `permission_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `permission_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `permission_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `permission_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `module_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_system_permissions` PRIMARY KEY (`permission_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `system_screens` (
    `screen_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `screen_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `screen_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `module_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_system_screens` PRIMARY KEY (`screen_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `system_settings` (
    `setting_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `setting_key` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `setting_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `setting_value` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `scope` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `effective_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_system_settings` PRIMARY KEY (`setting_id`),
    CONSTRAINT `ck_system_settings_scope` CHECK (`scope` IN ('SYSTEM','COMPANY','BRANCH','FISCAL_YEAR'))
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `tenant_groups` (
    `group_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `group_code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `group_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `group_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `short_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_default` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `parent_group_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `main_company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `default_currency_code` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `country_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `city_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `short_address` varchar(300) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `phone` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `manager_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `show_in_login` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT TRUE,
    `show_in_tree` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT TRUE,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `created_by` int COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `edit_count` int COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 0,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT TRUE,
    `stopped_by` int COLLATE utf8mb4_unicode_ci NULL,
    `stopped_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `stopped_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `reactivate_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_tenant_groups` PRIMARY KEY (`group_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `voucher_statuses` (
    `voucher_status_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `voucher_status_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_status_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_status_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_voucher_statuses` PRIMARY KEY (`voucher_status_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `voucher_types` (
    `voucher_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `voucher_type_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_type_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_type_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_voucher_types` PRIMARY KEY (`voucher_type_id`)
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `governorates` (
    `governorate_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `country_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `governorate_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `governorate_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `governorate_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_governorates` PRIMARY KEY (`governorate_id`),
    CONSTRAINT `FK_governorates_countries_country_id` FOREIGN KEY (`country_id`) REFERENCES `countries` (`country_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `role_permissions` (
    `permission_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `role_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `screen_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_view` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_add` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_edit` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_delete` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_print` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_export` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_import` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_approve` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_un_approve` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_role_permissions` PRIMARY KEY (`permission_id`),
    CONSTRAINT `FK_role_permissions_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_role_permissions_system_screens_screen_id` FOREIGN KEY (`screen_id`) REFERENCES `system_screens` (`screen_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `companies` (
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `group_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_prefix` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `activity_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `tax_number` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT TRUE,
    `phone` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `mobile` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `address` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `company_logo` longblob COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `created_by` int COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `edit_count` int COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 0,
    `stopped_by` int COLLATE utf8mb4_unicode_ci NULL,
    `stopped_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `stopped_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `reactivate_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_companies` PRIMARY KEY (`company_id`),
    CONSTRAINT `FK_companies_tenant_groups_group_id` FOREIGN KEY (`group_id`) REFERENCES `tenant_groups` (`group_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `cities` (
    `city_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `country_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `governorate_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `city_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `city_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `city_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `postal_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_cities` PRIMARY KEY (`city_id`),
    CONSTRAINT `FK_cities_countries_country_id` FOREIGN KEY (`country_id`) REFERENCES `countries` (`country_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cities_governorates_governorate_id` FOREIGN KEY (`governorate_id`) REFERENCES `governorates` (`governorate_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `account_categories` (
    `category_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `category_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `category_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `category_name_en` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `normal_balance` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_system` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `sort_order` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_account_categories` PRIMARY KEY (`category_id`),
    CONSTRAINT `FK_account_categories_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `account_code_settings` (
    `setting_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `level_no` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `segment_length` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `start_number` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `padding_char` varchar(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `parent_based` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `auto_generate` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `allow_manual_code` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `max_serial` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_account_code_settings` PRIMARY KEY (`setting_id`),
    CONSTRAINT `FK_account_code_settings_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `approval_requests` (
    `approval_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `request_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `reference_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `entity_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `entity_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NULL,
    `reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `status` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `requested_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `requested_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `approved_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `approved_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `approval_notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_approval_requests` PRIMARY KEY (`approval_id`),
    CONSTRAINT `FK_approval_requests_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `bank_accounts` (
    `bank_account_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `bank_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `bank_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `bank_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `iban` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `gl_account` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `branch_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_bank_accounts` PRIMARY KEY (`bank_account_id`),
    CONSTRAINT `FK_bank_accounts_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `chart_of_accounts` (
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `parent_account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_level` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_postable` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `normal_balance` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `allow_manual_entry` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `system_account` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `requires_cost_center` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `requires_party` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `requires_project` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_summary_account` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `affects_balance_sheet` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `affects_income_statement` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `multi_currency` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_control_account` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `control_account_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_path` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_serial` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_chart_of_accounts` PRIMARY KEY (`account_id`),
    CONSTRAINT `ck_chart_normal_balance` CHECK (`normal_balance` IS NULL OR `normal_balance` IN ('Debit','Credit')),
    CONSTRAINT `ck_chart_postable_summary` CHECK (NOT (`is_postable` = 1 AND `is_summary_account` = 1)),
    CONSTRAINT `FK_chart_of_accounts_chart_of_accounts_parent_account_id` FOREIGN KEY (`parent_account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_chart_of_accounts_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `cost_centers` (
    `cost_center_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `parent_cost_center_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `center_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `center_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `center_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `center_level` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_postable` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_cost_centers` PRIMARY KEY (`cost_center_id`),
    CONSTRAINT `FK_cost_centers_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cost_centers_cost_centers_parent_cost_center_id` FOREIGN KEY (`parent_cost_center_id`) REFERENCES `cost_centers` (`cost_center_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `currencies` (
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_name_ar` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_symbol` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `decimal_places` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `min_exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NULL,
    `max_exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NULL,
    `is_local_currency` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_default` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_currencies` PRIMARY KEY (`currency_id`),
    CONSTRAINT `FK_currencies_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `exchange_rates` (
    `exchange_rate_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `rate_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate_value` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `min_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NULL,
    `max_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NULL,
    `is_default` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_exchange_rates` PRIMARY KEY (`exchange_rate_id`),
    CONSTRAINT `ck_exchange_rate_positive` CHECK (`exchange_rate` > 0),
    CONSTRAINT `FK_exchange_rates_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `financial_limits` (
    `limit_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `entity_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `entity_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `limit_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `limit_amount` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `used_amount` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `period_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `requires_approval` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_financial_limits` PRIMARY KEY (`limit_id`),
    CONSTRAINT `FK_financial_limits_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `fiscal_years` (
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `year_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `start_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `end_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_default` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_closed` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_fiscal_years` PRIMARY KEY (`fiscal_year_id`),
    CONSTRAINT `ck_fiscal_year_dates` CHECK (`start_date` <= `end_date`),
    CONSTRAINT `FK_fiscal_years_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `parties` (
    `party_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `party_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `mobile_no` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `phone_no` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `identity_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `tax_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `address` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `city_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `credit_limit` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_parties` PRIMARY KEY (`party_id`),
    CONSTRAINT `FK_parties_chart_of_accounts_account_id` FOREIGN KEY (`account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE SET NULL,
    CONSTRAINT `FK_parties_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `tenant_branches` (
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_code` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `address` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `branch_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `branch_type_id` int COLLATE utf8mb4_unicode_ci NULL,
    `parent_branch_id` int COLLATE utf8mb4_unicode_ci NULL,
    `country_id` int COLLATE utf8mb4_unicode_ci NULL,
    `governorate_id` int COLLATE utf8mb4_unicode_ci NULL,
    `city_id` int COLLATE utf8mb4_unicode_ci NULL,
    `phone` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `mobile` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `email` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `website` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `manager_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `allow_credit` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `allow_percentage` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT TRUE,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `created_by` int COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `edit_count` int COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 0,
    `stopped_by` int COLLATE utf8mb4_unicode_ci NULL,
    `stopped_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `stopped_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_by` int COLLATE utf8mb4_unicode_ci NULL,
    `reactivated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `reactivate_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_tenant_branches` PRIMARY KEY (`branch_id`),
    CONSTRAINT `FK_tenant_branches_branch_types_branch_type_id` FOREIGN KEY (`branch_type_id`) REFERENCES `branch_types` (`branch_type_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_cities_city_id` FOREIGN KEY (`city_id`) REFERENCES `cities` (`city_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_countries_country_id` FOREIGN KEY (`country_id`) REFERENCES `countries` (`country_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_governorates_governorate_id` FOREIGN KEY (`governorate_id`) REFERENCES `governorates` (`governorate_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_tenant_branches_tenant_branches_parent_branch_id` FOREIGN KEY (`parent_branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `financial_limit_movements` (
    `movement_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `limit_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `movement_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `movement_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `reference_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `amount` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `balance_after` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_financial_limit_movements` PRIMARY KEY (`movement_id`),
    CONSTRAINT `FK_financial_limit_movements_financial_limits_limit_id` FOREIGN KEY (`limit_id`) REFERENCES `financial_limits` (`limit_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `audit_logs` (
    `audit_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `module_id` int COLLATE utf8mb4_unicode_ci NULL,
    `table_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `record_id` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `action_type` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NULL,
    `action_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `old_values` json NULL,
    `new_values` json NULL,
    `action_channel` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `device_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ip_address` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_audit_logs` PRIMARY KEY (`audit_id`),
    CONSTRAINT `ck_audit_action` CHECK (CHAR_LENGTH(`action_type`) BETWEEN 1 AND 64 AND `action_type` = UPPER(`action_type`)),
    CONSTRAINT `FK_audit_logs_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `cash_boxes` (
    `cash_box_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `currency_code` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `cash_box_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `box_name_ar` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `box_name_en` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `opening_balance` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `max_limit` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `min_limit` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_by` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_cash_boxes` PRIMARY KEY (`cash_box_id`),
    CONSTRAINT `FK_cash_boxes_chart_of_accounts_account_id` FOREIGN KEY (`account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cash_boxes_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_cash_boxes_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `financial_voucher_headers` (
    `voucher_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `voucher_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_status_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NULL,
    `voucher_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `transaction_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `cash_account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `received_from_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `payment_method_id` int COLLATE utf8mb4_unicode_ci NULL,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `foreign_total` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `local_total` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `reference_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `against_text` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `module_id` int COLLATE utf8mb4_unicode_ci NULL,
    `document_type_id` int COLLATE utf8mb4_unicode_ci NULL,
    `document_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `source_document_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `requires_approval` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `approval_status` tinyint unsigned COLLATE utf8mb4_unicode_ci NOT NULL,
    `approval_requested_by_user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `approval_requested_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `approved_by_user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `approved_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `rejected_by_user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `rejected_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `rejection_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `review_status` tinyint unsigned COLLATE utf8mb4_unicode_ci NOT NULL,
    `reviewed_by_user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reviewed_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `review_notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `edit_count` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `print_count` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `last_printed_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `last_print_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `undo_count` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `last_undo_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `last_undo_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `is_posted` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `journal_entry_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `posted_by_user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `posted_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `unposted_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `unposted_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `unpost_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_financial_voucher_headers` PRIMARY KEY (`voucher_id`),
    CONSTRAINT `ck_voucher_exchange_rate` CHECK (`exchange_rate` > 0),
    CONSTRAINT `ck_voucher_totals` CHECK (`amount` >= 0 AND `foreign_total` >= 0 AND `local_total` >= 0),
    CONSTRAINT `FK_financial_voucher_headers_chart_of_accounts_cash_account_id` FOREIGN KEY (`cash_account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_parties_party_id` FOREIGN KEY (`party_id`) REFERENCES `parties` (`party_id`) ON DELETE SET NULL,
    CONSTRAINT `FK_financial_voucher_headers_payment_methods_payment_method_id` FOREIGN KEY (`payment_method_id`) REFERENCES `payment_methods` (`payment_method_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_voucher_statuses_voucher_status_id` FOREIGN KEY (`voucher_status_id`) REFERENCES `voucher_statuses` (`voucher_status_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_headers_voucher_types_voucher_type_id` FOREIGN KEY (`voucher_type_id`) REFERENCES `voucher_types` (`voucher_type_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `fiscal_periods` (
    `fiscal_period_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `period_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `period_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `start_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `end_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_closed` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `close_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `close_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_fiscal_periods` PRIMARY KEY (`fiscal_period_id`),
    CONSTRAINT `ck_fiscal_period_dates` CHECK (`start_date` <= `end_date`),
    CONSTRAINT `FK_fiscal_periods_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_fiscal_periods_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_fiscal_periods_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `users` (
    `user_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `role_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `full_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `login_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `password_hash` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `phone` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `must_change_password` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `failed_login_count` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `last_failed_login_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `locked_until` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `last_login_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `last_login_ip` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_users` PRIMARY KEY (`user_id`),
    CONSTRAINT `FK_users_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_users_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_users_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `document_allocations` (
    `allocation_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `module_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_type_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `voucher_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `document_total` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `collected_before` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `collected_now` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `remaining_balance` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_document_allocations` PRIMARY KEY (`allocation_id`),
    CONSTRAINT `FK_document_allocations_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_document_allocations_financial_voucher_headers_voucher_id` FOREIGN KEY (`voucher_id`) REFERENCES `financial_voucher_headers` (`voucher_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_document_allocations_parties_party_id` FOREIGN KEY (`party_id`) REFERENCES `parties` (`party_id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `financial_voucher_details` (
    `voucher_detail_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `voucher_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `line_no` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `cost_center_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `project_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_name` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `foreign_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `local_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `debit_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `credit_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `line_type` tinyint unsigned COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_financial_voucher_details` PRIMARY KEY (`voucher_detail_id`),
    CONSTRAINT `ck_voucher_detail_debit_credit` CHECK (NOT (`debit_amount` > 0 AND `credit_amount` > 0)),
    CONSTRAINT `ck_voucher_detail_nonnegative` CHECK (`debit_amount` >= 0 AND `credit_amount` >= 0 AND `foreign_amount` >= 0 AND `local_amount` >= 0),
    CONSTRAINT `ck_voucher_detail_rate` CHECK (`exchange_rate` > 0),
    CONSTRAINT `FK_financial_voucher_details_chart_of_accounts_account_id` FOREIGN KEY (`account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_details_cost_centers_cost_center_id` FOREIGN KEY (`cost_center_id`) REFERENCES `cost_centers` (`cost_center_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_details_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_financial_voucher_details_financial_voucher_headers_voucher_~` FOREIGN KEY (`voucher_id`) REFERENCES `financial_voucher_headers` (`voucher_id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `journal_entry_headers` (
    `journal_entry_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `entry_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `entry_type` tinyint unsigned COLLATE utf8mb4_unicode_ci NOT NULL,
    `entry_status_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NULL,
    `entry_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `transaction_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `source_system` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_system_generated` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `source_voucher_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `source_document_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `source_document_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `total_debit` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `total_credit` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_posted` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `posted_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `posted_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `is_reversal` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `original_journal_entry_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `is_reversed` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `reversal_journal_entry_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `reversal_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reversed_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reversed_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `is_cancelled` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `cancellation_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `cancelled_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `cancelled_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_journal_entry_headers` PRIMARY KEY (`journal_entry_id`),
    CONSTRAINT `ck_journal_balanced` CHECK (`total_debit` = `total_credit`),
    CONSTRAINT `FK_journal_entry_headers_financial_voucher_headers_source_vouch~` FOREIGN KEY (`source_voucher_id`) REFERENCES `financial_voucher_headers` (`voucher_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_journal_entry_headers_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_journal_entry_headers_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `payment_requests` (
    `payment_request_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `request_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `request_date` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `status` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `beneficiary_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `party_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `payment_method_id` int COLLATE utf8mb4_unicode_ci NULL,
    `header_reference_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `approved_local_total` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `payment_voucher_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `review_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `approval_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_payment_requests` PRIMARY KEY (`payment_request_id`),
    CONSTRAINT `FK_payment_requests_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_requests_financial_voucher_headers_payment_voucher_id` FOREIGN KEY (`payment_voucher_id`) REFERENCES `financial_voucher_headers` (`voucher_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_requests_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_requests_parties_party_id` FOREIGN KEY (`party_id`) REFERENCES `parties` (`party_id`) ON DELETE SET NULL,
    CONSTRAINT `FK_payment_requests_payment_methods_payment_method_id` FOREIGN KEY (`payment_method_id`) REFERENCES `payment_methods` (`payment_method_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_requests_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `voucher_action_logs` (
    `voucher_action_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `voucher_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `action_type` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `old_status_id` int COLLATE utf8mb4_unicode_ci NULL,
    `new_status_id` int COLLATE utf8mb4_unicode_ci NULL,
    `user_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `action_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `action_channel` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `device_name` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ip_address` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_voucher_action_logs` PRIMARY KEY (`voucher_action_id`),
    CONSTRAINT `FK_voucher_action_logs_financial_voucher_headers_voucher_id` FOREIGN KEY (`voucher_id`) REFERENCES `financial_voucher_headers` (`voucher_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `login_attempts` (
    `login_attempt_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `user_id` int COLLATE utf8mb4_unicode_ci NULL,
    `login_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NULL,
    `attempted_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_success` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `failure_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `ip_address` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `user_agent` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `device_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `session_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `lockout_until` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_login_attempts` PRIMARY KEY (`login_attempt_id`),
    CONSTRAINT `FK_login_attempts_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `refresh_tokens` (
    `refresh_token_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `token_hash` char(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `session_id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `user_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `device_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `expires_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `revoked_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `replaced_by_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `revoked_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_refresh_tokens` PRIMARY KEY (`refresh_token_id`),
    CONSTRAINT `FK_refresh_tokens_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_refresh_tokens_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_refresh_tokens_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_refresh_tokens_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_permissions` (
    `permission_id` int COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `user_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `permission_category` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `permission_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_view` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_add` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_edit` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_delete` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_print` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_export` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_import` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_preview` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_approve` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `can_un_approve` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_user_permissions` PRIMARY KEY (`permission_id`),
    CONSTRAINT `FK_user_permissions_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `journal_entry_details` (
    `journal_entry_detail_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `journal_entry_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `line_no` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `cost_center_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `project_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(18,6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `foreign_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `local_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `debit_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `credit_amount` decimal(18,2) COLLATE utf8mb4_unicode_ci NOT NULL,
    `reference_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_name` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `reference_date` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    `source_voucher_detail_id` bigint COLLATE utf8mb4_unicode_ci NULL,
    `line_type` tinyint unsigned COLLATE utf8mb4_unicode_ci NOT NULL,
    `notes` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    `updated_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `updated_at` datetime(6) COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_journal_entry_details` PRIMARY KEY (`journal_entry_detail_id`),
    CONSTRAINT `ck_journal_detail_debit_credit` CHECK (NOT (`debit_amount` > 0 AND `credit_amount` > 0)),
    CONSTRAINT `ck_journal_detail_rate` CHECK (`exchange_rate` > 0),
    CONSTRAINT `FK_journal_entry_details_chart_of_accounts_account_id` FOREIGN KEY (`account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_journal_entry_details_cost_centers_cost_center_id` FOREIGN KEY (`cost_center_id`) REFERENCES `cost_centers` (`cost_center_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_journal_entry_details_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_journal_entry_details_journal_entry_headers_journal_entry_id` FOREIGN KEY (`journal_entry_id`) REFERENCES `journal_entry_headers` (`journal_entry_id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `payment_request_attachments` (
    `payment_request_attachment_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `payment_request_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `company_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `branch_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `fiscal_year_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `original_file_name` varchar(260) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `storage_key` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `content_type` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `file_size` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `is_active` tinyint(1) COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_by` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `created_at` datetime(6) COLLATE utf8mb4_unicode_ci NOT NULL,
    CONSTRAINT `PK_payment_request_attachments` PRIMARY KEY (`payment_request_attachment_id`),
    CONSTRAINT `FK_payment_request_attachments_companies_company_id` FOREIGN KEY (`company_id`) REFERENCES `companies` (`company_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_request_attachments_fiscal_years_fiscal_year_id` FOREIGN KEY (`fiscal_year_id`) REFERENCES `fiscal_years` (`fiscal_year_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_request_attachments_payment_requests_payment_request~` FOREIGN KEY (`payment_request_id`) REFERENCES `payment_requests` (`payment_request_id`) ON DELETE CASCADE,
    CONSTRAINT `FK_payment_request_attachments_tenant_branches_branch_id` FOREIGN KEY (`branch_id`) REFERENCES `tenant_branches` (`branch_id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `payment_request_lines` (
    `payment_request_line_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL AUTO_INCREMENT,
    `payment_request_id` bigint COLLATE utf8mb4_unicode_ci NOT NULL,
    `line_no` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `account_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
    `cost_center_id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `currency_id` int COLLATE utf8mb4_unicode_ci NOT NULL,
    `exchange_rate` decimal(19,8) COLLATE utf8mb4_unicode_ci NOT NULL,
    `foreign_amount` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `local_amount` decimal(19,4) COLLATE utf8mb4_unicode_ci NOT NULL,
    `reference_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
    CONSTRAINT `PK_payment_request_lines` PRIMARY KEY (`payment_request_line_id`),
    CONSTRAINT `ck_payment_request_line_amounts` CHECK (`foreign_amount` >= 0 AND `local_amount` >= 0),
    CONSTRAINT `ck_payment_request_line_rate` CHECK (`exchange_rate` > 0),
    CONSTRAINT `FK_payment_request_lines_chart_of_accounts_account_id` FOREIGN KEY (`account_id`) REFERENCES `chart_of_accounts` (`account_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_request_lines_cost_centers_cost_center_id` FOREIGN KEY (`cost_center_id`) REFERENCES `cost_centers` (`cost_center_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_request_lines_currencies_currency_id` FOREIGN KEY (`currency_id`) REFERENCES `currencies` (`currency_id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_payment_request_lines_payment_requests_payment_request_id` FOREIGN KEY (`payment_request_id`) REFERENCES `payment_requests` (`payment_request_id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO `approval_statuses` (`approval_status_id`, `approval_status_code`, `approval_status_name_ar`, `approval_status_name_en`, `is_active`, `sort_order`)
VALUES (1, 'PENDING', 'بانتظار الاعتماد', NULL, TRUE, 1),
(2, 'UNDER_REVIEW', 'تحت المراجعة', NULL, TRUE, 2),
(3, 'APPROVED', 'معتمد', NULL, TRUE, 3),
(4, 'REJECTED', 'مرفوض', NULL, TRUE, 4),
(5, 'RETURNED', 'معاد للتعديل', NULL, TRUE, 5),
(6, 'CANCELLED', 'ملغي', NULL, TRUE, 6);

INSERT INTO `branch_types` (`branch_type_id`, `branch_type_code`, `branch_type_name_ar`, `branch_type_name_en`, `created_at`, `is_active`, `sort_order`, `updated_at`)
VALUES (1, 'MAIN', 'رئيسي', 'Main', TIMESTAMP '2026-01-01 00:00:00', TRUE, 10, NULL),
(2, 'OPERATING', 'تشغيلي', 'Operating', TIMESTAMP '2026-01-01 00:00:00', TRUE, 20, NULL),
(3, 'DISTRIBUTION', 'نقطة توزيع', 'Distribution Point', TIMESTAMP '2026-01-01 00:00:00', TRUE, 30, NULL),
(4, 'WAREHOUSE', 'مستودع', 'Warehouse', TIMESTAMP '2026-01-01 00:00:00', TRUE, 40, NULL);

INSERT INTO `numbering_document_types` (`numbering_document_type_id`, `document_type_code`, `document_type_name_ar`, `document_type_name_en`, `is_active`, `sort_order`)
VALUES (1, 'BRANCH', 'فرع', NULL, TRUE, 1),
(2, 'COMPANY', 'شركة', NULL, TRUE, 2),
(3, 'RECEIPT_VOUCHER', 'سند قبض', NULL, TRUE, 3),
(4, 'PAYMENT_VOUCHER', 'سند صرف', NULL, TRUE, 4),
(5, 'PAYMENT_REQUEST', 'طلب صرف', NULL, TRUE, 5),
(6, 'JOURNAL_ENTRY', 'قيد محاسبي', NULL, TRUE, 6),
(7, 'CUSTOMER', 'عميل', NULL, TRUE, 7),
(8, 'VEHICLE', 'مركبة', NULL, TRUE, 8),
(9, 'DRIVER', 'سائق', NULL, TRUE, 9),
(10, 'SHIPMENT', 'بوليصة شحن', NULL, TRUE, 10),
(11, 'TICKET', 'تذكرة', NULL, TRUE, 11),
(12, 'TRIP', 'رحلة', NULL, TRUE, 12);

INSERT INTO `payment_methods` (`payment_method_id`, `is_active`, `is_bank`, `is_cash`, `payment_method_code`, `payment_method_name_ar`, `payment_method_name_en`, `requires_reference`, `requires_reference_date`, `sort_order`)
VALUES (1, TRUE, FALSE, TRUE, 'CASH', 'نقدي', NULL, FALSE, FALSE, 1),
(2, TRUE, TRUE, FALSE, 'CHEQUE', 'شيك', NULL, TRUE, TRUE, 2),
(3, TRUE, TRUE, FALSE, 'BANK_TRANSFER', 'تحويل بنكي', NULL, TRUE, TRUE, 3);

INSERT INTO `system_permissions` (`permission_id`, `created_at`, `is_active`, `module_name`, `permission_code`, `permission_name`, `permission_type`, `sort_order`)
VALUES (1, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'VIEW', 'عرض', 'DATA', 1),
(2, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'ADD', 'إضافة', 'DATA', 2),
(3, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'EDIT', 'تعديل', 'DATA', 3),
(4, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'DELETE', 'إيقاف أو حذف', 'DATA', 4),
(5, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'PRINT', 'طباعة', 'DATA', 5),
(6, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'EXPORT', 'تصدير', 'DATA', 6),
(7, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'IMPORT', 'استيراد', 'DATA', 7),
(8, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'APPROVE', 'اعتماد', 'DATA', 8),
(9, TIMESTAMP '2026-01-01 00:00:00', TRUE, NULL, 'UNAPPROVE', 'إلغاء اعتماد', 'DATA', 9);

INSERT INTO `system_screens` (`screen_id`, `created_at`, `is_active`, `module_name`, `screen_code`, `screen_name`, `sort_order`)
VALUES (1, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'TenantGroups', 'المجموعات التجارية', 5),
(2, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'Companies', 'الشركات', 10),
(3, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'Branches', 'الفروع', 20),
(4, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'Countries', 'الدول', 25),
(5, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'Governorates', 'المحافظات', 26),
(6, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الهيكل المؤسسي', 'Cities', 'المدن', 27),
(7, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'المستخدمون والصلاحيات', 'Users', 'المستخدمون', 40),
(8, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'المستخدمون والصلاحيات', 'Roles', 'الأدوار', 50),
(9, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'المستخدمون والصلاحيات', 'RolePermissions', 'صلاحيات الأدوار', 60),
(10, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الأمن والرقابة', 'AuditLogs', 'سجل التدقيق والرقابة', 70),
(11, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'المستخدمون والصلاحيات', 'Sessions', 'الجلسات النشطة', 80),
(12, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'GeneralSettings', 'الإعدادات العامة والمالية', 70),
(13, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'SystemScreens', 'كتالوج شاشات النظام', 80),
(14, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'NumberingSettings', 'إعدادات الترقيم', 90),
(15, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'FiscalYears', 'السنوات المالية', 95),
(16, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'FiscalPeriods', 'الفترات المالية', 100),
(17, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'ExchangeRates', 'أسعار الصرف', 110),
(18, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'PaymentMethods', 'طرق السداد', 120),
(19, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'VoucherTypes', 'أنواع السندات', 130),
(20, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'VoucherStatuses', 'حالات السندات', 140),
(21, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التهيئة والإعدادات', 'ApprovalPolicies', 'سياسات الاعتماد والسقوف', 150),
(22, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'ChartOfAccounts', 'الدليل المحاسبي', 160),
(23, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'Currencies', 'العملات', 170),
(24, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'CostCenters', 'مراكز التكلفة', 180),
(25, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'CashBoxes', 'الصناديق', 190),
(26, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'Banks', 'البنوك والحسابات البنكية', 200),
(27, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'Parties', 'الأطراف المالية', 210),
(28, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'ReceiptVoucher', 'سند القبض', 220),
(29, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'PaymentVoucher', 'سند الصرف', 230),
(30, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'PaymentRequest', 'طلب الصرف', 235),
(31, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'JournalVoucher', 'القيد اليومي', 240),
(32, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'DocumentSearch', 'البحث عن المستندات', 250),
(33, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'ApprovalRequests', 'طلبات الاعتماد', 260),
(34, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'الحسابات', 'FinancialLimits', 'السقوف المالية وحركات الاستخدام', 270),
(35, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التقارير المالية', 'TrialBalance', 'ميزان المراجعة', 280),
(36, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'التقارير المالية', 'GeneralLedger', 'الأستاذ العام', 290);

INSERT INTO `voucher_statuses` (`voucher_status_id`, `is_active`, `sort_order`, `voucher_status_code`, `voucher_status_name_ar`, `voucher_status_name_en`)
VALUES (1, TRUE, 1, 'DRAFT', 'مسودة', NULL),
(2, TRUE, 2, 'PENDING', 'معلق', NULL),
(3, TRUE, 3, 'REVIEWED', 'تمت المراجعة', NULL),
(4, TRUE, 4, 'RETURNED', 'معاد للتصحيح', NULL),
(5, TRUE, 5, 'APPROVED', 'معتمد', NULL),
(6, TRUE, 6, 'POSTED', 'مرحل', NULL),
(7, TRUE, 7, 'CANCELLED', 'ملغي', NULL),
(8, TRUE, 8, 'REVERSED', 'معكوس', NULL);

INSERT INTO `voucher_types` (`voucher_type_id`, `is_active`, `sort_order`, `voucher_type_code`, `voucher_type_name_ar`, `voucher_type_name_en`)
VALUES (1, TRUE, 1, 'RECEIPT', 'سند قبض', 'Receipt Voucher'),
(2, TRUE, 2, 'PAYMENT', 'سند صرف', 'Payment Voucher'),
(3, TRUE, 3, 'JOURNAL', 'قيد يومية', 'Journal Voucher'),
(4, TRUE, 4, 'ADJUSTMENT', 'قيد تسوية', 'Adjustment Voucher'),
(5, TRUE, 5, 'OPENING', 'قيد افتتاحي', 'Opening Voucher');

CREATE UNIQUE INDEX `IX_account_categories_company_id_category_code` ON `account_categories` (`company_id`, `category_code`);

CREATE UNIQUE INDEX `IX_account_code_settings_company_id_level_no` ON `account_code_settings` (`company_id`, `level_no`);

CREATE INDEX `IX_approval_requests_company_id` ON `approval_requests` (`company_id`);

CREATE UNIQUE INDEX `IX_approval_statuses_approval_status_code` ON `approval_statuses` (`approval_status_code`);

CREATE INDEX `IX_audit_logs_branch_id` ON `audit_logs` (`branch_id`);

CREATE INDEX `IX_audit_logs_table_name_record_id_action_at` ON `audit_logs` (`table_name`, `record_id`, `action_at`);

CREATE UNIQUE INDEX `IX_bank_accounts_company_id_account_no` ON `bank_accounts` (`company_id`, `account_no`);

CREATE UNIQUE INDEX `IX_branch_types_branch_type_code` ON `branch_types` (`branch_type_code`);

CREATE INDEX `IX_branch_types_is_active_sort_order` ON `branch_types` (`is_active`, `sort_order`);

CREATE INDEX `IX_cash_boxes_account_id` ON `cash_boxes` (`account_id`);

CREATE INDEX `IX_cash_boxes_branch_id` ON `cash_boxes` (`branch_id`);

CREATE UNIQUE INDEX `IX_cash_boxes_company_id_branch_id_cash_box_code` ON `cash_boxes` (`company_id`, `branch_id`, `cash_box_code`);

CREATE UNIQUE INDEX `IX_chart_of_accounts_company_id_account_code` ON `chart_of_accounts` (`company_id`, `account_code`);

CREATE INDEX `IX_chart_of_accounts_company_id_is_control_account_control_acco~` ON `chart_of_accounts` (`company_id`, `is_control_account`, `control_account_type`);

CREATE INDEX `IX_chart_of_accounts_parent_account_id` ON `chart_of_accounts` (`parent_account_id`);

CREATE INDEX `IX_cities_country_id` ON `cities` (`country_id`);

CREATE UNIQUE INDEX `IX_cities_governorate_id_city_code` ON `cities` (`governorate_id`, `city_code`);

CREATE UNIQUE INDEX `IX_cities_governorate_id_city_name_ar` ON `cities` (`governorate_id`, `city_name_ar`);

CREATE INDEX `IX_companies_group_id_is_active` ON `companies` (`group_id`, `is_active`);

CREATE UNIQUE INDEX `IX_cost_centers_company_id_center_code` ON `cost_centers` (`company_id`, `center_code`);

CREATE INDEX `IX_cost_centers_parent_cost_center_id` ON `cost_centers` (`parent_cost_center_id`);

CREATE UNIQUE INDEX `IX_countries_country_code` ON `countries` (`country_code`);

CREATE UNIQUE INDEX `IX_countries_iso2` ON `countries` (`iso2`);

CREATE UNIQUE INDEX `IX_countries_iso3` ON `countries` (`iso3`);

CREATE UNIQUE INDEX `IX_currencies_company_id_currency_code` ON `currencies` (`company_id`, `currency_code`);

CREATE INDEX `IX_document_allocations_currency_id` ON `document_allocations` (`currency_id`);

CREATE INDEX `IX_document_allocations_party_id` ON `document_allocations` (`party_id`);

CREATE INDEX `IX_document_allocations_voucher_id` ON `document_allocations` (`voucher_id`);

CREATE INDEX `IX_document_links_from_module_id_from_document_type_id_from_doc~` ON `document_links` (`from_module_id`, `from_document_type_id`, `from_document_id`);

CREATE INDEX `IX_document_links_to_module_id_to_document_type_id_to_document_~` ON `document_links` (`to_module_id`, `to_document_type_id`, `to_document_id`);

CREATE UNIQUE INDEX `IX_exchange_rates_company_id_currency_code_rate_date` ON `exchange_rates` (`company_id`, `currency_code`, `rate_date`);

CREATE INDEX `IX_financial_limit_movements_limit_id` ON `financial_limit_movements` (`limit_id`);

CREATE INDEX `IX_financial_limits_company_id` ON `financial_limits` (`company_id`);

CREATE INDEX `IX_financial_voucher_details_account_id` ON `financial_voucher_details` (`account_id`);

CREATE INDEX `IX_financial_voucher_details_cost_center_id` ON `financial_voucher_details` (`cost_center_id`);

CREATE INDEX `IX_financial_voucher_details_currency_id` ON `financial_voucher_details` (`currency_id`);

CREATE UNIQUE INDEX `IX_financial_voucher_details_voucher_id_line_no` ON `financial_voucher_details` (`voucher_id`, `line_no`);

CREATE UNIQUE INDEX `IX_financial_voucher_headers_branch_id_fiscal_year_id_voucher_t~` ON `financial_voucher_headers` (`branch_id`, `fiscal_year_id`, `voucher_type_id`, `voucher_no`);

CREATE INDEX `IX_financial_voucher_headers_cash_account_id` ON `financial_voucher_headers` (`cash_account_id`);

CREATE INDEX `IX_financial_voucher_headers_currency_id` ON `financial_voucher_headers` (`currency_id`);

CREATE INDEX `IX_financial_voucher_headers_fiscal_year_id` ON `financial_voucher_headers` (`fiscal_year_id`);

CREATE INDEX `IX_financial_voucher_headers_is_posted` ON `financial_voucher_headers` (`is_posted`);

CREATE INDEX `IX_financial_voucher_headers_party_id` ON `financial_voucher_headers` (`party_id`);

CREATE INDEX `IX_financial_voucher_headers_payment_method_id` ON `financial_voucher_headers` (`payment_method_id`);

CREATE INDEX `IX_financial_voucher_headers_voucher_date` ON `financial_voucher_headers` (`voucher_date`);

CREATE INDEX `IX_financial_voucher_headers_voucher_status_id` ON `financial_voucher_headers` (`voucher_status_id`);

CREATE INDEX `IX_financial_voucher_headers_voucher_type_id` ON `financial_voucher_headers` (`voucher_type_id`);

CREATE INDEX `IX_fiscal_periods_branch_id` ON `fiscal_periods` (`branch_id`);

CREATE UNIQUE INDEX `IX_fiscal_periods_company_id_branch_id_fiscal_year_id_period_co~` ON `fiscal_periods` (`company_id`, `branch_id`, `fiscal_year_id`, `period_code`);

CREATE INDEX `IX_fiscal_periods_fiscal_year_id` ON `fiscal_periods` (`fiscal_year_id`);

CREATE UNIQUE INDEX `IX_fiscal_years_company_id_year_name` ON `fiscal_years` (`company_id`, `year_name`);

CREATE UNIQUE INDEX `IX_governorates_country_id_governorate_code` ON `governorates` (`country_id`, `governorate_code`);

CREATE UNIQUE INDEX `IX_governorates_country_id_governorate_name_ar` ON `governorates` (`country_id`, `governorate_name_ar`);

CREATE INDEX `IX_journal_entry_details_account_id` ON `journal_entry_details` (`account_id`);

CREATE INDEX `IX_journal_entry_details_cost_center_id` ON `journal_entry_details` (`cost_center_id`);

CREATE INDEX `IX_journal_entry_details_currency_id` ON `journal_entry_details` (`currency_id`);

CREATE UNIQUE INDEX `IX_journal_entry_details_journal_entry_id_line_no` ON `journal_entry_details` (`journal_entry_id`, `line_no`);

CREATE INDEX `IX_journal_entry_headers_branch_id` ON `journal_entry_headers` (`branch_id`);

CREATE INDEX `IX_journal_entry_headers_entry_date` ON `journal_entry_headers` (`entry_date`);

CREATE UNIQUE INDEX `IX_journal_entry_headers_entry_no` ON `journal_entry_headers` (`entry_no`);

CREATE INDEX `IX_journal_entry_headers_fiscal_year_id` ON `journal_entry_headers` (`fiscal_year_id`);

CREATE INDEX `IX_journal_entry_headers_source_voucher_id` ON `journal_entry_headers` (`source_voucher_id`);

CREATE INDEX `IX_login_attempts_login_name_attempted_at` ON `login_attempts` (`login_name`, `attempted_at`);

CREATE INDEX `IX_login_attempts_session_id` ON `login_attempts` (`session_id`);

CREATE INDEX `IX_login_attempts_user_id_attempted_at` ON `login_attempts` (`user_id`, `attempted_at`);

CREATE UNIQUE INDEX `IX_numbering_counters_document_type_company_id_branch_id_year_v~` ON `numbering_counters` (`document_type`, `company_id`, `branch_id`, `year_value`);

CREATE UNIQUE INDEX `IX_numbering_document_types_document_type_code` ON `numbering_document_types` (`document_type_code`);

CREATE UNIQUE INDEX `IX_numbering_settings_document_type` ON `numbering_settings` (`document_type`);

CREATE INDEX `IX_parties_account_id` ON `parties` (`account_id`);

CREATE UNIQUE INDEX `IX_parties_company_id_party_code` ON `parties` (`company_id`, `party_code`);

CREATE UNIQUE INDEX `IX_payment_methods_payment_method_code` ON `payment_methods` (`payment_method_code`);

CREATE INDEX `IX_payment_request_attachments_branch_id` ON `payment_request_attachments` (`branch_id`);

CREATE INDEX `IX_payment_request_attachments_company_id` ON `payment_request_attachments` (`company_id`);

CREATE INDEX `IX_payment_request_attachments_fiscal_year_id` ON `payment_request_attachments` (`fiscal_year_id`);

CREATE INDEX `IX_payment_request_attachments_payment_request_id_is_active` ON `payment_request_attachments` (`payment_request_id`, `is_active`);

CREATE INDEX `IX_payment_request_lines_account_id` ON `payment_request_lines` (`account_id`);

CREATE INDEX `IX_payment_request_lines_cost_center_id` ON `payment_request_lines` (`cost_center_id`);

CREATE INDEX `IX_payment_request_lines_currency_id` ON `payment_request_lines` (`currency_id`);

CREATE UNIQUE INDEX `IX_payment_request_lines_payment_request_id_line_no` ON `payment_request_lines` (`payment_request_id`, `line_no`);

CREATE INDEX `IX_payment_requests_branch_id` ON `payment_requests` (`branch_id`);

CREATE UNIQUE INDEX `IX_payment_requests_company_id_branch_id_fiscal_year_id_request~` ON `payment_requests` (`company_id`, `branch_id`, `fiscal_year_id`, `request_no`);

CREATE INDEX `IX_payment_requests_company_id_branch_id_fiscal_year_id_status` ON `payment_requests` (`company_id`, `branch_id`, `fiscal_year_id`, `status`);

CREATE INDEX `IX_payment_requests_fiscal_year_id` ON `payment_requests` (`fiscal_year_id`);

CREATE INDEX `IX_payment_requests_party_id` ON `payment_requests` (`party_id`);

CREATE INDEX `IX_payment_requests_payment_method_id` ON `payment_requests` (`payment_method_id`);

CREATE INDEX `IX_payment_requests_payment_voucher_id` ON `payment_requests` (`payment_voucher_id`);

CREATE INDEX `IX_refresh_tokens_branch_id` ON `refresh_tokens` (`branch_id`);

CREATE INDEX `IX_refresh_tokens_company_id` ON `refresh_tokens` (`company_id`);

CREATE INDEX `IX_refresh_tokens_fiscal_year_id` ON `refresh_tokens` (`fiscal_year_id`);

CREATE INDEX `IX_refresh_tokens_session_id` ON `refresh_tokens` (`session_id`);

CREATE UNIQUE INDEX `IX_refresh_tokens_token_hash` ON `refresh_tokens` (`token_hash`);

CREATE INDEX `IX_refresh_tokens_user_id_expires_at` ON `refresh_tokens` (`user_id`, `expires_at`);

CREATE UNIQUE INDEX `IX_role_permissions_role_id_screen_id` ON `role_permissions` (`role_id`, `screen_id`);

CREATE INDEX `IX_role_permissions_screen_id` ON `role_permissions` (`screen_id`);

CREATE UNIQUE INDEX `IX_roles_role_code` ON `roles` (`role_code`);

CREATE UNIQUE INDEX `IX_system_permissions_permission_code` ON `system_permissions` (`permission_code`);

CREATE UNIQUE INDEX `IX_system_screens_screen_code` ON `system_screens` (`screen_code`);

CREATE UNIQUE INDEX `IX_system_settings_setting_key_scope_company_id_branch_id_fisca~` ON `system_settings` (`setting_key`, `scope`, `company_id`, `branch_id`, `fiscal_year_id`);

CREATE INDEX `IX_tenant_branches_branch_type_id` ON `tenant_branches` (`branch_type_id`);

CREATE INDEX `IX_tenant_branches_city_id` ON `tenant_branches` (`city_id`);

CREATE UNIQUE INDEX `IX_tenant_branches_company_id_branch_code` ON `tenant_branches` (`company_id`, `branch_code`);

CREATE INDEX `IX_tenant_branches_country_id` ON `tenant_branches` (`country_id`);

CREATE INDEX `IX_tenant_branches_currency_id` ON `tenant_branches` (`currency_id`);

CREATE INDEX `IX_tenant_branches_governorate_id` ON `tenant_branches` (`governorate_id`);

CREATE INDEX `IX_tenant_branches_parent_branch_id` ON `tenant_branches` (`parent_branch_id`);

CREATE UNIQUE INDEX `IX_tenant_groups_group_code` ON `tenant_groups` (`group_code`);

CREATE INDEX `IX_tenant_groups_show_in_login_is_active_sort_order` ON `tenant_groups` (`show_in_login`, `is_active`, `sort_order`);

CREATE UNIQUE INDEX `IX_user_permissions_user_id_permission_category_permission_name` ON `user_permissions` (`user_id`, `permission_category`, `permission_name`);

CREATE INDEX `IX_users_branch_id` ON `users` (`branch_id`);

CREATE UNIQUE INDEX `IX_users_company_id_user_code` ON `users` (`company_id`, `user_code`);

CREATE UNIQUE INDEX `IX_users_login_name` ON `users` (`login_name`);

CREATE INDEX `IX_users_role_id` ON `users` (`role_id`);

CREATE INDEX `IX_voucher_action_logs_voucher_id_action_at` ON `voucher_action_logs` (`voucher_id`, `action_at`);

CREATE UNIQUE INDEX `IX_voucher_statuses_voucher_status_code` ON `voucher_statuses` (`voucher_status_code`);

CREATE UNIQUE INDEX `IX_voucher_types_voucher_type_code` ON `voucher_types` (`voucher_type_code`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260729161536_Baseline_Phase1', '8.0.2');

COMMIT;

