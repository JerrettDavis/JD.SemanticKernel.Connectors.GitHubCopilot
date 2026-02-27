using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

public class CopilotModelsTests
{
    [Fact]
    public void Default_IsClaudeSonnet46() => Assert.Equal("claude-sonnet-4.6", CopilotModels.Default);

    // ── OpenAI (via Azure OpenAI) ───────────────────────────────────

    [Fact]
    public void Gpt4o_HasCorrectId() => Assert.Equal("gpt-4o", CopilotModels.Gpt4o);

    [Fact]
    public void Gpt41_HasCorrectId() => Assert.Equal("gpt-4.1", CopilotModels.Gpt41);

    [Fact]
    public void Gpt4oMini_HasCorrectId() => Assert.Equal("gpt-4o-mini", CopilotModels.Gpt4oMini);

    [Fact]
    public void Gpt5Mini_HasCorrectId() => Assert.Equal("gpt-5-mini", CopilotModels.Gpt5Mini);

    // ── OpenAI (direct) ─────────────────────────────────────────────

    [Fact]
    public void Gpt51_HasCorrectId() => Assert.Equal("gpt-5.1", CopilotModels.Gpt51);

    [Fact]
    public void Gpt51Codex_HasCorrectId() => Assert.Equal("gpt-5.1-codex", CopilotModels.Gpt51Codex);

    [Fact]
    public void Gpt51CodexMax_HasCorrectId() => Assert.Equal("gpt-5.1-codex-max", CopilotModels.Gpt51CodexMax);

    [Fact]
    public void Gpt51CodexMini_HasCorrectId() => Assert.Equal("gpt-5.1-codex-mini", CopilotModels.Gpt51CodexMini);

    [Fact]
    public void Gpt52_HasCorrectId() => Assert.Equal("gpt-5.2", CopilotModels.Gpt52);

    [Fact]
    public void Gpt52Codex_HasCorrectId() => Assert.Equal("gpt-5.2-codex", CopilotModels.Gpt52Codex);

    [Fact]
    public void Gpt53Codex_HasCorrectId() => Assert.Equal("gpt-5.3-codex", CopilotModels.Gpt53Codex);

    // ── Anthropic ──────────────────────────────────────────────────

    [Fact]
    public void ClaudeOpus46_HasCorrectId() => Assert.Equal("claude-opus-4.6", CopilotModels.ClaudeOpus46);

    [Fact]
    public void ClaudeOpus46Fast_HasCorrectId() => Assert.Equal("claude-opus-4.6-fast", CopilotModels.ClaudeOpus46Fast);

    [Fact]
    public void ClaudeOpus45_HasCorrectId() => Assert.Equal("claude-opus-4.5", CopilotModels.ClaudeOpus45);

    [Fact]
    public void ClaudeSonnet46_HasCorrectId() => Assert.Equal("claude-sonnet-4.6", CopilotModels.ClaudeSonnet46);

    [Fact]
    public void ClaudeSonnet45_HasCorrectId() => Assert.Equal("claude-sonnet-4.5", CopilotModels.ClaudeSonnet45);

    [Fact]
    public void ClaudeSonnet4_HasCorrectId() => Assert.Equal("claude-sonnet-4", CopilotModels.ClaudeSonnet4);

    [Fact]
    public void ClaudeHaiku45_HasCorrectId() => Assert.Equal("claude-haiku-4.5", CopilotModels.ClaudeHaiku45);

    // ── Google ─────────────────────────────────────────────────────

    [Fact]
    public void Gemini25Pro_HasCorrectId() => Assert.Equal("gemini-2.5-pro", CopilotModels.Gemini25Pro);

    [Fact]
    public void Gemini3Pro_HasCorrectId() => Assert.Equal("gemini-3-pro-preview", CopilotModels.Gemini3Pro);

    [Fact]
    public void Gemini31Pro_HasCorrectId() => Assert.Equal("gemini-3.1-pro-preview", CopilotModels.Gemini31Pro);

    [Fact]
    public void Gemini3Flash_HasCorrectId() => Assert.Equal("gemini-3-flash-preview", CopilotModels.Gemini3Flash);

    // ── xAI ────────────────────────────────────────────────────────

    [Fact]
    public void GrokCodeFast1_HasCorrectId() => Assert.Equal("grok-code-fast-1", CopilotModels.GrokCodeFast1);
}
