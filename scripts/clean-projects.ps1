param(
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

Write-Host "=== .NET Projects Cleanup Script ===" -ForegroundColor Cyan
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host "User: $env:USERNAME" -ForegroundColor Gray

if ($WhatIf) {
    Write-Host "🔍 SIMULATION MODE - No files will be deleted" -ForegroundColor Yellow
}

$baseDir = Get-Location
$dirsToClean = @("src", "test")

Write-Host "`n📁 Base directory: $baseDir" -ForegroundColor White

function Format-FileSize {
    param([long]$Size)
    
    if ($Size -gt 1GB) {
        return "{0:N2} GB" -f ($Size / 1GB)
    } elseif ($Size -gt 1MB) {
        return "{0:N2} MB" -f ($Size / 1MB)
    } elseif ($Size -gt 1KB) {
        return "{0:N2} KB" -f ($Size / 1KB)
    } else {
        return "$Size bytes"
    }
}

function Get-FolderSize {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) { return 0 }

    try {
        $size = Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue | 
                Measure-Object -Property Length -Sum | 
                Select-Object -ExpandProperty Sum
        return [long]($size ?? 0)
    }
    catch {
        return 0
    }
}

$totalBinFolders = 0
$totalObjFolders = 0
$totalSizeFreed = 0
$errors = @()

Write-Host "`n🧹 Cleaning 'bin' and 'obj' folders..." -ForegroundColor Yellow

foreach ($dir in $dirsToClean) {
    $fullPath = Join-Path $baseDir $dir

    if (-not (Test-Path $fullPath)) {
        Write-Host "  ⚠️  Folder not found: $dir" -ForegroundColor Yellow
        continue
    }

    Write-Host "`n📂 Processing: $dir" -ForegroundColor Cyan

    $binFolders = Get-ChildItem -Path $fullPath -Recurse -Directory -Name "bin" -ErrorAction SilentlyContinue
    $objFolders = Get-ChildItem -Path $fullPath -Recurse -Directory -Name "obj" -ErrorAction SilentlyContinue

    foreach ($binFolder in $binFolders) {
        $binPath = Join-Path $fullPath $binFolder
        $size = Get-FolderSize -Path $binPath
        $totalSizeFreed += $size

        if ($Verbose -or $WhatIf) {
            $sizeText = Format-FileSize -Size $size
            Write-Host "  🗑️  bin: $binFolder ($sizeText)" -ForegroundColor Red
        }

        if (-not $WhatIf) {
            try {
                Remove-Item -Path $binPath -Recurse -Force -ErrorAction Stop
                $totalBinFolders++
            }
            catch {
                $errors += "Failed to remove $binPath`: $($_.Exception.Message)"
                Write-Host "    ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            $totalBinFolders++
        }
    }

    foreach ($objFolder in $objFolders) {
        $objPath = Join-Path $fullPath $objFolder
        $size = Get-FolderSize -Path $objPath
        $totalSizeFreed += $size

        if ($Verbose -or $WhatIf) {
            $sizeText = Format-FileSize -Size $size
            Write-Host "  🗑️  obj: $objFolder ($sizeText)" -ForegroundColor Red
        }

        if (-not $WhatIf) {
            try {
                Remove-Item -Path $objPath -Recurse -Force -ErrorAction Stop
                $totalObjFolders++
            }
            catch {
                $errors += "Failed to remove $objPath`: $($_.Exception.Message)"
                Write-Host "    ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
            }
        } else {
            $totalObjFolders++
        }
    }

    if ($binFolders.Count -eq 0 -and $objFolders.Count -eq 0) {
        Write-Host "  ✅ No 'bin' or 'obj' folders found in $dir" -ForegroundColor Green
    }
}

Write-Host "`n🧽 Cleaning .NET caches..." -ForegroundColor Yellow

$cachesToClean = @{
    "NuGet packages" = "$env:USERPROFILE\.nuget\packages"
    "NuGet HTTP cache" = "$env:LOCALAPPDATA\NuGet\v3-cache"
    "NuGet temp" = "$env:LOCALAPPDATA\Temp\NuGetScratch"
    "MSBuild cache" = "$env:LOCALAPPDATA\Microsoft\MSBuild"
    "Temp .NET" = "$env:TEMP\.net"
}

foreach ($cache in $cachesToClean.GetEnumerator()) {
    if (Test-Path $cache.Value) {
        $size = Get-FolderSize -Path $cache.Value
        $sizeText = Format-FileSize -Size $size

        if ($WhatIf) {
            Write-Host "  🔍 $($cache.Key): $sizeText" -ForegroundColor Yellow
        } else {
            Write-Host "  🧽 Cleaning $($cache.Key)..." -ForegroundColor Gray
            try {
                if ($cache.Key -eq "NuGet packages") {
                    & dotnet nuget locals all --clear 2>$null | Out-Null
                } else {
                    Remove-Item -Path "$($cache.Value)\*" -Recurse -Force -ErrorAction SilentlyContinue
                }
                Write-Host "    ✅ $($cache.Key) cleaned" -ForegroundColor Green
            }
            catch {
                Write-Host "    ⚠️  Failed to clean $($cache.Key)" -ForegroundColor Yellow
            }
        }

        $totalSizeFreed += $size
    }
}

Write-Host "`n📊 FINAL REPORT" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor Gray

if ($WhatIf) {
    Write-Host "🔍 SIMULATION - What would be removed:" -ForegroundColor Yellow
} else {
    Write-Host "✅ Cleanup completed:" -ForegroundColor Green
}

Write-Host "  📁 'bin' folders processed: $totalBinFolders" -ForegroundColor White
Write-Host "  📁 'obj' folders processed: $totalObjFolders" -ForegroundColor White
Write-Host "  💾 Space freed: $(Format-FileSize -Size $totalSizeFreed)" -ForegroundColor Green

if ($errors.Count -gt 0) {
    Write-Host "  ⚠️  Errors found: $($errors.Count)" -ForegroundColor Yellow
    if ($Verbose) {
        foreach ($error in $errors) {
            Write-Host "    - $error" -ForegroundColor Red
        }
    }
}

if (-not $WhatIf) {
    Write-Host "`n💡 Additional suggestions:" -ForegroundColor Cyan
    Write-Host "  • Run 'dotnet restore' for required projects" -ForegroundColor Yellow
    Write-Host "  • For deeper cleaning, run: 'dotnet clean'" -ForegroundColor Yellow
    Write-Host "  • Consider restarting Visual Studio / VSCode" -ForegroundColor Yellow
}

Write-Host "`n🎉 Script finished!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════" -ForegroundColor Gray
