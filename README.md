# JD.SemanticKernel.Connectors.GitHubCopilot

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/JD.SemanticKernel.Connectors.GitHubCopilot.svg)](https://www.nuget.org/packages/JD.SemanticKernel.Connectors.GitHubCopilot)

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

Copilot provides access to multiple AI providers through a single subscription:

| Constant | Model ID | Provider |
|---|---|---|
| `CopilotModels.Gpt4o` | `gpt-4o` | OpenAI |
| `CopilotModels.Gpt41` | `gpt-4.1` | OpenAI |
| `CopilotModels.O4Mini` | `o4-mini` | OpenAI |
| `CopilotModels.ClaudeSonnet4` | `claude-sonnet-4` | Anthropic |
| `CopilotModels.ClaudeSonnet35` | `claude-3.5-sonnet` | Anthropic |
| `CopilotModels.Gemini20Flash` | `gemini-2.0-flash` | Google |
| `CopilotModels.Gemini25Pro` | `gemini-2.5-pro` | Google |

Discover models at runtime:

```csharp
var discovery = new CopilotModelDiscovery(provider, httpClient, logger);
var models = await discovery.DiscoverModelsAsync();
```

## Configuration

### Options

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(CopilotModels.Gpt4o, options =>
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

## Related Projects

- **[JD.SemanticKernel.Connectors.ClaudeCode](https://github.com/JerrettDavis/ClaudeCodeAuthenticationProvider)** — Same pattern for Claude Code subscriptions

## Prerequisites

- [GitHub Copilot](https://github.com/features/copilot) subscription (Individual, Business, or Enterprise)
- GitHub Copilot CLI installed and signed in
- .NET 8.0+ SDK

## License

[MIT](LICENSE)
