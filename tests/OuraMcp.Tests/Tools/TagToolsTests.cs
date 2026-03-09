using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class TagToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly TagTools _sut;

    public TagToolsTests()
    {
        _sut = new TagTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetTags_DelegatesToApiClient()
    {
        var expected = new List<Tag>
        {
            new() { Id = "tag-1", Day = new DateOnly(2025, 1, 15), Text = "feeling great" }
        };
        _mockClient
            .Setup(c => c.GetTagsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetTags(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetTagsAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEnhancedTags_DelegatesToApiClient()
    {
        var expected = new List<EnhancedTag>
        {
            new() { Id = "etag-1" }
        };
        _mockClient
            .Setup(c => c.GetEnhancedTagsAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetEnhancedTags(null, null);

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetEnhancedTagsAsync(null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTags_WithDates_PassesDatesToApiClient()
    {
        var startDate = "2025-06-01";
        var endDate = "2025-06-30";

        _mockClient
            .Setup(c => c.GetTagsAsync(
                new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 30), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        await _sut.GetTags(startDate, endDate);

        _mockClient.Verify(
            c => c.GetTagsAsync(
                new DateOnly(2025, 6, 1), new DateOnly(2025, 6, 30), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
