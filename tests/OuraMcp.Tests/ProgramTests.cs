using System.Diagnostics;
using FluentAssertions;

namespace OuraMcp.Tests;

public class ProgramTests
{
    private static readonly string ProjectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "OuraMcp"));

    /// <summary>
    /// Runs the OuraMcp project with the given arguments and an empty environment
    /// (no OURA_CLIENT_ID / OURA_CLIENT_SECRET) so the friendly validation fires.
    /// </summary>
    private static async Task<(int ExitCode, string Stderr)> RunAsync(
        string arguments = "", int timeoutMs = 15_000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --project \"{ProjectPath}\" -- {arguments}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Clear OAuth env vars so the validation triggers
        psi.Environment["OURA_CLIENT_ID"] = "";
        psi.Environment["OURA_CLIENT_SECRET"] = "";

        using var process = Process.Start(psi)!;
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(new CancellationTokenSource(timeoutMs).Token);

        return (process.ExitCode, stderr);
    }

    [Fact]
    public async Task MissingBothEnvVars_PrintsFriendlyError_AndExitsNonZero()
    {
        var (exitCode, stderr) = await RunAsync();

        exitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_ID");
        stderr.Should().Contain("OURA_CLIENT_SECRET");
        stderr.Should().Contain("export");
        stderr.Should().NotContain("Unhandled exception");
        stderr.Should().NotContain("System.InvalidOperationException");
    }

    [Fact]
    public async Task MissingClientId_PrintsFriendlyError()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --project \"{ProjectPath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.Environment["OURA_CLIENT_ID"] = "";
        psi.Environment["OURA_CLIENT_SECRET"] = "some-secret";

        using var process = Process.Start(psi)!;
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(new CancellationTokenSource(15_000).Token);

        process.ExitCode.Should().NotBe(0);
        stderr.Should().Contain("OURA_CLIENT_ID");
        stderr.Should().NotContain("OURA_CLIENT_SECRET", "only the missing var should be listed");
        stderr.Should().NotContain("Unhandled exception");
    }

    [Fact]
    public async Task MissingClientSecret_PrintsFriendlyError()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --no-build --project \"{ProjectPath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.Environment["OURA_CLIENT_ID"] = "some-id";
        psi.Environment["OURA_CLIENT_SECRET"] = "";

        using var process = Process.Start(psi)!;
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(new CancellationTokenSource(15_000).Token);

        process.ExitCode.Should().NotBe(0);
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
