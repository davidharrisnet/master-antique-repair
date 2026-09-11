# MasterAntiqueRepair

A repair-shop tracking application: customers submit repair requests for antique items, employees pick up and complete them, and managers oversee the whole workload. Built as an ASP.NET Web Forms "legacy build" exercise (see [Project goals](#project-goals) below).

## Stack

- ASP.NET Web Forms, C#, .NET Framework 4.7.2
- Entity Framework 6 (Code First + Migrations)
- SQL Server LocalDB
- Bootstrap 3 / jQuery, via `System.Web.Optimization` bundling

## Solution structure

`MasterAntiqueRepair.sln` contains three projects:

| Project | Type | Purpose |
|---|---|---|
| `MasterAntiqueRepair` | Website Project (no `.csproj`) | The web app — pages, authentication, styling |
| `MasterAntiqueRepairData` | Class Library | Domain model and EF6 `DbContext`s |
| `MasterAntiqueRepairScratch` | Console App | Scratch pad for experimenting against the domain model directly, with no web exposure |

## Getting started (new developer setup)

This project is a legacy ASP.NET Web Forms app with several non-obvious setup gotchas (classic tooling, Windows-only, no `dotnet` CLI). The steps below take a fresh clone to a fully working local instance, in the order that actually works — each has been run and verified for real (see `claude.log` for the verification trail), not just written from memory.

**Quick start** — once the prerequisites in step 1 are installed, this is the whole database side of setup, no Visual Studio required:
```powershell
# One-time only per machine — allows these local scripts to run
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Build the schema (see step 6 for how this works without F5)
.\Scripts\Initialize-Database.ps1

# Seed the Manager + two Employee accounts (see "Seeding initial accounts")
.\Scripts\Seed-InitialUsers.ps1
```
Then log in at `/Account/Login` (F5 in Visual Studio to actually run the site) as `manager` / `ManagerPass123!`. The full walkthrough below covers the rest (build/restore, MSBuild on PATH, troubleshooting) — read it in full the first time; this block is just the part worth copy-pasting on every subsequent fresh checkout.

1. **Install prerequisites**:

   | Tool | Why | Notes |
   |---|---|---|
   | **Windows 10/11** | This is a Windows-only stack (IIS Express, LocalDB, Windows PowerShell) — there's no cross-platform path here. | |
   | **Git** | To clone the repo. | Any recent version. |
   | **Visual Studio 2017 or later**, "ASP.NET and web development" workload | Brings IIS Express, LocalDB, NuGet, Package Manager Console, and support for classic Website Projects — `MasterAntiqueRepair` (the web project) has no `.csproj`; it's the pre-SDK "Website" project type, which needs this workload specifically, not just any .NET workload. | Built and verified against VS2017 Community. On a newer VS version, if the `.sln` won't open or offers to convert the website project, add Website Project support under the Visual Studio Installer's Individual Components. |
   | **SQL Server LocalDB** | The actual database engine — a lightweight, per-user SQL Server instance. | Installed automatically by the workload above; you shouldn't need to install it separately. |
   | **SQL Server Management Studio (SSMS)** — optional | Inspecting the database directly, outside the app. | Not required to build or run the app. See [Accessing the database](#accessing-the-database). |

   Verify what you actually have installed:
   ```powershell
   # Visual Studio + the required workload — should print an edition/version, not nothing
   & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.VisualStudio.Workload.NetWeb -property displayName

   # LocalDB — should list an "MSSQLLocalDB" instance
   SqlLocalDB.exe info

   # Git
   git --version
   ```
   An empty result from the first command means the "ASP.NET and web development" workload isn't installed — add it via the Visual Studio Installer (not a reinstall of Visual Studio itself).

2. **Clone the repo and open** `MasterAntiqueRepair/MasterAntiqueRepair.sln` in Visual Studio.

3. **Restore NuGet packages**: `nuget restore MasterAntiqueRepair/MasterAntiqueRepair.sln`, or let Visual Studio do it on open.

   **If the build then fails** with `Could not find file '...\Bin\roslyn\csc.exe'` or an auto-refresh error for `microsoft.aspnet.web.optimization.webforms.dll`: plain NuGet restore doesn't run packages' `install.ps1` scripts, which is how these two packages deliver files into the website's `Bin/`. Fix in Package Manager Console:
   ```
   Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -reinstall
   Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3
   ```

4. **Put MSBuild on PATH.** EF6 Migrations tooling (Package Manager Console's `Add-Migration`/`Update-Database`) shells out to `msbuild.exe` by bare name internally. If it's missing from PATH, those commands fail with a `Process.Start`-related error from inside `EntityFramework.psm1`. You only hit this if you add or apply a migration yourself (day-to-day setup doesn't need it), but it's cheap to fix now. In an **elevated-not-required** PowerShell prompt:
   ```powershell
   $vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
   $msbuildDir = Split-Path (& $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe)
   $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
   if ($currentPath -notlike "*$msbuildDir*") {
       [Environment]::SetEnvironmentVariable("PATH", "$currentPath;$msbuildDir", "User")
   }
   ```
   This uses `vswhere.exe` (ships with every VS2017+ install) to find the right MSBuild regardless of VS edition/version, and only appends it if it isn't already there. **Restart Visual Studio afterward** — it reads PATH once at launch.

5. **Allow this project's PowerShell scripts to run.** `Scripts/Seed-InitialUsers.ps1` and `Scripts/Reset-Database.ps1` are unsigned local scripts, which Windows' default execution policy blocks:
   ```
   File ...\Scripts\Reset-Database.ps1 cannot be loaded. The file ... is not digitally signed.
   ```
   Fix (doesn't need admin, doesn't affect scripts downloaded from the internet — those still require a signature):
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

6. **Build the schema.** RepairShopContext's schema only gets created the first time something actually queries it (its EF6 Migrations-based initializer — see [MasterAntiqueRepairData](#masterantiquerepairdata)). Either:
   - Run the site once (F5 / Ctrl+F5 in Visual Studio) and submit the Login form once — this starts IIS Express on port `57962` and the login attempt is what triggers it; or
   - Run `.\Scripts\Initialize-Database.ps1` instead — builds and runs `MasterAntiqueRepairScratch` (a plain console app that just opens a `RepairShopContext`), which triggers the exact same thing with no web server or port involved at all. Useful for scripting/CI, or any time you want the schema built without opening Visual Studio.

   **If Visual Studio reports `Unable to launch... Port 57962 is in use`**: something else (often a previous debug session, or an IIS Express instance started outside Visual Studio, e.g. from a terminal) is still bound to that port. Find and stop it:
   ```powershell
   Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force
   ```
   If the port is still reported busy afterward, find out what's actually holding it:
   ```powershell
   netstat -ano | findstr :57962
   ```
   The last column is the owning process ID — `Get-Process -Id <pid>` identifies it, or `Stop-Process -Id <pid> -Force` to free the port directly.

7. **Seed initial accounts** — there's no UI path to create the very first account (Register is Manager-only):
   ```
   .\Scripts\Seed-InitialUsers.ps1
   ```
   See [Seeding initial accounts](#seeding-initial-accounts) for the credentials it creates.

8. **Verify**: log in at `/Account/Login` with the seeded Manager account. You should land on `/ManagerView`.

## Domain model

Entities live in `MasterAntiqueRepairData/App_Code/` (see [MasterAntiqueRepairData](#masterantiquerepairdata) below).

- **`User`** — base type: `Id`, `Name`, `CreatedAt`, `PasswordHash`, `Orders` (collection). Mapped via Table-Per-Hierarchy — one `Users` table, with a `Discriminator` column identifying the concrete type.
  - **`Customer : User`** — submits repair requests.
  - **`Employee : User`** — picks up and completes repair requests. `TakeOrder(Order)` assigns an order to itself; `CompleteOrder(Order, comment)` marks one done with a comment.
  - **`Manager : User`** — oversees employees and jobs. `GetEmployeesWithOrders(db)` returns all employees with their assigned orders.
- **`Order`** — a repair request: `Id`, `Description`, `Comment`, `State` (see below), `Customer` (who submitted it), `User` (the employee it's assigned to, nullable until picked up), `SubmittedDate`, `AssignedDate`, `CompletedDate`.
- **`State.RepairState`** (enum) — `SUBMITTED` → `INPROGRESS` → `COMPLETED`.

## Application flow

**Customers** sign up at `/Account/CustomerSignUp`, then log in at `/Account/Login`. Logging in lands on `/CustomerView` ("My Repairs") — a list of their own requests with status and submitted date, plus a link to `/SubmitRepair`, a simple description form. Submitting creates an `Order` in `SUBMITTED` state tied to that customer.

**Employees** can only be created by a Manager, via `/Account/Register`. Logging in lands on `/EmployeeView`, showing two lists: unassigned jobs (each with an "Assign to Me" button) and the employee's own jobs. Each of the employee's own jobs has a "Mark Complete" button that opens a modal for entering a completion comment.

**Managers** log in and land on `/ManagerView`, showing every employee with their assigned jobs, plus a separate list of unassigned jobs. Only a Manager sees the "Register" link in the nav (to create Employee accounts).

There is currently no UI path to create a Manager account — one must be inserted directly into the database (see [Known limitations](#known-limitations)).

## Authentication

Authentication is custom, built directly on the domain model above — not ASP.NET Identity.

- **`MasterAntiqueRepairData/App_Code/PasswordHasher.cs`** — PBKDF2 password hashing (`Rfc2898DeriveBytes`, random salt per password), used by `User.SetPassword`/`VerifyPassword`.
- **`MasterAntiqueRepair/App_Code/RepairAuthHelper.cs`** — builds a `ClaimsIdentity` from a domain `User` (name, ID, and role claims — the role claim is the concrete type name, e.g. `"Employee"`) and signs in via OWIN's cookie middleware. `RequireRole(Response, "RoleName")` guards protected pages, redirecting to Login if the visitor isn't authenticated in that role.
- **`MasterAntiqueRepair/App_Code/Startup.Auth.cs`** — configures the OWIN cookie middleware (`DefaultAuthenticationTypes.ApplicationCookie`, login path `/Account/Login`) that `RepairAuthHelper` relies on.
- After login, each role lands on its own page (`Employee` → `/EmployeeView`, `Manager` → `/ManagerView`, `Customer` → `/CustomerView`), unless a `ReturnUrl` was specified.

## MasterAntiqueRepairData

A classic (non-SDK) Class Library project, referenced by the website. It exists as a separate project specifically because EF6 Code First Migrations tooling (`Enable-Migrations`, `Add-Migration`, `Update-Database`) does not work against a classic Website Project — it requires a real `.csproj`.

Its classes are namespaced `MasterAntiqueRepair` (matching the website), not `MasterAntiqueRepairData`.

### `RepairShopContext`

The application's real `DbContext` — `DbSet<User> Users`, `DbSet<Order> Orders`. Uses the `"DefaultConnection"` connection string (the same LocalDB database the website's `App_Data` folder holds). Under EF6 Migrations, with its own configuration in `Migrations/RepairShop/`.

Because a Migrations configuration exists for it, EF6 automatically uses `MigrateDatabaseToLatestVersion` as its initializer — not the plain `CreateDatabaseIfNotExists` default. In practice this means the very first request that touches `RepairShopContext` against a database with no `__MigrationHistory` row for this context runs every migration under `Migrations/RepairShop/` in order, starting with `InitialCreate` — which is what actually builds `dbo.Users`/`dbo.Orders` from nothing. There's no separate "create schema" step; hitting the site (or running any of the `Scripts/*.ps1` seed/reset flows) is what triggers it.

### `TestDbContext`

A separate, scratch `DbContext` (`TestItem`, `User`, `Order` `DbSet`s) using its own `"TestConnection"` connection string and its own database. Exercised by the website's `TestEF.aspx` — a disconnected scratch page, not part of the main application flow. Under its own EF6 Migrations configuration in `Migrations/` (the default location).

### Running migrations

Both connection strings live only in the website's `Web.config` — the class library itself has no `Web.config`/`App.config` connection strings; at runtime, the ASP.NET host's config is what EF reads regardless of which assembly the `DbContext` class lives in.

Since two Migrations configurations coexist in this one project, PMC commands need an explicit `-ConfigurationTypeName`:

```
# For RepairShopContext:
Add-Migration <Name> -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration

# For TestDbContext:
Add-Migration <Name> -ConfigurationTypeName MasterAntiqueRepairData.Migrations.Configuration
Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.Configuration
```

Set **Default project** to `MasterAntiqueRepairData` in Package Manager Console first. Running migrations commands against the website project itself fails outright (`You cannot call a method on a null-valued expression`) — that's the reason this project exists.

## MasterAntiqueRepairScratch

A plain Console App referencing `MasterAntiqueRepairData` directly, for trying out EF6/domain-model code without any web or IIS exposure — nothing in it can be reached by a browser. Its `App.config` has its own copies of both connection strings; `Program.cs` points `|DataDirectory|` at the website's real `App_Data` folder at startup, so it reads/writes the same databases as the live site (or `TestConnection`, matching `TestDbContext`/`TestEF.aspx`). Rewrite `Program.cs` freely — it's meant to be overwritten for whatever you're currently testing.

## Accessing the database

Both databases are LocalDB `.mdf` files under `MasterAntiqueRepair/MasterAntiqueRepair/App_Data/`:

- `aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4.mdf` — the live app database (`DefaultConnection`, used by `RepairShopContext`).
- `MasterAntiqueRepairTest.mdf` — the scratch database (`TestConnection`, used by `TestDbContext`).

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
It targets the live app database by default; pass `-Database "MasterAntiqueRepairTest"` (or `-Server`) to target a different one. It creates:

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

It targets the live app database by default; pass `-Database "MasterAntiqueRepairTest"` to wipe the scratch database instead. After running it, the database no longer exists — rebuild it with `.\Scripts\Initialize-Database.ps1` (or F5), then run `Seed-InitialUsers.ps1` afterward if you want the usual accounts back. The full cycle — reset, rebuild, seed — is testable end to end with no web server involved at all.

The detach must happen before the files are deleted, not after — deleting the files first leaves a stale LocalDB catalog entry, and the next attach fails with `Cannot attach the file ... as database ...`. The script handles this ordering; if you're ever doing it by hand in SSMS, detach first.

## Known limitations

- **`TestDbContext`/`TestEF.aspx`** are a separate, disconnected scratch pad, not guaranteed to stay in sync with the rest of the app as the domain model evolves. Prefer `MasterAntiqueRepairScratch` for new experiments.

## Project goals

This is explicitly a "legacy build" learning exercise. The original scope:

- A core domain of 3–4 related entities with basic CRUD for each — see [Domain model](#domain-model).
- An approval/status-transition workflow with at least 3 states — see `State.RepairState`.
- A simple login/role check (hardcoded roles are acceptable) — see [Authentication](#authentication).
- One list/search view with filtering and pagination — the `EmployeeView`/`ManagerView`/`CustomerView` job lists.
- Layered architecture (UI / business logic / data access separated, no logic in code-behind) — business logic lives on the domain classes (`Employee.TakeOrder`, `Customer.submit`, etc.), not in `.aspx.cs` files.
- Server-side input validation.
- Logging of workflow state changes — `Order.SubmittedDate`/`AssignedDate`/`CompletedDate` and `Comment`.
