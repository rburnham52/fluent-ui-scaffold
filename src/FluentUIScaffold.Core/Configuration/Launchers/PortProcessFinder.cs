namespace FluentUIScaffold.Core.Configuration.Launchers;

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class PortProcessFinder
{
    public static async Task<string> FindProcessesOnPortAsync(int port)
    {
        try
        {
            var (fileName, arguments) = GetNetstatCommand();

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return string.Empty;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}. Error: {error}");
            }

            // Filter the output in C# for cross-platform consistency
            return FilterOutputByPort(output, port);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to find processes on port {port}", ex);
        }
    }

    private static (string fileName, string arguments) GetNetstatCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("netstat", "-ano");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Try netstat first, fall back to ss if needed
            return ("netstat", "-tulpn");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ("netstat", "-anv");
        }
        else
        {
            // Default to Linux-style for unknown platforms
            return ("netstat", "-tulpn");
        }
    }

    private static string FilterOutputByPort(string output, int port)
    {
        if (string.IsNullOrEmpty(output))
            return string.Empty;

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var portTokens = new[] { $":{port}", $".{port}" }; // support Linux/Windows ":port" and macOS ".port" formats

        var matchingLines = lines
            .Where(line => portTokens.Any(tok => line.Contains(tok)))
            .Where(line =>
            {
                // More precise matching - ensure it's actually the port number
                foreach (var tok in portTokens)
                {
                    if (line.Contains($"{tok} ") || line.Contains($"{tok}\t") || line.EndsWith(tok))
                    {
                        return true;
                    }
                }
                return false;
            })
            .ToList();

        return string.Join(Environment.NewLine, matchingLines);
    }

    // Alternative method using 'ss' command on Linux (more modern)
    public static async Task<string> FindProcessesOnPortLinuxSsAsync(int port)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException("This method is only supported on Linux");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "ss",
            Arguments = "-tulpn",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
            return string.Empty;

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return FilterOutputByPort(output, port);
    }
}
