using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotSessionOptionsTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var options = new CopilotSessionOptions();

        Assert.Null(options.TokenFilePath);
        Assert.Equal("github.com", options.GitHubHost);
        Assert.Equal(CopilotModels.Default, options.DefaultModel);
        Assert.Equal("vscode/1.104.1", options.EditorVersion);
        Assert.Equal("https://api.github.com/copilot_internal/v2/token", options.TokenExchangeUrl);
        Assert.Null(options.OAuthToken);
        Assert.False(options.DangerouslyDisableSslValidation);
        Assert.Null(options.CustomEndpoint);
    }

    [Fact]
    public void SectionName_IsCopilotSession()
    {
        Assert.Equal("CopilotSession", CopilotSessionOptions.SectionName);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new CopilotSessionOptions
        {
            TokenFilePath = "/custom/path.json",
            GitHubHost = "github.enterprise.com",
            DefaultModel = "claude-sonnet-4",
            EditorVersion = "vscode/2.0.0",
            TokenExchangeUrl = "https://custom.endpoint/token",
            OAuthToken = "ghu_test123",
            DangerouslyDisableSslValidation = true,
            CustomEndpoint = "https://proxy.internal",
        };

        Assert.Equal("/custom/path.json", options.TokenFilePath);
        Assert.Equal("github.enterprise.com", options.GitHubHost);
        Assert.Equal("claude-sonnet-4", options.DefaultModel);
        Assert.Equal("vscode/2.0.0", options.EditorVersion);
        Assert.Equal("https://custom.endpoint/token", options.TokenExchangeUrl);
        Assert.Equal("ghu_test123", options.OAuthToken);
        Assert.True(options.DangerouslyDisableSslValidation);
        Assert.Equal("https://proxy.internal", options.CustomEndpoint);
    }
}
