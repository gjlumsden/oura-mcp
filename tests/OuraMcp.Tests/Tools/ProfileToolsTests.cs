using FluentAssertions;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;
using OuraMcp.Tools;

namespace OuraMcp.Tests.Tools;

public class ProfileToolsTests
{
    private readonly Mock<IOuraApiClient> _mockClient = new();
    private readonly ProfileTools _sut;

    public ProfileToolsTests()
    {
        _sut = new ProfileTools(_mockClient.Object);
    }

    [Fact]
    public async Task GetPersonalInfo_DelegatesToApiClient()
    {
        var expected = new PersonalInfo
        {
            Id = "user-1",
            Email = "test@example.com",
            Age = 30
        };
        _mockClient
            .Setup(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetPersonalInfo();

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetRingConfiguration_DelegatesToApiClient()
    {
        var expected = new RingConfiguration
        {
            Id = "ring-1",
            Color = "silver",
            Size = 10
        };
        _mockClient
            .Setup(c => c.GetRingConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetRingConfiguration();

        result.Should().NotBeNullOrEmpty();
        _mockClient.Verify(
            c => c.GetRingConfigurationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPersonalInfo_ApiThrows_PropagatesError()
    {
        _mockClient
            .Setup(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized"));

        var act = () => _sut.GetPersonalInfo();

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Unauthorized");
    }
}
