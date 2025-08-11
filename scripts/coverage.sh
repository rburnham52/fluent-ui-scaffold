#!/usr/bin/env bash
set -euo pipefail

# Defaults
FRAMEWORK="net8.0"
CONFIGURATION="Release"
OPEN_REPORT=false

usage() {
  echo "Usage: $0 [-f <framework>] [-c <configuration>] [-o]" >&2
  echo "  -f    Target framework (default: net8.0)" >&2
  echo "  -c    Build configuration (default: Release)" >&2
  echo "  -o    Open HTML report after generation" >&2
}

while getopts ":f:c:oh" opt; do
  case $opt in
    f) FRAMEWORK="$OPTARG" ;;
    c) CONFIGURATION="$OPTARG" ;;
    o) OPEN_REPORT=true ;;
    h) usage; exit 0 ;;
    :) echo "Option -$OPTARG requires an argument" >&2; usage; exit 1 ;;
    \?) echo "Invalid option -$OPTARG" >&2; usage; exit 1 ;;
  esac
done

COVERAGE_DIR="./coverage"
REPORT_DIR="$COVERAGE_DIR/report"
RUNSETTINGS="coverlet.runsettings"

# Ensure reportgenerator is available
if ! command -v reportgenerator >/dev/null 2>&1; then
  echo "Installing reportgenerator..."
  dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4 >/dev/null
  export PATH="$PATH:$HOME/.dotnet/tools"
fi

# Clean previous coverage
rm -rf "$COVERAGE_DIR" || true
mkdir -p "$COVERAGE_DIR"

# Run tests with coverage for core and playwright tests (if present)
set -x
if [ -f tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj ]; then
  dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj \
    --no-build \
    --verbosity normal \
    --configuration "$CONFIGURATION" \
    --framework "$FRAMEWORK" \
    --settings "$RUNSETTINGS" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$COVERAGE_DIR" \
    --logger trx
fi

if [ -f tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj ]; then
  dotnet test tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj \
    --no-build \
    --verbosity normal \
    --configuration "$CONFIGURATION" \
    --framework "$FRAMEWORK" \
    --settings "$RUNSETTINGS" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$COVERAGE_DIR" \
    --logger trx || true
fi
set +x

# Generate HTML report
reportgenerator \
  -reports:"$COVERAGE_DIR"/**/coverage.cobertura.xml \
  -targetdir:"$REPORT_DIR" \
  -reporttypes:'Html;TextSummary'

# Threshold check (90%) using first cobertura file
COVERAGE_VALUE=$(grep -o 'line-rate="[^"]*"' "$COVERAGE_DIR"/**/coverage.cobertura.xml | head -1 | sed 's/line-rate="//' | sed 's/"//')
if [ -z "$COVERAGE_VALUE" ]; then
  echo "Could not determine coverage from Cobertura XML" >&2
  exit 1
fi
COVERAGE_PERCENT=$(python3 - <<EOF
v=$COVERAGE_VALUE
print(int(float(v)*100))
EOF
)

echo "Coverage: ${COVERAGE_PERCENT}%"
if [ "$COVERAGE_PERCENT" -lt 90 ]; then
  echo "❌ Coverage is below 90% threshold (${COVERAGE_PERCENT}%)"
  exit 1
else
  echo "✅ Coverage meets 90% threshold (${COVERAGE_PERCENT}%)"
fi

echo "HTML report: $REPORT_DIR/index.html"

# Optionally open report
if [ "$OPEN_REPORT" = true ]; then
  if command -v xdg-open >/dev/null 2>&1; then
    xdg-open "$REPORT_DIR/index.html" || true
  elif command -v open >/dev/null 2>&1; then
    open "$REPORT_DIR/index.html" || true
  else
    echo "Open the report manually: $REPORT_DIR/index.html"
  fi
fi