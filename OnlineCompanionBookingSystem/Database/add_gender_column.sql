-- Adds the companion's gender (stated at registration, checked by the admin against the valid ID).
-- Safe to run more than once. The application also runs this check by itself, so running it is optional.
USE [OnlineCompanionBookingManagementSystem_Database]
GO

IF COL_LENGTH('dbo.CompanionProfiles', 'Gender') IS NULL
    ALTER TABLE dbo.CompanionProfiles ADD Gender NVARCHAR(20) NULL;
GO

-- Companions who registered before this field existed stay NULL ("Not provided" on the admin pages).
-- To fill them in after checking their ID, for example:
--   UPDATE dbo.CompanionProfiles SET Gender = 'Female' WHERE CompanionID = 1;
