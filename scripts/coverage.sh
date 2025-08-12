#!/bin/bash

# Default values
FRAMEWORK="net8.0"
CONFIGURATION="Release"
OPEN_REPORT=false
INCLUDE_ASSEMBLIES="FluentUIScaffold.Core"
EXCLUDE_ASSEMBLIES="FluentUIScaffold.Playwright"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -f|--framework)
            FRAMEWORK="$2"
            shift 2
            ;;
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -o|--open)
            OPEN_REPORT=true
            shift
            ;;
        --include-assemblies)
            INCLUDE_ASSEMBLIES="$2"
            shift 2
            ;;
        --exclude-assemblies)
            EXCLUDE_ASSEMBLIES="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -f, --framework FRAMEWORK        Target framework (default: net8.0)"
            echo "  -c, --configuration CONFIG        Build configuration (default: Release)"
            echo "  -o, --open                       Open coverage report after generation"
            echo "      --include-assemblies LIST     Semicolon- or space-separated assembly names to include (default: FluentUIScaffold.Core)"
            echo "      --exclude-assemblies LIST     Semicolon- or space-separated assembly names to exclude (default: FluentUIScaffold.Playwright)"
            echo "  -h, --help                       Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "Running coverage for framework: $FRAMEWORK"

# Clean previous coverage results
if [ -d "./coverage" ]; then
    rm -rf "./coverage"
fi

# Run tests with coverage (Core)
echo "Running tests with coverage (Core)..."
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj \
    --configuration "$CONFIGURATION" \
    --framework "$FRAMEWORK" \
    --settings ./coverlet.runsettings \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage/ \
    --verbosity normal

if [ $? -ne 0 ]; then
    echo "❌ Core tests failed!"
    exit 1
fi

# Run tests with coverage (Playwright) - best effort
if [ -f tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj ]; then
  echo "Running tests with coverage (Playwright)..."
  dotnet test tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj \
      --configuration "$CONFIGURATION" \
      --framework "$FRAMEWORK" \
      --settings ./coverlet.runsettings \
      --collect:"XPlat Code Coverage" \
      --results-directory ./coverage/ \
      --verbosity normal || true
fi

# Ensure reportgenerator is available
echo "Ensuring reportgenerator is installed..."
dotnet tool update --global dotnet-reportgenerator-globaltool --version 5.2.4 >/dev/null 2>&1 || \
  dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4 >/dev/null 2>&1

# Build assembly filters (+include;-exclude) separated by semicolons
build_filters() {
  local includes="$1"; shift
  local excludes="$1"; shift
  local arr=()
  # split on both spaces and semicolons
  for token in $includes; do
    token=${token//;/ } # replace ; with space then loop will split
  done
  for inc in ${includes//;/ } ; do
    [ -n "$inc" ] && arr+=("+$inc")
  done
  for exc in ${excludes//;/ } ; do
    [ -n "$exc" ] && arr+=("-$exc")
  done
  local IFS=';'
  echo "${arr[*]}"
}

ASSEMBLY_FILTERS=$(build_filters "$INCLUDE_ASSEMBLIES" "$EXCLUDE_ASSEMBLIES")

# Generate HTML/TextSummary report with filters
echo "Generating HTML coverage report..."
reportgenerator \
  -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage/report \
  -reporttypes:'Html;TextSummary' \
  -classfilters:'+FluentUIScaffold.Core.Configuration.Launchers.*;+FluentUIScaffold.Core.Configuration.ServerConfiguration*;+FluentUIScaffold.Core.Configuration.*ServerConfigurationBuilder' \
  -assemblyfilters:"$ASSEMBLY_FILTERS"

# Check coverage threshold using Summary.txt
echo "Checking coverage threshold..."
SUMMARY=./coverage/report/Summary.txt
if [ ! -f "$SUMMARY" ]; then
  echo "❌ Coverage summary not found at $SUMMARY"
  exit 1
fi
LINE=$(grep -E '^\s*Line coverage:' "$SUMMARY" | head -1)
PCT=$(echo "$LINE" | sed -E 's/.*Line coverage:\s*([0-9]+(\.[0-9]+)?)%.*/\1/')
if [ -z "$PCT" ]; then
  echo "❌ Could not parse coverage percentage"
  exit 1
fi
echo "Coverage: ${PCT}%"
# Convert to integer for threshold comparison
PCT_INT=$(printf '%.0f' "${PCT}")
if [ "$PCT_INT" -lt 70 ]; then
  echo "❌ Coverage is below 70% threshold (${PCT}%)"
  exit 1
else
  echo "✅ Coverage meets 70% threshold (${PCT}%)"
fi

# Open report if requested
if [ "$OPEN_REPORT" = true ]; then
    REPORT_PATH="./coverage/report/index.html"
    if command -v xdg-open &> /dev/null; then
        xdg-open "$REPORT_PATH"
    elif command -v open &> /dev/null; then
        open "$REPORT_PATH"
    else
        echo "Coverage report available at: $REPORT_PATH"
    fi
else
    echo "Coverage report available at: ./coverage/report/index.html"
fi

echo "Coverage analysis complete!" 