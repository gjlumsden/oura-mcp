using FluentAssertions;
using ModelContextProtocol;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class DateHelperTests
{
    [Fact]
    public void ParseDate_NullInput_ReturnsNull()
    {
        var result = DateHelper.ParseDate(null, "startDate");

        result.Should().BeNull();
    }

    [Fact]
    public void ParseDate_ValidDate_ReturnsDateOnly()
    {
        var result = DateHelper.ParseDate("2025-03-15", "startDate");

        result.Should().Be(new DateOnly(2025, 3, 15));
    }

    [Fact]
    public void ParseDate_InvalidFormat_ThrowsMcpException()
    {
        var act = () => DateHelper.ParseDate("not-a-date", "startDate");

        act.Should().Throw<McpException>();
    }

    [Fact]
    public void ParseDate_ErrorMessageIncludesParamName()
    {
        var act = () => DateHelper.ParseDate("bad", "myParam");

        act.Should().Throw<McpException>()
            .WithMessage("*myParam*");
    }

    [Fact]
    public void ParseDate_EmptyString_ThrowsMcpException()
    {
        var act = () => DateHelper.ParseDate("", "startDate");

        act.Should().Throw<McpException>();
    }
}
