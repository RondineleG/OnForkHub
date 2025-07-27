param (
    [switch]$DryRun = $true,
    [string]$FilePath = "Directory.Packages.props",
    [switch]$IncludePrerelease = $false,
    [string[]]$ExcludePackages = @(),
    [int]$TimeoutSeconds = 30
)

Write-Host "=== CHECK AND UPDATE PACKAGES ===" -ForegroundColor Cyan
Write-Host "Mode: " -NoNewline
Write-Host ($DryRun ? "CHECK ONLY" : "CHECK & UPDATE") -ForegroundColor ($DryRun ? "Yellow" : "Red")
Write-Host "File: $FilePath"
Write-Host "Include Prerelease: $IncludePrerelease"
if ($ExcludePackages.Count -gt 0) {
    Write-Host "Excluded Packages: $($ExcludePackages -join ', ')"
}
Write-Host ""

if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

try {
    [xml]$doc = Get-Content $FilePath -ErrorAction Stop
} catch {
    Write-Error "Failed to parse XML file: $_"
    exit 1
}

$packages = $doc.SelectNodes("//PackageVersion")
if (-not $packages -or $packages.Count -eq 0) {
    Write-Warning "No PackageVersion elements found in $FilePath"
    exit 0
}

$updates = @()
$majorUpdates = @()
$minorUpdates = @()
$patchUpdates = @()
$errors = @()

function Get-LatestVersion($packageId) {
    try {
        $url = "https://api.nuget.org/v3-flatcontainer/$packageId/index.json"
        $resp = Invoke-RestMethod -Uri $url -UseBasicParsing -TimeoutSec $TimeoutSeconds

        $versions = if ($IncludePrerelease) {
            $resp.versions
        } else {
            $resp.versions | Where-Object { $_ -notmatch "-(alpha|beta|rc|preview|pre)" }
        }

        if (-not $versions) {
            Write-Warning "⚠ No suitable versions found for $packageId"
            return $null
        }

        $sortedVersions = $versions | Sort-Object { [version]($_ -replace '-.+$', '') }
        return $sortedVersions[-1]
    } catch {
        Write-Warning "⚠ Could not fetch version for $packageId`: $($_.Exception.Message)"
        return $null
    }
}

function Get-VersionDifference($current, $latest) {
    try {
        $currentVersion = [version]($current -replace '-.+$', '')
        $latestVersion = [version]($latest -replace '-.+$', '')

        if ($latestVersion -gt $currentVersion) {
            if ($latestVersion.Major -gt $currentVersion.Major) {
                return "Major"
            } elseif ($latestVersion.Minor -gt $currentVersion.Minor) {
                return "Minor"
            } elseif ($latestVersion.Build -gt $currentVersion.Build) {
                return "Patch"
            } else {
                return "Revision"
            }
        } else {
            return "None"
        }
    } catch {
        return if ($latest -ne $current) { "Unknown" } else { "None" }
    }
}

function Write-UpdateLine($name, $current, $latest, $updateType) {
    $message = "⬆ {0}: {1} → {2}" -f $name, $current, $latest

    switch ($updateType) {
        "Major" {
            Write-Host $message -ForegroundColor Red
            Write-Host "  ⚠ MAJOR VERSION CHANGE - Potential breaking changes!" -ForegroundColor DarkRed
        }
        "Minor" {
            Write-Host $message -ForegroundColor Yellow
        }
        "Patch" {
            Write-Host $message -ForegroundColor Cyan
        }
        "Revision" {
            Write-Host $message -ForegroundColor Blue
        }
        default {
            Write-Host $message -ForegroundColor Yellow
        }
    }
}

Write-Host "Checking $($packages.Count) packages..." -ForegroundColor Gray
Write-Host ""

