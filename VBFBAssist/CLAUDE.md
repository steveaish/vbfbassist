# CLAUDE.md

## Overview

VBFBAssist: a React 18 + Vite SPA (plain JSX) for VBFB Assist (Bedfordale Volunteer Bush Fire Brigade). It replaces the Laravel UI in `../Laravel_version` and follows the conventions of `C:\Dev\ReadyWebApp`. The backend is `../VBFB_API`, at `https://localhost:7184/api/v1` in development.

## Commands

```
npm run dev        # http://localhost:5183
npm run build      # dist/
npm run lint       # --max-warnings 0
```

## Environment

`.env` is git-ignored; copy `.env.example`. Variables must start with `REACT_`. `vite.config.js` injects them as `process.env.*`, CRA-style, so don't use `import.meta.env`. `REACT_APP_CURRENTVERSION` is computed in `vite.config.js`.

## Architecture

- `src/App.jsx` is the hub: Syncfusion licence, navbar, and all routes. Menu entries live in `src/menu.js`, each with a `permission` flag or a list of flags (any of them), and the screen for each key is in the `screens` map.
- `/resetpassword` (the emailed link) renders before the login check, so it works whether or not someone is signed in.
- **Feature-per-folder**, as in ReadyWebApp: `src/<feature>/<Feature>.jsx` for the list (a Syncfusion grid with `DataManager` + `UrlAdaptor` pointed at `GetApiUrl() + '/<endpoint>'`, `headers: [ApiHeaders()]`) and `<Feature>Edit.jsx` for the form.
- `src/utils/ApiUtils.jsx`:
  - `api` is an axios instance that adds the auth headers and returns to login on a 401.
  - `ApiHeaders()` is for Syncfusion.
  - `ApiQueryAuth()` is for links opened in a new tab, such as PDFs and downloads.
- Session state lives in `sessionStorage`, set by `SaveSession`: `loggedIn`, `userid`, `userkey`, `username`, `firstname`, `profileid`, and `permissions` (JSON). Check permissions with `hasPermission('<flag>')` from `src/utils/LoginUtils.jsx`. The flags are `isadmin`, `siteadmin`, `cadetleader`, `isfacilitator`, `iscoordinator`, `isvehiclemaint`, `viewmembers`, `canbulksms`, `caneditdocuments`, `canseecalendar`, `caneditcalendar`, `profile_profiles` and `profile_cadets`.

## Shared building blocks

- `utils/GridUtils.jsx`:
  - `createGridDataManager('/x/list')` creates a server-side grid source. Send filters with `new Query().addParams(...)`.
  - Use `DESKTOP_ONLY` as `hideAtMedia` on columns to hide on phones.
  - Also: `useIsMobile()`, `formatDate()`, `yesNo()`.
- `components/FormFields.jsx`: `TextInput`, `SelectInput` and `YesNoSwitch`, bound to a `form` object keyed by DB column names. The options live in `components/formOptions.js`.
- `components/ListEditor.jsx`: a generic Settings list with an add/edit dialog over `GET/POST /x` and `PUT/DELETE /x/{id}`.
- `components/PpeTable.jsx`: PPE held by a member, shown as a table on desktop and as cards on phones.
- `components/Modals.jsx`: `ConfirmDialog`.
- **Screens swap list/edit with state** (`editId`, `viewId`) rather than routes, the ReadyWebApp convention.
- Ported screens are registered in the `screens` map in `App.jsx`.
- The Profiles grid exports to Excel with `includeHiddenColumn: true`. The hidden columns are the old Laravel CSV fields.

## Look and feel

- Brand colours were sampled from the Bush Fire Service logo: green `#12654A` (dark `#0B4A35`) as primary, red `#F61814` (dark `#C8110E`) as the accent and for danger.
- They are defined twice; keep both in sync:
  - `src/index.css`: CSS variables and the Bootstrap overrides.
  - `src/theme.js`: the MUI theme.
- **Every screen must work at phone width** (375px), with no horizontal scroll and large tap targets. Vehicle checks and PPE are done on phones in the shed.

## Versioning

History lives in `CHANGES.md`. The top `## YY.MMDD.NN` heading is the current version.
