using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.Tests.OuraClient;

public class CachingOuraApiClientTests : IDisposable
{
    private readonly Mock<IOuraApiClient> _mockInner = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly CacheSettings _settings = new();
    private readonly CachingOuraApiClient _sut;

    public CachingOuraApiClientTests()
    {
        _sut = new CachingOuraApiClient(_mockInner.Object, _cache, _settings);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }

    // ── Cache miss calls through to inner client ─────────────────────

    [Fact]
    public async Task GetDailySleep_CacheMiss_CallsInnerClient()
    {
        var expected = new List<DailySleep> { new() { Id = "s1" } };
        _mockInner
            .Setup(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetDailySleepAsync(null, null);

        result.Should().BeSameAs(expected);
        _mockInner.Verify(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPersonalInfo_CacheMiss_CallsInnerClient()
    {
        var expected = new PersonalInfo { Id = "p1", Email = "test@example.com" };
        _mockInner
            .Setup(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetPersonalInfoAsync();

        result.Should().BeSameAs(expected);
        _mockInner.Verify(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Cache hit returns cached data without calling inner ──────────

    [Fact]
    public async Task GetDailySleep_CacheHit_ReturnsCachedData()
    {
        var expected = new List<DailySleep> { new() { Id = "s1" } };
        _mockInner
            .Setup(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var first = await _sut.GetDailySleepAsync(null, null);
        var second = await _sut.GetDailySleepAsync(null, null);

        second.Should().BeSameAs(first);
        _mockInner.Verify(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPersonalInfo_CacheHit_ReturnsCachedData()
    {
        var expected = new PersonalInfo { Id = "p1" };
        _mockInner
            .Setup(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var first = await _sut.GetPersonalInfoAsync();
        var second = await _sut.GetPersonalInfoAsync();

        second.Should().BeSameAs(first);
        _mockInner.Verify(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Different date ranges produce different cache keys ───────────

    [Fact]
    public async Task GetDailySleep_DifferentDateRanges_CallsInnerTwice()
    {
        var data1 = new List<DailySleep> { new() { Id = "s1" } };
        var data2 = new List<DailySleep> { new() { Id = "s2" } };
        var start1 = new DateOnly(2026, 1, 1);
        var end1 = new DateOnly(2026, 1, 7);
        var start2 = new DateOnly(2026, 2, 1);
        var end2 = new DateOnly(2026, 2, 7);

        _mockInner
            .Setup(c => c.GetDailySleepAsync(start1, end1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data1);
        _mockInner
            .Setup(c => c.GetDailySleepAsync(start2, end2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data2);

        var result1 = await _sut.GetDailySleepAsync(start1, end1);
        var result2 = await _sut.GetDailySleepAsync(start2, end2);

        result1.Should().BeSameAs(data1);
        result2.Should().BeSameAs(data2);
        _mockInner.Verify(c => c.GetDailySleepAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ── Same date range across different endpoints caches independently ──

    [Fact]
    public async Task DifferentEndpoints_SameDateRange_CacheIndependently()
    {
        var sleepData = new List<DailySleep> { new() { Id = "sleep-1" } };
        var activityData = new List<DailyActivity> { new() { Id = "activity-1" } };

        _mockInner
            .Setup(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sleepData);
        _mockInner
            .Setup(c => c.GetDailyActivityAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activityData);

        var sleep = await _sut.GetDailySleepAsync(null, null);
        var activity = await _sut.GetDailyActivityAsync(null, null);

        sleep.Should().BeSameAs(sleepData);
        activity.Should().BeSameAs(activityData);
        _mockInner.Verify(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
        _mockInner.Verify(c => c.GetDailyActivityAsync(null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── TTL tier assignment ──────────────────────────────────────────

    [Theory]
    [InlineData(nameof(IOuraApiClient.GetPersonalInfoAsync), "Stable")]
    [InlineData(nameof(IOuraApiClient.GetRingConfigurationAsync), "Stable")]
    [InlineData(nameof(IOuraApiClient.GetDailySleepAsync), "Moderate")]
    [InlineData(nameof(IOuraApiClient.GetDailyReadinessAsync), "Moderate")]
    [InlineData(nameof(IOuraApiClient.GetSleepPeriodsAsync), "Moderate")]
    [InlineData(nameof(IOuraApiClient.GetSleepTimeAsync), "Moderate")]
    [InlineData(nameof(IOuraApiClient.GetDailyActivityAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetDailyStressAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetDailyResilienceAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetWorkoutsAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetSessionsAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetDailySpo2Async), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetVo2MaxAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetCardiovascularAgeAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetTagsAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetEnhancedTagsAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetRestModePeriodsAsync), "Standard")]
    [InlineData(nameof(IOuraApiClient.GetHeartRateAsync), "Short")]
    public void GetTtl_ReturnsCorrectTier(string methodName, string expectedTier)
    {
        var expectedTtl = expectedTier switch
        {
            "Stable" => _settings.StableTtl,
            "Moderate" => _settings.ModerateTtl,
            "Standard" => _settings.StandardTtl,
            "Short" => _settings.ShortTtl,
            _ => throw new ArgumentException($"Unknown tier: {expectedTier}")
        };

        var actual = CachingOuraApiClient.GetTtl(methodName, _settings);

        actual.Should().Be(expectedTtl);
    }

    // ── All 18 methods delegate correctly ────────────────────────────

    [Fact]
    public async Task GetRingConfiguration_DelegatesToInner()
    {
        var expected = new RingConfiguration { Id = "r1" };
        _mockInner.Setup(c => c.GetRingConfigurationAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetRingConfigurationAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetDailyActivity_DelegatesToInner()
    {
        var expected = new List<DailyActivity> { new() { Id = "a1" } };
        _mockInner.Setup(c => c.GetDailyActivityAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetDailyActivityAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetDailyReadiness_DelegatesToInner()
    {
        var expected = new List<DailyReadiness> { new() { Id = "r1" } };
        _mockInner.Setup(c => c.GetDailyReadinessAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetDailyReadinessAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetDailyStress_DelegatesToInner()
    {
        var expected = new List<DailyStress> { new() { Id = "st1" } };
        _mockInner.Setup(c => c.GetDailyStressAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetDailyStressAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetDailyResilience_DelegatesToInner()
    {
        var expected = new List<DailyResilience> { new() { Id = "res1" } };
        _mockInner.Setup(c => c.GetDailyResilienceAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetDailyResilienceAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetSleepPeriods_DelegatesToInner()
    {
        var expected = new List<SleepPeriod> { new() { Id = "sp1" } };
        _mockInner.Setup(c => c.GetSleepPeriodsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetSleepPeriodsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetSleepTime_DelegatesToInner()
    {
        var expected = new List<SleepTime> { new() { Id = "st1" } };
        _mockInner.Setup(c => c.GetSleepTimeAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetSleepTimeAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetWorkouts_DelegatesToInner()
    {
        var expected = new List<Workout> { new() { Id = "w1" } };
        _mockInner.Setup(c => c.GetWorkoutsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetWorkoutsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetSessions_DelegatesToInner()
    {
        var expected = new List<Session> { new() { Id = "se1" } };
        _mockInner.Setup(c => c.GetSessionsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetSessionsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetHeartRate_DelegatesToInner()
    {
        var expected = new List<HeartRate> { new() { Bpm = 72 } };
        _mockInner.Setup(c => c.GetHeartRateAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetHeartRateAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetDailySpo2_DelegatesToInner()
    {
        var expected = new List<DailySpo2> { new() { Id = "o1" } };
        _mockInner.Setup(c => c.GetDailySpo2Async(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetDailySpo2Async();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetVo2Max_DelegatesToInner()
    {
        var expected = new List<Vo2Max> { new() { Id = "v1" } };
        _mockInner.Setup(c => c.GetVo2MaxAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetVo2MaxAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetCardiovascularAge_DelegatesToInner()
    {
        var expected = new List<CardiovascularAge> { new() { VascularAge = 30 } };
        _mockInner.Setup(c => c.GetCardiovascularAgeAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetCardiovascularAgeAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetTags_DelegatesToInner()
    {
        var expected = new List<Tag> { new() { Id = "t1" } };
        _mockInner.Setup(c => c.GetTagsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetTagsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetEnhancedTags_DelegatesToInner()
    {
        var expected = new List<EnhancedTag> { new() { Id = "et1" } };
        _mockInner.Setup(c => c.GetEnhancedTagsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetEnhancedTagsAsync();

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetRestModePeriods_DelegatesToInner()
    {
        var expected = new List<RestModePeriod> { new() { Id = "rm1" } };
        _mockInner.Setup(c => c.GetRestModePeriodsAsync(null, null, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await _sut.GetRestModePeriodsAsync();

        result.Should().BeSameAs(expected);
    }

    // ── Inner client exceptions propagate ────────────────────────────

    [Fact]
    public async Task GetDailySleep_InnerThrows_PropagatesException()
    {
        _mockInner
            .Setup(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        var act = () => _sut.GetDailySleepAsync(null, null);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("API unavailable");
    }

    [Fact]
    public async Task GetPersonalInfo_InnerThrows_PropagatesException()
    {
        _mockInner
            .Setup(c => c.GetPersonalInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Token expired"));

        var act = () => _sut.GetPersonalInfoAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Token expired");
    }

    // ── Failed calls are not cached ──────────────────────────────────

    [Fact]
    public async Task GetDailySleep_InnerThrows_DoesNotCacheFailure()
    {
        var expected = new List<DailySleep> { new() { Id = "s1" } };
        _mockInner
            .SetupSequence(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Transient"))
            .ReturnsAsync(expected);

        var act = () => _sut.GetDailySleepAsync(null, null);
        await act.Should().ThrowAsync<HttpRequestException>();

        var result = await _sut.GetDailySleepAsync(null, null);

        result.Should().BeSameAs(expected);
        _mockInner.Verify(c => c.GetDailySleepAsync(null, null, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}

public class CacheSettingsTests
{
    [Fact]
    public void BuildCacheKey_WithDates_IncludesFormattedDates()
    {
        var key = CacheSettings.BuildCacheKey("GetDailySleepAsync", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10));

        key.Should().Be("GetDailySleepAsync:2026-03-01:2026-03-10");
    }

    [Fact]
    public void BuildCacheKey_WithNullDates_IncludesNullPlaceholders()
    {
        var key = CacheSettings.BuildCacheKey("GetDailySleepAsync", null, null);

        key.Should().Be("GetDailySleepAsync:null:null");
    }

    [Fact]
    public void BuildCacheKey_SingleEndpoint_ReturnsMethodName()
    {
        var key = CacheSettings.BuildCacheKey("GetPersonalInfoAsync");

        key.Should().Be("GetPersonalInfoAsync");
    }

    [Fact]
    public void BuildCacheKey_MixedNullDates_HandlesCorrectly()
    {
        var key = CacheSettings.BuildCacheKey("GetDailySleepAsync", new DateOnly(2026, 1, 1), null);

        key.Should().Be("GetDailySleepAsync:2026-01-01:null");
    }

    [Fact]
    public void DefaultTtls_AreCorrect()
    {
        var settings = new CacheSettings();

        settings.StableTtl.Should().Be(TimeSpan.FromMinutes(60));
        settings.ModerateTtl.Should().Be(TimeSpan.FromMinutes(15));
        settings.StandardTtl.Should().Be(TimeSpan.FromMinutes(10));
        settings.ShortTtl.Should().Be(TimeSpan.FromMinutes(1));
    }
}
