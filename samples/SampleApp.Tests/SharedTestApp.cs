using System;

using FluentUIScaffold.Core;


/// <summary>
/// Shared accessor for the test app scaffold instance.
/// Set by TestAssemblyHooks in each test project (standard or Aspire).
/// Tests reference this instead of a specific TestAssemblyHooks class.
/// </summary>
public static class SharedTestApp
{
    private static AppScaffold<WebApp>? _app;

    public static AppScaffold<WebApp> App
    {
        get => _app ?? throw new InvalidOperationException(
            "App not initialized. Ensure AssemblyInitialize has run.");
        set => _app = value;
    }
}

