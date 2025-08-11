# Code Coverage Guide

This repo mirrors FluentTestScaffold's coverage experience.

## Quick start

Windows (PowerShell):
```powershell
./scripts/coverage.ps1
```

Linux/macOS:
```bash
chmod +x scripts/coverage.sh
./scripts/coverage.sh
```

What it does:
- Runs tests with coverage (Coverlet collector)
- Generates HTML report via ReportGenerator
- Enforces 90% threshold
- Writes to `./coverage/` and `./coverage/report/index.html`

Options:
- PowerShell: `-Framework net7.0`, `-Configuration Debug`, `-OpenReport`
- Bash: `-f net7.0`, `-c Debug`, `-o`

## Manual commands

```bash
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj \
  --configuration Release \
  --framework net8.0 \
  --settings coverlet.runsettings \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage/

reportgenerator \
  -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage/report \
  -reporttypes:Html
```

## CI/CD

GitHub Actions CI:
- Runs tests with coverage for net6/net7/net8
- Generates HTML report
- Enforces 90% threshold
- Uploads Cobertura XML and HTML report as artifacts

See `.github/workflows/ci.yml` for details.