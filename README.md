# JD.SemanticKernel.Connectors.GitHubCopilot

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![CI](https://github.com/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot/actions/workflows/ci.yml/badge.svg)](https://github.com/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/JD.SemanticKernel.Connectors.GitHubCopilot.svg)](https://www.nuget.org/packages/JD.SemanticKernel.Connectors.GitHubCopilot)
[![codecov](https://codecov.io/gh/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot/graph/badge.svg)](https://codecov.io/gh/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot)

**Use your GitHub Copilot subscription as an AI backend for [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) applications.**

This connector reads your local Copilot OAuth credentials, exchanges them for short-lived API tokens, and injects authentication into SK's OpenAI-compatible chat completion — giving you access to GPT-4o, Claude, Gemini, and more through a single Copilot subscription.

## Quick Start

```bash
dotnet add package JD.SemanticKernel.Connectors.GitHubCopilot
```

```csharp
using JD.SemanticKernel.Connectors.GitHubCopilot;
using Microsoft.SemanticKernel;

// One-liner: reads credentials from your local Copilot installation
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion()
    .Build();

// Use any model available through Copilot
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet4)
    .Build();
```

## How It Works

```
┌──────────────────────┐     ┌─────────────────┐     ┌──────────────────────┐
│  Local Copilot Auth  │     │  Token Exchange  │     │    Copilot API       │
│  apps.json/hosts.json│────▶│  api.github.com  │────▶│  chat/completions    │
│  (ghu_* OAuth token) │     │  /copilot_internal│    │  (OpenAI-compatible) │
└──────────────────────┘     │  /v2/token       │     └──────────────────────┘
                             └─────────────────┘
                              ▲ Short-lived token
                              │ (~30 min TTL)
                              │ Auto-refreshed
```

1. **Reads** your local Copilot OAuth token from `%LOCALAPPDATA%/github-copilot/apps.json` (Windows) or `~/.config/github-copilot/hosts.json` (Linux/macOS)
2. **Exchanges** it for a short-lived API token via GitHub's internal token endpoint
3. **Caches** the API token with TTL-aware refresh (thread-safe, SemaphoreSlim double-check locking)
4. **Injects** auth headers into every SK request via a DelegatingHandler

## Available Models

Copilot provides access to **41+ models** from multiple AI providers through a single subscription.
Use `CopilotModelDiscovery` to discover all available models at runtime, or reference well-known constants:

### Key Models (Constants)

| Constant | Model ID | Provider | Best For |
|---|---|---|---|
| **Anthropic** | | | |
| `CopilotModels.ClaudeOpus46` | `claude-opus-4.6` | Anthropic | Deep reasoning, complex analysis |
| `CopilotModels.ClaudeOpus46Fast` | `claude-opus-4.6-fast` | Anthropic | Faster Opus variant |
| `CopilotModels.ClaudeSonnet46` | `claude-sonnet-4.6` | Anthropic | Balanced quality/speed (default) |
| `CopilotModels.ClaudeSonnet4` | `claude-sonnet-4` | Anthropic | Reliable completions |
| `CopilotModels.ClaudeHaiku45` | `claude-haiku-4.5` | Anthropic | Fast, lightweight tasks |
| **OpenAI** | | | |
| `CopilotModels.Gpt53Codex` | `gpt-5.3-codex` | OpenAI | Latest code-specialized model |
| `CopilotModels.Gpt52` | `gpt-5.2` | OpenAI | Complex reasoning |
| `CopilotModels.Gpt51Codex` | `gpt-5.1-codex` | OpenAI | Code generation |
| `CopilotModels.Gpt5Mini` | `gpt-5-mini` | Azure OpenAI | Fast general-purpose |
| `CopilotModels.Gpt4o` | `gpt-4o` | Azure OpenAI | Multimodal |
| **Google** | | | |
| `CopilotModels.Gemini31Pro` | `gemini-3.1-pro-preview` | Google | Latest Gemini model |
| `CopilotModels.Gemini3Pro` | `gemini-3-pro-preview` | Google | Long-context reasoning |
| `CopilotModels.Gemini3Flash` | `gemini-3-flash-preview` | Google | Fast responses |
| **xAI** | | | |
| `CopilotModels.GrokCodeFast1` | `grok-code-fast-1` | xAI | Code generation |

### Runtime Discovery

```csharp
var discovery = new CopilotModelDiscovery(provider, httpClient, logger);
var models = await discovery.DiscoverModelsAsync();
// Returns all 41+ models including embedding and legacy variants
```

## Configuration

### Options

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet46, options =>
    {
        // Override token file location
        options.TokenFilePath = "/custom/path/apps.json";

        // GitHub Enterprise Server
        options.GitHubHost = "github.enterprise.com";

        // Enterprise SSL bypass
        options.DangerouslyDisableSslValidation = true;

        // Custom endpoint
        options.CustomEndpoint = "https://proxy.internal/copilot";
    })
    .Build();
```

### Dependency Injection

```csharp
services.AddCopilotAuthentication(configuration);
// or
services.AddCopilotAuthentication(o => o.OAuthToken = "ghu_...");
```

### Environment Variables

| Variable | Description |
|---|---|
| `GITHUB_COPILOT_TOKEN` | Override OAuth token (skips file lookup) |

## Packages

| Package | Description |
|---|---|
| [`JD.SemanticKernel.Connectors.Abstractions`](https://www.nuget.org/packages/JD.SemanticKernel.Connectors.Abstractions) | Shared interfaces for multi-provider connectors |
| [`JD.SemanticKernel.Connectors.GitHubCopilot`](https://www.nuget.org/packages/JD.SemanticKernel.Connectors.GitHubCopilot) | GitHub Copilot authentication connector |
| [`JD.Tools.CopilotChat`](https://www.nuget.org/packages/JD.Tools.CopilotChat) | `jdcplt` — Interactive chat CLI demo |

## Sample CLI: `jdcplt`

```bash
dotnet tool install -g JD.Tools.CopilotChat

# Interactive chat
jdcplt

# Single prompt
jdcplt --prompt "Explain async/await in C#"

# Choose a model
jdcplt --model claude-sonnet-4 --prompt "Write a haiku about code"

# List available models
jdcplt --list-models

# Enterprise SSL bypass
jdcplt --insecure
```

## Documentation

Full documentation is available at [GitHub Pages](https://jerrettdavis.github.io/JD.SemanticKernel.Connectors.GitHubCopilot/)
or in the [`docs/`](docs/) directory:

- [Getting Started](docs/articles/getting-started.md)
- [Authentication Flow](docs/articles/authentication-flow.md)
- [Configuration Reference](docs/articles/configuration-reference.md)
- [Model Discovery](docs/articles/model-discovery.md)

## Related Projects

- **[JD.SemanticKernel.Connectors.ClaudeCode](https://github.com/JerrettDavis/ClaudeCodeAuthenticationProvider)** — Same pattern for Claude Code subscriptions

Both connectors implement the shared `JD.SemanticKernel.Connectors.Abstractions` interfaces,
enabling **MCP server bridging** across subscriptions.

## Building

```bash
git clone https://github.com/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot.git
cd JD.SemanticKernel.Connectors.GitHubCopilot
dotnet build
dotnet test
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

[MIT](LICENSE)
