<#
.SYNOPSIS
    Seeds a Manager and two Employee accounts into the MasterAntiqueRepair database.

.DESCRIPTION
    Run this once against a freshly-created (empty Users table) database to get past
    the bootstrap problem: Register is Manager-only, so there's no UI path to create
    the very first account. This script inserts rows directly via sqlcmd.

    It only creates the schema's *data*, not the schema itself - the database and its
    tables must already exist. The normal way to get there is to run the web app once
    (F5 in Visual Studio): the first request that touches RepairShopContext runs its
    EF6 Migrations (MigrateDatabaseToLatestVersion - the default whenever a Migrations
    Configuration exists for a context) up to the latest migration, building the
    Users/Orders tables automatically. Then run this script to populate it.

    If LocalDB doesn't currently have the database attached (e.g. it's been idle and
    LocalDB dropped it, or the .mdf/.ldf were deleted), this script re-attaches it from
    the .mdf on disk before doing anything else - it does not create the database from
    nothing itself.

    Safe to re-run: an account is skipped (not duplicated) if a User with that Name
    already exists.

.PARAMETER Server
    SQL Server instance to connect to. Defaults to the LocalDB instance this project
    uses.

.PARAMETER Database
    Database (catalog) name to seed. Defaults to the Initial Catalog configured for
    "DefaultConnection" in Web.config - the live application database.

.PARAMETER AppDataPath
    Folder containing the .mdf file, used only to re-attach the database if LocalDB
    doesn't currently have it online (see DESCRIPTION). Defaults to the website's
    App_Data folder.

.EXAMPLE
    .\Seed-InitialUsers.ps1

.EXAMPLE
    .\Seed-InitialUsers.ps1 -Database "MasterAntiqueRepairTest"
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

function Get-SqlIdentifier {
    param([string]$Value)
    return "[" + ($Value -replace "\]", "]]") + "]"
}

function Invoke-SqlCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Query,
        # Strips headers/dashes/row-count footer so the output is just the raw value(s) -
        # needed whenever the caller parses the result, not just checks success.
        [switch]$Raw
    )
    if ($Raw) {
        $output = sqlcmd -S $Server -d $Database -b -h -1 -W -Q $Query 2>&1
    }
    else {
        $output = sqlcmd -S $Server -d $Database -b -Q $Query 2>&1
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Host ($output -join "`n") -ForegroundColor Red
        throw "sqlcmd failed against database '$Database' (exit code $LASTEXITCODE)."
    }
    return $output
}

# LocalDB only has a database "online" (queryable by name) after something connects to
# it with AttachDbFilename in the connection string - which is what the web app's
# Web.config does, but only while it's actually run recently. If IIS Express hasn't
# touched it lately, LocalDB can drop the attachment, and a plain `sqlcmd -d <name>`
# then fails with a login/"Cannot open database" error that has nothing to do with
# credentials. Explicitly re-attach it from the .mdf on disk first, if needed.
$mdfPath = Join-Path $AppDataPath "$Database.mdf"
$dbId = (Invoke-SqlCommand -Database "master" -Query "SELECT DB_ID(N'$($Database -replace "'", "''")');" -Raw) | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($dbId) -or $dbId.Trim() -eq "NULL") {
    if (-not (Test-Path $mdfPath)) {
        throw "Database '$Database' isn't attached and no .mdf was found at $mdfPath. Run the web app once (F5) to create it first."
    }
    Write-Host "Database '$Database' isn't currently attached - attaching from $mdfPath ..."
    $attachQuery = "CREATE DATABASE $(Get-SqlIdentifier $Database) ON (FILENAME = N'$($mdfPath -replace "'", "''")') FOR ATTACH_REBUILD_LOG;"
    Invoke-SqlCommand -Database "master" -Query $attachQuery | Out-Null
}

# Mirrors MasterAntiqueRepairData/App_Code/PasswordHasher.cs exactly - same salt size,
# iteration count, hash size, and salt+hash layout - so these accounts can log in
# through the normal app (User.VerifyPassword) like any other.
function New-PasswordHash {
    param([Parameter(Mandatory = $true)][string]$Password)

    $saltSize = 16
    $hashSize = 32
    $iterations = 10000

    $deriveBytes = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($Password, $saltSize, $iterations)
    try {
        $salt = $deriveBytes.Salt
        $hash = $deriveBytes.GetBytes($hashSize)
        $combined = New-Object byte[] ($saltSize + $hashSize)
        [Array]::Copy($salt, 0, $combined, 0, $saltSize)
        [Array]::Copy($hash, 0, $combined, $saltSize, $hashSize)
        return [Convert]::ToBase64String($combined)
    }
    finally {
        $deriveBytes.Dispose()
    }
}

function Get-SqlLiteral {
    param([string]$Value)
    return "'" + ($Value -replace "'", "''") + "'"
}

$accounts = @(
    @{ Name = "manager";   Password = "ManagerPass123!";  Discriminator = "Manager" },
    @{ Name = "employee1"; Password = "EmployeePass123!"; Discriminator = "Employee" },
    @{ Name = "employee2"; Password = "EmployeePass456!"; Discriminator = "Employee" }
)

Write-Host "Seeding $Database on $Server ..."
Write-Host ""

foreach ($account in $accounts) {
    $existsQuery = "SELECT COUNT(*) FROM dbo.Users WHERE Name = $(Get-SqlLiteral $account.Name);"
    $existingCount = (Invoke-SqlCommand -Database $Database -Query $existsQuery -Raw) | Select-Object -First 1

    if ([int]$existingCount -gt 0) {
        Write-Host "Skipped '$($account.Name)' - already exists."
        continue
    }

    $hash = New-PasswordHash -Password $account.Password
    $insertQuery = @"
SET NOCOUNT ON;
INSERT INTO dbo.Users (Name, CreatedAt, PasswordHash, Discriminator)
VALUES ($(Get-SqlLiteral $account.Name), GETDATE(), $(Get-SqlLiteral $hash), $(Get-SqlLiteral $account.Discriminator));
"@

    Invoke-SqlCommand -Database $Database -Query $insertQuery | Out-Null
    Write-Host "Created $($account.Discriminator.ToLower()) '$($account.Name)' (password: $($account.Password))"
}

Write-Host ""
Write-Host "Done. Log in at /Account/Login with the accounts above."
Write-Host "Change these passwords (or delete/re-seed) before using this database for anything real."
