using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotModelsTests
{
    [Fact]
    public void Default_IsGpt4o() => Assert.Equal("gpt-4o", CopilotModels.Default);

    [Fact]
    public void Gpt4o_HasCorrectId() => Assert.Equal("gpt-4o", CopilotModels.Gpt4o);

    [Fact]
    public void Gpt41_HasCorrectId() => Assert.Equal("gpt-4.1", CopilotModels.Gpt41);

    [Fact]
    public void O4Mini_HasCorrectId() => Assert.Equal("o4-mini", CopilotModels.O4Mini);

    [Fact]
    public void ClaudeSonnet4_HasCorrectId() => Assert.Equal("claude-sonnet-4", CopilotModels.ClaudeSonnet4);

    [Fact]
    public void ClaudeSonnet35_HasCorrectId() => Assert.Equal("claude-3.5-sonnet", CopilotModels.ClaudeSonnet35);

    [Fact]
    public void Gemini20Flash_HasCorrectId() => Assert.Equal("gemini-2.0-flash", CopilotModels.Gemini20Flash);

    [Fact]
    public void Gemini25Pro_HasCorrectId() => Assert.Equal("gemini-2.5-pro", CopilotModels.Gemini25Pro);
}
