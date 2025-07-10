param(
    [string]$ScriptName = "",
    [switch]$List = $false,
    [switch]$Help = $false
)

$scriptsFolder = "scripts"
$projectName = "OnForkHub"

$colors = @{
    Title = "Cyan"
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "White"
    Gray = "Gray"
}

function Write-Header {
    param([string]$Title)
    
    $border = "═" * 60
    Write-Host $border -ForegroundColor $colors.Title
    Write-Host " $Title" -ForegroundColor $colors.Title
    Write-Host $border -ForegroundColor $colors.Title
    Write-Host " 📅 $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor $colors.Gray
    Write-Host " 👤 User: $env:USERNAME" -ForegroundColor $colors.Gray
    Write-Host " 📁 Project: $projectName" -ForegroundColor $colors.Gray
    Write-Host $border -ForegroundColor $colors.Title
}

function Get-AvailableScripts {
    $scriptsPath = Join-Path $PSScriptRoot $scriptsFolder
    
    if (-not (Test-Path $scriptsPath)) {
        Write-Host "❌ Folder 'scripts' not found!" -ForegroundColor $colors.Error
        return @()
    }
    
    $scripts = Get-ChildItem -Path $scriptsPath -Filter "*.ps1" | Sort-Object Name
    return $scripts
}

function Show-ScriptMenu {
    $scripts = Get-AvailableScripts
    
    if ($scripts.Count -eq 0) {
        Write-Host "📂 No scripts found in '$scriptsFolder'" -ForegroundColor $colors.Warning
        return
    }
    
    Write-Host "`n📋 Available scripts:" -ForegroundColor $colors.Info
    Write-Host "─" * 40 -ForegroundColor $colors.Gray
    
    for ($i = 0; $i -lt $scripts.Count; $i++) {
        $script = $scripts[$i]
        $number = ($i + 1).ToString().PadLeft(2)
        
        $description = ""
        try {
            $firstLines = Get-Content $script.FullName -TotalCount 10
            $descLine = $firstLines | Where-Object { $_ -match "^#.*[Dd]escrição.*:" } | Select-Object -First 1
            if ($descLine) {
                $description = ($descLine -split ":", 2)[1].Trim()
            }
        }
        catch {
            $description = "PowerShell script"
        }
        
        if (-not $description) {
            $description = "PowerShell script"
        }
        
        Write-Host " $number. " -NoNewline -ForegroundColor $colors.Title
        Write-Host "$($script.BaseName)" -NoNewline -ForegroundColor $colors.Success
        Write-Host " - $description" -ForegroundColor $colors.Gray
    }
    
    Write-Host "─" * 40 -ForegroundColor $colors.Gray
    Write-Host " 0.  Exit" -ForegroundColor $colors.Warning
    Write-Host ""
}

function Execute-Script {
    param(
        [string]$ScriptPath,
        [string]$ScriptName
    )
    
    Write-Host "🚀 Running: $ScriptName" -ForegroundColor $colors.Title
    Write-Host "─" * 40 -ForegroundColor $colors.Gray
    
    try {
        $originalLocation = Get-Location
        Set-Location $PSScriptRoot
        
        & $ScriptPath
        $exitCode = $LASTEXITCODE
        
        Set-Location $originalLocation
        
        Write-Host "─" * 40 -ForegroundColor $colors.Gray
        
        if ($exitCode -eq 0 -or $null -eq $exitCode) {
            Write-Host "✅ Script '$ScriptName' executed successfully!" -ForegroundColor $colors.Success
        } else {
            Write-Host "⚠️  Script '$ScriptName' finished with code: $exitCode" -ForegroundColor $colors.Warning
        }
    }
    catch {
        Write-Host "❌ Error while executing '$ScriptName': $($_.Exception.Message)" -ForegroundColor $colors.Error
    }
    
    Write-Host ""
}

