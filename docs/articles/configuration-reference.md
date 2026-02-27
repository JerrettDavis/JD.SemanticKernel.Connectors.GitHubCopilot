# Configuration Reference

All configuration options for the GitHub Copilot connector.

## CopilotSessionOptions

| Property | Type | Default | Description |
|---|---|---|---|
| `TokenFilePath` | `string?` | `null` | Explicit path to `apps.json` or `hosts.json` |
| `GitHubHost` | `string` | `"github.com"` | GitHub host for token lookup |
| `DefaultModel` | `string` | `"gpt-4o"` | Model ID for chat completions |
| `EditorVersion` | `string` | `"vscode/1.104.1"` | Editor-Version header value |
| `TokenExchangeUrl` | `string` | `"https://api.github.com/copilot_internal/v2/token"` | Token exchange endpoint |
| `OAuthToken` | `string?` | `null` | Explicit OAuth token override |
| `DangerouslyDisableSslValidation` | `bool` | `false` | Disable SSL certificate validation |
| `CustomEndpoint` | `string?` | `null` | Override API endpoint URL |

## Environment variables

| Variable | Purpose |
|---|---|
| `GITHUB_COPILOT_TOKEN` | OAuth token override (skips file lookup) |

## Token file locations

The connector searches these paths in order:

### Windows

1. `%LOCALAPPDATA%\github-copilot\apps.json`
2. `%LOCALAPPDATA%\github-copilot\hosts.json`

### macOS / Linux

1. `~/.config/github-copilot/apps.json`
2. `~/.config/github-copilot/hosts.json`

## Enterprise configuration

### SSL bypass

For networks with SSL inspection proxies:

```csharp
var options = new CopilotSessionOptions
{
    DangerouslyDisableSslValidation = true
};
```

### Custom endpoint

For GitHub Enterprise Server:

```csharp
var options = new CopilotSessionOptions
{
    CustomEndpoint = "https://copilot.internal.company.com",
    TokenExchangeUrl = "https://github.internal.company.com/copilot_internal/v2/token"
};
```

### GitHub Enterprise host

```csharp
var options = new CopilotSessionOptions
{
    GitHubHost = "github.mycompany.com"
};
```

## CLI options (jdcplt)

| Flag | Description |
|---|---|
| `--model` / `-m` | Model ID (default: `gpt-4o`) |
| `--prompt` / `-p` | Single-shot prompt (exits after response) |
| `--system` / `-s` | System prompt |
| `--list-models` | List available models and exit |
| `--insecure` | Disable SSL validation |
