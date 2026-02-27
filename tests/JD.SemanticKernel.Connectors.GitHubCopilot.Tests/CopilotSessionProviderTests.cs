using System.Net;
using System.Text.Json;
using JD.SemanticKernel.Connectors.GitHubCopilot.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotSessionProviderTests : IDisposable
{
    private readonly string _tempDir;

    public CopilotSessionProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"copilot-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { /* cleanup best-effort */ }
    }

    [Fact]
    public async Task GetApiToken_UsesExplicitOAuthToken()
    {
        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_test_explicit" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var result = await provider.GetApiTokenAsync();

        Assert.Equal("copilot-api-token-123", result.Token);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal("token ghu_test_explicit",
            handler.LastRequest!.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetApiToken_ReadsFromFile()
    {
        var appsFile = Path.Combine(_tempDir, "apps.json");
        var entries = new Dictionary<string, object>
(StringComparer.Ordinal)
        {
            ["github.com:TestAppId"] = new { user = "testuser", oauth_token = "ghu_fromfile123" },
        };
        await File.WriteAllTextAsync(appsFile, JsonSerializer.Serialize(entries));

        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { TokenFilePath = appsFile };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var result = await provider.GetApiTokenAsync();

        Assert.Equal("copilot-api-token-123", result.Token);
        Assert.Equal("token ghu_fromfile123",
            handler.LastRequest!.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetApiToken_CachesToken()
    {
        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_cache_test" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var result1 = await provider.GetApiTokenAsync();
        var result2 = await provider.GetApiTokenAsync();

        Assert.Same(result1, result2);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GetCredentials_ReturnsSessionCredentials()
    {
        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_creds_test" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var creds = await provider.GetCredentialsAsync();

        Assert.Equal("copilot-api-token-123", creds.Token);
        Assert.NotNull(creds.ExpiresAt);
    }

    [Fact]
    public async Task IsAuthenticated_ReturnsTrue_WhenValid()
    {
        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_auth_test" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        Assert.True(await provider.IsAuthenticatedAsync());
    }

    [Fact]
    public async Task IsAuthenticated_ReturnsFalse_WhenExchangeFails()
    {
        using var handler = new RecordingHandler(HttpStatusCode.Unauthorized);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_bad_token" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        Assert.False(await provider.IsAuthenticatedAsync());
    }

    [Fact]
    public async Task GetApiToken_ThrowsOnMissingCredentials()
    {
        using var handler = new RecordingHandler(HttpStatusCode.OK);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions
        {
            TokenFilePath = Path.Combine(_tempDir, "nonexistent.json"),
        };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var ex = await Assert.ThrowsAsync<CopilotSessionException>(
            () => provider.GetApiTokenAsync());

        Assert.Contains("No GitHub Copilot credentials found", ex.Message);
    }

    [Fact]
    public async Task GetApiToken_ThrowsOnExchangeFailure()
    {
        using var handler = new RecordingHandler(HttpStatusCode.Forbidden, "Subscription expired");
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_expired" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var ex = await Assert.ThrowsAsync<CopilotSessionException>(
            () => provider.GetApiTokenAsync());

        Assert.Contains("403", ex.Message);
    }

    [Fact]
    public async Task GetApiToken_ThreadSafe_ConcurrentAccess()
    {
        var tokenResponse = CreateTokenResponse();
        using var handler = new RecordingHandler(tokenResponse);
        using var httpClient = new HttpClient(handler);

        var options = new CopilotSessionOptions { OAuthToken = "ghu_concurrent" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            httpClient);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => provider.GetApiTokenAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, r => Assert.Equal("copilot-api-token-123", r.Token));
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public void InvalidateCache_ForcesReExchange()
    {
        var options = new CopilotSessionOptions { OAuthToken = "ghu_invalidate" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            new HttpClient());

        // Should not throw — just clears internal state
        provider.InvalidateCache();
    }

    [Fact]
    public void Constructor_ThrowsOnNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotSessionProvider(null!, NullLogger<CopilotSessionProvider>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsOnNullLogger()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotSessionProvider(
                Options.Create(new CopilotSessionOptions()), null!));
    }

    private static string CreateTokenResponse(long? expiresAt = null)
    {
        var expiry = expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
        return JsonSerializer.Serialize(new
        {
            token = "copilot-api-token-123",
            expires_at = expiry,
            refresh_in = 1500,
            endpoints = new { api = "https://api.githubcopilot.com" },
            sku = "copilot_for_business",
        });
    }

    /// <summary>Test handler that records requests and returns canned responses.</summary>
    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;
        private int _requestCount;

        public RecordingHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _statusCode = statusCode;
        }

        public RecordingHandler(HttpStatusCode statusCode, string body = "")
        {
            _statusCode = statusCode;
            _responseBody = body;
        }

        public HttpRequestMessage? LastRequest { get; private set; }
        public int RequestCount => _requestCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            Interlocked.Increment(ref _requestCount);

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }
}
