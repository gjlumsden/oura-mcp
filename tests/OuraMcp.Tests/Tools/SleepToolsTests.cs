using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class SleepToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly SleepTools _sut;

    public SleepToolsTests()
    {
        _sut = new SleepTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetDailySleep_DelegatesToApiClient()
    {
        var expected = new List<DailySleep>
        {
            new() { Id = "sleep-1", Day = new DateOnly(2025, 1, 15), Score = 85 }
        };
        _mockClient
            .Setup(c => c.GetDailySleepAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailySleep(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSleepPeriods_DelegatesToApiClient()
    {
        var expected = new List<SleepPeriod>
        {
            new() { Id = "period-1", Day = new DateOnly(2025, 1, 15) }
        };
        _mockClient
            .Setup(c => c.GetSleepPeriodsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetSleepPeriods(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetSleepPeriodsAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSleepTime_DelegatesToApiClient()
    {
        var expected = new List<SleepTime>
        {
            new() { Id = "time-1", Day = new DateOnly(2025, 1, 15) }
        };
        _mockClient
            .Setup(c => c.GetSleepTimeAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetSleepTime(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetSleepTimeAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailySleep_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-01-01";
        var endDate = "2025-01-31";
        var expectedStart = new DateOnly(2025, 1, 1);
        var expectedEnd = new DateOnly(2025, 1, 31);

        _mockClient
            .Setup(c => c.GetDailySleepAsync(expectedStart, expectedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailySleep>());

        await _sut.GetDailySleep(startDate, endDate);

        _mockClient.Verify(
            c => c.GetDailySleepAsync(expectedStart, expectedEnd, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailySleep_ApiThrows_PropagatesError()
    {
        _mockClient
            .Setup(c => c.GetDailySleepAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var act = () => _sut.GetDailySleep(null, null);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("API unavailable");
    }
}
