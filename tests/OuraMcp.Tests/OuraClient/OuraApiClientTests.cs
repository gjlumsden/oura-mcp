using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using OuraMcp.Auth;
using OuraMcp.OuraClient;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.Tests.OuraClient;

public class OuraApiClientTests
{
    private const string FakeToken = "fake-access-token";
    private const string FakeMcpToken = "fake-mcp-token";

    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IOuraTokenService> _tokenService = new();
    private readonly Mock<HttpMessageHandler> _httpHandler = new();

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
            .Setup(t => t.GetAccessTokenAsync(FakeMcpToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeToken);

        return new OuraApiClient(_httpClientFactory.Object, _tokenService.Object);
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

        var result = await client.GetDailySleepAsync(mcpToken: FakeMcpToken);

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

        await client.GetDailySleepAsync(startDate, endDate, mcpToken: FakeMcpToken);

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

        var result = await client.GetDailySleepAsync(mcpToken: FakeMcpToken);

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

        var result = await client.GetPersonalInfoAsync(mcpToken: FakeMcpToken);

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

        _tokenService
            .Setup(t => t.GetAccessTokenAsync(FakeMcpToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(FakeToken);

        var client = CreateClient();

        var result = await client.GetPersonalInfoAsync(mcpToken: FakeMcpToken);

        result.Should().NotBeNull();
        result.Id.Should().Be("abc");
        _httpHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ApiCall_429Response_RespectsRateLimit()
    {
        var rateLimitJson = """{"detail":"Rate limit exceeded"}""";
        var successJson = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;

        var rateLimitResponse = new HttpResponseMessage
        {
            StatusCode = (HttpStatusCode)429,
            Content = new StringContent(rateLimitJson, System.Text.Encoding.UTF8, "application/json")
        };
        rateLimitResponse.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(1));

        _httpHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(rateLimitResponse)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(successJson, System.Text.Encoding.UTF8, "application/json")
            });

        var client = CreateClient();

        var result = await client.GetPersonalInfoAsync(mcpToken: FakeMcpToken);

        result.Should().NotBeNull();
        result.Id.Should().Be("abc");
    }

    [Fact]
    public async Task ApiCall_403Response_ThrowsMeaningfulException()
    {
        var forbiddenJson = """{"detail":"Subscription expired"}""";
        SetupHttpResponse(HttpStatusCode.Forbidden, forbiddenJson);
        var client = CreateClient();

        var act = () => client.GetPersonalInfoAsync(mcpToken: FakeMcpToken);

        await act.Should().ThrowAsync<HttpRequestException>()
            .Where(e => e.Message.Contains("403") || e.Message.Contains("Forbidden") || e.Message.Contains("Subscription"));
    }

    [Fact]
    public async Task ApiCall_IncludesBearerToken()
    {
        var json = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;
        SetupHttpResponse(HttpStatusCode.OK, json);
        var client = CreateClient();

        await client.GetPersonalInfoAsync(mcpToken: FakeMcpToken);

        _httpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Authorization != null &&
                req.Headers.Authorization.Scheme == "Bearer" &&
                req.Headers.Authorization.Parameter == FakeToken),
            ItExpr.IsAny<CancellationToken>());
    }
}
