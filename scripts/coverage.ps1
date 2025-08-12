param(
    [string]$Framework = "net8.0",
    [string]$Configuration = "Release",
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"

# Resolve repo root relative to this script
$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$coverageDir = Join-Path $root "coverage"
$reportDir = Join-Path $coverageDir "report"
$runsettings = Join-Path $root "coverlet.runsettings"

# Ensure reportgenerator is available
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Installing reportgenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4 | Out-Null
    $env:PATH += ";$HOME/.dotnet/tools"
}

# Clean previous coverage
if (Test-Path $coverageDir) { Remove-Item -Recurse -Force $coverageDir }
New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null

# Helper to run tests with coverage
function Invoke-CoverageTest {
    param(
        [Parameter(Mandatory=$true)][string]$ProjectPath,
        [Parameter(Mandatory=$true)][string]$LogName
    )

    dotnet test $ProjectPath `
        --no-build `
        --verbosity normal `
        --configuration $Configuration `
        --framework $Framework `
        --settings "$runsettings" `
        --collect:"XPlat Code Coverage" `
        --results-directory "$coverageDir" `
        --logger "trx;LogFileName=$LogName"
}

# Run tests (core)
$coreProj = Join-Path $root "tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj"
if (Test-Path $coreProj) {
    Invoke-CoverageTest -ProjectPath $coreProj -LogName "core.trx"
}

# Run tests (playwright) - best effort
$pwProj = Join-Path $root "tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj"
if (Test-Path $pwProj) {
    try {
        Invoke-CoverageTest -ProjectPath $pwProj -LogName "playwright.trx"
    } catch { }
}

# Generate HTML/TextSummary merged report
reportgenerator `
    -reports:"$coverageDir/**/coverage.cobertura.xml" `
    -targetdir:"$reportDir" `
    -reporttypes:"Html;TextSummary"

# Threshold check (90%) using ReportGenerator Summary
$summaryPath = Join-Path $reportDir "Summary.txt"
if (-not (Test-Path $summaryPath)) { throw "Coverage summary not found at $summaryPath" }
$summary = Get-Content $summaryPath
$match = ($summary | Select-String -Pattern '^\s*Line coverage:\s+([0-9]+(?:\.[0-9]+)?)%').Matches | Select-Object -First 1
if (-not $match) { throw "Could not parse Line coverage from $summaryPath" }
$percent = [double]::Parse($match.Groups[1].Value, [System.Globalization.CultureInfo]::InvariantCulture)
$percentInt = [int]([math]::Round($percent))
Write-Host "Coverage: $percent%"
if ($percent -lt 90) { throw "Coverage below 90% ($percentInt%)" }

Write-Host "HTML report: $reportDir/index.html"
if ($OpenReport) {
    try { Start-Process (Join-Path $reportDir "index.html") } catch { }
}