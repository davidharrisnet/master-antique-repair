# MasterAntiqueRepair

Master Antique Repair is a repair-shop tracking application.  Customers submit repair requests for antique items and mployees pick up the tickets and complete them.  Managers oversee work status and administer employee and cutomer accournts. They also view Metrics and Audit Logs and have a business model Search panel. 

## Overview

**PHASE 1 — Legacy Build**
Stack: ASP.NET Framework 4.7.2, C#, WebForms, SQL Server

Functional requirements:

*	A single core domain with 3–4 related entities (e.g., a simple case/request tracking app: Requests → Assignees → Status History)
*	Basic CRUD for each entity
*	One approval/status-transition workflow with at least 3 states (e.g., Submitted → In Review → Closed)
*	A simple login/role check (hardcoded roles are fine)
*	One list/search view with filtering and pagination

Non-functional requirements:

*	Layered architecture (UI / business logic / data access clearly separated — no logic in code-behind)*	Server-side input validation
*	Logging of workflow state changes (this becomes your audit trail in Phase 2)
*	A short README explaining the structure and how to run it

## Project 

### Stac

- ASP.NET Web Forms, C#, .NET Framework 4.7.2
- Entity Framework 6 (Code First + Migrations)
- SQL Server LocalDB
- Bootstrap 3 / jQuery, via `System.Web.Optimization` bundling

