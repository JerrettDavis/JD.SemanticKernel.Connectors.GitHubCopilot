using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCopilotAuthentication_WithConfiguration_RegistersProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
(StringComparer.Ordinal)
            {
                ["CopilotSession:OAuthToken"] = "ghu_test123",
                ["CopilotSession:GitHubHost"] = "github.enterprise.com",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCopilotAuthentication(config);

        using var sp = services.BuildServiceProvider();
        var provider = sp.GetRequiredService<CopilotSessionProvider>();
        Assert.NotNull(provider);

        var options = sp.GetRequiredService<IOptions<CopilotSessionOptions>>().Value;
        Assert.Equal("ghu_test123", options.OAuthToken);
        Assert.Equal("github.enterprise.com", options.GitHubHost);
    }

    [Fact]
    public void AddCopilotAuthentication_WithDelegate_RegistersProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCopilotAuthentication(o => o.OAuthToken = "ghu_delegate");

        using var sp = services.BuildServiceProvider();
        var provider = sp.GetRequiredService<CopilotSessionProvider>();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddCopilotAuthentication_WithNoArgs_RegistersProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCopilotAuthentication();

        using var sp = services.BuildServiceProvider();
        var provider = sp.GetRequiredService<CopilotSessionProvider>();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddCopilotAuthentication_WithConfiguration_ThrowsOnNull()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() =>
            services.AddCopilotAuthentication((IConfiguration)null!));
    }
}
