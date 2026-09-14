# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project type

This is an **ASP.NET Web Forms "Website" project** (the pre-2010, non-SDK-style project type), not a Web Application project and not a modern SDK-style .NET project. It's a repair-shop tracking app: customers submit repair requests for antique items, employees pick up and complete them, and managers oversee the workload — built as a "legacy build" learning exercise (see `README.md`).

The solution (`MasterAntiqueRepair/MasterAntiqueRepair.sln`) has **three projects**, all targeting .NET Framework 4.7.2 and pinned to the same EF6 version (EntityFramework 6.5.1 in every `packages.config`) — a mismatch between them breaks migrations tooling:

- **`MasterAntiqueRepair`** — the Website project. No `.csproj`; defined by the `.sln`'s `WebsiteProperties` section (VirtualPath `/localhost_57962`, port `57962`). Code-behind (`*.aspx.cs`, `*.master.cs`, `*.ascx.cs`) compiles per-page; there's no `Program.cs`/`Startup` entry point in the SDK sense. Non-page C# helpers live in `App_Code/` and compile automatically from there. Dependencies via `packages.config` (classic NuGet); restored packages land in `MasterAntiqueRepair/packages/`.
- **`MasterAntiqueRepairData`** (`MasterAntiqueRepairData/MasterAntiqueRepairData.csproj`) — a classic Class Library, referenced by the website via a `ProjectReferences` entry in the `.sln`'s `WebsiteProperties` (not a normal MSBuild `ProjectReference`). Holds the real domain model and both EF6 `DbContext`s, because EF6 Code First Migrations tooling (`Add-Migration`/`Update-Database`) doesn't work against a classic Website Project — it needs a real `.csproj`. Its classes are namespaced `MasterAntiqueRepair` (matching the website), **not** `MasterAntiqueRepairData`.
- **`MasterAntiqueRepairScratch`** — a plain Console App referencing `MasterAntiqueRepairData` directly, for trying out EF6/domain-model code with no web/IIS exposure. Its `App.config` has its own copies of both connection strings; `Program.cs` points `|DataDirectory|` at the website's real `App_Data` folder, so it reads/writes the same LocalDB databases as the live site. **`Program.cs` is meant to be rewritten freely** for whatever's currently being tested — don't treat its contents as meaningful state to preserve.

## Build / run / restore

There is no `dotnet build`/`dotnet test` workflow here. See `README.md`'s "Getting started" section for the full first-time setup walkthrough (prerequisites, verified gotchas, troubleshooting) — the summary:

