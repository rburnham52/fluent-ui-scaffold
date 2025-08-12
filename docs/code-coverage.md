# Code Coverage Guide

This document explains how to use the code coverage setup in the FluentUIScaffold project.

## Quick Start for New Developers

### 🚀 Get Coverage Running in 30 Seconds

**Windows:**
```powershell
.\scripts\coverage.ps1
```

**Linux/macOS:**
```bash
chmod +x scripts/coverage.sh
./scripts/coverage.sh
```

**That's it!** The script will:
- ✅ Run all tests with coverage collection
- ✅ Generate an HTML report
- ✅ Check if coverage meets 90% threshold
- ✅ Show you where to find the report

**To view the coverage report:**
- Open `./coverage/report/index.html` in your browser
- Or run with `-OpenReport` flag to auto-open

### 📊 What You'll See
- **Coverage percentage** (must be ≥90%)
- **File-by-file breakdown** of what's tested
- **Line-by-line details** showing covered/uncovered code
- **HTML report** with interactive navigation

### 🔧 Troubleshooting
- **Script not found?** Make sure you're in the project root
- **Coverage too low?** Add tests for uncovered code
- **Report not opening?** Check `./coverage/report/index.html` manually

---

## Overview

The project uses [Coverlet](https://github.com/coverlet-coverage/coverlet) for code coverage collection and [ReportGenerator](https://github.com/danielpalme/ReportGenerator) for generating HTML reports. Coverage is configured to enforce a 90% minimum threshold.

## Coverage Configuration

### What's Covered
- All core FluentUIScaffold packages (Core, AspNetCore, Autofac, Bdd, EntityFrameworkCore, Nunit)
- Integration tests and unit tests

### What's Excluded
- Sample applications (`Samples/` directory)
- SpecFlow test projects
- Generated code (`.g.cs`, `.generated.cs`, `.Designer.cs`)
- Assembly info files
- Program.cs and Startup.cs files
- Bin and obj directories

### Coverage Thresholds
- **Minimum Overall Coverage**: 90%
- **Enforcement**: Build fails if coverage drops below threshold
- **Coverage Types**: Line, branch, and method coverage

## Running Coverage Locally

### Prerequisites
- .NET 8.0 SDK or later
- PowerShell (Windows) or Bash (Linux/macOS)

### Using Scripts

#### Windows (PowerShell)
```powershell
# Run coverage for default framework (net8.0)
.\scripts\coverage.ps1

# Run coverage for specific framework
.\scripts\coverage.ps1 -Framework net7.0

# Run coverage and open report
.\scripts\coverage.ps1 -OpenReport

# Run coverage with debug configuration
.\scripts\coverage.ps1 -Configuration Debug
```

#### Linux/macOS (Bash)
```bash
# Make script executable (first time only)
chmod +x scripts/coverage.sh

# Run coverage for default framework (net8.0)
./scripts/coverage.sh

# Run coverage for specific framework
./scripts/coverage.sh -f net7.0

# Run coverage and open report
./scripts/coverage.sh -o

# Run coverage with debug configuration
./scripts/coverage.sh -c Debug
```

### Manual Commands

If you prefer to run coverage manually:

```bash
# Run tests with coverage
dotnet test Tests/FluentUIScaffold.Tests/FluentUIScaffold.Tests.csproj \
    --configuration Release \
    --framework net8.0 \
    --settings coverlet.runsettings \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage/

# Generate HTML report
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4
reportgenerator \
    -reports:./coverage/**/coverage.cobertura.xml \
    -targetdir:./coverage/report \
    -reporttypes:Html
```

## Viewing Coverage Reports

### HTML Reports
After running coverage, HTML reports are generated in `./coverage/report/`. Open `index.html` in your browser to view:

- Overall coverage percentage
- File-by-file coverage breakdown
- Line-by-line coverage details
- Coverage trends and statistics

### Coverage Files
- **Cobertura XML**: `./coverage/**/coverage.cobertura.xml` - Used by CI/CD
- **HTML Report**: `./coverage/report/` - Human-readable reports
- **Coverage JSON**: `./coverage/**/coverage.json` - Raw coverage data

## CI/CD Integration

### GitHub Actions
The CI workflow automatically:
1. Runs tests with coverage collection
2. Generates HTML reports
3. Checks coverage threshold (90%)
4. Uploads coverage artifacts
5. Fails the build if coverage is below threshold

### Coverage Artifacts
Coverage reports are uploaded as GitHub Actions artifacts:
- Available for download from the Actions tab
- Retained for 30 days
- Includes both Cobertura XML and HTML reports

## Troubleshooting

### Common Issues

#### No Coverage Files Generated
- Ensure `coverlet.collector` package is installed in test projects
- Check that tests are actually running
- Verify the `coverlet.runsettings` file is in the project root

#### Coverage Below Threshold
- Add more tests to uncovered code
- Review exclusion patterns in `coverlet.runsettings`
- Check if new code is properly covered

#### ReportGenerator Not Found
```bash
# Install ReportGenerator globally
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4
```

### Debugging Coverage

#### Verbose Output
```bash
dotnet test --verbosity detailed --settings coverlet.runsettings
```

#### Check Coverage Configuration
```bash
# Validate runsettings file
dotnet test --settings coverlet.runsettings --list-tests
```

## Configuration Files

### coverlet.runsettings
Main configuration file for coverage collection:
- Exclusion patterns
- Coverage thresholds
- Output formats
- Collection settings

### Project Files
Test projects include `coverlet.collector` package:
```xml
<PackageReference Include="coverlet.collector" Version="6.0.2">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

## Best Practices

### For Contributors
1. **Run coverage locally** before submitting PRs
2. **Maintain 90% coverage** for new code
3. **Add tests** for uncovered code paths
4. **Use exclusion patterns** for generated code

### For Maintainers
1. **Monitor coverage trends** in CI
2. **Review coverage reports** for quality issues
3. **Update exclusions** as needed
4. **Adjust thresholds** based on project needs

### Excluding Code from Coverage
When you need to exclude specific code:

```csharp
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class ExcludedClass
{
    // This class will be excluded from coverage
}
```

Or add to `coverlet.runsettings`:
```xml
<ExcludeByFile>**/path/to/exclude/**/*</ExcludeByFile>
```

## Coverage Metrics

The project tracks:
- **Line Coverage**: Percentage of code lines executed
- **Branch Coverage**: Percentage of conditional branches taken
- **Method Coverage**: Percentage of methods called

All metrics must meet the 90% threshold for the build to pass. 