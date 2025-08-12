# Code Coverage Guide

This repo mirrors FluentTestScaffold's coverage experience.

## Quick start

Windows (PowerShell):
```powershell
./scripts/coverage.ps1 -Framework net8.0 -Configuration Debug -OpenReport
```

Linux/macOS:
```bash
chmod +x scripts/coverage.sh
./scripts/coverage.sh -f net8.0 -c Debug -o
```

What it does:
- Runs tests with coverage (Coverlet collector)
- Generates HTML report via ReportGenerator (Html + TextSummary)
- Enforces 90% threshold
- Writes to `./coverage/` and `./coverage/report/index.html`

Options:
- PowerShell: `-Framework net8.0`, `-Configuration Debug|Release`, `-OpenReport`
- Bash: `-f net8.0`, `-c Debug|Release`, `-o`

## Manual commands

PowerShell (note quoting of reporttypes; run from repo root):
```powershell
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj `
  --configuration Debug `
  --framework net8.0 `
  --settings "$PSScriptRoot/../coverlet.runsettings" `
  --collect:"XPlat Code Coverage" `
  --results-directory ./coverage/ `
  --logger trx

reportgenerator `
  -reports:./coverage/**/coverage.cobertura.xml `
  -targetdir:./coverage/report `
  -reporttypes:"Html;TextSummary"
```

Bash:
```bash
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj \
  --configuration Debug \
  --framework net8.0 \
  --settings ./coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/ \
  --logger trx

reportgenerator \
  -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage/report \
  -reporttypes:'Html;TextSummary'
```

Notes:
- Default target is net8.0. Some tests exist only on net8.0; using net7.0 may reduce discovered tests (and coverage).
- Always pass `--settings ./coverlet.runsettings` or the absolute path; otherwise coverage aggregation may include unintended assemblies (lowering %).
- On Windows, ensure `-reporttypes:"Html;TextSummary"` is quoted as one argument.

## CI/CD

GitHub Actions CI:
- Runs tests with coverage for net6/net7/net8
- Generates HTML/TextSummary report
- Enforces 90% threshold
- Uploads Cobertura XML and HTML report as artifacts

See `.github/workflows/ci.yml` for details.