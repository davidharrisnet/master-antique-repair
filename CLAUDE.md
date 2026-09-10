# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project type

This is an **ASP.NET Web Forms "Website" project** (the pre-2010, non-SDK-style project type), not a Web Application project and not a modern SDK-style .NET project. Both the repo-root wrapper folder and the website project/namespace inside it are named `MasterAntiqueRepair` (originally scaffolded as "PizzaGo" — fully renamed, including the C# namespace, `.sln`/folder names, and the LocalDB catalog name). The solution now has **two projects**:

- **`MasterAntiqueRepair`** — the Website project described above. Key implications:
  - There is **no `.csproj`** for it — it's defined entirely by `MasterAntiqueRepair/MasterAntiqueRepair.sln`, which references the `MasterAntiqueRepair/MasterAntiqueRepair/` folder directly via a `WebsiteProperties` section (VirtualPath `/localhost_57962`, port `57962`).
  - Code-behind files (`*.aspx.cs`, `*.master.cs`, `*.ascx.cs`) are compiled dynamically per-page; there is no central `Program.cs`/`Startup` entry point in the SDK sense.
  - Shared, non-page C# classes live in `MasterAntiqueRepair/MasterAntiqueRepair/App_Code/` and are compiled automatically by ASP.NET from that folder — this is where new helpers/services/models should generally go unless they belong to a specific page.
  - Dependencies are managed via `packages.config` (classic NuGet, not `PackageReference`) — see `MasterAntiqueRepair/MasterAntiqueRepair/packages.config`. Restored packages land in `MasterAntiqueRepair/packages/`.
- **`MasterAntiqueRepairData`** (`MasterAntiqueRepair/MasterAntiqueRepairData/`) — a real classic (non-SDK) **Class Library** project with an actual `.csproj`, referenced by the website via a `ProjectReferences` entry in the `.sln`'s `WebsiteProperties` section (not a normal MSBuild `ProjectReference`). This is where EF6 `DbContext`/entity classes that need **Code First Migrations** must live — see "Data project / EF6 Migrations" below for why. Its classes are namespaced `MasterAntiqueRepair` (matching the website), **not** `MasterAntiqueRepairData` — don't assume the namespace matches the folder/assembly name.

Both projects target .NET Framework 4.7.2 (`Web.config` `targetFramework`, both `packages.config` files `net472`) and must use the **exact same EF version** (currently EntityFramework 6.5.1) — a mismatch between the two `packages.config` files breaks migrations tooling.

## Build / run / restore

There is no `dotnet build`/`dotnet test` workflow here (this predates the SDK-style tooling). Typical workflow:

- **Open and run**: Open `MasterAntiqueRepair/MasterAntiqueRepair.sln` in Visual Studio and press F5/Ctrl+F5. This starts IIS Express against the `MasterAntiqueRepair/MasterAntiqueRepair/` folder on port `57962` and compiles pages on request.
- **Restore packages**: `nuget restore MasterAntiqueRepair/MasterAntiqueRepair.sln` (classic `packages.config` restore, covers both projects) if `MasterAntiqueRepair/packages/` is missing or out of sync with either `packages.config`.
- **Fresh-restore gotcha**: NuGet restore alone doesn't run packages' `install.ps1` (how some packages copy files into the website's `Bin/`). If build fails with `Could not find file '...\Bin\roslyn\csc.exe'` or an auto-refresh error for `microsoft.aspnet.web.optimization.webforms.dll`, run in Package Manager Console: `Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -reinstall` then `Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3`.
- **EF6 migrations** (only affect `MasterAntiqueRepairData`): from Package Manager Console, set **Default project** to `MasterAntiqueRepairData`, then `Add-Migration <Name>` / `Update-Database`. Running these against the website project itself fails (`You cannot call a method on a null-valued expression`) — that's exactly why the data project exists.
- **No test project exists** in this repo currently.

## Architecture

### Request pipeline / startup

- `Global.asax` — `Application_Start` calls `RouteConfig.RegisterRoutes(...)` and `BundleConfig.RegisterBundles(...)`.
- `App_Code/RouteConfig.cs` — enables Microsoft FriendlyUrls (extensionless, SEO-friendly URLs) with permanent auto-redirects.
- `App_Code/BundleConfig.cs` — CSS/JS bundling & minification via `System.Web.Optimization` + WebGrease.
- `App_Code/Startup.cs` + `App_Code/Startup.Auth.cs` — OWIN startup (`[assembly: OwinStartupAttribute(typeof(MasterAntiqueRepair.Startup))]`). `ConfigureAuth` wires up cookie authentication (`DefaultAuthenticationTypes.ApplicationCookie`, login path `/Account/Login`) and the external-login cookie. Third-party OAuth providers (Microsoft, Twitter, Facebook, Google) are referenced in packages but commented out/unconfigured — enabling one requires supplying real client id/secret here.

### Identity / data access

- `App_Code/IdentityModels.cs` defines:
  - `ApplicationUser : IdentityUser`
  - `ApplicationDbContext : IdentityDbContext<ApplicationUser>` (EF6 Code First, connection string name `"DefaultConnection"`)
  - `UserManager : UserManager<ApplicationUser>`
  - `IdentityHelper` — sign-in helper, external-login redirect URL builder, and open-redirect-safe `RedirectToReturnUrl`.
- Connection string `DefaultConnection` in `Web.config` points at LocalDB (`(LocalDb)\MSSQLLocalDB`) using an `.mdf` file in `App_Data`, auto-created by EF6 Code First's default `CreateDatabaseIfNotExists` initializer on first use (e.g. the first Register/Login attempt) — no manual DB setup or migrations needed. The `App_Data` folder is tracked via a `.gitkeep` placeholder; the `.mdf`/`.ldf` files themselves are gitignored. (SQLite was tried and reverted — see README.md.)
- Account/auth pages under `MasterAntiqueRepair/MasterAntiqueRepair/Account/` (`Login`, `Register`, `Manage`, `RegisterExternalLogin`, `OpenAuthProviders.ascx`) follow the standard ASP.NET Identity template pattern: code-behind calls into `UserManager`/`ApplicationDbContext` directly (no repository/service layer).

### Data project / EF6 Migrations

- `MasterAntiqueRepairData/App_Code/TestDbContext.cs`, `TestItem.cs`, `User.cs` are a **scratch proof-of-concept** for the migrations workflow (exercised by `TestEF.aspx`/`.aspx.cs` in the website, which writes/reads a `TestItem` and a `User` on every page load) — not the real Phase 1 domain model described below. Expect these to be replaced once actual domain entities (Requests/Assignees/Status History, etc.) are added.
- `TestDbContext` uses connection string name `"TestConnection"`, separate from Identity's `"DefaultConnection"`. **Both connection strings live only in the website's `Web.config`** — the class library has no `Web.config`/`app.config`-based connection string of its own; at runtime the ASP.NET host's config is what EF reads regardless of which assembly the `DbContext` lives in.
- `MasterAntiqueRepairData/Migrations/Configuration.cs` has `AutomaticMigrationsEnabled = false` — schema changes must go through explicit `Add-Migration`/`Update-Database` (see Build section above), not automatic migrations.
- Full rationale and troubleshooting (why Website Projects can't run EF6 migrations tooling, MSBuild-on-PATH requirement, EF version-mismatch pitfalls) is documented in `README.md` — read it before touching migrations if something breaks.

### Pages & layout

- `Site.master` / `Site.master.cs` — desktop master page. Includes anti-XSRF token handling in `Page_Init`/`master_Page_PreLoad` (cookie-backed `ViewStateUserKey`, validated on postback) — don't remove this without understanding the XSRF protection it provides.
- `Site.Mobile.master` — separate mobile master page; `ViewSwitcher.ascx`/`.cs` toggles between desktop/mobile views (uses FriendlyUrls' mobile display mode support).
- Top-level content pages (`Default.aspx`, `About.aspx`, `Contact.aspx`) are currently the unmodified Visual Studio template scaffold (e.g. `Default.aspx` still has the generic "ASP.NET" jumbotron) — no antique-repair domain functionality has been built yet.

### Front-end

- Bootstrap 3.3.7 + jQuery 3.3.1 + Modernizr, delivered via NuGet packages under `MasterAntiqueRepair/packages/` and referenced/bundled through `BundleConfig.cs`.
- `Microsoft.AspNet.ScriptManager.WebForms`/`MSAjax` provide the WebForms `ScriptManager`/partial-postback (UpdatePanel) infrastructure.

## Project goals (Phase 1 — see README.md)

This is explicitly a "legacy build" learning exercise, not a real pizza-ordering product. `README.md` defines the target scope:

- A core domain of 3–4 related entities (e.g. a case/request-tracking style flow: Requests → Assignees → Status History) with basic CRUD for each.
- One approval/status-transition workflow with at least 3 states (e.g. Submitted → In Review → Closed).
- A simple login/role check (hardcoded roles are acceptable).
- One list/search view with filtering and pagination.
- **Layered architecture** — UI / business logic / data access clearly separated, with no logic in code-behind. This is a deliberate constraint for new domain work, even though the existing scaffolded Identity/Account pages (from the VS template) call `UserManager`/`ApplicationDbContext` directly from code-behind.
- Server-side input validation.
- Logging of workflow state changes (intended to become an audit trail in a later phase).

## Working in this codebase

- When adding a new page, follow the existing `.aspx` + `.aspx.cs` + `MasterPageFile="~/Site.Master"` pattern (see `Default.aspx`/`Default.aspx.cs`).
- When adding shared C# logic that isn't tied to a single page, put it in `App_Code/`.
- New domain entities/`DbContext`s that will need EF6 migrations should go in `MasterAntiqueRepairData` (following the `TestDbContext`/`TestItem` pattern), not in the website's `App_Code/`.
- New domain logic should follow the layered-architecture requirement above (keep business logic and data access out of code-behind), rather than mirroring the direct-`DbContext`-from-code-behind pattern used in the pre-existing Identity/Account pages.
- `MasterAntiqueRepair/MasterAntiqueRepair/Bin/`, `MasterAntiqueRepair/MasterAntiqueRepairData/bin|obj/`, and `MasterAntiqueRepair/packages/` contain restored/copied binaries — treat them as generated output, not source to edit.
