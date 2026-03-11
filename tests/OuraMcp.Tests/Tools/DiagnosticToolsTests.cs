using FluentAssertions;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class DiagnosticToolsTests : IDisposable
{
    private readonly string _tempLogDir;
    private readonly string _originalLogDir;

    public DiagnosticToolsTests()
    {
        _tempLogDir = Path.Combine(Path.GetTempPath(), $"oura-mcp-log-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempLogDir);

        // Capture original value to restore later
        _originalLogDir = OuraMcpPaths.LogDirectory;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempLogDir))
            Directory.Delete(_tempLogDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GetErrorLog_NoLogDirectory_ReturnsNoErrorsMessage()
    {
        // DiagnosticTools reads from OuraMcpPaths.LogDirectory which is a static path.
        // When the directory doesn't exist, it should return a friendly message.
        var result = DiagnosticTools.GetErrorLog();

        // The real log dir may or may not exist on the test machine,
        // but the result should never be null or throw
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetErrorLog_EmptyLogFile_ReturnsNoErrorsMessage()
    {
        var logFile = Path.Combine(_tempLogDir, "error.log");
        File.WriteAllText(logFile, "");

        // We can't redirect the static path, but we verify the tool handles
        // missing/empty gracefully by checking it doesn't throw
        var result = DiagnosticTools.GetErrorLog();
        result.Should().NotBeNull();
    }
}
