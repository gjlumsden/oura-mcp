using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Moq;
using Moq.Protected;
using OuraMcp.Auth;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.Tests.OuraClient;

public class OuraApiClientTests
{
    private const string FakeToken = "fake-access-token";

    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IOuraTokenService> _tokenService = new();
    private readonly Mock<HttpMessageHandler> _httpHandler = new();
    private readonly Mock<ILogger<OuraApiClient>> _logger = new();

    private OuraApiClient CreateClient()
    {
        var httpClient = new HttpClient(_httpHandler.Object)
        {
            BaseAddress = new Uri("https://api.ouraring.com/")
        };
        _httpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        _tokenService
            .Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeToken);

        return new OuraApiClient(_httpClientFactory.Object, _tokenService.Object, _logger.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
    }

    private void SetupHttpResponseSequence(params (HttpStatusCode statusCode, string content)[] responses)
    {
        var setup = _httpHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var (statusCode, content) in responses)
        {
            setup = setup.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    [Fact]
    public async Task GetDailySleepAsync_ReturnsDeserializedData()
    {
        var json = """
            {
                "data": [
                    {
                        "id": "sleep-1",
                        "score": 85,
                        "day": "2024-01-15",
                        "timestamp": "2024-01-15T22:30:00+00:00",
                        "contributors": {
                            "deep_sleep": 80,
                            "efficiency": 90,
                            "latency": 70,
                            "rem_sleep": 85,
                            "restfulness": 75,
                            "timing": 88,
                            "total_sleep": 92
                        }
                    }
                ],
                "next_token": null
            }
            """;
        SetupHttpResponse(HttpStatusCode.OK, json);
        var client = CreateClient();

        var result = await client.GetDailySleepAsync();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("sleep-1");
        result[0].Score.Should().Be(85);
        result[0].Contributors.Should().NotBeNull();
        result[0].Contributors!.DeepSleep.Should().Be(80);
    }

    [Fact]
    public async Task GetDailySleepAsync_WithDateRange_SendsCorrectQueryParams()
    {
        var json = """{"data":[],"next_token":null}""";
        SetupHttpResponse(HttpStatusCode.OK, json);
        var client = CreateClient();

        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 1, 31);

        await client.GetDailySleepAsync(startDate, endDate);

        _httpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.Query.Contains("start_date=2024-01-01") &&
                req.RequestUri.Query.Contains("end_date=2024-01-31")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetDailySleepAsync_MultiplePages_AggregatesResults()
    {
        var page1Json = """
            {
                "data": [
                    {"id":"sleep-1","score":85,"day":"2024-01-15","timestamp":"2024-01-15T22:30:00+00:00","contributors":null}
                ],
                "next_token": "page2token"
            }
            """;
        var page2Json = """
            {
                "data": [
                    {"id":"sleep-2","score":90,"day":"2024-01-16","timestamp":"2024-01-16T22:30:00+00:00","contributors":null}
                ],
                "next_token": null
            }
            """;

        SetupHttpResponseSequence(
            (HttpStatusCode.OK, page1Json),
            (HttpStatusCode.OK, page2Json));
        var client = CreateClient();

        var result = await client.GetDailySleepAsync();

        result.Should().HaveCount(2);
        result[0].Id.Should().Be("sleep-1");
        result[1].Id.Should().Be("sleep-2");
    }

    [Fact]
    public async Task GetPersonalInfoAsync_ReturnsDirectResponse()
    {
        var json = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;
        SetupHttpResponse(HttpStatusCode.OK, json);
        var client = CreateClient();

        var result = await client.GetPersonalInfoAsync();

        result.Should().NotBeNull();
        result.Id.Should().Be("abc");
        result.Age.Should().Be(30);
        result.Weight.Should().Be(70.5);
        result.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task ApiCall_401Response_TriggersRefreshAndRetries()
    {
        var unauthorizedJson = """{"detail":"Unauthorized"}""";
        var successJson = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;

        SetupHttpResponseSequence(
            (HttpStatusCode.Unauthorized, unauthorizedJson),
            (HttpStatusCode.OK, successJson));

        var client = CreateClient();

        var result = await client.GetPersonalInfoAsync();

        result.Should().NotBeNull();
        result.Id.Should().Be("abc");
        _httpHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ApiCall_403Response_ThrowsMcpExceptionWithFriendlyMessage()
    {
        var forbiddenJson = """{"detail":"Subscription expired"}""";
        SetupHttpResponse(HttpStatusCode.Forbidden, forbiddenJson);
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        await act.Should().ThrowAsync<McpException>()
            .Where(e => e.Message.Contains("Access denied") && e.Message.Contains("subscription"));
    }

    [Fact]
    public async Task CollectionEndpoint_NullDataArray_ReturnsEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"data":null,"next_token":null}""");
        var client = CreateClient();

        var result = await client.GetDailyActivityAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CollectionEndpoint_EmptyDataArray_ReturnsEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.OK, """{"data":[],"next_token":null}""");
        var client = CreateClient();

        var result = await client.GetDailyActivityAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CollectionEndpoint_NullResponseBody_ThrowsMcpException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "null");
        var client = CreateClient();

        var act = () => client.GetDailyActivityAsync();

        await act.Should().ThrowAsync<McpException>();
    }

    [Fact]
    public async Task ApiCall_IncludesBearerToken()
    {
        var json = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;
        SetupHttpResponse(HttpStatusCode.OK, json);
        var client = CreateClient();

        await client.GetPersonalInfoAsync();

        _httpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Authorization != null &&
                req.Headers.Authorization.Scheme == "Bearer" &&
                req.Headers.Authorization.Parameter == FakeToken),
            ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData(nameof(IOuraApiClient.GetPersonalInfoAsync), "v2/usercollection/personal_info", true)]
    [InlineData(nameof(IOuraApiClient.GetRingConfigurationAsync), "v2/usercollection/ring_configuration", true)]
    [InlineData(nameof(IOuraApiClient.GetDailySleepAsync), "v2/usercollection/daily_sleep", false)]
    [InlineData(nameof(IOuraApiClient.GetDailyActivityAsync), "v2/usercollection/daily_activity", false)]
    [InlineData(nameof(IOuraApiClient.GetDailyReadinessAsync), "v2/usercollection/daily_readiness", false)]
    [InlineData(nameof(IOuraApiClient.GetDailyStressAsync), "v2/usercollection/daily_stress", false)]
    [InlineData(nameof(IOuraApiClient.GetDailyResilienceAsync), "v2/usercollection/daily_resilience", false)]
    [InlineData(nameof(IOuraApiClient.GetSleepPeriodsAsync), "v2/usercollection/sleep", false)]
    [InlineData(nameof(IOuraApiClient.GetSleepTimeAsync), "v2/usercollection/sleep_time", false)]
    [InlineData(nameof(IOuraApiClient.GetWorkoutsAsync), "v2/usercollection/workout", false)]
    [InlineData(nameof(IOuraApiClient.GetSessionsAsync), "v2/usercollection/session", false)]
    [InlineData(nameof(IOuraApiClient.GetHeartRateAsync), "v2/usercollection/heartrate", false)]
    [InlineData(nameof(IOuraApiClient.GetDailySpo2Async), "v2/usercollection/daily_spo2", false)]
    [InlineData(nameof(IOuraApiClient.GetVo2MaxAsync), "v2/usercollection/vO2_max", false)]
    [InlineData(nameof(IOuraApiClient.GetCardiovascularAgeAsync), "v2/usercollection/daily_cardiovascular_age", false)]
    [InlineData(nameof(IOuraApiClient.GetTagsAsync), "v2/usercollection/tag", false)]
    [InlineData(nameof(IOuraApiClient.GetEnhancedTagsAsync), "v2/usercollection/enhanced_tag", false)]
    [InlineData(nameof(IOuraApiClient.GetRestModePeriodsAsync), "v2/usercollection/rest_mode_period", false)]
    public async Task EndpointMethod_CallsCorrectApiPath(string methodName, string expectedPath, bool isSingleObject)
    {
        Uri? capturedUri = null;
        var responseJson = isSingleObject ? "{}" : """{"data":[],"next_token":null}""";

        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedUri = req.RequestUri)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            });

        var client = CreateClient();
        var method = typeof(OuraApiClient).GetMethod(methodName)
            ?? throw new InvalidOperationException($"Method {methodName} not found");

        var defaultArgs = method.GetParameters()
            .Select(p => p.HasDefaultValue ? p.DefaultValue : null)
            .ToArray();

        var result = method.Invoke(client, defaultArgs);
        await (Task)result!;

        capturedUri.Should().NotBeNull();
        capturedUri!.PathAndQuery.Should().Contain(expectedPath);
    }

    // ── Error handling tests ─────────────────────────────────────────

    [Fact]
    public async Task ApiCall_NetworkError_PropagatesException()
    {
        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ApiCall_TokenRetrievalFails_ThrowsMcpExceptionAndLogs()
    {
        SetupHttpResponse(HttpStatusCode.OK, "{}");
        var client = CreateClient();
        _tokenService.Reset();
        _tokenService
            .Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No Oura tokens found"));

        var act = () => client.GetPersonalInfoAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("Unable to authenticate");
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Failed to retrieve")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ApiCall_UnexpectedHttpStatus_ThrowsMcpException()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, "Not found");
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("HTTP 404");
    }

    [Fact]
    public async Task ApiCall_MalformedJson_ThrowsMcpException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "not valid json{{{");
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task CollectionEndpoint_MalformedJson_ThrowsMcpException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "not valid json{{{");
        var client = CreateClient();

        var act = () => client.GetDailyActivityAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task ApiCall_SingleEndpoint_NullResponse_ThrowsMcpException()
    {
        SetupHttpResponse(HttpStatusCode.OK, "null");
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("empty response");
    }

    [Fact]
    public async Task ApiCall_401ThenRefreshFails_ThrowsMcpExceptionAndLogs()
    {
        var tokenCallCount = 0;
        SetupHttpResponseSequence(
            (HttpStatusCode.Unauthorized, """{"detail":"Unauthorized"}"""),
            (HttpStatusCode.OK, "{}"));
        var client = CreateClient();
        _tokenService.Reset();
        _tokenService
            .Setup(t => t.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                tokenCallCount++;
                if (tokenCallCount > 1)
                    throw new InvalidOperationException("Refresh failed");
                return FakeToken;
            });

        var act = () => client.GetPersonalInfoAsync();

        var ex = await act.Should().ThrowAsync<McpException>();
        ex.Which.Message.Should().Contain("Authentication failed");
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Token refresh failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ApiCall_SingleEndpoint_NullResponse_LogsBeforeThrowing()
    {
        SetupHttpResponse(HttpStatusCode.OK, "null");
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync();

        await act.Should().ThrowAsync<McpException>();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("null/empty")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
