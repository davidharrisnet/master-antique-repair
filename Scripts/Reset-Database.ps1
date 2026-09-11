<#
.SYNOPSIS
    Wipes a MasterAntiqueRepair LocalDB database completely - schema, data, everything.

.DESCRIPTION
    DESTRUCTIVE. Detaches the database from the LocalDB instance, then deletes its
    .mdf/_log.ldf files from App_Data. There is no undo.

    Detach happens BEFORE the files are deleted, not after - deleting the files first
    leaves a stale catalog entry in LocalDB and the next attach fails with "Cannot
    attach the file ... as database ...". Always detach first.

    After this runs, the database no longer exists at all. The next time the app
    touches it (e.g. F5, or hitting any page that uses RepairShopContext), EF6's
    CreateDatabaseIfNotExists initializer recreates it from scratch - empty, no
    accounts, no orders. Run Seed-InitialUsers.ps1 afterward if you want the usual
    Manager/Employee accounts back.

.PARAMETER Server
    SQL Server instance to connect to. Defaults to the LocalDB instance this project
    uses.

.PARAMETER Database
    Database (catalog) name to wipe. Defaults to the Initial Catalog configured for
    "DefaultConnection" in Web.config - the live application database.

.PARAMETER AppDataPath
    Folder containing the .mdf/_log.ldf files. Defaults to the website's App_Data
    folder.

.EXAMPLE
    .\Reset-Database.ps1

.EXAMPLE
    .\Reset-Database.ps1 -Database "MasterAntiqueRepairTest"
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
if (-not $AppDataPath) {
    $scriptRoot = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
    $AppDataPath = Join-Path $scriptRoot "..\MasterAntiqueRepair\MasterAntiqueRepair\App_Data"
}

# LocalDB auto-starts on first connection, but that can take a moment - if the
# instance has been stopped for a while (e.g. IIS Express hasn't run recently), the
# very first sqlcmd call can lose that race and fail with a misleading
# "Unable to complete login process due to delay in opening server connection." error.
# Starting it explicitly first (idempotent - harmless if already running) avoids that.
if ($Server -match '^\(localdb\)\\(.+)$') {
    & SqlLocalDB.exe start $Matches[1] | Out-Null
}

$mdfPath = Join-Path $AppDataPath "$Database.mdf"
$ldfPath = Join-Path $AppDataPath "${Database}_log.ldf"

Write-Host "This will PERMANENTLY delete database '$Database' on $Server," -ForegroundColor Yellow
Write-Host "including every table, account, and order in it." -ForegroundColor Yellow
Write-Host ""

if (-not (Test-Path $mdfPath)) {
    Write-Host "No .mdf found at $mdfPath - nothing to delete on disk."
}

Write-Host "Detaching '$Database' (if attached) ..."
$detachQuery = "IF DB_ID(N'$($Database -replace "'", "''")') IS NOT NULL EXEC sp_detach_db @dbname = N'$($Database -replace "'", "''")';"
sqlcmd -S $Server -d "master" -b -Q $detachQuery
if ($LASTEXITCODE -ne 0) {
    throw "Detach failed (exit code $LASTEXITCODE) - aborting before deleting any files, to avoid leaving a stale LocalDB catalog entry."
}

if (Test-Path $mdfPath) {
    Remove-Item $mdfPath -Force
    Write-Host "Deleted $mdfPath"
}
if (Test-Path $ldfPath) {
    Remove-Item $ldfPath -Force
    Write-Host "Deleted $ldfPath"
}

Write-Host ""
Write-Host "Done. '$Database' no longer exists."
Write-Host "Run the app once (F5) to recreate an empty schema, then optionally:"
Write-Host "  .\Scripts\Seed-InitialUsers.ps1"
