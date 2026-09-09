# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project type

This is an **ASP.NET Web Forms "Website" project** (the pre-2010, non-SDK-style project type), not a Web Application project and not a modern SDK-style .NET project. Key implications:

- There is **no `.csproj`** — the project is defined entirely by `PizzaGo/PizzaGo.sln`, which references the `PizzaGo/PizzaGo/` folder directly via a `WebsiteProperties` section (VirtualPath `/localhost_57962`, port `57962`).
- Code-behind files (`*.aspx.cs`, `*.master.cs`, `*.ascx.cs`) are compiled dynamically per-page; there is no central `Program.cs`/`Startup` entry point in the SDK sense.
- Shared, non-page C# classes live in `PizzaGo/PizzaGo/App_Code/` and are compiled automatically by ASP.NET from that folder — this is where new helpers/services/models should generally go unless they belong to a specific page.
- Dependencies are managed via `packages.config` (classic NuGet, not `PackageReference`) — see `PizzaGo/PizzaGo/packages.config`. Restored packages land in `PizzaGo/packages/`.
- Targets .NET Framework 4.6.1.

## Build / run / restore

There is no `dotnet build`/`dotnet test` workflow here (this predates the SDK-style tooling). Typical workflow:

- **Open and run**: Open `PizzaGo/PizzaGo.sln` in Visual Studio and press F5/Ctrl+F5. This starts IIS Express against the `PizzaGo/PizzaGo/` folder on port `57962` and compiles pages on request.
- **Restore packages**: `nuget restore PizzaGo/PizzaGo.sln` (classic `packages.config` restore) if `PizzaGo/packages/` is missing or out of sync with `packages.config`.
- **No test project exists** in this repo currently.

## Architecture

### Request pipeline / startup

- `Global.asax` — `Application_Start` calls `RouteConfig.RegisterRoutes(...)` and `BundleConfig.RegisterBundles(...)`.
- `App_Code/RouteConfig.cs` — enables Microsoft FriendlyUrls (extensionless, SEO-friendly URLs) with permanent auto-redirects.
- `App_Code/BundleConfig.cs` — CSS/JS bundling & minification via `System.Web.Optimization` + WebGrease.
- `App_Code/Startup.cs` + `App_Code/Startup.Auth.cs` — OWIN startup (`[assembly: OwinStartupAttribute(typeof(PizzaGo.Startup))]`). `ConfigureAuth` wires up cookie authentication (`DefaultAuthenticationTypes.ApplicationCookie`, login path `/Account/Login`) and the external-login cookie. Third-party OAuth providers (Microsoft, Twitter, Facebook, Google) are referenced in packages but commented out/unconfigured — enabling one requires supplying real client id/secret here.

### Identity / data access

- `App_Code/IdentityModels.cs` defines:
  - `ApplicationUser : IdentityUser`
  - `ApplicationDbContext : IdentityDbContext<ApplicationUser>` (EF6 Code First, connection string name `"DefaultConnection"`)
  - `UserManager : UserManager<ApplicationUser>`
  - `IdentityHelper` — sign-in helper, external-login redirect URL builder, and open-redirect-safe `RedirectToReturnUrl`.
- Connection string `DefaultConnection` in `Web.config` points at LocalDB (`(LocalDb)\MSSQLLocalDB`) using an `.mdf` file in `App_Data` (auto-created on first use — `App_Data` doesn't exist in the repo yet).
- Account/auth pages under `PizzaGo/PizzaGo/Account/` (`Login`, `Register`, `Manage`, `RegisterExternalLogin`, `OpenAuthProviders.ascx`) follow the standard ASP.NET Identity template pattern: code-behind calls into `UserManager`/`ApplicationDbContext` directly (no repository/service layer).

### Pages & layout

- `Site.master` / `Site.master.cs` — desktop master page. Includes anti-XSRF token handling in `Page_Init`/`master_Page_PreLoad` (cookie-backed `ViewStateUserKey`, validated on postback) — don't remove this without understanding the XSRF protection it provides.
- `Site.Mobile.master` — separate mobile master page; `ViewSwitcher.ascx`/`.cs` toggles between desktop/mobile views (uses FriendlyUrls' mobile display mode support).
- Top-level content pages (`Default.aspx`, `About.aspx`, `Contact.aspx`) are currently the unmodified Visual Studio template scaffold (e.g. `Default.aspx` still has the generic "ASP.NET" jumbotron) — no pizza-ordering domain functionality has been built yet.

### Front-end

- Bootstrap 3.3.7 + jQuery 3.3.1 + Modernizr, delivered via NuGet packages under `PizzaGo/packages/` and referenced/bundled through `BundleConfig.cs`.
- `Microsoft.AspNet.ScriptManager.WebForms`/`MSAjax` provide the WebForms `ScriptManager`/partial-postback (UpdatePanel) infrastructure.

## Working in this codebase

- When adding a new page, follow the existing `.aspx` + `.aspx.cs` + `MasterPageFile="~/Site.Master"` pattern (see `Default.aspx`/`Default.aspx.cs`).
- When adding shared C# logic that isn't tied to a single page, put it in `App_Code/`.
- `PizzaGo/PizzaGo/Bin/` and `PizzaGo/packages/` contain restored/copied binaries — treat them as generated output, not source to edit.
