# MasterAntiqueRepair

## PHASE 1 — Legacy Build
Stack: ASP.NET Framework 4.7.2, C#, WebForms, SQL Server (or SQLite is fine)

Functional requirements:
•	A single core domain with 3–4 related entities (e.g., a simple case/request tracking app: Requests → Assignees → Status History)
•	Basic CRUD for each entity
•	One approval/status-transition workflow with at least 3 states (e.g., Submitted → In Review → Closed)
•	A simple login/role check (hardcoded roles are fine)
•	One list/search view with filtering and pagination

Non-functional requirements:
•	Layered architecture (UI / business logic / data access clearly separated — no logic in code-behind)
•	Server-side input validation
•	Logging of workflow state changes (this becomes your audit trail in Phase 2)
•	A short README explaining the structure and how to run it.



## Database: LocalDB

* Entity Framework 6.5.1
* SQL Server Express LocalDB

This project uses LocalDB via EF6 Code First (`ApplicationDbContext` in `App_Code/IdentityModels.cs`). No manual setup is needed — the `DefaultConnection` string in `Web.config` points at `(LocalDb)\MSSQLLocalDB`, and EF6's default `CreateDatabaseIfNotExists` initializer creates the `.mdf` file in `App_Data/` (auto-created on first use, along with the full Identity schema) the first time the context is actually used — e.g. the first Register or Login attempt.

### The catch: `CreateDatabaseIfNotExists` only runs once *per context type*

EF6 stores a hash of your model in a `__MigrationHistory` table (tagged with a `ContextKey` per `DbContext` type) the first time each context creates its tables. If you later add/change entity classes or `DbSet<T>` properties on a context that's *already* initialized against a database, you'll get:

```
The model backing the '<Context>' context has changed since the database was created.
```

