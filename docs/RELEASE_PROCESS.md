# Release Process

This document describes the release process for FluentUIScaffold beta releases.

## Version Numbering

- **Current Beta Version**: `0.3.0-beta`
- **Milestone Alignment**: Version matches completed milestones (3 milestones completed)
- **Format**: `0.{milestone-count}.0-beta`
- **Auto-Sync**: Assembly and package versions automatically sync with Git tag version

## Release Process

### Creating a Beta Release

1. **Create GitHub Release**
   - Go to your repository on GitHub
   - Click "Releases" in the right sidebar
   - Click "Create a new release"
   - Tag: `v0.3.0-beta` (this becomes the package version)
   - Release title: `FluentUIScaffold v0.3.0-beta`
   - Description: Add release notes and changes
   - ✅ Mark as pre-release
   - Click "Publish release"

2. **Automated Pipeline**
   - Build and test all frameworks
   - Create NuGet packages
   - Require manual approval
   - Publish to NuGet (after approval)

## Manual Approval Process

1. **Build and Test**: Automated pipeline runs all tests
2. **Package Creation**: NuGet packages are created
3. **Manual Approval**: Required before publishing to NuGet
4. **NuGet Publishing**: Only after approval

### Approval Steps

1. Navigate to the GitHub Actions run
2. Find the "Manual Approval for Release" job
3. Click "Review deployments"
4. Review the build artifacts and test results
5. Click "Approve and deploy"

## Package Information

### FluentUIScaffold.Core
- **Package ID**: `FluentUIScaffold.Core`
- **Description**: Core framework for E2E testing
- **Target Frameworks**: net6.0, net7.0, net8.0

### FluentUIScaffold.Playwright
- **Package ID**: `FluentUIScaffold.Playwright`
- **Description**: Playwright integration for FluentUIScaffold
- **Target Frameworks**: net6.0, net7.0, net8.0
- **Dependencies**: FluentUIScaffold.Core

## Pre-release Checklist

Before creating a release:

- [ ] All tests pass
- [ ] Code coverage meets requirements
- [ ] Security analysis passes
- [ ] Code quality checks pass
- [ ] Documentation is updated
- [ ] Version numbers are updated
- [ ] Release notes are prepared

## Next Steps

After the beta release:

1. Monitor usage and feedback
2. Address any issues reported
3. Plan the next milestone
4. Update version for next release

## Environment Setup

To enable manual approvals:

1. Go to Settings > Environments in your GitHub repository
2. Create environment named `release-approval`
3. Add protection rules as needed
4. Configure required reviewers if desired 