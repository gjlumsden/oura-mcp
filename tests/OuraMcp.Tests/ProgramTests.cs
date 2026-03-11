using System.Diagnostics;
using FluentAssertions;

namespace OuraMcp.Tests;

public class ProgramTests
{
    private static readonly string ProjectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "OuraMcp"));

    /// <summary>
    /// Runs the OuraMcp project as a subprocess and captures stderr.
    /// Uses <c>dotnet run</c> (with build) to avoid Debug/Release mismatch in CI.
    /// Drains stdout concurrently to prevent deadlocks and kills the process on timeout.
    /// </summary>
    private static async Task<(int ExitCode, string Stderr)> RunAsync(
        string arguments = "",
        Dictionary<string, string?>? envOverrides = null,
        int timeoutMs = 30_000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{ProjectPath}\" -- {arguments}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Apply env var overrides (default: clear both OAuth vars)
        psi.Environment["OURA_CLIENT_ID"] = "";
        psi.Environment["OURA_CLIENT_SECRET"] = "";
        if (envOverrides is not null)
        {
            foreach (var (key, value) in envOverrides)
                psi.Environment[key] = value;
        }

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException(
                $"Failed to start process '{psi.FileName}' with arguments '{psi.Arguments}'.");

        using (process)
        {
            // Drain stdout and stderr concurrently to prevent deadlocks
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException($"Process did not exit within {timeoutMs}ms");
            }

            var stderr = await stderrTask;
            await stdoutTask;
            return (process.ExitCode, stderr);
        }
    }

    [Fact]
    public async Task MissingBothEnvVars_PrintsFriendlyError_AndExitsNonZero()
    {
        var (exitCode, stderr) = await RunAsync();

        exitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_ID");
        stderr.Should().Contain("OURA_CLIENT_SECRET");
        stderr.Should().NotContain("Unhandled exception");
        stderr.Should().NotContain("System.InvalidOperationException");
    }

    [Fact]
    public async Task MissingClientId_PrintsFriendlyError()
    {
        var (exitCode, stderr) = await RunAsync(
            envOverrides: new() { ["OURA_CLIENT_SECRET"] = "some-secret" });

        exitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_ID");
        stderr.Should().NotContain("OURA_CLIENT_SECRET", "only the missing var should be listed");
        stderr.Should().NotContain("Unhandled exception");
    }

    [Fact]
    public async Task MissingClientSecret_PrintsFriendlyError()
    {
        var (exitCode, stderr) = await RunAsync(
            envOverrides: new() { ["OURA_CLIENT_ID"] = "some-id" });

        exitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_SECRET");
        stderr.Should().NotContain("OURA_CLIENT_ID", "only the missing var should be listed");
        stderr.Should().NotContain("Unhandled exception");
    }

    [Fact]
    public async Task LoginWithMissingEnvVars_PrintsFriendlyError()
    {
        var (exitCode, stderr) = await RunAsync("login");

        exitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_ID");
        stderr.Should().NotContain("Unhandled exception");
    }
}
