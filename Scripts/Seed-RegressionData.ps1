<#
.SYNOPSIS
    Seeds a fixed, deterministic Manager/Employee/Customer/Ticket dataset for regression
    testing.

.DESCRIPTION
    Builds and runs MasterAntiqueRepairSeed, which populates 1 Manager, 3 Employees, and
    8 Customers, then has each customer submit 3 Tickets (one ends COMPLETED with a
    completion comment plus a separate follow-up comment, one ends INPROGRESS, one stays
    SUBMITTED) - all via the real domain/service methods, so validation and audit logging
    behave exactly like production. Every timestamp is then patched to a fixed offset from
    the moment this script runs (not a fixed calendar date), so the results land inside
    MetricsService's rolling 7-day window no matter what day you seed on, while the counts
    themselves are identical every time.

    Requires the target database to be empty (no Users, no Tickets) - it checks first and
    refuses to run otherwise, since seeding on top of existing data would not produce
    repeatable counts. Reset first - this targets the working LocalDB database by default,
    and Reset-Database.ps1 is destructive (takes any dev accounts/tickets in it with it):
        .\Scripts\Reset-Database.ps1
        .\Scripts\Seed-RegressionData.ps1

    Schema creation is not a separate step - EF6's migrations-based initializer runs
    automatically the moment this tool opens its first RepairShopContext against the
    target database.

    See the "Regression seed data" and "Regression test accounts" sections of README.md
    for the expected baseline counts and seeded login credentials.

.PARAMETER Server
    SQL Server instance to connect to. Defaults to the LocalDB instance this project uses.

.PARAMETER Database
    Database (catalog) name to seed into. Defaults to the working LocalDB database (the
    same one the app uses via F5) - pass a different name here to target a separate
    catalog instead.

.PARAMETER AppDataPath
    Folder for the .mdf/_log.ldf files. Defaults to the website's App_Data folder.

.EXAMPLE
    .\Scripts\Reset-Database.ps1
    .\Scripts\Seed-RegressionData.ps1
#>
[CmdletBinding()]
param(
    [string]$Server = "(localdb)\MSSQLLocalDB",
    [string]$Database = "aspnet-MasterAntiqueRepair-e93a6129-7f74-4486-97e8-8d4ab1a709b4",
    [string]$AppDataPath
)

$ErrorActionPreference = "Stop"

# $PSScriptRoot isn't populated yet when used as a param() default in Windows
# PowerShell 5.1 (it's blank until the script body runs), so it can't be used up in
# the param block above - compute the default down here instead.
$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $AppDataPath) {
    $AppDataPath = Join-Path $scriptRoot "..\MasterAntiqueRepair\MasterAntiqueRepair\App_Data"
}

if (-not (Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue)) {
    throw "SqlLocalDB.exe isn't on PATH. It ships with SQL Server LocalDB (installed by Visual Studio's 'ASP.NET and web development' workload) and is normally added to the machine-wide PATH by that installer - if it's missing, LocalDB itself likely isn't installed. Typical location: C:\Program Files\Microsoft SQL Server\<version>\Tools\Binn\SqlLocalDB.exe"
}
if ($Server -match '^\(localdb\)\\(.+)$') {
    & SqlLocalDB.exe start $Matches[1] | Out-Null
}

if (-not (Get-Command msbuild -ErrorAction SilentlyContinue)) {
    throw "msbuild isn't on PATH. See step 4 ('Put MSBuild on PATH') in the Getting started section of README.md."
}

$repoRoot = Join-Path $scriptRoot ".."
$seedProj = Join-Path $repoRoot "MasterAntiqueRepair\MasterAntiqueRepairSeed\MasterAntiqueRepairSeed.csproj"
$seedExe = Join-Path $repoRoot "MasterAntiqueRepair\MasterAntiqueRepairSeed\bin\Debug\MasterAntiqueRepairSeed.exe"

Write-Host "Building MasterAntiqueRepairSeed ..."
& msbuild $seedProj /p:Configuration=Debug /nologo /v:quiet
if ($LASTEXITCODE -ne 0) {
    throw "Build failed (exit code $LASTEXITCODE)."
}

Write-Host "Seeding '$Database' on $Server ..."
Write-Host ""
& $seedExe --server $Server --database $Database --appDataPath $AppDataPath
if ($LASTEXITCODE -ne 0) {
    throw "$seedExe failed (exit code $LASTEXITCODE) - see output above. If the database wasn't empty, reset it first with: Scripts\Reset-Database.ps1 -Database $Database"
}

Write-Host ""
Write-Host "Done. Compare the counts above against the baseline table in README.md's 'Regression seed data' section."
Write-Host "Log in at /Account/Login with any account from README.md's 'Regression test accounts' section to cross-check /AuditLogView and /Metrics."
