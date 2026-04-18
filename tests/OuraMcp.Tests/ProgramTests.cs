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

    /// <summary>
    /// Starts the server as a subprocess and reads stderr until <paramref name="expectedSubstring"/>
    /// appears, then kills the process. Used for scenarios where the server would otherwise block
    /// (e.g., waiting for an OAuth callback or reading from stdio).
    /// </summary>
    // Path to the OuraMcp assembly that is copied next to the test binaries via the project reference.
    // Running the prebuilt dll directly (rather than `dotnet run`) avoids triggering a NuGet restore
    // when tests override USERPROFILE/HOME — `dotnet run` resolves NuGet caches under those paths,
    // so an overridden USERPROFILE turns each invocation into a slow fresh restore.
    private static readonly string OuraMcpAssemblyPath = Path.Combine(
        AppContext.BaseDirectory, "OuraMcp.dll");

    private static async Task<string> RunUntilStderrContainsAsync(
        string expectedSubstring,
        string arguments = "",
        Dictionary<string, string?>? envOverrides = null,
        int timeoutMs = 60_000)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"exec \"{OuraMcpAssemblyPath}\" {arguments}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.Environment["OURA_CLIENT_ID"] = "test-client-id";
        psi.Environment["OURA_CLIENT_SECRET"] = "test-client-secret";
        if (envOverrides is not null)
        {
            foreach (var (key, value) in envOverrides)
                psi.Environment[key] = value;
        }

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

        var stderrBuffer = new System.Text.StringBuilder();
        var bufferLock = new object();
        var matchTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // Capture stderr via the event-driven API. Process guarantees that all queued
        // ErrorDataReceived callbacks fire (and the final null sentinel arrives) before
        // WaitForExitAsync completes, so we cannot lose buffered output from a process
        // that exits very quickly — unlike polling on process.HasExited + ReadLineAsync.
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            lock (bufferLock)
            {
                stderrBuffer.AppendLine(e.Data);
            }
            if (e.Data.Contains(expectedSubstring, StringComparison.Ordinal))
            {
                matchTcs.TrySetResult(true);
            }
        };
        // Drain stdout to prevent buffer-fill deadlock; we don't inspect it.
        process.OutputDataReceived += (_, _) => { };

        if (!process.Start())
        {
            throw new InvalidOperationException(
                $"Failed to start process '{psi.FileName}' with arguments '{psi.Arguments}'.");
        }

        using (process)
        {
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            var exitTask = process.WaitForExitAsync();
            var timeoutTask = Task.Delay(timeoutMs);

            try
            {
                var completed = await Task.WhenAny(matchTcs.Task, exitTask, timeoutTask);

                if (completed == matchTcs.Task)
                {
                    lock (bufferLock)
                    {
                        return stderrBuffer.ToString();
                    }
                }

                if (completed == exitTask)
                {
                    // Process exited without printing the substring on a line we saw.
                    // WaitForExitAsync (with EnableRaisingEvents = true and after BeginErrorReadLine)
                    // ensures all stderr callbacks have flushed before returning, so the buffer
                    // is the full captured stderr.
                    string captured;
                    lock (bufferLock)
                    {
                        captured = stderrBuffer.ToString();
                    }
                    if (captured.Contains(expectedSubstring, StringComparison.Ordinal))
                    {
                        return captured;
                    }
                    throw new InvalidOperationException(
                        $"Process exited (code {process.ExitCode}) without stderr containing '{expectedSubstring}'. Captured stderr:\n{captured}");
                }

                // Timed out.
                string capturedAtTimeout;
                lock (bufferLock)
                {
                    capturedAtTimeout = stderrBuffer.ToString();
                }
                throw new InvalidOperationException(
                    $"Timed out after {timeoutMs}ms waiting for stderr to contain '{expectedSubstring}'. Captured stderr:\n{capturedAtTimeout}");
            }
            finally
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch { /* ignore */ }
                }
            }
        }
    }

    [Fact]
    public async Task Startup_WithNoTokens_TriggersAutoLoginFlow()
    {
        // Use a throw-away HOME so ~/.oura-mcp/tokens.json does not exist regardless of
        // what the developer machine currently has. FileTokenStore reads from
        // Environment.SpecialFolder.UserProfile, which on Unix is controlled by $HOME.
        var tempTokenDir = Directory.CreateTempSubdirectory("oura-mcp-auto-login-").FullName;
        try
        {
            var stderr = await RunUntilStderrContainsAsync(
                expectedSubstring: "No Oura tokens found",
                envOverrides: new()
                {
                    ["OURA_CLIENT_ID"] = "test-client-id",
                    ["OURA_CLIENT_SECRET"] = "test-client-secret",
                    // Point the token store at an empty isolated directory so the auto-login
                    // path is exercised. Overriding HOME/USERPROFILE instead would destabilise
                    // the .NET host on Windows by breaking DataProtection key resolution.
                    ["OURA_MCP_TOKEN_DIR"] = tempTokenDir,
                    // Suppress real browser launch and HttpListener bind during the test.
                    ["OURA_MCP_DISABLE_BROWSER"] = "1"
                });

            stderr.Should().Contain("No Oura tokens found");
        }
        finally
        {
            try { Directory.Delete(tempTokenDir, recursive: true); } catch { /* best effort */ }
        }
    }

    [Fact]
    public async Task Startup_WithNoLoginFlag_SkipsAutoLogin()
    {
        // With --no-login the server must NOT prompt for login, even when no tokens exist.
        // It should proceed straight to starting the stdio MCP transport, which blocks reading
        // stdin — we kill it after a short wait and assert the auto-login message never appeared.
        var tempTokenDir = Directory.CreateTempSubdirectory("oura-mcp-no-login-").FullName;
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{ProjectPath}\" -- --no-login",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            psi.Environment["OURA_CLIENT_ID"] = "test-client-id";
            psi.Environment["OURA_CLIENT_SECRET"] = "test-client-secret";
            psi.Environment["OURA_MCP_TOKEN_DIR"] = tempTokenDir;

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException(
                    $"Failed to start process '{psi.FileName}' with arguments '{psi.Arguments}'.");
            using (process)
            {
                var stderrTask = process.StandardError.ReadToEndAsync();
                _ = Task.Run(async () =>
                {
                    try { await process.StandardOutput.ReadToEndAsync(); } catch { }
                });

                // Give the server time to start up and reach the stdio read loop.
                await Task.Delay(8_000);

                // Close stdin so the stdio transport exits cleanly.
                try { process.StandardInput.Close(); } catch { }

                using var cts = new CancellationTokenSource(10_000);
                try { await process.WaitForExitAsync(cts.Token); }
                catch (OperationCanceledException) { process.Kill(entireProcessTree: true); }

                var stderr = await stderrTask;
                stderr.Should().NotContain("No Oura tokens found");
                stderr.Should().NotContain("Opening browser for Oura authorization");
            }
        }
        finally
        {
            try { Directory.Delete(tempTokenDir, recursive: true); } catch { /* best effort */ }
        }
    }
}
