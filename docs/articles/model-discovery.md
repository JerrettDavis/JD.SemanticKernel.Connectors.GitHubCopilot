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
gpt-4o (openai) — GPT-4o
gpt-4.1 (openai) — GPT-4.1
claude-sonnet-4 (anthropic) — Claude Sonnet 4
gemini-2.5-pro (google) — Gemini 2.5 Pro
o4-mini (openai) — o4-mini
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

The `CopilotModels` class provides constants for commonly used models:

```csharp
public static class CopilotModels
{
    public const string Default = "gpt-4o";
    public const string Gpt4o = "gpt-4o";
    public const string Gpt41 = "gpt-4.1";
    public const string ClaudeSonnet4 = "claude-sonnet-4";
    public const string Gemini25Pro = "gemini-2.5-pro";
    public const string O4Mini = "o4-mini";
}
```
