# MasterAntiqueRepair

Master Antique Repair is a repair-shop tracking application. Customers submit repair requests for antique items; employees pick up and complete them; managers oversee the workload, administer employee and customer accounts, and have Metrics, an Audit Log, and a cross-entity Search page.

## Overview

**PHASE 1 — Legacy Build**
Stack: ASP.NET Framework 4.7.2, C#, WebForms, SQL Server

This is explicitly a "legacy build" learning exercise, built to a fixed spec. The sections below walk through that spec item by item and say exactly how (and where) each requirement is met in the code as it stands today.

## Phase 1 requirements

### Functional requirements

1. **A single core domain with 3–4 related entities.** `User` (base class for `Customer`/`Employee`/`Manager`), `Ticket`, and `Comment` — see [Domain model](#domain-model).
2. **Basic CRUD for each entity.** Comments are Create/Read only by design (see [Comments](#comments) — Edit and Delete were deliberately removed). Tickets are Create/Read from the customer/employee side, with state transitions (assign, complete) standing in for update; a Manager can Read/Update/soft-Delete both Employees and Customers via the "Employee Management"/"Customer Management" panels on `/ManagerView`.
3. **One approval/status-transition workflow with at least 3 states.** `Ticket.State` (`State.RepairState`): `SUBMITTED` → `INPROGRESS` → `COMPLETED`.
4. **A simple login/role check (hardcoded roles are fine).** Three roles — Customer, Employee, Manager — enforced server-side on every protected page via `RepairAuthHelper.RequireRole`, independent of what the UI shows. See [Authentication](#authentication).
5. **One list/search view with filtering and pagination.** The Search page (`/TicketDetailView`) has four tabs — ticket, customer, employee, and free-text comment search — plus the Manager-only Audit Log's Entity Id filter and adjustable page size. See [Manager tools](#manager-tools).

### Non-functional requirements

1. **Layered architecture (UI / business logic / data access clearly separated — no logic in code-behind).** This is met with a real three-layer split, not just a loose convention:
   - **UI (View)** — the `.aspx` markup. Presentation only: `Eval`/`Bind` expressions, visibility toggles, display helpers (`UiHelpers.Truncate`, `StatusLabelClass`). No decisions get made here.
   - **Controller** — the `.aspx.cs` code-behind. Web Forms has no Controller in the MVC-framework sense, so code-behind plays that role: it reads posted values, calls exactly one Service method, and binds the result to the View. No `DbContext`, no LINQ, no business rules live here.
   - **Model** — everything else, split into two sub-layers that both live in `MasterAntiqueRepairData/App_Code/`:
     - **Services** (`App_Code/Services/`) — one per feature area (`TicketService`, `CommentService`, `AccountService`, `SearchService`, `MetricsService`, `AuditLogService`, plus `AuthService` in the website project — see below). Each owns a `RepairShopContext` for its lifetime, orchestrates a use case, and calls domain-object methods for the actual rules.
     - **Repositories** (`App_Code/Repositories/`) — `TicketRepository`, `UserRepository`, `CommentRepository`, `AuditLogRepository`, `PasswordResetTokenRepository`. Pure `DbContext`-bound query/persist methods, no business rules. Services depend on Repositories; Repositories never call back up.
     - The domain classes (`Ticket`, `User`/`Customer`/`Employee`/`Manager`) keep their entity-intrinsic rules exactly as before (`Employee.TakeTicket`, `Ticket.CreateSubmitted`, `User.SetPassword`, comment validation) — Services call these, they don't duplicate them, and these classes never touch a `DbContext` directly.

     `AuthService` is the one deliberate exception to "Services live in `MasterAntiqueRepairData`": it needs the OWIN cookie sign-in and per-request IP address, neither of which the class library has access to, so it lives in the website's own `App_Code/` alongside `RepairAuthHelper` (which stays as-is — a stateless, cross-cutting auth guard used by every protected page, not business logic specific to login/signup/reset).
2. **Server-side input validation.** See [Security](#security) — length limits, control-character rejection, password composition rules, all enforced on the domain classes, not just presence-checks.
3. **Logging of workflow state changes.** The `AuditLog` entity and the Manager-only Audit Log page (`/AuditLogView`) — see [Domain model](#domain-model) and [Manager tools](#manager-tools).
4. **A short README explaining the structure and how to run it.** This document.

## Domain model

- **`User`** — base type: `Id`, `Name`, `CreatedAt`, `PasswordHash`, `FailedLoginAttempts`/`LockedOutUntil` (login lockout), `DeletedAt` (soft delete), `Tickets` (tickets *assigned to* this user as an employee). Mapped Table-Per-Hierarchy — one `Users` table with a `Discriminator` column.
  - **`Customer : User`** — submits repair requests.
  - **`Employee : User`** — `TakeTicket(Ticket)` assigns a ticket to itself; `CompleteTicket(Ticket, comment)` marks one done, optionally with a comment.
  - **`Manager : User`** — no self-registration; created via `Scripts/Seed-InitialUsers.ps1` or direct DB insert only.
- **`Ticket`** — `Id`, `Description`, `State`, `Customer` (submitter), `User` (assigned employee, nullable until picked up), `Comments`, `SubmittedDate`, `AssignedDate`, `CompletedDate`.
- **`Comment`** — `Id`, `Text`, `TicketId`, `UserId`, `CreatedAt`. **Add-only**: once posted, a comment can't be edited or deleted by anyone, including its author — see [Comments](#comments).
- **`AuditLog`** — append-only action log (`ActionType`, `EntityKind`, `EntityId`, `Timestamp`, acting `User`) — never stores comment text or ticket descriptions themselves. Viewable at `/AuditLogView` (Manager-only).
- **`PasswordResetToken`** — backs self-service password reset (`Account/ForgotPassword`/`Account/ResetPassword`).
- **`State.RepairState`** — `SUBMITTED` → `INPROGRESS` → `COMPLETED`.
- **`PasswordHasher`** — PBKDF2 (`Rfc2898DeriveBytes`, random salt per password, 100,000 iterations, versioned format with a legacy fallback for older hashes).

## Comments

Comments are **add-only**. A Customer or Employee can post a comment on a ticket (Customers once it's `COMPLETED`; Employees on a ticket assigned to them, including as part of marking it complete) — but once posted, nobody can edit or delete it, not even its author. This was a deliberate design decision: a comment is a permanent record of what was said and when, matching the spirit of the Audit Log sitting alongside it. `CommentService.AddCustomerComment`/`AddEmployeeComment` are the only comment operations in the app.

## User Stories

- **Customer** — I want to submit a repair request for an antique item, so an employee can address it. I want to see the status and comment history of my own tickets, so I know progress without asking. I want to add a comment to a completed ticket, so I can ask a follow-up question or leave feedback.
- **Employee** — I want to see unassigned tickets, so I can pick up new work. I want to assign a ticket to myself, so others know I'm handling it. I want to mark a ticket complete with a note, so the customer knows what was done.
- **Manager** — I want to manage Employee and Customer accounts (add/edit/soft-delete), so I can control who has access. I want to see all tickets and workload across the shop, so I can track overall status. I want to search tickets/customers/employees/comments, so I can answer questions quickly. I want to review the Audit Log, so I have accountability.

## Use Cases

1. **Submit and track a repair** — Customer signs up (`/Account/CustomerSignUp`) → submits a ticket (`/SubmitRepair`) → an Employee picks it up (`/EmployeeView`, "Assign to Me") → completes it with a comment → the Customer sees the update on `/CustomerView` and can reply with their own comment.
2. **Manage accounts** — Manager adds an Employee or Customer, edits their name/password, or soft-deletes them (history, tickets, and comments are all retained — a soft-deleted account just can't log in or receive new assignments).
3. **Investigate via Search and Audit Log** — Manager looks up a ticket/customer/employee by id, or free-text-searches comments, on `/TicketDetailView`; cross-checks against `/AuditLogView`'s action history, filterable by Entity Id.
4. **Self-service password reset** — a locked-out or forgetful user requests a reset link (`/Account/ForgotPassword`) and sets a new password (`/Account/ResetPassword`) without Manager involvement.

## Getting the code

**Latest code** (`main`):
```bash
git clone https://github.com/davidharrisnet/master-antique-repair.git
```

**A specific released version** (e.g. `v2.0` — see all released versions at [github.com/davidharrisnet/master-antique-repair/releases](https://github.com/davidharrisnet/master-antique-repair/releases)):
```bash
git clone --branch v2.0 https://github.com/davidharrisnet/master-antique-repair.git
```

## Stack

- ASP.NET Web Forms, C#, .NET Framework 4.7.2
- Entity Framework 6 (Code First + Migrations)
- SQL Server LocalDB
- Bootstrap 3 / jQuery, via `System.Web.Optimization` bundling

## Solution structure

`MasterAntiqueRepair.sln` contains three projects:

| Project | Type | Purpose |
|---|---|---|
| `MasterAntiqueRepair` | Website Project (no `.csproj`) | The web app — pages (View/Controller), authentication guard, `AuthService`, styling |
| `MasterAntiqueRepairData` | Class Library | Domain model, EF6 `DbContext`, Services, Repositories |
| `MasterAntiqueRepairScratch` | Console App | Scratch experiments against the domain model; also builds the schema headlessly (see [Getting started](#getting-started)) |

## Getting started

1. **Prerequisites**: Visual Studio 2019+ with the ASP.NET/web workload, SQL Server LocalDB (installed with VS by default).
2. **Restore packages**: `nuget restore MasterAntiqueRepair/MasterAntiqueRepair.sln`. If a fresh restore leaves the website unable to build (`Could not find file '...\Bin\roslyn\csc.exe'`, or an auto-refresh error for `microsoft.aspnet.web.optimization.webforms.dll`) — plain restore doesn't run packages' `install.ps1`, which is how those two land in `Bin/`. Fix in Package Manager Console:
   ```
   Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -reinstall
   Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3
   ```
3. **MSBuild on PATH**: both the EF6 migrations tooling in Package Manager Console and `Scripts/Initialize-Database.ps1` shell out to `msbuild` by bare name. If `Get-Command msbuild` fails, add it via `vswhere.exe` (bundled with VS):
   ```powershell
   $msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin
   [Environment]::SetEnvironmentVariable("PATH", "$env:PATH;$msbuildPath", "User")
   ```
   Restart Visual Studio (and any open terminal) afterward for the change to take effect.
4. **Build the schema without opening the website**: `.\Scripts\Initialize-Database.ps1` builds and runs `MasterAntiqueRepairScratch`, which opens `RepairShopContext` and triggers its Migrations-based initializer — same effect as F5 + one login attempt, no web server involved.
5. **Seed the first accounts**: `.\Scripts\Seed-InitialUsers.ps1` — see [Seeding initial accounts](#seeding-initial-accounts) below. (If PowerShell refuses to run it with a "not digitally signed" error, `Unblock-File .\Scripts\Seed-InitialUsers.ps1` first.)
6. **Run it**: open `MasterAntiqueRepair/MasterAntiqueRepair.sln` in Visual Studio, F5. Starts IIS Express on port `57962`.

## User roles and actions

Three roles — Customer, Employee, Manager — each landing on its own page after login, each restricted server-side (`RepairAuthHelper.RequireRole`) regardless of what the UI shows. See [User Stories](#user-stories) and [Use Cases](#use-cases) above for the "why"; this section is the concrete "what."

### Customer
- Sign up at `/Account/CustomerSignUp` (self-service — the only role that is). Lands on `/CustomerView` ("My Repairs") after login.
- Submit a repair request at `/SubmitRepair` — creates a `Ticket` in `SUBMITTED` state.
- View own tickets, with status, dates, and both comment threads, on `/CustomerView`.
- Add a comment to their own ticket once it's `COMPLETED` (see [Comments](#comments)).
- Cannot: see other customers' tickets, act on any employee/manager page, or comment on a ticket that isn't theirs or isn't yet completed.

### Employee
- Cannot self-register — created only by a Manager, via the "Add New Employee" panel on `/ManagerView`. Lands on `/EmployeeView` after login.
- View unassigned tickets, each with "Assign to Me" (`Employee.TakeTicket` — moves to `INPROGRESS`, records `AssignedDate`).
- View "My Tickets" — everything currently or previously assigned to them, each with "Mark Complete" while not yet `COMPLETED` (`Employee.CompleteTicket` — records `CompletedDate`, optionally posts a comment).
- Add a comment to a ticket assigned to them once `COMPLETED`.
- Cannot: take or complete a ticket assigned to someone else.

### Manager
- Standing credentials seeded by `Scripts/Seed-InitialUsers.ps1` — no UI path exists to create a Manager account.
- Lands on `/ManagerView` ("Administration") — Employee Management and Customer Management panels (add/edit/soft-delete), a dropdown to view one employee's tickets, and every unassigned ticket with its submitting customer.
- `/AuditLogView` and `/TicketDetailView` (Search) — see [Manager tools](#manager-tools).
- `/Metrics` — ticket/comment charts and stats over a rolling window.
- Does not add, edit, or delete comments — Manager's view of a ticket's comments (via Search) is read-only.

Login credentials for the pre-seeded accounts:

|Name|Password|Role|
|---|---|---|
|manager|ManagerPass123!|Manager|
|employee1|EmployeePass123!|Employee|
|employee2|EmployeePass456!|Employee|

There's no seeded customer account — sign up as one via `/Account/CustomerSignUp`.

## Manager tools

Two Manager-only pages exist purely for oversight — neither is part of the customer/employee workflow, and neither lets a Manager add, edit, or delete anything on a ticket.

### Audit Log (`/AuditLogView`)

Lists every tracked `AuditLog` row — Timestamp, User, Action, Entity type, Entity Id — newest first, backed by `AuditLogService`. A "Rows per page" dropdown (10/20) controls `GridView` paging (`PagerSettings Mode="NumericFirstLast"`). An "Entity Id" box filters the log to rows matching that id. Each row also has a **View** link resolving to the actual thing acted on: `Ticket` rows link straight to `/TicketDetailView.aspx?id={EntityId}`; `Comment` rows resolve that comment's `TicketId` first (via `AuditLogService.GetTicketIdForComment`) and link to the same ticket; `User` rows have no link (no separate user-detail page).

### Search (`/TicketDetailView`)

Four tabs, backed by `SearchService`:
- **Ticket** — pick from a dropdown or type an id; shows Description, Status, Customer, Assigned To, all three dates, and both comment threads, read-only, with cross-links to the Customer/Employee tabs.
- **Customer** — pick or type an id; shows the customer and every ticket they've submitted.
- **Employee** — pick or type an id; shows the employee and every ticket assigned to them.
- **Comment** — free-text search across comment text *and* ticket descriptions, newest first, each result linking back to its ticket.

Customer/Employee lookups here are deliberately unfiltered (include soft-deleted accounts) — a Manager investigating a ticket needs to find historical records too, not just currently-active ones.

## Authentication

Custom, built directly on the domain model — not ASP.NET Identity. `AuthService` (website `App_Code/`) owns the actual login/signup/reset business logic:

- **Login** — throttle check, lookup by name, soft-delete/lockout checks, password verification, audit log, sign-in via `RepairAuthHelper.SignIn`.
- **Sign up** — throttle check, duplicate-username check (plus a DB-level unique-index backstop for the race), `Customer.SetPassword`, audit log, sign-in.
- **Password reset** — `Account/ForgotPassword` issues a `PasswordResetToken` (1-hour expiry); since this app has no SMTP configured, the reset link is shown directly on the page rather than emailed. `Account/ResetPassword` validates the token, applies the new password, and marks the token used.

`RepairAuthHelper` (also website `App_Code/`) stays separate from `AuthService` — it's the stateless, `DbContext`-free plumbing (`SignIn`/`SignOut`/`GetCurrentUserId`/`RequireRole`) that every protected page's `Page_Load` calls as a guard, not a business flow of its own. It builds a `ClaimsIdentity` from a domain `User` (name, ID, and a role claim — the concrete type name, e.g. `"Employee"`, unwrapped from EF6's lazy-loading proxy via `ObjectContext.GetObjectType`) and signs in via OWIN's cookie middleware (`App_Code/Startup.Auth.cs`). After login, each role lands on its own page unless a `ReturnUrl` was specified.

### Running migrations

The connection string lives only in the website's `Web.config` — the class library has no connection string of its own; at runtime the ASP.NET host's config is what EF reads regardless of which assembly the `DbContext` lives in.

```
Add-Migration <Name> -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
```

Set **Default project** to `MasterAntiqueRepairData` in Package Manager Console first. Running migrations commands against the website project itself fails outright (`You cannot call a method on a null-valued expression`) — that's the reason `MasterAntiqueRepairData` exists as a separate project.

## Accessing the database

The database is a LocalDB `.mdf` file under `MasterAntiqueRepair/MasterAntiqueRepair/App_Data/` (`aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4`, the `DefaultConnection` catalog).

Via **SQL Server Management Studio**: server name `(localdb)\MSSQLLocalDB`, Windows Authentication; the catalog only shows up once the app has run at least once (LocalDB auto-attaches on first access).

Via **`sqlcmd`**:
```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4"
```

## Seeding initial accounts

Sign-up is Customer-only, and Employee/Manager accounts have no self-service path — so a brand-new, empty database has no way to create its first account through the UI. `Scripts/Seed-InitialUsers.ps1` solves this by inserting a Manager and two Employees directly, hashing each password with the exact same PBKDF2 parameters as `PasswordHasher.cs` so they log in normally afterward.

```
.\Scripts\Seed-InitialUsers.ps1
```

Targets the live app database by default; pass `-Database "<CatalogName>"` (or `-Server`) for a different LocalDB catalog. Safe to re-run — an account already present (matched by name) is skipped, not duplicated. Change these passwords (or delete and re-seed) before using a database for anything beyond local development.

## Resetting the database

`Scripts/Reset-Database.ps1` wipes a database completely — detaches it from LocalDB and deletes its `.mdf`/`_log.ldf` files. No undo.

```
.\Scripts\Reset-Database.ps1
```

Targets the live app database by default; pass `-Database "<CatalogName>"` for a different catalog. Rebuild afterward with `.\Scripts\Initialize-Database.ps1` (or F5), then re-seed if wanted. The detach must happen before the files are deleted — deleting first leaves a stale LocalDB catalog entry, and the next attach fails with `Cannot attach the file ... as database ...`; the script handles the ordering.

## Security

This app went through an explicit security review pass during development. Full narrative detail (what was found, exactly how it was fixed, and why) lives in `claude.log` — this section is the summary of what's in place today.

- **Cross-site scripting (XSS)** — all user-supplied text (comment text, ticket descriptions, names) is HTML-encoded at output: `<%#: %>` binding expressions, `asp:Literal Mode="Encode"` for code-behind-set literals, `HttpUtility.JavaScriptStringEncode` for JS string contexts.
- **Oversized input** — `Comment.Text` and `Ticket.Description` are capped at 2000 characters and reject embedded control characters, enforced in the domain layer (`ValidateCommentText`, `Ticket.CreateSubmitted`) and backed by `[MaxLength(2000)]` on the database columns.
- **SQL injection** — not a live risk; every data access goes through EF6/LINQ-to-Entities, which parameterizes automatically. No raw SQL string-concatenation anywhere in the app.
- **Authorization / IDOR** — every mutating action re-validates ownership server-side (ticket/comment queries are always scoped to the acting user's id), and every page gates on role via `RepairAuthHelper.RequireRole` before any data access.
- **CSRF** — `Site.master.cs` carries anti-XSRF protection (a token tied to a cookie and username, re-validated every postback), which protects against a different-origin attack, not against a same-origin XSS payload — the actual defense against that is the output-encoding above plus the ownership checks.
- **Open redirect** — `IdentityHelper.RedirectToReturnUrl` only honors a `ReturnUrl` confirmed to be a same-site relative path.
- **Password hashing** — PBKDF2 (`Rfc2898DeriveBytes`), random 16-byte salt per password, 100,000 iterations (embedded in the stored hash so it can be raised again later without invalidating existing passwords), constant-time comparison.
- **Password policy** (`User.SetPassword`) — 8–128 characters, and must contain at least one letter, one digit, and one non-alphanumeric character. All of these checks report through a single combined message: "Passwords must have 8 to 128 characters and one or more letters, digits, and special characters."
- **Login lockout** — 5 consecutive failed attempts locks an account for 15 minutes (per-account); `IpThrottle` (in-memory, per-IP, 20 attempts/15 min per scope) covers the gap that leaves for an attacker spreading attempts across many usernames from one source. Applied to login, signup, and forgot-password.
- **Auth cookie** — `CookieHttpOnly = true`, `CookieSecure = CookieSecureOption.SameAsRequest` (upgrades to HTTPS-only automatically once actually served over HTTPS; `Always` would break the documented plain-HTTP local dev workflow).
- **Audit logging** — never stores comment text or ticket descriptions, only ids/types/timestamps/acting user.
- **Bad/unknown URLs** — 404s (both IIS-level and ASP.NET-level) redirect to Home via `Web.config`'s `<httpErrors>` and `Global.asax`'s `Application_Error`.
- **HTTPS enforcement** — opt-in via `Web.config`'s `RequireHttps` app setting (default `false`); when enabled, `Global.asax` redirects non-local, non-secure requests to HTTPS.

## Known limitations

- Password reset has no email/SMTP behind it — the reset link is shown directly on the `Forgot Password` page, which only works because the requester is also the viewer (fine for local/dev use; a real deployment needs to swap that one line for an actual email send).
- `IpThrottle`'s rate-limiting state is in-memory per process — it resets on an app restart and wouldn't be shared across server instances if this app were ever scaled out.
- No UI path exists to create a Manager account — must be seeded (`Scripts/Seed-InitialUsers.ps1`) or inserted directly.
