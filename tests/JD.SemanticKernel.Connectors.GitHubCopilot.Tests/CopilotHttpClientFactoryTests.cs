using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotHttpClientFactoryTests
{
    [Fact]
    public void Create_WithNoArgs_ReturnsHttpClient()
    {
        using var client = CopilotHttpClientFactory.Create();
        Assert.NotNull(client);
    }

    [Fact]
    public void Create_WithOAuthToken_ReturnsHttpClient()
    {
        using var client = CopilotHttpClientFactory.Create("ghu_test_token");
        Assert.NotNull(client);
    }

    [Fact]
    public void Create_WithConfigure_ReturnsHttpClient()
    {
        using var client = CopilotHttpClientFactory.Create(o =>
        {
            o.OAuthToken = "ghu_configured";
            o.DangerouslyDisableSslValidation = true;
        });
        Assert.NotNull(client);
    }

    [Fact]
    public void Create_WithProvider_ReturnsHttpClient()
    {
        var options = new CopilotSessionOptions { OAuthToken = "ghu_provider_test" };
        using var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance);

        using var client = CopilotHttpClientFactory.Create(provider);
        Assert.NotNull(client);
    }

    [Fact]
    public void Create_WithProvider_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CopilotHttpClientFactory.Create(provider: null!));
    }
}
