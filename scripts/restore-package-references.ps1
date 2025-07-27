Write-Host "=== Fixing NU1008 and NU1605 Errors (Dynamic) ===" -ForegroundColor Cyan

$packagesPropsPath = ".\Directory.Packages.props"

if (-not (Test-Path $packagesPropsPath)) {
    Write-Host "❌ Directory.Packages.props not found!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Directory.Packages.props found" -ForegroundColor Green

Write-Host "`n1. Reading packages from Directory.Packages.props..." -ForegroundColor Yellow

try {
    [xml]$packagesXml = Get-Content $packagesPropsPath
    $packageVersions = @{}
    
    $packagesXml.Project.ItemGroup.PackageVersion | ForEach-Object {
        if ($_.Include -and $_.Version) {
            $packageVersions[$_.Include] = $_.Version
            Write-Host "  → $($_.Include) = $($_.Version)" -ForegroundColor Gray
        }
    }
    
    Write-Host "  ✓ Loaded $($packageVersions.Count) packages" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to read Directory.Packages.props: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n2. Updating .csproj files (removing versions)..." -ForegroundColor Yellow

$csprojFiles = Get-ChildItem -Path "." -Filter "*.csproj" -Recurse
$csprojFixed = 0

foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $content = $content -replace '(<PackageReference[^>]+)\s+Version="[^"]*"([^>]*>)', '$1$2'
    
    if ($content -ne $originalContent) {
        Set-Content $file.FullName -Value $content -Encoding UTF8
        Write-Host "  ✓ Fixed: $($file.Name)" -ForegroundColor Green
        $csprojFixed++
    }
}

Write-Host "  → $csprojFixed .csproj files updated" -ForegroundColor Green

Write-Host "`n3. Verifying if all packages are defined..." -ForegroundColor Yellow

$allPackageReferences = @{}

foreach ($file in $csprojFiles) {
    try {
        [xml]$csprojXml = Get-Content $file.FullName
        $csprojXml.Project.ItemGroup.PackageReference | ForEach-Object {
            if ($_.Include) {
                $packageName = $_.Include
                if (-not $allPackageReferences.ContainsKey($packageName)) {
                    $allPackageReferences[$packageName] = @()
                }
                $allPackageReferences[$packageName] += $file.Name
            }
        }
    }
    catch {
        Write-Host "  ⚠️  Failed to read $($file.Name): $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

$missingPackages = @()
foreach ($packageRef in $allPackageReferences.GetEnumerator()) {
    $packageName = $packageRef.Key
    $usedInFiles = $packageRef.Value -join ", "
    
    if (-not $packageVersions.ContainsKey($packageName)) {
        $missingPackages += $packageName
        Write-Host "  ❌ Missing: $packageName (used in: $usedInFiles)" -ForegroundColor Red
    } else {
        Write-Host "  ✓ OK: $packageName v$($packageVersions[$packageName])" -ForegroundColor Green
    }
}

if ($missingPackages.Count -gt 0) {
    Write-Host "`n4. Adding missing packages to Directory.Packages.props..." -ForegroundColor Yellow
    
    $content = Get-Content $packagesPropsPath -Raw
    $insertPoint = $content.LastIndexOf('</ItemGroup>')
    $beforeInsert = $content.Substring(0, $insertPoint)
    $afterInsert = $content.Substring($insertPoint)
    
    $newPackages = "`r`n    <!-- Automatically added packages -->`r`n"
    
    foreach ($packageName in $missingPackages) {
        $suggestedVersion = "1.0.0"
        switch -Regex ($packageName) {
            "^Microsoft\.Extensions\." { $suggestedVersion = "9.0.2" }
            "^Microsoft\.AspNetCore\." { $suggestedVersion = "9.0.0" }
            "^Microsoft\.EntityFrameworkCore" { $suggestedVersion = "9.0.6" }
            "^System\." { $suggestedVersion = "9.0.0" }
            "^HotChocolate" { $suggestedVersion = "15.1.5" }
            "^xunit" { $suggestedVersion = "2.9.3" }
            "FluentAssertions" { $suggestedVersion = "8.3.0" }
            "Moq" { $suggestedVersion = "4.20.72" }
            "AutoMapper" { $suggestedVersion = "13.0.1" }
            "FluentValidation" { $suggestedVersion = "11.10.0" }
            "MediatR" { $suggestedVersion = "12.4.1" }
            "Swashbuckle" { $suggestedVersion = "7.2.0" }
            "Polly" { $suggestedVersion = "8.5.0" }
        }
        
        $newPackages += "    <PackageVersion Include=`"$packageName`" Version=`"$suggestedVersion`" />`r`n"
        Write-Host "  ✓ Added: $packageName v$suggestedVersion" -ForegroundColor Green
    }
    
    $content = $beforeInsert + $newPackages + $afterInsert
    Set-Content $packagesPropsPath -Value $content -Encoding UTF8
    
    Write-Host "  ✓ Directory.Packages.props updated with $($missingPackages.Count) packages" -ForegroundColor Green
} else {
    Write-Host "`n4. ✓ All packages are already defined in Directory.Packages.props" -ForegroundColor Green
}

Write-Host "`n5. Cleaning and restoring..." -ForegroundColor Yellow

Write-Host "  → Running dotnet clean..." -ForegroundColor Gray
& dotnet clean --verbosity quiet

Write-Host "  → Removing bin and obj folders..." -ForegroundColor Gray
Get-ChildItem -Path "." -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path "." -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "  → Clearing NuGet cache..." -ForegroundColor Gray
& dotnet nuget locals all --clear

Write-Host "  → Running dotnet restore..." -ForegroundColor Gray
& dotnet restore --verbosity quiet

Write-Host "`n6. Final check..." -ForegroundColor Yellow

$remainingVersions = @()
foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    $matches = [regex]::Matches($content, '<PackageReference[^>]+Version="([^"]*)"[^>]*Include="([^"]*)"')
    if ($matches.Count -gt 0) {
        foreach ($match in $matches) {
            $version = $match.Groups[1].Value
            $package = $match.Groups[2].Value
            $remainingVersions += "$($file.Name): $package v$version"
        }
    }
}

if ($remainingVersions.Count -gt 0) {
    Write-Host "  ❌ Still found version attributes in .csproj files:" -ForegroundColor Red
    $remainingVersions | ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
} else {
    Write-Host "  ✓ No version attributes found in .csproj files" -ForegroundColor Green
}

Write-Host "`n7. Testing build..." -ForegroundColor Yellow

$buildResult = & dotnet build --verbosity quiet 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✅ BUILD SUCCESSFUL!" -ForegroundColor Green
} else {
    Write-Host "  ❌ Build has errors:" -ForegroundColor Red
    
    $nugetErrors = $buildResult | Where-Object { $_ -match "error NU\d+" }
    $otherErrors = $buildResult | Where-Object { $_ -match "error" -and $_ -notmatch "error NU\d+" }
    
    if ($nugetErrors) {
        Write-Host "    NuGet Errors:" -ForegroundColor Yellow
        $nugetErrors | Select-Object -First 5 | ForEach-Object {
            Write-Host "      $_" -ForegroundColor Red
        }
    }
    
    if ($otherErrors) {
        Write-Host "    Other Errors:" -ForegroundColor Yellow
        $otherErrors | Select-Object -First 3 | ForEach-Object {
            Write-Host "      $_" -ForegroundColor Red
        }
    }
}

Write-Host "`n=== FINAL REPORT ===" -ForegroundColor Cyan
Write-Host "Packages defined in Directory.Packages.props: $($packageVersions.Count)" -ForegroundColor White
Write-Host ".csproj files updated: $csprojFixed" -ForegroundColor White
Write-Host "Missing packages added: $($missingPackages.Count)" -ForegroundColor White

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ ALL FIXED SUCCESSFULLY!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Some errors remain — check above for details" -ForegroundColor Yellow
}

Write-Host "`n=== Fix completed ===" -ForegroundColor Cyan
