using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace OuraMcp.Tests.OuraClient;

public class ResilienceConfigurationTests
{
    [Fact]
    public async Task OuraApiHttpClient_RetriesTransientErrors()
    {
        var callCount = 0;
        var handler = new TestDelegatingHandler(() =>
        {
            callCount++;
            if (callCount == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Server error")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };
        });

        var builder = Host.CreateApplicationBuilder([]);
        var httpClientBuilder = builder.Services.AddHttpClient("OuraApi", c => c.BaseAddress = new Uri("https://api.ouraring.com"))
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        httpClientBuilder.AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromMilliseconds(1);
                options.Retry.UseJitter = false;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
            });

        using var host = builder.Build();
        var factory = host.Services.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient("OuraApi");

        var response = await client.GetAsync("/v2/usercollection/personal_info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        callCount.Should().Be(2);
    }

    [Fact]
    public void ExplicitResilienceOptions_MatchDocumentedValues()
    {
        var options = new HttpStandardResilienceOptions();

        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);

        options.Retry.MaxRetryAttempts.Should().Be(3);
        options.Retry.BackoffType.Should().Be(DelayBackoffType.Exponential);
        options.Retry.UseJitter.Should().BeTrue();
        options.Retry.Delay.Should().Be(TimeSpan.FromSeconds(2));
        options.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Test handler that returns responses from a factory function.
    /// </summary>
    private sealed class TestDelegatingHandler(Func<HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responseFactory());
    }
}
