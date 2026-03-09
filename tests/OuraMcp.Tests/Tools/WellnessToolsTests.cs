using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class WellnessToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly WellnessTools _sut;

    public WellnessToolsTests()
    {
        _sut = new WellnessTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetDailyStress_DelegatesToApiClient()
    {
        var expected = new List<DailyStress>
        {
            new() { Id = "stress-1", Day = new DateOnly(2025, 1, 15), DaySummary = "restored" }
        };
        _mockClient
            .Setup(c => c.GetDailyStressAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailyStress(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailyStressAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailyResilience_DelegatesToApiClient()
    {
        var expected = new List<DailyResilience>
        {
            new() { Id = "resilience-1", Day = new DateOnly(2025, 1, 15), Level = "strong" }
        };
        _mockClient
            .Setup(c => c.GetDailyResilienceAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailyResilience(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailyResilienceAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRestModePeriods_DelegatesToApiClient()
    {
        var expected = new List<RestModePeriod>
        {
            new() { Id = "rest-1" }
        };
        _mockClient
            .Setup(c => c.GetRestModePeriodsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetRestModePeriods(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetRestModePeriodsAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailyStress_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-05-01";
        var endDate = "2025-05-31";

        _mockClient
            .Setup(c => c.GetDailyStressAsync(
                new DateOnly(2025, 5, 1), new DateOnly(2025, 5, 31), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyStress>());

        await _sut.GetDailyStress(startDate, endDate);

        _mockClient.Verify(
            c => c.GetDailyStressAsync(
                new DateOnly(2025, 5, 1), new DateOnly(2025, 5, 31), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
