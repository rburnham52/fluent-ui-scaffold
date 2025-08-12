#!/bin/bash

# Default values
FRAMEWORK="net8.0"
CONFIGURATION="Release"
OPEN_REPORT=false

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
        -h|--help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  -f, --framework FRAMEWORK    Target framework (default: net8.0)"
            echo "  -c, --configuration CONFIG    Build configuration (default: Release)"
            echo "  -o, --open                   Open coverage report after generation"
            echo "  -h, --help                   Show this help message"
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

# Run tests with coverage
echo "Running tests with coverage..."
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj \
    --configuration $CONFIGURATION \
    --framework $FRAMEWORK \
    --settings coverlet.runsettings \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage/ \
    --verbosity normal

if [ $? -ne 0 ]; then
    echo "❌ Tests failed!"
    exit 1
fi

# Install reportgenerator if not available
echo "Installing reportgenerator..."
dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.2.4

# Generate HTML report
echo "Generating HTML coverage report..."
reportgenerator \
    -reports:./coverage/**/coverage.cobertura.xml \
    -targetdir:./coverage/report \
    -reporttypes:'Html;TextSummary' \
    -assemblyfilters:+FluentUIScaffold.Core;-FluentUIScaffold.Playwright \
    -classfilters:+FluentUIScaffold.Core.Configuration.Launchers.*;+FluentUIScaffold.Core.Configuration.ServerConfiguration*;+FluentUIScaffold.Core.Configuration.*ServerConfigurationBuilder

# Check coverage threshold
echo "Checking coverage threshold..."
COVERAGE_FILES=$(find ./coverage -name "coverage.cobertura.xml" -type f)
if [ -z "$COVERAGE_FILES" ]; then
    echo "❌ No coverage files found!"
    exit 1
fi

COVERAGE_FILE=$(echo "$COVERAGE_FILES" | head -1)
LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$COVERAGE_FILE" | head -1 | sed 's/line-rate="//' | sed 's/"//')
COVERAGE_PERCENT=$(echo "$LINE_RATE * 100" | bc -l | cut -d. -f1)

echo "Coverage: ${COVERAGE_PERCENT}%"

if [ "$COVERAGE_PERCENT" -lt 90 ]; then
    echo "❌ Coverage is below 90% threshold (${COVERAGE_PERCENT}%)"
    exit 1
else
    echo "✅ Coverage meets 90% threshold (${COVERAGE_PERCENT}%)"
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