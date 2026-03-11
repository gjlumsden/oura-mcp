using FluentAssertions;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class DiagnosticToolsTests : IDisposable
{
    private readonly string _tempLogDir;

    public DiagnosticToolsTests()
    {
        _tempLogDir = Path.Combine(Path.GetTempPath(), $"oura-mcp-log-test-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempLogDir))
            Directory.Delete(_tempLogDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ReadErrorLog_NoLogDirectory_ReturnsNoErrorsMessage()
    {
        var result = DiagnosticTools.ReadErrorLog(_tempLogDir);

        result.Should().Contain("No error log directory found");
    }

    [Fact]
    public void ReadErrorLog_EmptyLogFile_ReturnsEmptyMessage()
    {
        Directory.CreateDirectory(_tempLogDir);
        File.WriteAllText(Path.Combine(_tempLogDir, "error.log"), "");

        var result = DiagnosticTools.ReadErrorLog(_tempLogDir);

        result.Should().Contain("Error log is empty");
    }

    [Fact]
    public void ReadErrorLog_WithEntries_ReturnsTailLines()
    {
        Directory.CreateDirectory(_tempLogDir);
        var lines = Enumerable.Range(1, 200).Select(i => $"Line {i}").ToArray();
        File.WriteAllLines(Path.Combine(_tempLogDir, "error.log"), lines);

        var result = DiagnosticTools.ReadErrorLog(_tempLogDir, tailLines: 5);

        result.Should().Contain("Line 200");
        result.Should().Contain("Line 196");
        result.Should().NotContain("Line 1\n");
    }

    [Fact]
    public void ReadErrorLog_NoLogFiles_ReturnsNoFilesMessage()
    {
        Directory.CreateDirectory(_tempLogDir);

        var result = DiagnosticTools.ReadErrorLog(_tempLogDir);

        result.Should().Contain("No error log files found");
    }
}
