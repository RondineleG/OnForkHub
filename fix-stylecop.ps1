# Script para corrigir erros StyleCop SA1412 e SA1305

$projectRoot = "C:\Dev\OnForkHub"
$errorLog = "$projectRoot\build.log"

# Funcao para corrigir SA1412 (UTF-8 BOM)
function Fix-UTF8BOM {
    param([string]$filePath)
    
    $content = Get-Content -Path $filePath -Raw -Encoding UTF8
    
    # Remove BOM if exists
    if ($content.StartsWith([char]0xFEFF)) {
        $content = $content.Substring(1)
    }
    
    Set-Content -Path $filePath -Value $content -Encoding UTF8NoBOM
    Write-Host "Fixed UTF8 BOM: $filePath"
}

# Funcao para corrigir SA1305 (Hungarian notation)
function Fix-HungarianNotation {
    param([string]$filePath)
    
    $content = Get-Content -Path $filePath -Raw
    
    $modified = $false
    
    # Replacements
    if ($content -match '\bprConfig\b') {
        $content = $content -replace '\bprConfig\b', 'pullRequestConfig'
        $modified = $true
    }
    if ($content -match '\bprInfo\b') {
        $content = $content -replace '\bprInfo\b', 'pullRequestInfo'
        $modified = $true
    }
    if ($content -match '\bprNumber\b') {
        $content = $content -replace '\bprNumber\b', 'pullRequestNumber'
        $modified = $true
    }
    if ($content -match '\bvIndex\b') {
        $content = $content -replace '\bvIndex\b', 'versionIndex'
        $modified = $true
    }
    if ($content -match '\bvsCodePath\b') {
        $content = $content -replace '\bvsCodePath\b', 'vscodeEditorPath'
        $modified = $true
    }
    if ($content -match '\bidValue\b') {
        $content = $content -replace '\bidValue\b', 'identifierValue'
        $modified = $true
    }
    if ($content -match '\bjsRuntime\b') {
        $content = $content -replace '\bjsRuntime\b', 'jsInteropRuntime'
        $modified = $true
    }
    if ($content -match '\bpIPControl\b') {
        $content = $content -replace '\bpIPControl\b', 'playbackIPControl'
        $modified = $true
    }
    
    if ($modified) {
        Set-Content -Path $filePath -Value $content -Encoding UTF8NoBOM
        Write-Host "Fixed Hungarian notation: $filePath"
    }
}

# Get all C# files
$csharpFiles = Get-ChildItem -Path $projectRoot -Include "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

Write-Host "Processing $($csharpFiles.Count) C# files..."

$count = 0
foreach ($file in $csharpFiles) {
    Fix-UTF8BOM -filePath $file.FullName
    Fix-HungarianNotation -filePath $file.FullName
    $count++
    if ($count % 50 -eq 0) {
        Write-Host "Processed $count files..."
    }
}

Write-Host "All files processed!"
