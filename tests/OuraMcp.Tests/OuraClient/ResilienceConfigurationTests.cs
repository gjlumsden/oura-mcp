using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using OuraMcp.OuraClient;
using Polly;

namespace OuraMcp.Tests.OuraClient;

public class ResilienceConfigurationTests
{
    [Fact]
    public async Task OuraApiHttpClient_RetriesTransientErrors()
    {
        var callCount = 0;
        var handler = new TestMessageHandler(() =>
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
                OuraResilienceDefaults.Configure(options);
                // Override backoff for fast tests
                options.Retry.BackoffType = DelayBackoffType.Constant;
                options.Retry.Delay = TimeSpan.FromMilliseconds(1);
                options.Retry.UseJitter = false;
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
    public void OuraResilienceDefaults_ConfiguresExpectedValues()
    {
        var options = new HttpStandardResilienceOptions();

        OuraResilienceDefaults.Configure(options);

        options.Retry.MaxRetryAttempts.Should().Be(3);
        options.Retry.BackoffType.Should().Be(DelayBackoffType.Exponential);
        options.Retry.UseJitter.Should().BeTrue();
        options.Retry.Delay.Should().Be(TimeSpan.FromSeconds(2));
        options.CircuitBreaker.FailureRatio.Should().Be(0.1);
        options.CircuitBreaker.MinimumThroughput.Should().Be(100);
        options.CircuitBreaker.BreakDuration.Should().Be(TimeSpan.FromSeconds(5));
        options.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Test handler that returns responses from a factory function.
    /// </summary>
    private sealed class TestMessageHandler(Func<HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responseFactory());
    }
}
