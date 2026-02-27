# Authentication Flow

The GitHub Copilot connector uses a **two-step OAuth token exchange** to authenticate
with the Copilot API.

## Step 1: Read local OAuth token

When you use GitHub Copilot in an editor (VS Code, JetBrains, etc.), the Copilot extension
stores an OAuth token locally:

| Platform | Path |
|---|---|
| Windows | `%LOCALAPPDATA%\github-copilot\apps.json` |
| macOS | `~/.config/github-copilot/apps.json` |
| Linux | `~/.config/github-copilot/apps.json` |

The file contains one or more entries:

```json
{
  "github.com:Iv23ctfURkiMfJ4xr5mv": {
    "user": "yourusername",
    "oauth_token": "ghu_xxxxxxxxxxxxxxxxxxxx"
  }
}
```

The connector reads the first `ghu_*` token it finds.

## Step 2: Exchange for Copilot API token

The OAuth token is exchanged for a short-lived Copilot API token:

```
GET https://api.github.com/copilot_internal/v2/token
Authorization: token ghu_xxxxxxxxxxxxxxxxxxxx
```

The response includes:

```json
{
  "token": "tid=...",
  "expires_at": 1700000000,
  "endpoints": {
    "api": "https://api.githubcopilot.com"
  }
}
```

The API token is valid for approximately **30 minutes** and is automatically refreshed
60 seconds before expiry.

## Step 3: API requests

All API calls use the Copilot endpoint with OpenAI-compatible paths:

```
POST https://api.githubcopilot.com/chat/completions
Authorization: Bearer tid=...
Editor-Version: vscode/1.104.1
User-Agent: GitHubCopilotChat/0.1
```

## Token priority

The connector resolves tokens in this order:

1. **Explicit `OAuthToken`** in `CopilotSessionOptions`
2. **`GITHUB_COPILOT_TOKEN`** environment variable
3. **`TokenFilePath`** option pointing to a specific file
4. **Auto-discovery** from well-known file locations

## Caching and thread safety

- API tokens are cached in memory until expiry
- A `SemaphoreSlim` ensures thread-safe token exchange
- On HTTP 401, the cache is invalidated and one retry is attempted

## Enterprise networks

For enterprise networks with SSL inspection:

```csharp
var options = new CopilotSessionOptions
{
    DangerouslyDisableSslValidation = true
};
```

Or via the CLI: `jdcplt --insecure`
