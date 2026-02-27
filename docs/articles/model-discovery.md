# Model Discovery

The `CopilotModelDiscovery` class lets you enumerate all models available through your
Copilot subscription at runtime.

## Usage

```csharp
using JD.SemanticKernel.Connectors.GitHubCopilot;

var provider = new CopilotSessionProvider(new CopilotSessionOptions());
var discovery = new CopilotModelDiscovery(provider);

var models = await discovery.DiscoverModelsAsync();

foreach (var model in models)
{
    Console.WriteLine($"{model.Id} ({model.Provider}) — {model.Name}");
}
```

## Output example

```
claude-opus-4.6 (Anthropic) — Claude Opus 4.6
claude-opus-4.6-fast (Anthropic) — Claude Opus 4.6 (fast mode)
claude-sonnet-4.6 (Anthropic) — Claude Sonnet 4.6
claude-sonnet-4 (Anthropic) — Claude Sonnet 4
claude-haiku-4.5 (Anthropic) — Claude Haiku 4.5
gpt-5.3-codex (OpenAI) — GPT-5.3-Codex
gpt-5.2 (OpenAI) — GPT-5.2
gpt-5-mini (Azure OpenAI) — GPT-5 mini
gemini-3.1-pro-preview (Google) — Gemini 3.1 Pro
grok-code-fast-1 (xAI) — Grok Code Fast 1
... (41+ models total)
```

## Interface

`CopilotModelDiscovery` implements `IModelDiscoveryProvider` from the shared abstractions:

```csharp
public interface IModelDiscoveryProvider
{
    Task<IReadOnlyList<ModelInfo>> DiscoverModelsAsync(
        CancellationToken cancellationToken = default);
}
```

## ModelInfo record

```csharp
public sealed record ModelInfo(string Id, string Name, string Provider);
```

## CLI model listing

The `jdcplt` tool includes model listing:

```bash
jdcplt --list-models
```

## Known model constants

The `CopilotModels` class provides constants for commonly used models across 5 providers:

```csharp
public static class CopilotModels
{
    // Default
    public const string Default = "claude-sonnet-4.6";

    // Anthropic
    public const string ClaudeOpus46 = "claude-opus-4.6";
    public const string ClaudeOpus46Fast = "claude-opus-4.6-fast";
    public const string ClaudeSonnet46 = "claude-sonnet-4.6";
    public const string ClaudeSonnet4 = "claude-sonnet-4";
    public const string ClaudeHaiku45 = "claude-haiku-4.5";

    // OpenAI
    public const string Gpt53Codex = "gpt-5.3-codex";
    public const string Gpt52 = "gpt-5.2";
    public const string Gpt51Codex = "gpt-5.1-codex";
    public const string Gpt5Mini = "gpt-5-mini";
    public const string Gpt4o = "gpt-4o";

    // Google
    public const string Gemini31Pro = "gemini-3.1-pro-preview";
    public const string Gemini3Pro = "gemini-3-pro-preview";
    public const string Gemini3Flash = "gemini-3-flash-preview";

    // xAI
    public const string GrokCodeFast1 = "grok-code-fast-1";
}
```
