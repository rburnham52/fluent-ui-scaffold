var builder = DistributedApplication.CreateBuilder(args);

// Add the main SampleApp as a project reference with a fixed port for testing
var sampleApp = builder.AddProject("sampleapp", "../SampleApp/SampleApp.csproj")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DOTNET_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:5204"); // Use fixed port for testing

// Build the application
var app = builder.Build();

// Run the application
app.Run();
