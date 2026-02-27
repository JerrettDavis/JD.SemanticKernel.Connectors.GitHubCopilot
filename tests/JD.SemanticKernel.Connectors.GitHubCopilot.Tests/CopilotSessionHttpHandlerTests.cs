using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotSessionHttpHandlerTests : IDisposable
{
    private readonly CopilotSessionProvider _provider;
    private readonly string _tokenResponseJson;

    public CopilotSessionHttpHandlerTests()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
        _tokenResponseJson = JsonSerializer.Serialize(new
        {
            token = "test-bearer-token",
            expires_at = expiry,
            refresh_in = 1500,
            endpoints = new { api = "https://api.githubcopilot.com" },
        });

        var options = new CopilotSessionOptions { OAuthToken = "ghu_handler_test" };
        _provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            CreateExchangeClient());
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task SendAsync_InjectsBearerHeader()
    {
        using var inner = new RecordingHandler(HttpStatusCode.OK);
        using var handler = new CopilotSessionHttpHandler(_provider, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://api.githubcopilot.com/chat/completions");

        Assert.NotNull(inner.LastRequest);
        Assert.Equal("Bearer test-bearer-token",
            inner.LastRequest!.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task SendAsync_InjectsEditorVersion()
    {
        using var inner = new RecordingHandler(HttpStatusCode.OK);
        using var handler = new CopilotSessionHttpHandler(_provider, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://api.githubcopilot.com/test");

        Assert.Contains("vscode/1.104.1",
            inner.LastRequest!.Headers.GetValues("Editor-Version"));
    }

    [Fact]
    public async Task SendAsync_InjectsUserAgent()
    {
        using var inner = new RecordingHandler(HttpStatusCode.OK);
        using var handler = new CopilotSessionHttpHandler(_provider, inner);
        using var client = new HttpClient(handler);

        await client.GetAsync("https://api.githubcopilot.com/test");

        Assert.Contains("GitHubCopilotChat/0.1",
            inner.LastRequest!.Headers.GetValues("User-Agent"));
    }

    [Fact]
    public async Task SendAsync_RetriesOn401()
    {
        using var inner = new UnauthorizedThenOkHandler();
        using var handler = new CopilotSessionHttpHandler(_provider, inner);
        using var client = new HttpClient(handler);

        var response = await client.GetAsync("https://api.githubcopilot.com/test");

        // First request gets 401, second gets 200 after cache invalidation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, inner.RequestCount);
    }

    [Fact]
    public void CreateInnerHandler_WithSslBypass_DoesNotThrow()
    {
        using var handler = CopilotSessionHttpHandler.CreateInnerHandler(true);
        Assert.NotNull(handler);
    }

    [Fact]
    public void CreateInnerHandler_WithoutSslBypass_DoesNotThrow()
    {
        using var handler = CopilotSessionHttpHandler.CreateInnerHandler(false);
        Assert.NotNull(handler);
    }

    [Fact]
    public void Constructor_ThrowsOnNullProvider()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotSessionHttpHandler(null!));
    }

    private HttpClient CreateExchangeClient()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK, _tokenResponseJson);
        return new HttpClient(handler);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public RecordingHandler(HttpStatusCode statusCode, string body = "")
        {
            _statusCode = statusCode;
            _body = body;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }

    private sealed class UnauthorizedThenOkHandler : HttpMessageHandler
    {
        private int _requestCount;
        public int RequestCount => _requestCount;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var count = Interlocked.Increment(ref _requestCount);
            var statusCode = count <= 1 ? HttpStatusCode.Unauthorized : HttpStatusCode.OK;

            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }
}
