using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class ReadinessToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly ReadinessTools _sut;

    public ReadinessToolsTests()
    {
        _sut = new ReadinessTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetDailyReadiness_DelegatesToApiClient()
    {
        var expected = new List<DailyReadiness>
        {
            new() { Id = "readiness-1", Day = new DateOnly(2025, 1, 15), Score = 78 }
        };
        _mockClient
            .Setup(c => c.GetDailyReadinessAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailyReadiness(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailyReadinessAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailyReadiness_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-03-01";
        var endDate = "2025-03-31";

        _mockClient
            .Setup(c => c.GetDailyReadinessAsync(
                new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyReadiness>());

        await _sut.GetDailyReadiness(startDate, endDate);

        _mockClient.Verify(
            c => c.GetDailyReadinessAsync(
                new DateOnly(2025, 3, 1), new DateOnly(2025, 3, 31), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailyReadiness_ApiThrows_PropagatesError()
    {
        _mockClient
            .Setup(c => c.GetDailyReadinessAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var act = () => _sut.GetDailyReadiness(null, null);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Service unavailable");
    }
}
