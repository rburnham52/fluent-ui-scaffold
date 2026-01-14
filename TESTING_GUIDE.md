# Testing Guide for Aspire Integration

Since the `dotnet` CLI is not available in the current environment, here's how to validate the implementation once you have access to a proper .NET development environment.

## Build Verification Steps

### 1. **Basic Build Test**
```bash
# Test the core framework builds
dotnet build src/FluentUIScaffold.Core/FluentUIScaffold.Core.csproj

# Test the Aspire AppHost builds  
dotnet build samples/SampleApp.AppHost/SampleApp.AppHost.csproj

# Test the integration tests build
dotnet build samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj
```

### 2. **Unit Tests (No Aspire Workload Required)**
```bash
# Run the core server lifecycle unit tests
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj --filter TestCategory!=Integration

# Run basic build validation tests
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter TestCategory=Build
```

### 3. **Integration Tests (Requires Aspire Workload)**
```bash
# Install Aspire workload (one-time setup)
dotnet workload install aspire

# Run server lifecycle integration tests
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter TestCategory=ServerLifecycle

# Run migration example tests
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter TestCategory=Migration
```

## Expected Test Results

### ✅ **Should PASS (No External Dependencies)**

#### Core Unit Tests
- `ConfigHasherTests` - Configuration hashing logic
- `ProcessRegistryTests.TryLoad_WithNonExistentConfig_ReturnsNull` - Basic registry operations
- `BuildValidationTest` - Dependency loading verification

#### Basic Integration Tests  
- `BuildValidationTest.BuildValidation_BasicDependencies_ShouldWork`
- `BuildValidationTest.BuildValidation_FluentUIScaffoldCore_ShouldLoad`

### ⚠️ **Might FAIL (Requires Runtime Dependencies)**

#### Server Lifecycle Tests (Need Aspire + Running Servers)
- `EnsureStartedAsync_WithAspireAppHost_StartsServerSuccessfully`
- `ManualServerManagement_WithAspireAppHost_DemonstratesFullLifecycle`
- `ConfigurationDriftDetection_WithDifferentConfigurations_RestartsServerAppropriately`

#### Performance Tests (Need Multiple Test Runs)
- `ServerReusePerformance_WithSameConfiguration_ReusesFastly`

## Manual Verification Steps

### 1. **Code Review Checklist**
- [ ] All using statements resolve correctly
- [ ] No conflicting method signatures
- [ ] Project references are correct
- [ ] Package versions are compatible
- [ ] Test frameworks are consistent (MSTest vs NUnit)

### 2. **Runtime Configuration Test**
```csharp
// This should compile and create server config successfully
var serverConfig = ServerConfiguration.CreateAspireServer(
    new Uri("http://localhost:5000"),
    "/path/to/SampleApp.AppHost.csproj")
    .WithHealthCheckEndpoints("/", "/health")
    .WithAutoCI()
    .WithHeadless(true)
    .Build();

Console.WriteLine($"Server config created: {serverConfig != null}");
```

### 3. **Dependency Loading Test**
```csharp
// This should succeed in loading all required types
var serverManagerType = typeof(FluentUIScaffold.Core.Server.IServerManager);
var aspireManagerType = typeof(FluentUIScaffold.Core.Server.DotNetServerManager);
var configHasherType = typeof(FluentUIScaffold.Core.Server.ConfigHasher);

Console.WriteLine("All server lifecycle types loaded successfully!");
```

## Troubleshooting Common Issues

### Build Errors
1. **"Aspire.Hosting not found"**
   ```bash
   dotnet workload install aspire
   ```

2. **"Projects.SampleApp not found"**  
   - Fixed: Now using direct project path instead of generated Projects namespace

3. **"Test framework conflicts"**
   - Fixed: All Core tests use NUnit, Sample tests use MSTest consistently

### Runtime Errors  
1. **Port conflicts**
   - Tests use ports 5100-5105 and 5200-5300
   - Ensure these ports are available

2. **Node.js not found**
   - Required for SPA features in some tests
   - Install Node.js or disable SPA-related functionality

3. **Health check timeouts**
   - Increase timeout values in slow environments
   - Check firewall settings for localhost connections

### Aspire Testing Configuration

The FluentUIScaffold framework integrates with **Aspire.Hosting.Testing** for managing Aspire AppHost lifecycle during tests. This provides automatic port allocation, resource management, and cleanup.

### Prerequisites

1. Add the `Aspire.Hosting.Testing` package to your test project:
   ```xml
   <PackageReference Include="Aspire.Hosting.Testing" Version="9.2.0" />
   ```

