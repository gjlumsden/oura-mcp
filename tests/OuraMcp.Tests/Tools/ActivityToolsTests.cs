using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class ActivityToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly ActivityTools _sut;

    public ActivityToolsTests()
    {
        _sut = new ActivityTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetDailyActivity_DelegatesToApiClient()
    {
        var expected = new List<DailyActivity>
        {
            new() { Id = "activity-1", Day = new DateOnly(2025, 1, 15), Score = 90, Steps = 8500 }
        };
        _mockClient
            .Setup(c => c.GetDailyActivityAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailyActivity(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailyActivityAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkouts_DelegatesToApiClient()
    {
        var expected = new List<Workout>
        {
            new() { Id = "workout-1", Day = new DateOnly(2025, 1, 15), Activity = "running" }
        };
        _mockClient
            .Setup(c => c.GetWorkoutsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetWorkouts(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetWorkoutsAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetSessions_DelegatesToApiClient()
    {
        var expected = new List<Session>
        {
            new() { Id = "session-1", Day = new DateOnly(2025, 1, 15), Type = "meditation" }
        };
        _mockClient
            .Setup(c => c.GetSessionsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetSessions(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetSessionsAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailyActivity_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-02-01";
        var endDate = "2025-02-28";

        _mockClient
            .Setup(c => c.GetDailyActivityAsync(
                new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyActivity>());

        await _sut.GetDailyActivity(startDate, endDate);

        _mockClient.Verify(
            c => c.GetDailyActivityAsync(
                new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
