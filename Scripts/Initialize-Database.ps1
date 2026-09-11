<#
.SYNOPSIS
    Builds the database schema without needing Visual Studio or IIS Express open.

.DESCRIPTION
    RepairShopContext's schema only gets created the first time something actually
    queries it (its EF6 Migrations-based initializer - see the "MasterAntiqueRepairData"
    section of README.md). Normally that "something" is the running web app's first
    request. This script triggers the exact same thing directly, by building and
    running MasterAntiqueRepairScratch - a plain console app that does nothing but open
    a RepairShopContext and run one query. No web server, no port, and nothing is left
    running afterward, so it can't collide with your own F5/IIS Express session the way
    running a temporary web server here would.

    Safe to re-run: if the schema already exists, this is a no-op query.

.PARAMETER Server
    SQL Server instance to connect to. Defaults to the LocalDB instance this project
    uses.

.EXAMPLE
    .\Initialize-Database.ps1
#>
[CmdletBinding()]
param(
    [string]$Server = "(localdb)\MSSQLLocalDB"
)

$ErrorActionPreference = "Stop"

$scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$repoRoot = Join-Path $scriptRoot ".."
$scratchProj = Join-Path $repoRoot "MasterAntiqueRepair\MasterAntiqueRepairScratch\MasterAntiqueRepairScratch.csproj"
$scratchExe = Join-Path $repoRoot "MasterAntiqueRepair\MasterAntiqueRepairScratch\bin\Debug\MasterAntiqueRepairScratch.exe"

if (-not (Get-Command SqlLocalDB.exe -ErrorAction SilentlyContinue)) {
    throw "SqlLocalDB.exe isn't on PATH. It ships with SQL Server LocalDB (installed by Visual Studio's 'ASP.NET and web development' workload) and is normally added to the machine-wide PATH by that installer - if it's missing, LocalDB itself likely isn't installed. Typical location: C:\Program Files\Microsoft SQL Server\<version>\Tools\Binn\SqlLocalDB.exe"
}
if ($Server -match '^\(localdb\)\\(.+)$') {
    & SqlLocalDB.exe start $Matches[1] | Out-Null
}

if (-not (Get-Command msbuild -ErrorAction SilentlyContinue)) {
    throw "msbuild isn't on PATH. See step 4 ('Put MSBuild on PATH') in the Getting started section of README.md."
}

Write-Host "Building MasterAntiqueRepairScratch ..."
& msbuild $scratchProj /p:Configuration=Debug /nologo /v:quiet
if ($LASTEXITCODE -ne 0) {
    throw "Build failed (exit code $LASTEXITCODE)."
}

Write-Host "Running it once to trigger RepairShopContext's schema bootstrap ..."
& $scratchExe
if ($LASTEXITCODE -ne 0) {
    throw "$scratchExe failed (exit code $LASTEXITCODE) - the schema was not created. See the output above."
}

Write-Host ""
Write-Host "Done. Database schema is up to date."