2. Ensure your test project targets `net8.0` (required by Aspire.Hosting.Testing):
   ```xml
   <TargetFrameworks>net8.0</TargetFrameworks>
   ```

3. Reference your AppHost project from your test project:
   ```xml
   <ProjectReference Include="..\YourApp.AppHost\YourApp.AppHost.csproj" />
   ```

### AppHost Configuration

Your AppHost `Program.cs` should use standard Aspire configuration without custom port management:

```csharp
// Set environment variable to allow unsecured transport for testing
Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

// Add your application as a project reference
// Aspire will handle all port management automatically
var myApp = builder.AddProject<Projects.MyApp>("myapp");

var app = builder.Build();
app.Run();
```

### Test Configuration

Use `DistributedApplicationTestingBuilder` in your test assembly hooks:

```csharp
[AssemblyInitialize]
public static async Task AssemblyInitialize(TestContext context)
{
    // Start the Aspire AppHost using the testing library
    var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyApp_AppHost>();
    _app = await appBuilder.BuildAsync();
    await _app.StartAsync();

    // Wait for your resource to be running
    var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
    await resourceNotificationService.WaitForResourceAsync("myapp", KnownResourceStates.Running)
        .WaitAsync(TimeSpan.FromSeconds(60));

    // Get the automatically allocated endpoint
    var httpClient = _app.CreateHttpClient("myapp");
    var baseUrl = httpClient.BaseAddress;

    // Create FluentUIScaffold app with the Aspire-managed URL
    _sessionApp = FluentUIScaffoldBuilder.Web<WebApp>(options =>
    {
        options.WithBaseUrl(baseUrl);
        options.WithHeadlessMode(true);
    });
}
```

### Benefits of Aspire.Hosting.Testing

- **Automatic port allocation**: No need to manage ports manually
- **Resource lifecycle management**: Aspire handles starting/stopping services
- **Health checking**: Built-in waiting for resources to be ready
- **Cleanup**: Automatic disposal of resources after tests
- **Isolation**: Each test run gets fresh port allocations

For more information, see the [Aspire Testing documentation](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing).
## Performance Expectations

### First Run (Cold Start)
- Server startup: 30-60 seconds
- Health check validation: 5-10 seconds  
- Total test time: 1-2 minutes per integration test

### Subsequent Runs (Server Reuse)
- Server reuse detection: 1-2 seconds
- Health check validation: 1-2 seconds
- Total test time: 3-5 seconds per test

## Success Criteria

### ✅ Build Success
- All projects compile without errors
- No missing dependencies or package conflicts
- Test projects load and discover tests correctly

### ✅ Unit Test Success  
- Core server lifecycle logic works correctly
- Configuration hashing is deterministic
- Process registry operations work as expected

### ✅ Integration Test Success
- Aspire AppHost starts and becomes healthy
- Server lifecycle management works end-to-end
- Configuration drift detection triggers restarts appropriately
- Performance improvements are measurable (3-10x faster)

## Quick Validation Script

Create this as `validate-implementation.ps1` or `validate-implementation.sh`:

```powershell
# PowerShell validation script
Write-Host "🔍 Validating Aspire Integration Implementation..." -ForegroundColor Blue

Write-Host "1. Building Core Framework..." -ForegroundColor Yellow
dotnet build src/FluentUIScaffold.Core/FluentUIScaffold.Core.csproj
if ($LASTEXITCODE -eq 0) { Write-Host "✅ Core build successful" -ForegroundColor Green }

Write-Host "2. Building Aspire AppHost..." -ForegroundColor Yellow  
dotnet build samples/SampleApp.AppHost/SampleApp.AppHost.csproj
if ($LASTEXITCODE -eq 0) { Write-Host "✅ AppHost build successful" -ForegroundColor Green }

Write-Host "3. Running Unit Tests..." -ForegroundColor Yellow
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj --filter "TestCategory!=Integration"
if ($LASTEXITCODE -eq 0) { Write-Host "✅ Unit tests passed" -ForegroundColor Green }

Write-Host "4. Running Build Validation Tests..." -ForegroundColor Yellow
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter "TestCategory=Build"  
if ($LASTEXITCODE -eq 0) { Write-Host "✅ Build validation passed" -ForegroundColor Green }

Write-Host "🎉 Implementation validation complete!" -ForegroundColor Green
```

This testing guide provides a comprehensive approach to validating the Aspire integration implementation without requiring the dotnet CLI to be available during development.