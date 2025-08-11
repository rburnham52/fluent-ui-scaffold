param(
    [string]$Framework = "net8.0",
    [string]$Configuration = "Release",
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"
$coverageDir = "./coverage"
$reportDir = "$coverageDir/report"
$runsettings = "coverlet.runsettings"

# Ensure reportgenerator is available
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Installing reportgenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4 | Out-Null
    $env:PATH += ";$HOME/.dotnet/tools"
}

# Clean previous coverage
if (Test-Path $coverageDir) { Remove-Item -Recurse -Force $coverageDir }
New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null

# Run tests (core)
if (Test-Path "tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj") {
    dotnet test "tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj" `
        --no-build `
        --verbosity normal `
        --configuration $Configuration `
        --framework $Framework `
        --settings $runsettings `
        --collect:"XPlat Code Coverage" `
        --results-directory $coverageDir `
        --logger trx
}

# Run tests (playwright) - best effort
if (Test-Path "tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj") {
    try {
        dotnet test "tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj" `
            --no-build `
            --verbosity normal `
            --configuration $Configuration `
            --framework $Framework `
            --settings $runsettings `
            --collect:"XPlat Code Coverage" `
            --results-directory $coverageDir `
            --logger trx
    } catch { }
}

# Generate HTML report
reportgenerator `
    -reports:"$coverageDir/**/coverage.cobertura.xml" `
    -targetdir:"$reportDir" `
    -reporttypes:Html;TextSummary

# Threshold check (90%)
$xml = Get-ChildItem $coverageDir -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1
if (-not $xml) { throw "Cobertura file not found" }
$content = Get-Content $xml.FullName -Raw
if ($content -match 'line-rate="([0-9\.]+)"') {
    $rate = [double]$matches[1]
    $percent = [int]([math]::Round($rate * 100))
    Write-Host "Coverage: $percent%"
    if ($percent -lt 90) { throw "Coverage below 90% ($percent%)" }
} else {
    throw "Could not parse coverage percentage"
}

Write-Host "HTML report: $reportDir/index.html"
if ($OpenReport) {
    try { Start-Process "$reportDir/index.html" } catch { }
}