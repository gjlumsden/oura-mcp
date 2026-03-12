using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace OuraMcp.OuraClient;

/// <summary>
/// Centralised resilience configuration for the Oura API HTTP client.
/// Used by both the production DI registration and integration tests.
/// </summary>
internal static class OuraResilienceDefaults
{
    /// <summary>
    /// Configures the standard resilience handler options for Oura API requests.
    /// </summary>
    public static void Configure(HttpStandardResilienceOptions options)
    {
        // Retry: 3 attempts with exponential backoff + jitter
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.Retry.Delay = TimeSpan.FromSeconds(2);

        // Circuit breaker: opens at 10% failure rate, breaks for 5s
        options.CircuitBreaker.FailureRatio = 0.1;
        options.CircuitBreaker.MinimumThroughput = 100;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(5);

        // Timeouts
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    }
}
