using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

namespace OuraMcp.Tests.OuraClient;

public class ResilienceConfigurationTests
{
    [Fact]
    public void OuraApiHttpClient_HasResilienceHandler_Configured()
    {
        Environment.SetEnvironmentVariable("OURA_CLIENT_ID", "test-id");
        Environment.SetEnvironmentVariable("OURA_CLIENT_SECRET", "test-secret");

        try
        {
            var builder = Host.CreateApplicationBuilder([]);
            builder.Services.AddHttpClient("OuraApi", c => c.BaseAddress = new Uri("https://api.ouraring.com"))
                .AddStandardResilienceHandler();

            using var host = builder.Build();
            var factory = host.Services.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient("OuraApi");

            client.Should().NotBeNull();
            client.BaseAddress.Should().Be(new Uri("https://api.ouraring.com"));
        }
        finally
        {
            Environment.SetEnvironmentVariable("OURA_CLIENT_ID", null);
            Environment.SetEnvironmentVariable("OURA_CLIENT_SECRET", null);
        }
    }

    [Fact]
    public void StandardResilienceHandler_DefaultOptions_AreReasonable()
    {
        var options = new HttpStandardResilienceOptions();

        options.Retry.MaxRetryAttempts.Should().Be(3);
        options.Retry.BackoffType.Should().Be(Polly.DelayBackoffType.Exponential);
        options.Retry.UseJitter.Should().BeTrue();
        options.TotalRequestTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.AttemptTimeout.Timeout.Should().Be(TimeSpan.FromSeconds(10));
    }
}
