using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class BodyToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly BodyTools _sut;

    public BodyToolsTests()
    {
        _sut = new BodyTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetHeartRate_DelegatesToApiClient()
    {
        var expected = new List<HeartRate>
        {
            new() { Bpm = 72, Source = "awake", Timestamp = DateTimeOffset.UtcNow }
        };
        _mockClient
            .Setup(c => c.GetHeartRateAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetHeartRate(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetHeartRateAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHeartRateVariability_DelegatesToApiClient()
    {
        var expected = new List<HeartRateVariability>
        {
            new() { Id = "hrv-1", Day = new DateOnly(2025, 1, 15) }
        };
        _mockClient
            .Setup(c => c.GetHeartRateVariabilityAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetHeartRateVariability(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetHeartRateVariabilityAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDailySpo2_DelegatesToApiClient()
    {
        var expected = new List<DailySpo2>
        {
            new() { Id = "spo2-1", Day = new DateOnly(2025, 1, 15) }
        };
        _mockClient
            .Setup(c => c.GetDailySpo2Async(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailySpo2(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetDailySpo2Async(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVo2Max_DelegatesToApiClient()
    {
        var expected = new List<Vo2Max>
        {
            new() { Id = "vo2-1", Day = new DateOnly(2025, 1, 15) }
        };
        _mockClient
            .Setup(c => c.GetVo2MaxAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetVo2Max(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetVo2MaxAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCardiovascularAge_DelegatesToApiClient()
    {
        var expected = new List<CardiovascularAge>
        {
            new() { Day = new DateOnly(2025, 1, 15), VascularAge = 32 }
        };
        _mockClient
            .Setup(c => c.GetCardiovascularAgeAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetCardiovascularAge(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetCardiovascularAgeAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetHeartRate_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-04-01";
        var endDate = "2025-04-30";

        _mockClient
            .Setup(c => c.GetHeartRateAsync(
                new DateOnly(2025, 4, 1), new DateOnly(2025, 4, 30), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HeartRate>());

        await _sut.GetHeartRate(startDate, endDate);

        _mockClient.Verify(
            c => c.GetHeartRateAsync(
                new DateOnly(2025, 4, 1), new DateOnly(2025, 4, 30), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
