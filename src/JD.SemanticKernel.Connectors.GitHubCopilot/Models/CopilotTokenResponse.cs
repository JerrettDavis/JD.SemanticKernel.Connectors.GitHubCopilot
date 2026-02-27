using System.Text.Json.Serialization;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Models;

/// <summary>
/// Response from the Copilot token exchange endpoint
/// (<c>GET /copilot_internal/v2/token</c>).
/// </summary>
public sealed record CopilotTokenResponse
{
    /// <summary>Short-lived API token for Copilot requests.</summary>
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    /// <summary>Unix epoch seconds at which the token expires.</summary>
    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; init; }

    /// <summary>Suggested refresh interval in seconds.</summary>
    [JsonPropertyName("refresh_in")]
    public int RefreshIn { get; init; }

    /// <summary>Endpoints block containing the API base URL.</summary>
    [JsonPropertyName("endpoints")]
    public CopilotEndpoints? Endpoints { get; init; }

    /// <summary>SKU identifier for the Copilot subscription.</summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; init; }

    /// <summary>The expiry time as a <see cref="DateTimeOffset"/>.</summary>
    public DateTimeOffset ExpiresAtUtc => DateTimeOffset.FromUnixTimeSeconds(ExpiresAt);
}

/// <summary>Endpoints advertised by the token exchange response.</summary>
public sealed record CopilotEndpoints
{
    /// <summary>The base URL for Copilot API requests (e.g. <c>https://api.githubcopilot.com</c>).</summary>
    [JsonPropertyName("api")]
    public string Api { get; init; } = "https://api.githubcopilot.com";
}
