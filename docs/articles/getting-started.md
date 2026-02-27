# Getting Started

Install the NuGet package and start using GitHub Copilot models in your Semantic Kernel
application.

## Prerequisites

- A [GitHub Copilot](https://github.com/features/copilot) subscription (Individual, Business, or Enterprise)
- GitHub Copilot activated in at least one editor (VS Code, JetBrains, etc.)
- .NET 8.0 or later (for `KernelBuilder` extensions)

## Install

```bash
dotnet add package JD.SemanticKernel.Connectors.GitHubCopilot
```

## Minimal example

```csharp
using Microsoft.SemanticKernel;
using JD.SemanticKernel.Connectors.GitHubCopilot;

var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion()          // auto-discovers credentials
    .Build();

var result = await kernel.InvokePromptAsync("What is dependency injection?");
Console.WriteLine(result);
```

That is the entire setup. The connector reads your local Copilot OAuth token, exchanges it
for a short-lived API token, and routes the chat completion through the Copilot API.

## Choosing a model

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet4)
    .Build();
```

Available model constants are in `CopilotModels`:

| Constant | Model ID |
|---|---|
| `Default` | `gpt-4o` |
| `Gpt4o` | `gpt-4o` |
| `Gpt41` | `gpt-4.1` |
| `ClaudeSonnet4` | `claude-sonnet-4` |
| `Gemini25Pro` | `gemini-2.5-pro` |
| `O4Mini` | `o4-mini` |

## Next steps

- [Authentication Flow](authentication-flow.md) — understand the two-step token exchange
- [Configuration Reference](configuration-reference.md) — all available options
- [Model Discovery](model-discovery.md) — enumerate models at runtime
