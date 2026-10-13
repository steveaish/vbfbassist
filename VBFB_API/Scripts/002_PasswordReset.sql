/*
    002_PasswordReset.sql

    Columns for "Forgot my password". Laravel linked to a reset page but its password_resets table
    never existed, so resets never worked. Only a SHA-256 hash of the emailed token is stored.
    Additive and safe to re-run; Laravel does not read these columns.
*/
USE bedfordale_vbfbassist;
GO

IF COL_LENGTH('dbo.users', 'resettokenhash') IS NULL ALTER TABLE dbo.users ADD resettokenhash nvarchar(100) NULL;
IF COL_LENGTH('dbo.users', 'resettokenexpiry') IS NULL ALTER TABLE dbo.users ADD resettokenexpiry datetime NULL;
GO
