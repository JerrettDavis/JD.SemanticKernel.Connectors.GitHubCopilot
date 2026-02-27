using JD.SemanticKernel.Connectors.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.IntegrationTests;

/// <summary>
/// Integration tests for the Copilot authentication flow.
/// Requires a valid Copilot subscription and local credentials.
/// </summary>
public class CopilotAuthFlowTests : IDisposable
{
    private readonly CopilotSessionProvider _provider;

    public CopilotAuthFlowTests()
    {
        var options = new CopilotSessionOptions();
        _provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance);
    }

    public void Dispose() => _provider.Dispose();

    [SkippableFact]
    public async Task CanReadLocalCredentials()
    {
        IntegrationGuard.EnsureEnabled();

        var isAuthenticated = await _provider.IsAuthenticatedAsync();

        Assert.True(isAuthenticated, "Expected local Copilot credentials to be available.");
    }

    [SkippableFact]
    public async Task TokenExchange_ReturnsValidToken()
    {
        IntegrationGuard.EnsureEnabled();

        var credentials = await _provider.GetCredentialsAsync();

        Assert.NotNull(credentials);
        Assert.False(string.IsNullOrWhiteSpace(credentials.Token), "Token should not be empty.");
        Assert.False(credentials.IsExpired, "Freshly exchanged token should not be expired.");
    }

    [SkippableFact]
    public async Task TokenExchange_ReturnsExpirationTime()
    {
        IntegrationGuard.EnsureEnabled();

        var credentials = await _provider.GetCredentialsAsync();

        Assert.NotNull(credentials.ExpiresAt);
        Assert.True(credentials.ExpiresAt > DateTimeOffset.UtcNow,
            "Expiry should be in the future.");
    }

    [SkippableFact]
    public async Task ModelDiscovery_ReturnsModels()
    {
        IntegrationGuard.EnsureEnabled();

        using var httpClient = new HttpClient();
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        try
        {
            var models = await discovery.DiscoverModelsAsync();

            Assert.NotEmpty(models);
            Assert.All(models, m =>
                Assert.False(string.IsNullOrWhiteSpace(m.Id), "All models must have an ID."));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400") || ex.Message.Contains("404"))
        {
            Skip.If(true, "Copilot /models endpoint not available on this API proxy.");
        }
    }

    [SkippableFact]
    public async Task ModelDiscovery_ContainsKnownModels()
    {
        IntegrationGuard.EnsureEnabled();

        using var httpClient = new HttpClient();
        var discovery = new CopilotModelDiscovery(
            _provider, httpClient, NullLogger<CopilotModelDiscovery>.Instance);

        try
        {
            var models = await discovery.DiscoverModelsAsync();

            // At minimum, gpt-4o should be available on all Copilot subscriptions
            Assert.Contains(models, m => string.Equals(m.Id, CopilotModels.Gpt4o, StringComparison.Ordinal));
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400") || ex.Message.Contains("404"))
        {
            Skip.If(true, "Copilot /models endpoint not available on this API proxy.");
        }
    }

    [SkippableFact]
    public async Task ApiEndpoint_IsPopulatedAfterExchange()
    {
        IntegrationGuard.EnsureEnabled();

        await _provider.GetCredentialsAsync();

        Assert.False(string.IsNullOrWhiteSpace(_provider.ApiEndpoint),
            "ApiEndpoint should be set after token exchange.");
    }
}
