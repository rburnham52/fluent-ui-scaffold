// Set environment variable to allow unsecured transport for testing
Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var builder = DistributedApplication.CreateBuilder(args);

// Add the main SampleApp as a project reference
var sampleApp = builder.AddProject<Projects.SampleApp>("sampleapp");

// Build the application
var app = builder.Build();

// Run the application
app.Run();
