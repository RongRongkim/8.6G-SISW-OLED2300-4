# LadderTextExtractor launcher (v2)
param(
    [ValidateSet('All','HighScan','Build')]
    [string]$Mode = 'All'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Join-Path $ScriptDir 'LadderTextExtractor'
$Exe = Join-Path $ProjectDir 'bin\Debug\net5.0\LadderTextExtractor.exe'
$PdfAll = 'D:\000.Remodel\002.SDC\009.외주사\8.6G SISW-OLED2300-4\13.Source\02.MP\PDF'
$PdfHigh = Join-Path $PdfAll 'Ladder_HighScan'
$GlobalValue = (Resolve-Path (Join-Path $ScriptDir '..\..\GlobalValue.txt') -ErrorAction SilentlyContinue).Path

function Write-Title([string]$Text) {
    Write-Host ''
    Write-Host "=== $Text ===" -ForegroundColor Cyan
}

if ($Mode -eq 'Build') {
    Write-Title 'LadderTextExtractor Build'
    Push-Location $ProjectDir
    dotnet build
    $code = $LASTEXITCODE
    Pop-Location
    if ($code -ne 0) { throw "Build failed (exit $code)" }
    Write-Host '[OK] Build success' -ForegroundColor Green
    Write-Host "Run: Run-LadderTextExtractor.cmd"
    return
}

if (-not (Test-Path $Exe)) {
    throw "EXE not found. Run Build-LadderTextExtractor.cmd first.`n$Exe"
}

if ($Mode -eq 'HighScan') {
    $PdfRoot = $PdfHigh
    $OutRoot = Join-Path $ScriptDir 'sample-output'
    $title = 'LadderTextExtractor (HighScan only)'
} else {
    $PdfRoot = $PdfAll
    $OutRoot = Join-Path $ScriptDir 'full-output'
    $title = 'LadderTextExtractor (All Ladder PDF)'
}

if (-not (Test-Path $PdfRoot)) {
    throw "PDF folder not found: $PdfRoot"
}

Write-Title $title
Write-Host "Source : $PdfRoot"
Write-Host "Output : $OutRoot"
if ($GlobalValue) { Write-Host "Global : $GlobalValue" } else { Write-Host 'Global : (not found)' }

$argsList = @($PdfRoot, $OutRoot)
if ($GlobalValue) { $argsList += $GlobalValue }

& $Exe @argsList
$code = $LASTEXITCODE

Write-Host ''
if ($code -eq 0 -or $code -eq 2) {
    Write-Host "[OK] Done. index: $(Join-Path $OutRoot 'index.json')" -ForegroundColor Green
} else {
    throw "Extractor failed (exit $code)"
}