function Show-Help {
    Write-Header "HELP - Script Launcher"
    
    Write-Host "💡 How to use:" -ForegroundColor $colors.Info
    Write-Host ""
    Write-Host "  Interactive mode:" -ForegroundColor $colors.Success
    Write-Host "    .\run-scripts.ps1" -ForegroundColor $colors.Gray
    Write-Host ""
    Write-Host "  Run a specific script:" -ForegroundColor $colors.Success
    Write-Host "    .\run-scripts.ps1 -ScriptName clean-projects" -ForegroundColor $colors.Gray
    Write-Host ""
    Write-Host "  List available scripts:" -ForegroundColor $colors.Success
    Write-Host "    .\run-scripts.ps1 -List" -ForegroundColor $colors.Gray
    Write-Host ""
    Write-Host "  Show this help:" -ForegroundColor $colors.Success
    Write-Host "    .\run-scripts.ps1 -Help" -ForegroundColor $colors.Gray
    Write-Host ""
    
    Write-Host "📁 Expected structure:" -ForegroundColor $colors.Info
    Write-Host "  📂 Project/" -ForegroundColor $colors.Gray
    Write-Host "  ├── 📄 run-scripts.ps1" -ForegroundColor $colors.Gray
    Write-Host "  ├── 📂 scripts/" -ForegroundColor $colors.Gray
    Write-Host "  │   ├── 📄 clean-projects.ps1" -ForegroundColor $colors.Gray
    Write-Host "  │   ├── 📄 fix-packages.ps1" -ForegroundColor $colors.Gray
    Write-Host "  │   └── 📄 other-scripts.ps1" -ForegroundColor $colors.Gray
    Write-Host "  ├── 📂 src/" -ForegroundColor $colors.Gray
    Write-Host "  └── 📂 test/" -ForegroundColor $colors.Gray
}

if ($Help) {
    Show-Help
    return
}

if ($List) {
    Write-Header "Available Scripts - $projectName"
    Show-ScriptMenu
    return
}

if ($ScriptName) {
    $scripts = Get-AvailableScripts
    $targetScript = $scripts | Where-Object { $_.BaseName -eq $ScriptName }
    
    if ($targetScript) {
        Write-Header "Running Script - $projectName"
        Execute-Script -ScriptPath $targetScript.FullName -ScriptName $targetScript.BaseName
    } else {
        Write-Host "❌ Script '$ScriptName' not found!" -ForegroundColor $colors.Error
        Write-Host ""
        Write-Host "📋 Available scripts:" -ForegroundColor $colors.Info
        $scripts | ForEach-Object { Write-Host "  • $($_.BaseName)" -ForegroundColor $colors.Gray }
    }
    return
}

Write-Header "Script Launcher - $projectName"

do {
    Show-ScriptMenu
    
    $scripts = Get-AvailableScripts
    if ($scripts.Count -eq 0) {
        break
    }
    
    do {
        Write-Host "🔢 Choose an option (0-$($scripts.Count)): " -NoNewline -ForegroundColor $colors.Info
        $choice = Read-Host
        
        if ($choice -eq "0") {
            Write-Host "👋 Exiting..." -ForegroundColor $colors.Warning
            return
        }
        
        $choiceNum = $null
        if ([int]::TryParse($choice, [ref]$choiceNum) -and $choiceNum -ge 1 -and $choiceNum -le $scripts.Count) {
            $selectedScript = $scripts[$choiceNum - 1]
            Execute-Script -ScriptPath $selectedScript.FullName -ScriptName $selectedScript.BaseName
            break
        } else {
            Write-Host "❌ Invalid option! Enter a number between 0 and $($scripts.Count)" -ForegroundColor $colors.Error
        }
    } while ($true)
    
    Write-Host "🔄 Run another script? (y/N): " -NoNewline -ForegroundColor $colors.Info
    $continue = Read-Host
    
    if ($continue -notmatch "^[yY]") {
        Write-Host "👋 Exiting..." -ForegroundColor $colors.Warning
        break
    }
    
    Write-Host ""
    
} while ($true)

Write-Host "🎉 Script Launcher completed!" -ForegroundColor $colors.Success
