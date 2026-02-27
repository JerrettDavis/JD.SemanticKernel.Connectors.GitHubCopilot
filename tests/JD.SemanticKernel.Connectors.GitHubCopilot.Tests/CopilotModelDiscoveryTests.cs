using System.Net;
using System.Text.Json;
using JD.SemanticKernel.Connectors.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotModelDiscoveryTests : IDisposable
{
    private readonly CopilotSessionProvider _provider;
    private readonly string _tokenResponse;

    public CopilotModelDiscoveryTests()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds();
        _tokenResponse = JsonSerializer.Serialize(new
        {
            token = "copilot-api-token-123",
            expires_at = expiry,
            refresh_in = 1500,
            endpoints = new { api = "https://api.githubcopilot.com" },
        });

        var options = new CopilotSessionOptions { OAuthToken = "ghu_test_discovery" };
        _provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance,
            new HttpClient(new FakeHandler(_tokenResponse)));
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public void Implements_IModelDiscoveryProvider()
    {
        using var httpClient = new HttpClient();
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        Assert.IsAssignableFrom<IModelDiscoveryProvider>(discovery);
    }

    [Fact]
    public async Task DiscoverModelsAsync_ReturnsModels()
    {
        var modelsJson = CreateModelsResponse(
            ("gpt-4o", "openai"),
            ("claude-sonnet-4.6", "anthropic"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.NotNull(models);
        Assert.Equal(2, models.Count);
    }

    [Fact]
    public async Task DiscoverModelsAsync_AllModels_HaveIds()
    {
        var modelsJson = CreateModelsResponse(
            ("gpt-4o", "openai"),
            ("claude-sonnet-4.6", "anthropic"),
            ("gemini-2.5-pro", "google"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.All(models, m =>
        {
            Assert.False(string.IsNullOrWhiteSpace(m.Id), "Model Id must not be empty");
            Assert.False(string.IsNullOrWhiteSpace(m.Name), "Model Name must not be empty");
        });
    }

    [Fact]
    public async Task DiscoverModelsAsync_ModelsHaveProviders()
    {
        var modelsJson = CreateModelsResponse(
            ("gpt-4o", "openai"),
            ("claude-sonnet-4.6", "anthropic"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.Contains(models, m => string.Equals(m.Provider, "openai", StringComparison.Ordinal));
        Assert.Contains(models, m => string.Equals(m.Provider, "anthropic", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DiscoverModelsAsync_KnownModels_ArePresent()
    {
        var modelsJson = CreateModelsResponse(
            ("gpt-4o", "openai"),
            ("claude-sonnet-4.6", "anthropic"),
            ("gemini-2.5-pro", "google"),
            ("grok-code-fast-1", "xai"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.Contains(models, m => string.Equals(m.Id, "gpt-4o", StringComparison.Ordinal));
        Assert.Contains(models, m => string.Equals(m.Id, "claude-sonnet-4.6", StringComparison.Ordinal));
        Assert.Contains(models, m => string.Equals(m.Id, "gemini-2.5-pro", StringComparison.Ordinal));
        Assert.Contains(models, m => string.Equals(m.Id, "grok-code-fast-1", StringComparison.Ordinal));
    }

    [Fact]
    public async Task DiscoverModelsAsync_EmptyData_ReturnsEmpty()
    {
        var modelsJson = JsonSerializer.Serialize(new { data = Array.Empty<object>() });

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.Empty(models);
    }

    [Fact]
    public async Task DiscoverModelsAsync_MissingDataProperty_ReturnsEmpty()
    {
        var modelsJson = JsonSerializer.Serialize(new { other = "value" });

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.Empty(models);
    }

    [Fact]
    public async Task DiscoverModelsAsync_ModelWithoutVendor_HasNullProvider()
    {
        var modelsJson = JsonSerializer.Serialize(new
        {
            data = new[] { new { id = "custom-model" } },
        });

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        var models = await discovery.DiscoverModelsAsync();

        Assert.Single(models);
        Assert.Equal("custom-model", models[0].Id);
        Assert.Null(models[0].Provider);
    }

    [Fact]
    public async Task DiscoverModelsAsync_SendsBearerAuth()
    {
        var modelsJson = CreateModelsResponse(("gpt-4o", "openai"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        await discovery.DiscoverModelsAsync();

        Assert.NotNull(modelsHandler.LastRequest);
        Assert.Equal("Bearer", modelsHandler.LastRequest!.Headers.Authorization?.Scheme);
        Assert.Equal("copilot-api-token-123", modelsHandler.LastRequest.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task DiscoverModelsAsync_RequestsModelsEndpoint()
    {
        var modelsJson = CreateModelsResponse(("gpt-4o", "openai"));

        using var modelsHandler = new FakeHandler(modelsJson);
        using var httpClient = new HttpClient(modelsHandler);
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        await discovery.DiscoverModelsAsync();

        Assert.NotNull(modelsHandler.LastRequest);
        Assert.EndsWith("/models", modelsHandler.LastRequest!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public void Constructor_ThrowsOnNullProvider()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotModelDiscovery(null!, new HttpClient(), NullLogger<CopilotModelDiscovery>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsOnNullHttpClient()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotModelDiscovery(_provider, null!, NullLogger<CopilotModelDiscovery>.Instance));
    }

    [Fact]
    public void Constructor_ThrowsOnNullLogger()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CopilotModelDiscovery(_provider, new HttpClient(), null!));
    }

    private static string CreateModelsResponse(params (string Id, string Vendor)[] models)
    {
        var data = models.Select(m => new { id = m.Id, vendor = m.Vendor }).ToArray();
        return JsonSerializer.Serialize(new { data });
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _responseBody;
        private readonly HttpStatusCode _statusCode;

        public FakeHandler(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _responseBody = responseBody;
            _statusCode = statusCode;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json"),
            });
        }
    }
}