$progress = 0
foreach ($pkg in $packages) {
    $progress++
    $name = $pkg.Include
    $current = $pkg.Version

    if ($name -in $ExcludePackages) {
        Write-Host "⏭ Skipping excluded package: $name" -ForegroundColor DarkGray
        continue
    }

    Write-Progress -Activity "Checking packages" -Status "Checking $name" -PercentComplete (($progress / $packages.Count) * 100)

    $latest = Get-LatestVersion $name

    if (-not $latest) {
        $errors += "Failed to get version for $name"
        continue
    }

    $updateType = Get-VersionDifference $current $latest

    if ($updateType -ne "None") {
        Write-UpdateLine $name $current $latest $updateType

        if (-not $DryRun) {
            $pkg.Version = $latest
        }

        $updateInfo = [PSCustomObject]@{
            Name       = $name
            Current    = $current
            Latest     = $latest
            UpdateType = $updateType
        }

        $updates += $updateInfo

        switch ($updateType) {
            "Major" { $majorUpdates += $updateInfo }
            "Minor" { $minorUpdates += $updateInfo }
            "Patch" { $patchUpdates += $updateInfo }
        }
    } else {
        Write-Host "✓ $name is up to date ($current)" -ForegroundColor Green
    }
}

Write-Progress -Activity "Checking packages" -Completed

Write-Host ""
Write-Host "=== SUMMARY ===" -ForegroundColor Cyan

if ($updates.Count -gt 0) {
    Write-Host "📊 Update Statistics:" -ForegroundColor White
    if ($majorUpdates.Count -gt 0) {
        Write-Host "  🔴 Major updates: $($majorUpdates.Count) (WARNING: Potential breaking changes)" -ForegroundColor Red
    }
    if ($minorUpdates.Count -gt 0) {
        Write-Host "  🟡 Minor updates: $($minorUpdates.Count)" -ForegroundColor Yellow
    }
    if ($patchUpdates.Count -gt 0) {
        Write-Host "  🔵 Patch updates: $($patchUpdates.Count)" -ForegroundColor Cyan
    }

    Write-Host ""

    if (-not $DryRun) {
        try {
            $backupPath = "$FilePath.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item $FilePath $backupPath
            Write-Host "📁 Backup created: $backupPath" -ForegroundColor Gray

            $doc.Save($FilePath)
            Write-Host "✔ Updated $($updates.Count) packages in $FilePath" -ForegroundColor Green

            if ($majorUpdates.Count -gt 0) {
                Write-Host ""
                Write-Host "⚠ IMPORTANT: $($majorUpdates.Count) major version update(s) applied!" -ForegroundColor Red
                Write-Host "  We recommend testing thoroughly before deployment." -ForegroundColor Yellow
                Write-Host "  Please check changelogs for the following packages:" -ForegroundColor Yellow
                $majorUpdates | ForEach-Object {
                    Write-Host "    • $($_.Name): $($_.Current) → $($_.Latest)" -ForegroundColor DarkYellow
                }
            }
        } catch {
            Write-Error "Failed to save changes: $_"
            exit 1
        }
    } else {
        Write-Host "📋 $($updates.Count) packages can be updated" -ForegroundColor Yellow

        if ($majorUpdates.Count -gt 0) {
            Write-Host ""
            Write-Host "⚠ WARNING: $($majorUpdates.Count) major version update(s) detected!" -ForegroundColor Red
            Write-Host "  Please check changelogs before applying:" -ForegroundColor Yellow
            $majorUpdates | ForEach-Object {
                Write-Host "    • $($_.Name): $($_.Current) → $($_.Latest)" -ForegroundColor DarkYellow
            }
        }

        Write-Host ""
        Write-Host "ℹ Use -DryRun:`$false to apply updates" -ForegroundColor Gray
    }
} else {
    Write-Host "✔ All packages are up to date" -ForegroundColor Green
}

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "⚠ Encountered $($errors.Count) errors:" -ForegroundColor Yellow
    $errors | ForEach-Object { Write-Host "  • $_" -ForegroundColor DarkYellow }
}

if ($updates.Count -gt 0) {
    Write-Host ""
    Write-Host "Detailed Updates:" -ForegroundColor Cyan
    $updates | Format-Table Name, Current, Latest, UpdateType -AutoSize
}
