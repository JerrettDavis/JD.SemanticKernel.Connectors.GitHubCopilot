---
_layout: landing
---

# JD.SemanticKernel.Connectors.GitHubCopilot

Wire your **GitHub Copilot** subscription into any
[Semantic Kernel](https://learn.microsoft.com/semantic-kernel) application with a single
extension method call.

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion()   // reads local Copilot credentials automatically
    .Build();
```

No API keys to manage. No token files to maintain. If you have a GitHub Copilot subscription
and have used Copilot in VS Code or another editor, the package finds your credentials and
handles the two-step token exchange automatically.

---

## Why this package?

GitHub Copilot exposes an **OpenAI-compatible API** behind a two-step OAuth token exchange.
Your local editor stores a `ghu_*` OAuth token in `apps.json`. This package reads that token,
exchanges it for a short-lived Copilot API token via the GitHub API, and injects the
appropriate `Authorization`, `Editor-Version`, and `User-Agent` headers transparently.

The result: any Semantic Kernel application can use **GPT-4o, Claude, Gemini, o4-mini**, and
other models available through your Copilot subscription — without managing separate API keys
for each provider.

---

## Quick links

| Topic | Description |
|---|---|
| [Getting Started](articles/getting-started.md) | Minimal working example — SK kernel in five lines |
| [Authentication Flow](articles/authentication-flow.md) | How the two-step token exchange works |
| [Kernel Builder Integration](articles/kernel-builder-integration.md) | Full `UseCopilotChatCompletion()` reference |
| [Configuration Reference](articles/configuration-reference.md) | All options, environment variables, enterprise config |
| [Model Discovery](articles/model-discovery.md) | Query available models from your subscription |
| [Sample: CopilotChat CLI](samples/copilot-chat.md) | Interactive chat sample using the connector |
| [API Reference](api/index.md) | Full generated API docs |

---

## Available models (through Copilot)

| Model | Provider | ID |
|---|---|---|
| GPT-4o | OpenAI | `gpt-4o` |
| GPT-4.1 | OpenAI | `gpt-4.1` |
| Claude Sonnet 4 | Anthropic | `claude-sonnet-4` |
| Gemini 2.5 Pro | Google | `gemini-2.5-pro` |
| o4-mini | OpenAI | `o4-mini` |

Use `CopilotModelDiscovery` to enumerate all models available to your subscription at runtime.

---

## Shared Abstractions

This package includes **JD.SemanticKernel.Connectors.Abstractions** — a shared interface
layer for multi-provider connector bridging:

| Interface | Purpose |
|---|---|
| `ISessionProvider` | Credential resolution and lifecycle |
| `IModelDiscoveryProvider` | Runtime model enumeration |
| `SessionCredentials` | Normalized token + expiry record |
| `SessionOptionsBase` | Shared config (SSL bypass, custom endpoint) |

The [ClaudeCode connector](https://github.com/JerrettDavis/ClaudeCodeAuthenticationProvider)
also implements these interfaces, enabling **MCP server bridging** across subscriptions.

---

## Supported targets

| TFM | SK extensions | Core auth types |
|---|---|---|
| `netstandard2.0` | — | ✓ |
| `net8.0` | ✓ | ✓ |
| `net10.0` | ✓ | ✓ |
