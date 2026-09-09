# MasterAntiqueRepair

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

PHASE 1 — Legacy Build
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