**But this is scoped per context, not per physical database.** A *different* `DbContext` type that has never touched that database before — even if the database already exists and already has other tables in it (e.g. `ApplicationDbContext`'s Identity tables) — gets its tables created automatically the first time it's used, exactly like a fresh database. This is how `RepairShopContext` (see "Refactor: domain-backed authentication" below) got its `Users`/`Orders`/`UserRoles` tables created in the *same* database as Identity's `AspNetUsers` tables, with zero Migrations setup — confirmed by checking `INFORMATION_SCHEMA.TABLES` before and after its first use. The "model changed" exception above only fires once a *given context* already has a `__MigrationHistory` entry and its current model no longer matches it.

### Fixing it without migrations (data loss — only fine for disposable/test databases)

Detach the database from LocalDB *before* deleting the file — deleting the `.mdf`/`.ldf` first leaves a stale catalog entry in LocalDB pointing at a file that no longer exists, causing a *different* error ("Cannot attach the file... as database...") on the next connection attempt. Correct order:

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "EXEC sp_detach_db 'DatabaseName';"
```
then delete the `.mdf`/`.ldf` files in `App_Data/`. (If LocalDB still holds a lock on the files, `sqllocaldb stop MSSQLLocalDB` first releases it — it auto-restarts on the next connection.)

### The real fix: EF6 Code First Migrations — in a separate Class Library project

For any database with real data you want to keep, use Migrations instead of resetting. **This does not work directly in the Website Project.** `Enable-Migrations`/`Add-Migration`/`Update-Database` fail with `You cannot call a method on a null-valued expression` — EF6's PowerShell migration tooling reflects over MSBuild-style project properties that only exist on a real `.csproj`-based project, and a classic Website Project (confirm via `Get-Project` in PMC — it reports `Type: Web Site`) has none of them. There is no config workaround for this; the fix is structural.

**Solution**: entity classes and the `DbContext` that needs migrations live in a separate **Class Library (.NET Framework)** project — `MasterAntiqueRepairData` — added to `MasterAntiqueRepair.sln` alongside the website, referenced by it via **Add Reference → Projects**. Migrations tooling works normally against a Class Library. Setup, once per new solution/clone:

1. Add a new **Class Library (.NET Framework)** project to the solution, targeting **.NET Framework 4.7.2** (matching everything else).
2. `Install-Package EntityFramework` into it — **must match the website's EF version exactly** (currently 6.5.1; check both projects' `packages.config`). A mismatch (we hit 6.5.1 vs 6.5.2) causes the same null-reference error as the Website Project issue, just later in the process (after `Enable-Migrations` succeeds, `Add-Migration` fails).
3. From the website project, **Add Reference → Projects** → check the class library. This copies its compiled DLL into the website's `Bin/` automatically on build.
4. Connection strings (e.g. `DefaultConnection`, `TestConnection`) stay in the **website's** `Web.config` only — the class library has no `Web.config`, and at runtime the ASP.NET host process's config is what's read regardless of which assembly the `DbContext` class lives in. No connection string duplication needed.
5. In PMC, set **Default project:** to the class library project, then:
   ```
   Enable-Migrations -ContextTypeName MasterAntiqueRepair.<YourContext>
   ```
   (No `-MigrationsDirectory` override needed here — that was only a workaround for the Website Project's `App_Code`-only compilation rule, which doesn't apply to a real Class Library; it compiles every `.cs` file normally.)
6. If `Add-Migration`/`Update-Database` fails with `Exception calling "Start" with "1" argument(s): "The system cannot find the file specified"`: EF6's migration PowerShell script invokes `msbuild.exe` by bare name, and it isn't resolvable on `PATH`. Fix by adding MSBuild's install directory to your user `PATH` (e.g. `C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin` — adjust for your VS edition/version), then **fully restart Visual Studio** (an already-running `devenv.exe` won't see a `PATH` change made after it launched).

**Ongoing workflow, once set up** — this is what to do every time you refactor entity classes in the class library:

```
Add-Migration <DescriptiveName>
Update-Database
```

Run both from PMC with **Default project:** set to the class library. `Add-Migration` generates a timestamped file under its `Migrations/` folder with the actual `CreateTable`/`AddColumn`/etc. calls for just the diff — open and read it, it's the real schema change in code. `Update-Database` applies it and records it in `__MigrationHistory` — no data loss, no manual resets, regardless of how many times you refactor.


## Refactor: domain-backed authentication

`Account/Register.aspx.cs` and `Account/Login.aspx.cs` no longer use ASP.NET Identity's `UserManager`/`ApplicationUser` for account storage or credential checking — they use the domain model (`Customer`/`Employee`/`Manager`, all `MasterAntiqueRepairData` classes) directly:

- **Passwords**: `User.PasswordHash` + `User.SetPassword(string)`/`VerifyPassword(string)`, backed by `MasterAntiqueRepairData/App_Code/PasswordHasher.cs` — PBKDF2 via `System.Security.Cryptography.Rfc2898DeriveBytes` (10,000 iterations, random 16-byte salt per password, stored as `salt + hash` base64). Deliberately *not* `Microsoft.AspNet.Identity.PasswordHasher` — same underlying algorithm, but avoids adding another NuGet package that has to stay version-aligned across both projects (see the `EntityFramework` 6.5.1/6.5.2 mismatch earlier in this doc for why that's worth avoiding).
- **Sign-in**: `MasterAntiqueRepair/App_Code/RepairAuthHelper.cs` (website-side, since it's `HttpContext`/OWIN-specific) builds a `ClaimsIdentity` from a domain `User` and signs in via the *existing* OWIN cookie middleware from `Startup.Auth.cs` — reuses the same cookie transport Identity was using, just sources the identity from the domain model instead of `ApplicationUser`. Must use `DefaultAuthenticationTypes.ApplicationCookie` (from `Microsoft.AspNet.Identity`, not `Microsoft.Owin.Security` — easy to get wrong) as the identity's auth type, matching what `Startup.Auth.cs`'s `CookieAuthenticationOptions.AuthenticationType` is configured for — a mismatch there means the middleware silently won't recognize the identity at all.
- **Storage**: `MasterAntiqueRepairData/App_Code/RepairShopContext.cs` — a new, non-scratch `DbContext` (separate from `TestDbContext`, which stays untouched/scratch-only), using `DefaultConnection` — the *same* database Identity already uses. Its `Users`/`Orders`/`UserRoles` tables were created automatically alongside Identity's pre-existing `AspNetUsers` tables with zero Migrations setup, since `CreateDatabaseIfNotExists` scopes its "already initialized" check per `DbContext` type, not per database — see the corrected note above.
- **Register scope**: the public `Register.aspx` only ever creates `Customer` accounts (with a basic duplicate-username check, since Identity's built-in validation is gone). `Employee`/`Manager` account provisioning isn't wired up yet — a natural next page, likely restricted to an authenticated `Manager`/`Owner`, not public self-service.
- `ApplicationUser`/`ApplicationDbContext`/`UserManager` (`IdentityModels.cs`) and the external-login pages (`RegisterExternalLogin.aspx`, `OpenAuthProviders.ascx`, `Account/Manage.aspx`) are left in place but now unused/dead — not deleted, since removing that dependency chain is a separate decision from wiring up the new auth.

Verified live (curl against a running instance, same method as elsewhere in this doc): Register → `HTTP 302` + real `.AspNet.ApplicationCookie` issued; Login with the same credentials → same; confirmed via `sqlcmd` that `PasswordHash` is a real hash, not the plaintext password.


## Fixing a fresh-restore build

After cloning/moving the repo and restoring NuGet packages, the build can still fail with:

- `Could not find file '...\Bin\roslyn\csc.exe'`
- `Unable to update auto-refresh reference 'microsoft.aspnet.web.optimization.webforms.dll'`

This is because "Restore NuGet Packages" only downloads packages listed in `packages.config` — it does not run their `install.ps1` scripts, which is how some packages copy files into `Bin/` on classic Website Projects. Fix in Visual Studio's Package Manager Console (Tools → NuGet Package Manager → Package Manager Console):

```
Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -reinstall
Install-Package Microsoft.AspNet.Web.Optimization.WebForms -Version 1.1.3
```

The first re-copies the Roslyn compiler (`csc.exe`, `vbc.exe`, etc.) into `Bin/roslyn/`. The second adds `Microsoft.AspNet.Web.Optimization.WebForms`, which was referenced in `Bin/` but missing from `packages.config`.
