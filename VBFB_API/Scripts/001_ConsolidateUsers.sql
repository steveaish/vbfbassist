/*
    001_ConsolidateUsers.sql

    Brings the login users from bedfordale_master.dbo.users into bedfordale_vbfbassist.dbo.users
    so the new API only needs the one database.

    - Additive only: the Laravel app still authenticates against bedfordale_master, so nothing it
      reads is altered.
    - Safe to re-run (MERGE on id). Run it again at cut-over to pick up any users or password
      changes made through Laravel in the meantime.
    - bcrypt hashes ($2y$) are copied unchanged; the API verifies them with BCrypt.Net.
*/
USE bedfordale_vbfbassist;
GO

IF COL_LENGTH('dbo.users', 'profile_profiles') IS NULL ALTER TABLE dbo.users ADD profile_profiles char(1) NULL;
IF COL_LENGTH('dbo.users', 'profile_cadets') IS NULL   ALTER TABLE dbo.users ADD profile_cadets char(1) NULL;
-- ReadyApi-style session key: issued at login, sent back in the UserKey header
IF COL_LENGTH('dbo.users', 'userkey') IS NULL          ALTER TABLE dbo.users ADD userkey nvarchar(50) NULL;
IF COL_LENGTH('dbo.users', 'userkeyexpiry') IS NULL    ALTER TABLE dbo.users ADD userkeyexpiry datetime NULL;
GO

SET IDENTITY_INSERT dbo.users ON;

MERGE dbo.users AS t
USING bedfordale_master.dbo.users AS s
    ON t.id = s.id
WHEN MATCHED THEN UPDATE SET
    t.company = s.company, t.name = s.name, t.email = s.email, t.password = s.password,
    t.status = s.status, t.admin = s.admin, t.remember_token = s.remember_token,
    t.created_at = s.created_at, t.updated_at = s.updated_at, t.password2 = s.password2,
    t.profile_profiles = s.profile_profiles, t.profile_cadets = s.profile_cadets
WHEN NOT MATCHED BY TARGET THEN
    INSERT (id, company, name, email, password, status, admin, remember_token, created_at, updated_at, password2, profile_profiles, profile_cadets)
    VALUES (s.id, s.company, s.name, s.email, s.password, s.status, s.admin, s.remember_token, s.created_at, s.updated_at, s.password2, s.profile_profiles, s.profile_cadets);

SET IDENTITY_INSERT dbo.users OFF;
GO

SELECT COUNT(*) AS users_in_vbfbassist FROM dbo.users;