- **Open and run**: Open `MasterAntiqueRepair/MasterAntiqueRepair.sln` in Visual Studio and press F5/Ctrl+F5. Starts IIS Express on port `57962`.
- **Restore packages**: `nuget restore MasterAntiqueRepair/MasterAntiqueRepair.sln`.
- **Fresh-restore gotcha**: plain NuGet restore doesn't run packages' `install.ps1` (how some packages copy files into the website's `Bin/`). If build fails with `Could not find file '...\Bin\roslyn\csc.exe'` or an auto-refresh error for `microsoft.aspnet.web.optimization.webforms.dll`, in Package Manager Console: `Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -reinstall` then `Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3`.
- **MSBuild must be on PATH** — both EF6 migrations tooling in Package Manager Console and `Scripts/Initialize-Database.ps1` (which builds `MasterAntiqueRepairScratch`) shell out to it by bare name. See README step 4 for the `vswhere.exe`-based PATH setup.
- **Build the schema without Visual Studio**: `.\Scripts\Initialize-Database.ps1` builds and runs `MasterAntiqueRepairScratch`, which opens `RepairShopContext` and triggers its Migrations-based initializer — same effect as F5 + one login attempt, no web server involved.
- **Seed initial accounts**: `.\Scripts\Seed-InitialUsers.ps1` — there's no UI path to create the first account (Register is Manager-only; CustomerSignUp only makes Customers). Creates `manager`/`ManagerPass123!`, `employee1`/`EmployeePass123!`, `employee2`/`EmployeePass456!`. Safe to re-run (skips existing accounts by name).
- **Reset a database**: `.\Scripts\Reset-Database.ps1` (detaches from LocalDB and deletes the `.mdf`/`_log.ldf`; no undo). Targets the live app DB by default; pass `-Database "MasterAntiqueRepairTest"` for the scratch DB.
- **EF6 migrations**: from Package Manager Console, set **Default project** to `MasterAntiqueRepairData`. Because two Migrations configurations coexist in that project (`RepairShopContext` and the scratch `TestDbContext`), commands need an explicit `-ConfigurationTypeName`:
  ```
  Add-Migration <Name> -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
  Update-Database -ConfigurationTypeName MasterAntiqueRepairData.Migrations.RepairShop.Configuration
  ```
  Running these against the website project itself fails (`You cannot call a method on a null-valued expression`) — that's exactly why the data project exists. `AutomaticMigrationsEnabled = false` in both `Migrations/**/Configuration.cs` — schema changes must go through explicit `Add-Migration`/`Update-Database`.
- **No test project exists** in this repo currently.

## Architecture

### Domain model (`MasterAntiqueRepairData/App_Code/`)

- **`User`** — base type: `Id`, `Name`, `CreatedAt`, `PasswordHash`, `Orders` (collection). Mapped Table-Per-Hierarchy — one `Users` table with a `Discriminator` column for the concrete type.
  - **`Customer : User`** — submits repair requests.
  - **`Employee : User`** — `TakeOrder(Order)` assigns an order to itself; `CompleteOrder(Order, comment)` marks one done with a comment.
  - **`Manager : User`** — `GetEmployeesWithOrders(db)` / `GetCustomersWithOrders(db)`.
- **`Order`** — `Id`, `Description`, `Comment`, `State`, `Customer`, `User` (assigned employee, nullable until picked up), `SubmittedDate`, `AssignedDate`, `CompletedDate`.
- **`State.RepairState`** (enum) — `SUBMITTED` → `INPROGRESS` → `COMPLETED`.
- **`PasswordHasher`** — PBKDF2 (`Rfc2898DeriveBytes`, random salt per password), backs `User.SetPassword`/`VerifyPassword`. `Scripts/Seed-InitialUsers.ps1` hashes with the exact same parameters so its seeded accounts log in normally.

Business logic (state transitions, employee/manager queries) lives on these domain classes, not in code-behind — that's the layered-architecture requirement from `README.md`'s project goals.

### Two `DbContext`s in `MasterAntiqueRepairData` — don't confuse them

- **`RepairShopContext`** (`"DefaultConnection"`) — the real app context: `DbSet<User> Users`, `DbSet<Order> Orders`. Has its own Migrations configuration under `Migrations/RepairShop/`, so EF6 uses `MigrateDatabaseToLatestVersion` as its initializer (not the plain `CreateDatabaseIfNotExists` default) — the first request against a DB with no matching `__MigrationHistory` row runs every migration starting with `InitialCreate`, building `dbo.Users`/`dbo.Orders` from nothing.
- **`TestDbContext`** (`"TestConnection"`) — a separate scratch context (`TestItem`, `User`, `Order` `DbSet`s), its own database, its own Migrations configuration in `Migrations/` (the default location). Exercised only by the website's disconnected `TestEF.aspx` scratch page. **Not guaranteed to stay in sync** with the real domain model — prefer `MasterAntiqueRepairScratch` for new experiments, not this.

Both connection strings live only in the website's `Web.config` — the class library has no config of its own; at runtime the ASP.NET host's config is what EF reads regardless of which assembly the `DbContext` lives in. Both LocalDB `.mdf`s live under `MasterAntiqueRepair/MasterAntiqueRepair/App_Data/` (gitignored; `.gitkeep` tracks the folder).

### Authentication — custom, not ASP.NET Identity

Despite `App_Code/IdentityModels.cs` (`ApplicationUser`/`ApplicationDbContext`/`UserManager`) still being present from the original VS template, **it is not the live auth path** — `Web.config`'s `<authentication mode="None" />` and the real login (`Account/Login.aspx.cs`) query `RepairShopContext.Users` directly and call `user.VerifyPassword(...)`. The only piece of that file still in use is `IdentityHelper.RedirectToReturnUrl` (open-redirect-safe `ReturnUrl` handling). Don't extend `ApplicationDbContext`/`UserManager` for new work — they're vestigial scaffold, not a second auth system in play.

The real mechanism:
- **`MasterAntiqueRepair/App_Code/RepairAuthHelper.cs`** — `SignIn(User, isPersistent)` builds a `ClaimsIdentity` (name, ID, and a role claim = the concrete domain type name, e.g. `"Employee"` — note `ObjectContext.GetObjectType` is used to unwrap EF6's lazy-loading proxy type first) and signs in via OWIN's cookie middleware. `RequireRole(Response, "RoleName")` guards protected pages. `GetCurrentUserId()` reads the ID claim back.
- **`MasterAntiqueRepair/App_Code/Startup.Auth.cs`** — configures the OWIN cookie middleware (`DefaultAuthenticationTypes.ApplicationCookie`, login path `/Account/Login`) that `RepairAuthHelper` relies on. Wired up via `App_Code/Startup.cs`'s `[assembly: OwinStartupAttribute]`.
- Third-party OAuth providers (Microsoft, Twitter, Facebook, Google) are referenced in packages but commented out/unconfigured.
- After login, role dictates landing page unless `ReturnUrl` is set: `Employee` → `/EmployeeView`, `Manager` → `/ManagerView`, `Customer` → `/CustomerView`.

### Application flow / pages

- **Customers**: sign up at `/Account/CustomerSignUp`, log in, land on `/CustomerView` (their own requests + link to `/SubmitRepair`). Submitting creates an `Order` in `SUBMITTED` state.
- **Employees**: created only by a Manager via `/Account/Register` (no self-signup). Land on `/EmployeeView` — unassigned jobs (Assign to Me) and their own jobs (Mark Complete, with a comment modal).
- **Managers**: land on `/ManagerView` — every employee with assigned jobs, unassigned jobs with submitting customer, every customer with their orders. Only Managers see the "Register" nav link. **No UI path exists to create a Manager account** — must be inserted directly (or via `Seed-InitialUsers.ps1`).
- `Default.aspx`/`About.aspx`/`Contact.aspx` are still the unmodified VS template scaffold (not part of the real app flow).
- `Site.master`/`Site.master.cs` — desktop master page; anti-XSRF handling in `Page_Init`/`master_Page_PreLoad` (cookie-backed `ViewStateUserKey`) — don't remove without understanding the protection it provides.
- `Site.Mobile.master` + `ViewSwitcher.ascx` — separate mobile master page toggle, via FriendlyUrls' mobile display mode support.
- `App_Code/RouteConfig.cs` enables FriendlyUrls (extensionless URLs); `App_Code/BundleConfig.cs` handles CSS/JS bundling via `System.Web.Optimization` + WebGrease, both wired from `Global.asax`'s `Application_Start`.

### Front-end

Bootstrap 3.3.7 + jQuery 3.3.1 + Modernizr via NuGet, referenced/bundled through `BundleConfig.cs`. `Microsoft.AspNet.ScriptManager.WebForms`/`MSAjax` provide WebForms `ScriptManager`/`UpdatePanel` infrastructure.

## Accessing the database directly

`(localdb)\MSSQLLocalDB`, Windows Authentication, via SSMS or `sqlcmd -S "(localdb)\MSSQLLocalDB" -d "<catalog>"`. Catalog names are the `Initial Catalog` values in `Web.config`'s connection strings (`aspnet-MasterAntiqueRepair-...` for the live app, `MasterAntiqueRepairTest` for scratch) — LocalDB only auto-attaches once the app has run at least once.

## Working in this codebase

- New domain entities/logic that need EF6 migrations go in `MasterAntiqueRepairData/App_Code/` on `RepairShopContext`, following the existing `User`/`Order` pattern — not in the website's `App_Code/`, and not on `TestDbContext` (that's the disconnected scratch pad).
- Keep business logic and data access out of code-behind for new work (the layered-architecture requirement) — mirror `Employee.TakeOrder`/`Manager.GetEmployeesWithOrders` style methods on domain classes, not the direct-DbContext-in-code-behind pattern the pages currently use for simple queries.
- When adding a page, follow the existing `.aspx` + `.aspx.cs` + `MasterPageFile="~/Site.Master"` pattern, and gate it with `RepairAuthHelper.RequireRole` if it's role-restricted.
- For quick experiments against the domain model, rewrite `MasterAntiqueRepairScratch/Program.cs` rather than adding new scratch pages to the website.
- `MasterAntiqueRepair/MasterAntiqueRepair/Bin/`, `*/bin|obj/`, and `MasterAntiqueRepair/packages/` are generated output — don't edit.
- `claude.log` at the repo root is a verbose, ongoing diary of work/verification in this repo — check it for the trail behind non-obvious setup steps, and keep it updated (append, don't replace) rather than treating it as a one-time snapshot.
- `README.md` is the source of truth for setup/troubleshooting detail and is kept in sync with real, verified runs — prefer it over this file for step-by-step instructions, and update it (not just this file) when setup steps change.
