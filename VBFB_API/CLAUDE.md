# CLAUDE.md

## Overview

VBFB_API: the ASP.NET Core (`net10.0`) backend for VBFB Assist (Bedfordale Volunteer Bush Fire Brigade). The sibling `../VBFBAssist` React SPA talks to it. It replaces the Laravel app in `../Laravel_version` and follows the conventions of `C:\Dev\ReadyApi`, simplified to a single database. In development it runs at `https://localhost:7184` and every endpoint is under `api/v1/[controller]`.

## Commands

```
dotnet build VBFB_API.csproj                      # build (build.bat builds Release)
dotnet run --launch-profile VBFB_API              # https://localhost:7184
dotnet dotnet-ef dbcontext scaffold ...           # re-scaffold Models (see "Models" below)
```

`dotnet-ef` is a local tool (`dotnet-tools.json`) pinned to 10.0.0.

**Build output is flat.** It goes to `C:\TonicApps\VBFB_API\` directly, and `VBFB_API.dat` is read from there. The csproj enforces this:

- No `Debug\`, `Release\` or `net10.0\` subfolders, in the output or in `obj\`.
- No satellite language folders (`SatelliteResourceLanguages=en`).
- No `BuildHost-*` folders: the EF Design package doesn't include `contentfiles`.

Keep it that way when adding packages: check the output folder after a build.

## Configuration: VBFB_API.dat

DB credentials, the `APIKEY`, the SMTP settings and `UploadsPath` come from `VBFB_API.dat` (JSON) next to the executable. It is loaded by `Ready.LoadIni()`, the same way ReadyApi loads `ReadyApi.dat`; `appsettings.json` holds logging settings only. The file is git-ignored; copy `VBFB_API.dat.example` to start one. On first load, plain-text passwords are encrypted in place (`Enc: "Y"`). A password that will not decrypt (typed in plain text with `Enc` left at `Y`) is treated as plain text and re-encrypted, instead of failing startup. If the file is missing, an empty one is written and the app exits.

## Database

There is a single database, `bedfordale_vbfbassist`. The Laravel app also used `bedfordale_master` for logins. Its `users` rows are merged in by `Scripts/001_ConsolidateUsers.sql`, which is re-runnable; run it again at cut-over.

**The Laravel app is still live against this database.** Schema changes must be additive only: new columns or tables, never renames or drops. Put each one in a numbered `Scripts/NNN_*.sql` file that is safe to re-run.

## Models

The models are scaffolded with `--use-database-names`, so classes and properties keep the Laravel table and column names (`profiles.isadmin`, `VEH_Check_Header`). `CS8981` is suppressed for this reason. To re-scaffold:

```
dotnet dotnet-ef dbcontext scaffold "<conn>" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context ApplicationDbContext --context-dir Models --namespace VBFB_API.Models --use-database-names --no-onconfiguring --no-pluralize --force
```

**Almost no tables have primary keys**, so the scaffolder maps them as keyless, read-only entities. `Models/ApplicationDbContext.Keys.cs` (hand-written, and kept when re-scaffolding) declares each table's identity column as its EF key. It also registers the triggers on `profiles` and `VEH_Check_Header`. Add any new table there too, or EF will silently skip updates to it.

## Authentication

Auth is ReadyApi-style, implemented in `Middleware/AuthenticationMiddleware.cs`:

- Every `/api/` request needs the `APIKEY` header.
- `POST api/v1/login` and `GET api/v1/about` need only the API key.
- Every other request also needs `UserId` and `UserKey` headers matching `users.id`, `users.userkey` and an unexpired `users.userkeyexpiry`. Unlike ReadyApi, a bad key gets a 401. Links opened in a new tab can pass the same values as `?apikey=&userid=&userkey=` query parameters.

Login matches Laravel's behaviour:

- Users sign in with `users.email` and a bcrypt (`$2y$`) password. `users.status` must be 1.
- New hashes are written as `$2y$` with cost 10, so Laravel can still verify them.
- Permission flags come from the `profiles` row with the same email; `Login.GetProfileForEmail` prefers the active profile, then the lowest id. `isadmin` implies every other flag. `users.admin = 1` is `siteadmin`.

## Conventions

- **No interfaces.** Write concrete classes, registered directly or used statically (`Classes/`).
- **JSON uses the exact database column names.** Program.cs swaps the camelCase resolver for `DefaultContractResolver`, so the screens get `CrewName` and `IsVehicleMaint`, as Laravel did.
- **Permissions are checked server-side.** Put `[RequirePermission("isadmin")]` on a controller or action; several flags means any of them. Inside an action, use `Permissions.Has(HttpContext, _context, "flag")`. `Permissions.CurrentProfile` returns the caller's `profiles` row.
- **Grid list endpoints** are `POST <controller>/list`:
  - They take the Syncfusion DataManager body as a `JObject` and parse it with `ApiUtils.SyncFusionBreakDownApiBody`, which validates field names and parameterizes values.
  - They query with `SqlQuery.Select` / `SqlQuery.Count` and return `{ result, count }`.
  - Screen filters arrive in `data["params"]` (DataManager `addParams`); see `ProfileFields.BuildFilters`.
- **Writes from a JSON body** go through `EntityUpdater.Fill(entity, data, allowedFields)`, the equivalent of Laravel's `fill()` with `$fillable`. Never fill without a field list.
- **Profiles:**
  - Never return `profiles.username` or `profiles.password`; use `ProfileFields.SafeColumns` / `ToSafeDictionary`.
  - `dob` and `startdate` are text (dd/mm/yyyy), each with a parsed `_date` column that `ProfileFields.ApplyDates` keeps in step.
- **Audit:** `program_log` entries are written by `ProgramLog.Write`. Screens call `POST <controller>/opened` once when they open.
- **Uploaded files:**
  - They live flat in `UploadsPath` (`VBFB_API.dat`, currently `C:\DevPlay\VBFBData\uploads`) under random 40-character names, the same scheme as Laravel. The database keeps the original filename.
  - Save new files with `FileStore.SaveAsync` and serve them with `DocumentsController.ServeFile`. `FileStore.PathFor` rejects anything that isn't a plain stored name.
  - `Scripts/CopyUploads.ps1` copies new files from Laravel's `storage\app\uploads`. Re-run it at cut-over.
  - The cadet document `C Barrett.pdf` (documents.id 34) was a 0-byte upload in Laravel too.
- **Passwords and logins:**
  - Forgot/reset password and admin "set-password" links go through `Classes/PasswordReset.cs`. The emailed token is random and only its SHA-256 hash is stored, in `users.resettokenhash` / `resettokenexpiry` (`Scripts/002_PasswordReset.sql`).
  - Links point at `AppUrl` in `VBFB_API.dat`; set it to the real site address when deployed.
  - Admin > Users (`AdminUsersController`) is for `users.admin = 1` (`siteadmin`). Logins are deactivated, never deleted.
- **Dashboard:**
  - `DashboardController` runs the SQL stored in the active `reports` rows, each inside a transaction that is always rolled back, so a report can read but never write.
  - There is no editor for those rows (Laravel's dashboard config screen wasn't linked anywhere). Change them in SQL.
- **Mail and SMS:**
  - Email goes through `Mailer.Send`, using SMTP2GO settings from `VBFB_API.dat`.
  - Bulk SMS is sent as email to `<mobile>@sms.clicksend.com` from `SmsFromAddress`, which must be a sender registered with ClickSend. Recipients are sent in batches of 50.
- Use `SqlParameter` for every value in raw SQL.

## Versioning

History lives in `CHANGES.md`. The top `## YY.MMDD.NN` heading is the current version. Add a `- DEV | CASE | Notes` line for each change.
