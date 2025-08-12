#!/usr/bin/env pwsh

param(
    [string]$Framework = "net8.0",
    [string]$Configuration = "Release",
    [switch]$OpenReport
)

Write-Host "Running coverage for framework: $Framework" -ForegroundColor Green

# Clean previous coverage results
# Clean previous coverage results
if (Test-Path "./coverage") {
    Remove-Item -Recurse -Force "./coverage"
}

# Run tests with coverage
Write-Host "Running tests with coverage..." -ForegroundColor Yellow
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj `
    --configuration $Configuration `
    --framework $Framework `
    --settings coverlet.runsettings `
    --collect:"XPlat Code Coverage" `
    --results-directory ./coverage/ `
    --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Install reportgenerator if not available
Write-Host "Installing reportgenerator..." -ForegroundColor Yellow
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4

# Generate HTML report
Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow
reportgenerator `
    -reports:./coverage/**/coverage.cobertura.xml `
    -targetdir:./coverage/report `
    -reporttypes:Html

# Check coverage threshold
Write-Host "Checking coverage threshold..." -ForegroundColor Yellow
$coverageFiles = Get-ChildItem -Path "./coverage" -Recurse -Filter "coverage.cobertura.xml"
if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found!" -ForegroundColor Red
    exit 1
}

$coverageFile = $coverageFiles[0].FullName
$xml = [xml](Get-Content $coverageFile)
$lineRate = [double]$xml.coverage.'line-rate'
$coveragePercent = [math]::Round($lineRate * 100, 2)

Write-Host "Coverage: $coveragePercent%" -ForegroundColor Cyan

if ($coveragePercent -lt 90) {
    Write-Host "❌ Coverage is below 90% threshold ($coveragePercent%)" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ Coverage meets 90% threshold ($coveragePercent%)" -ForegroundColor Green
}

# Open report if requested
if ($OpenReport) {
    $reportPath = Resolve-Path "./coverage/report/index.html"
    Write-Host "Opening coverage report: $reportPath" -ForegroundColor Green
    Start-Process $reportPath
} else {
    Write-Host "Coverage report available at: ./coverage/report/index.html" -ForegroundColor Green
}

Write-Host "Coverage analysis complete!" -ForegroundColor Green 