### Functional Features
- A core domain of 3–4 related entities with basic CRUD for each — see [Domain model](#domain-model). Create/Read/Update/Delete are all covered for comments (see [Comments](#comments)); tickets and users currently support Create/Read (no UI path to edit/delete a ticket or user yet).
- An approval/status-transition workflow with at least 3 states — see `State.RepairState`.
- A simple login/role check (hardcoded roles are acceptable) — see [Authentication](#authentication), now with login lockout after repeated failures (see [Security](#security)).
- One list/search view with filtering and pagination — the `EmployeeView`/`ManagerView`/`CustomerView` ticket lists, plus the Manager-only Audit Log's Entity Id search and adjustable page size (see [Manager tools](#manager-tools)).

### Non-functional Featueres
- Layered architecture (UI / business logic / data access separated, no logic in code-behind) — business logic lives on the domain classes (`Employee.TakeTicket`/`CompleteTicket`, `Customer.submit`, `User.AddComment`/`EditComment`/`DeleteComment`, etc.), not in `.aspx.cs` files.
- Server-side input validation — see [Security](#security) for the full rundown (length limits, control-character rejection, password policy, etc.), not just presence-checks.
- Logging of workflow state changes — the `AuditLog` entity and Manager-only Audit Log page (see [Domain model](#domain-model) and [Manager tools](#manager-tools)) now cover this as a real audit trail, not just the `Ticket.SubmittedDate`/`AssignedDate`/`CompletedDate` timestamps this started as.

## Getting the code

**Latest code** (`main`):
```bash
git clone https://github.com/davidharrisnet/master-antique-repair.git
```

**A specific released version** (e.g. `v2.0` — see all released versions at [github.com/davidharrisnet/master-antique-repair/releases](https://github.com/davidharrisnet/master-antique-repair/releases)):
```bash
git clone --branch v2.0 https://github.com/davidharrisnet/master-antique-repair.git
```


## Solution structure

`MasterAntiqueRepair.sln` contains three projects:

| Project | Type | Purpose |
|---|---|---|
| `MasterAntiqueRepair` | Website Project (no `.csproj`) | The web app — pages, authentication, styling |
| `MasterAntiqueRepairData` | Class Library | Domain model and EF6 `DbContext`s |

# Project 

## User roles and actions

There are three roles — Customer, Employee, Manager — each landing on its own page after login, and each restricted server-side (`RepairAuthHelper.RequireRole`) to only the pages/actions listed below, regardless of what the UI shows.

### Customer

- **Sign up** at `/Account/CustomerSignUp` (self-service — the only role that is). **Log in** at `/Account/Login`; lands on `/CustomerView` ("My Repairs").
- **Submit a repair request**: `/SubmitRepair` — a description form. Creates a `Ticket` in `SUBMITTED` state tied to that customer.
- **View their own tickets**: `/CustomerView` lists every ticket they've submitted, with status, submitted date, the employee's comment thread, and their own comment thread (see [Comments](#comments)).
- **Add / edit / delete their own comments** on their own tickets, once a ticket reaches `COMPLETED` — see [Comments](#comments) for the exact rules.
- Cannot: see other customers' tickets, act on any employee/manager page, or comment on a ticket that isn't theirs or isn't yet completed.

### Employee

- Cannot self-register — created only by a Manager, via the "Add New Employee" panel on `/ManagerView`. **Log in** at `/Account/Login`; lands on `/EmployeeView`.
- **View unassigned tickets**: a list of every `SUBMITTED` ticket with no employee yet, each with an "Assign to Me" button.
- **Assign to Me**: takes an unassigned ticket — moves it to `INPROGRESS`, records `AssignedDate`, and assigns it to that employee (`Employee.TakeTicket`).
- **View "My Tickets"**: every ticket currently or previously assigned to them, each with a "Mark Complete" button (visible while not yet `COMPLETED`).
- **Mark Complete**: moves a ticket they own to `COMPLETED`, records `CompletedDate`, and — if a comment was typed into the modal — posts it as the first entry in that ticket's Employee Comments thread (`Employee.CompleteTicket`).
- **Add / edit / delete their own comments** on tickets assigned to them, once `COMPLETED` — see [Comments](#comments).
- Cannot: take/complete a ticket assigned to a different employee, or act on tickets they were never assigned.

### Manager

- Has standing credentials seeded by `Scripts/Seed-InitialUsers.ps1` (see [Overview](#overview)) — there is currently no UI path to create a Manager account; one must be inserted directly into the database (see [Known limitations](#known-limitations)).
- **Log in** at `/Account/Login`; lands on `/ManagerView` ("Employees"), showing: an Employee Management panel (add/edit/soft-delete employees), a dropdown to view one employee's tickets at a time, and every unassigned ticket (with the submitting customer's name).
- **Manage employees**: the "Employee Management" panel on `/ManagerView` (collapsed by default) — add a new employee, or edit an existing one's name/password, or soft-delete them (they can no longer log in, but their tickets/comments/history are kept).
- **View the Audit Log**: `/AuditLogView` — see [Manager tools](#manager-tools).
- **Search for a ticket** by Id and see its full comment thread: `/TicketDetailView` — see [Manager tools](#manager-tools).
- Managers do not add, edit, or delete comments themselves — the comment feature is Customer/Employee only; a Manager's view of a ticket's comments (via Ticket Search) is read-only.


Login credentials for the pre-seeded accounts (see [Seeding initial accounts](#seeding-initial-accounts) for how these get created):

|Name|Password|Role|
|---|---|---|
|manager|ManagerPass123!|Manager|
|employee1|EmployeePass123!|Employee|
|employee2|EmployeePass456!|Employee|

There's no seeded customer account — sign up as one via `/Account/CustomerSignUp`.

See [User roles and actions](#user-roles-and-actions) below for what each role can actually do, and [Project goals](#project-goals) for how this maps back to the original Phase 1 spec.


## Manager tools

Two Manager-only pages exist purely for oversight — neither one is part of the customer/employee workflow, and neither lets a Manager add, edit, or delete anything on a ticket.

### Audit Log (`/AuditLogView`)

Lists every tracked `AuditLog` row — Timestamp, User (who did it), Action, Entity type, Entity Id — newest first. See [Domain model](#domain-model) for exactly which actions are tracked and why comment/ticket text is never one of the columns.

### Pagination

A "Rows per page" dropdown (10 or 20) sits above the grid; changing it re-binds the grid at the new page size and resets to page 1 (`PageSizeList_SelectedIndexChanged`). The grid itself uses standard `GridView` paging (`AllowPaging`, `OnPageIndexChanging`) with `PagerSettings Mode="NumericFirstLast"` — First / numbered pages / Last controls — so you can jump around a large log rather than only stepping one page at a time.

### Search function

An "Entity Id" box above the grid filters the log to rows matching that id (`AuditLogGrid`'s data source becomes `db.AuditLogs.Where(a => a.EntityId == id)`). Since a log row only stores an id and a type, each row also has a **View** link that resolves to the actual thing that was acted on:

- `EntityType == Ticket` → links straight to `/TicketDetailView.aspx?id={EntityId}`.
- `EntityType == Comment` → looks up that comment's `TicketId` and links to the same ticket-detail page (a bare comment id isn't meaningful without the ticket it belongs to).
- `EntityType == User` → no link — there's no separate "user detail" page.

`/TicketDetailView` is also reachable directly (it's in the nav as "Ticket Search," independent of the Audit Log) — enter a Ticket Id and see that ticket's fields (Description, Status, Customer, Assigned To, Submitted/Assigned/Completed dates) plus both of its comment threads, read-only, in the same bulleted format used on `EmployeeView`/`CustomerView`. This is what makes "Entity 7 is a `CreateTicket`" concretely actionable — click through and you're looking at ticket #7 itself, comments included, not just a log line referencing it.

## Authentication

Authentication is custom, built directly on the domain model above — not ASP.NET Identity.

- **`MasterAntiqueRepairData/App_Code/PasswordHasher.cs`** — PBKDF2 password hashing (`Rfc2898DeriveBytes`, random salt per password), used by `User.SetPassword`/`VerifyPassword`.
- **`MasterAntiqueRepair/App_Code/RepairAuthHelper.cs`** — builds a `ClaimsIdentity` from a domain `User` (name, ID, and role claims — the role claim is the concrete type name, e.g. `"Employee"`) and signs in via OWIN's cookie middleware. `RequireRole(Response, "RoleName")` guards protected pages, redirecting to Login if the visitor isn't authenticated in that role.
- **`MasterAntiqueRepair/App_Code/Startup.Auth.cs`** — configures the OWIN cookie middleware (`DefaultAuthenticationTypes.ApplicationCookie`, login path `/Account/Login`) that `RepairAuthHelper` relies on.
- After login, each role lands on its own page (`Employee` → `/EmployeeView`, `Manager` → `/ManagerView`, `Customer` → `/CustomerView`), unless a `ReturnUrl` was specified.
- `Account/ForgotPassword` / `Account/ResetPassword` provide self-service password recovery — see [IP-based rate limiting, password reset, and HTTPS enforcement](#ip-based-rate-limiting-password-reset-and-https-enforcement) under Security for the full design (there's no email in this app, so the reset link is shown on-screen rather than sent).


### Running migrations

The connection string lives only in the website's `Web.config` — the class library itself has no `Web.config`/`App.config` connection string of its own; at runtime, the ASP.NET host's config is what EF reads regardless of which assembly the `DbContext` class lives in.

```
Add-Migration <Name> -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
```

(There used to be a second, scratch `TestDbContext` with its own Migrations configuration at the default `Migrations/` location, which is why commands got an explicit `-ConfigurationTypeName` in the first place — that context, its migrations, and its `TestConnection` connection string have all been deleted; `RepairShopContext`'s configuration is the only one left, so the flag is no longer strictly required, but there's no harm in keeping it.)

Set **Default project** to `MasterAntiqueRepairData` in Package Manager Console first. Running migrations commands against the website project itself fails outright (`You cannot call a method on a null-valued expression`) — that's the reason this project exists.


## Accessing the database

The database is a LocalDB `.mdf` file under `MasterAntiqueRepair/MasterAntiqueRepair/App_Data/`:

- `aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4.mdf` — the live app database (`DefaultConnection`, used by `RepairShopContext`).

To connect with **SQL Server Management Studio**:

1. Server name: `(localdb)\MSSQLLocalDB`
2. Authentication: **Windows Authentication**
3. Connect, then under **Databases** find the catalog by name (matching the `Initial Catalog` values above — LocalDB auto-attaches the `.mdf` under that name once the app has run at least once).

If the database doesn't show up, it means the app hasn't created it yet — run the site once (F5), which triggers `RepairShopContext`'s Migrations-based initializer on first access (see [MasterAntiqueRepairData](#masterantiquerepairdata) below).

`sqlcmd` works the same way from a terminal, e.g.:
```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4"
```

## Seeding initial accounts

Register is Manager-only, and CustomerSignUp only creates Customers — so a brand-new, empty database has no way to create its first account through the UI. `Scripts/Seed-InitialUsers.ps1` solves this by inserting a Manager and two Employees directly.

Build the schema first — F5 in Visual Studio, or `.\Scripts\Initialize-Database.ps1` (see step 6 of [Getting started](#getting-started-new-developer-setup)) — then run this from a PowerShell prompt (if you hit a "not digitally signed" error, see step 5):
```
.\Scripts\Seed-InitialUsers.ps1
```
It targets the live app database by default; pass `-Database "<CatalogName>"` (or `-Server`) to target a different LocalDB catalog. It creates:

   | Role | Username | Password |
   |---|---|---|
   | Manager | `manager` | `ManagerPass123!` |
   | Employee | `employee1` | `EmployeePass123!` |
   | Employee | `employee2` | `EmployeePass456!` |

The script hashes each password with the exact same PBKDF2 parameters as `PasswordHasher.cs`, so the accounts it creates log in normally through `/Account/Login`. It's safe to re-run — an account already present (matched by name) is skipped, not duplicated. Change these passwords (or delete and re-seed) before using a database for anything beyond local development.

## Resetting the database

`Scripts/Reset-Database.ps1` wipes a database completely — schema and data both — by detaching it from LocalDB and deleting its `.mdf`/`_log.ldf` files. There's no undo.

```
.\Scripts\Reset-Database.ps1
```

It targets the live app database by default; pass `-Database "<CatalogName>"` to wipe a different LocalDB catalog instead. After running it, the database no longer exists — rebuild it with `.\Scripts\Initialize-Database.ps1` (or F5), then run `Seed-InitialUsers.ps1` afterward if you want the usual accounts back. The full cycle — reset, rebuild, seed — is testable end to end with no web server involved at all.

The detach must happen before the files are deleted, not after — deleting the files first leaves a stale LocalDB catalog entry, and the next attach fails with `Cannot attach the file ... as database ...`. The script handles this ordering; if you're ever doing it by hand in SSMS, detach first.

## Security

This app went through an explicit security review pass during development. This section documents what was found, what was fixed, how, and what was deliberately left as an accepted risk (with the reasoning), so the "why" isn't lost.

### Cross-site scripting (XSS)

**The problem**: several pages rendered user-supplied text (comment text, ticket descriptions, account names) via WebForms data-binding expressions like `<%# Eval("Text") %>` or via `asp:Literal`/`asp:Label` controls set from code-behind — neither of which HTML-encodes by default. A ticket description or comment containing `<script>...</script>` would be written into the page's HTML verbatim and executed by the browser of *anyone* who viewed it (an employee, a manager, another customer) — a classic **stored XSS** vulnerability, not limited to whoever submitted the payload.

**The fix — encode on output, not input**: the correct defense is encoding untrusted data at the point it's written into HTML, not trying to blacklist "dangerous" input (which is trivially bypassed — case variants, encoded payloads, non-`<script>` vectors like `<img onerror=...>`, etc.). Concretely:
- Data-binding expressions changed from `<%# %>` to the encoding form `<%#: %>` (e.g. `<%#: Eval("Text") %>`) everywhere a bound value could contain attacker-controlled text — comment text and author names across `CustomerView.aspx`, `EmployeeView.aspx`, `ManagerView.aspx`, and `TicketDetailView.aspx`.
- `asp:Literal` controls set from code-behind got `Mode="Encode"` (`TicketDetailView.aspx`'s ticket-field literals, `SubmitRepair.aspx`'s `ErrorMessage`).
- Fields that are never rendered as HTML at all (JS string arguments like the "Mark Complete" modal's description) already went through `HttpUtility.JavaScriptStringEncode` for that context, and setting them via `.innerText` (not `.innerHTML`) client-side avoids re-introducing the same class of bug there.

**What this doesn't protect against**: nothing here — output encoding is a complete fix for this specific bug once applied everywhere untrusted data is written into HTML. The residual risk is only "did we miss a spot," which is why this was swept across every view, not just the one first reported.

### Oversized input / resource exhaustion

**The problem**: `Comment.Text` and `Ticket.Description` were unbounded `string` properties with no length limit anywhere — client, server, or database (`nvarchar(max)`). Nothing stopped a customer or employee from submitting megabytes of text per comment or ticket, bloating storage and slowing down every page that lists them.

**The fix**: both fields now enforce a 2000-character cap, and reject embedded control characters (other than `\n`/`\r`/`\t`, which are legitimate in free text) that can behave oddly in storage/rendering:
- `User.ValidateCommentText` (shared by `AddComment`/`EditComment`) and `Ticket.CreateSubmitted` both throw a clear `ArgumentException` before anything touches the database.
- `[MaxLength(2000)]` on `Comment.Text` and `Ticket.Description` bounds the actual database column, so the limit holds even if some future code path bypasses the domain method.
- Client-side `TextBox.MaxLength` was **not** used for the comment/description boxes — they're `TextMode="MultiLine"` (`<textarea>`), and WebForms' `TextBox.MaxLength` is silently ignored for multi-line boxes (it only renders the HTML `maxlength` attribute for single-line/password boxes). The server-side check is what actually matters; a JS-only client cap would need custom script and wasn't worth the complexity for a soft UX nicety.

### SQL injection

Not a live risk in this codebase: every data access goes through EF6/LINQ-to-Entities (`db.Tickets.Where(...)`, `db.Comments.FirstOrDefault(...)`, etc.), which parameterizes values automatically. There is no raw `SqlCommand`/`ExecuteSqlCommand` string-concatenation anywhere in the app. The only thing that would introduce this risk is adding raw SQL built from string concatenation in the future — there's nothing to "harden" today, only a discipline to keep (always use LINQ/parameters, never concatenate user input into a SQL string).

### Authorization / IDOR (Insecure Direct Object Reference)

Every mutating action re-validates ownership **server-side**, independent of what the UI shows or what the client sends:
- `User.EditComment`/`DeleteComment` throw `InvalidOperationException` unless `comment.UserId == Id` — a user can only ever edit or delete their own comments, even if they somehow submit another comment's id.
- Ticket queries in code-behind are always scoped to the acting user: `EmployeeView`'s complete/comment actions filter by `t.User.Id == employeeId`; `CustomerView`'s filter by `t.Customer.Id == customerId`. An employee can't complete a ticket assigned to someone else by guessing its id, and a customer can't comment on someone else's ticket the same way.
- Every page gates on role via `RepairAuthHelper.RequireRole` as the first statement in `Page_Load`, before any data access — `Manager`-only pages (`ManagerView`, `AuditLogView`, `TicketDetailView`, `Metrics`) reject anyone else.

### Cross-Site Request Forgery (CSRF) vs. XSS — and why `confirm()` isn't a security control

`Site.master.cs` carries the VS template's anti-XSRF protection: a random token tied to a cookie and the current username, stored in `ViewStateUserKey`, and re-validated on every postback. This protects against a **different-origin** attack — some other website tricking a logged-in user's browser into submitting a form to this app.

It does **not** protect against a script already running on this app's own pages (XSS). If a stored-XSS payload existed (see above), a script running in that context could trigger the same postback a real click would — either by calling the rendered link's `.click()`, or by calling `__doPostBack(...)` directly — and it could defeat the "Are you sure?" `confirm()` dialog on the Delete buttons just by overriding `window.confirm` first, since that's an ordinary same-origin JS global, not a browser-enforced boundary. The `confirm()` prompt is a UX safety net against an accidental real click; it is not, and was never intended to be, a defense against a script that's already executing on the page. The actual defenses against that scenario are (1) not letting attacker-controlled text become executable markup in the first place (the XSS fixes above) and (2) the server-side ownership checks in the previous section, which limit the blast radius to the acting user's own data even in a worst-case compromised-session scenario.

### Open redirect

`IdentityHelper.RedirectToReturnUrl` (`App_Code/IdentityModels.cs`) only honors a `ReturnUrl` query-string value if `IsLocalUrl` confirms it's a same-site relative path (starts with `/` but not `//` or `/\`, or `~/`); anything else — `https://evil.example/phish`, `//evil.example`, etc. — falls back to `~/`. This prevents the login flow from being used to redirect users to an attacker-controlled site after a legitimate-looking login on this domain.

### Authentication

- **Password hashing** (`PasswordHasher.cs`): PBKDF2 (`Rfc2898DeriveBytes`) with a random 16-byte salt per password. The iteration count is now embedded in every newly-created hash (`"{iterations}.{salt}.{hash}"`), raised from 10,000 to 100,000. Embedding it means it can be raised again later without invalidating existing passwords — the previous design used a single hardcoded constant for both hashing and verifying, so bumping it would have silently locked every existing account (including the seeded demo accounts) out. Hashes created before this change (a bare base64 blob with no `.` separators) still verify correctly via a legacy fallback path at the old fixed 10,000 iterations — nothing already stored breaks. `Scripts/Seed-InitialUsers.ps1` still produces the old-format hash (it has its own PowerShell reimplementation of the same algorithm at 10,000 iterations) — this still logs in fine via the legacy path; it just won't benefit from the higher iteration count unless that script is updated too, or the seeded account's password is changed through the app.
- **Constant-time comparison**: the byte-by-byte hash comparison now always inspects every byte rather than returning on the first mismatch, closing a (minor, hard-to-exploit-remotely, but free-to-fix) timing side channel.
- **Password policy** (`User.SetPassword`): minimum length raised from 6 to 8 characters; a 128-character maximum was added (defense against feeding pathologically long input into the hashing step). Composition rules were initially left out on purpose — NIST SP 800-63B favors length over composition, since forced complexity tends to produce predictable patterns rather than real entropy — but basic composition rules were added afterward at the user's request anyway: a password must contain at least one letter, one digit, and one character that's neither (a "special" character, defined simply as `!char.IsLetterOrDigit`). This is a deliberate step back from the NIST-only stance above, not a correction of it — both are defensible; this app just ended up wanting the latter. All of these checks (length range and composition) report through a single combined message rather than pinpointing which rule failed: "Passwords must have 8 to 128 characters and one or more letters, digits, and special characters."
- **Login lockout** (`User.RecordFailedLogin`/`RecordSuccessfulLogin`/`IsLockedOut`, wired into `Account/Login.aspx.cs`): 5 consecutive failed attempts against an account locks it for 15 minutes; a successful login resets the counter. This is **per-account**, not per-IP — see [IP-based rate limiting](#ip-based-rate-limiting-password-reset-and-https-enforcement) below for what covers the gap that leaves.
- **Auth cookie** (`App_Code/Startup.Auth.cs`): explicit `CookieHttpOnly = true` and `CookieSecure = CookieSecureOption.SameAsRequest`. `SameAsRequest` (rather than `Always`) is deliberate — this project's documented dev workflow is plain HTTP via IIS Express, and `Always` would silently stop the auth cookie from ever being sent back to the browser under HTTP, breaking login for every contributor following the README as written. It upgrades to HTTPS-only automatically the moment this is actually served over HTTPS.

### Audit logging and comment content

`AuditLog` (see the main architecture section) deliberately never stores comment text or ticket descriptions — only `EntityId`/`EntityType`/`Action`/`Timestamp`/the acting `User`. This was a design requirement from the start, not an afterthought: an audit trail that itself stores freeform user content becomes another place that content has to be protected (encoding, length limits, access control) all over again, and it's not needed for the trail's actual purpose (who did what, to which record, when).

### Bad/unknown URLs

Extensionless requests IIS resolves natively before ASP.NET routing ever sees them (e.g. a path with no matching route or file) are redirected to Home via `Web.config`'s `<httpErrors>` (`404` → `responseMode="Redirect"` to `/`). Requests that *do* reach the ASP.NET pipeline and throw a 404 (`HttpException`) are caught the same way via `Global.asax`'s `Application_Error`. Together these cover both paths a "page not found" can take through this stack.

### IP-based rate limiting, password reset, and HTTPS enforcement

These four were originally logged as accepted risks (see `claude.log` for that write-up); all four now have real mitigations.

- **`App_Code/IpThrottle.cs`** (website `App_Code/`) — an in-memory, per-IP rate limiter keyed by `Request.UserHostAddress` plus a `scope` string, so login/signup/forgot-password attempts are tracked independently. It caps any single IP at 20 attempts per 15-minute sliding window per scope, then blocks further attempts in that scope until the window rolls over. This is deliberately separate from `User`'s per-account lockout above: the account lockout stops someone hammering *one* username; the IP throttle stops someone spreading attempts across *many* usernames from one source, which the account-level check alone can't see. State is process-local (an `ConcurrentDictionary`, not backed by the database) — it resets on app restart and isn't shared across server instances if this app were ever scaled out, which is an accepted limitation given this app has no reverse proxy/WAF layer to do this instead.
  - Wired into `Account/Login.aspx.cs` (scope `"login"`, recorded on any failed or locked-out attempt), `Account/CustomerSignUp.aspx.cs` (scope `"signup"`, recorded whenever a chosen username is already taken), and `Account/ForgotPassword.aspx.cs` (scope `"forgotpassword"`, recorded on every request regardless of outcome).
- **Username enumeration**: `Account/CustomerSignUp.aspx` still says "That username is already taken" rather than a generic message — that part of the original reasoning still holds (usernames in this app aren't secrets; they're already visible in Manager views and the Audit Log's User column, and a real fix would mean redesigning signup around email verification, out of scope here). What changed is the `IpThrottle` "signup" scope above now caps how fast one IP can sweep through candidate usernames looking for hits, which is the part of this risk that's actually exploitable at scale.
- **Password reset**: `PasswordResetToken` (`MasterAntiqueRepairData/App_Code/`) — `Id`, `UserId`, `Token` (a random 32-byte value, URL-safe base64-encoded), `CreatedAt`, `ExpiresAt` (1 hour after creation), `UsedAt` (null until consumed). `Account/ForgotPassword.aspx` takes a username, throttles by IP (see above), and — if the account exists — creates a token and displays the reset link directly on the page. **This app has no SMTP configured anywhere**, so rather than fake an email that never sends, the link is shown on-screen, clearly labeled as a local/dev stand-in; a real deployment would email `resetUrl` instead of rendering it (that's the one line in `ForgotPassword.aspx.cs` to change). `Account/ResetPassword.aspx?token=...` validates the token (exists, unused, unexpired) before accepting a new password, marks the token used on success, and — since successfully using a valid token proves account ownership — also clears any existing account lockout on that user. Both requesting and completing a reset are logged (`AuditLog.ActionType.RequestPasswordReset`/`ResetPassword`) without storing the token itself in the log.
- **HTTPS enforcement**: opt-in via `Web.config`'s new `<appSettings>` key `RequireHttps` (default `false`). `Global.asax`'s `Application_BeginRequest` redirects to HTTPS only when that flag is `true` **and** the request isn't already secure **and** `Request.IsLocal` is false — so the default local dev workflow (plain HTTP via IIS Express, no HTTPS binding configured) is completely unaffected unless someone deliberately flips the flag in an environment that actually has an HTTPS binding.

### Migrations introduced by this section

The login-lockout fields were a schema change:
```
Add-Migration AddLoginLockout -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
```
(`Comment.Text`'s and `Ticket.Description`'s `[MaxLength(2000)]` changes were covered by earlier migrations already described elsewhere in this file's history — see `claude.log` for the full trail.)

The new `PasswordResetToken` entity/table is also a schema change, not yet applied to any database as of this writing:
```
Add-Migration AddPasswordResetTokens -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
```
Until this migration is applied, EF6 throws `The model backing the 'RepairShopContext' context has changed since the database was created` on the **first** request that touches `RepairShopContext` — which in practice is almost every page, not just the new reset pages, since the model check runs on first use per process. Run the migration before testing any of this.

## Known limitations

- The password reset flow (see [Security](#security)) has no email/SMTP behind it — the reset link is shown directly on the `Forgot Password` page instead of being sent anywhere, which only works because the person requesting the reset is also the one viewing that page (fine for local/dev use; a real deployment needs to swap that one line for an actual email send).
- `IpThrottle`'s rate-limiting state is in-memory per process — it resets on an app restart and wouldn't be shared across server instances if this app were ever scaled out to more than one.

