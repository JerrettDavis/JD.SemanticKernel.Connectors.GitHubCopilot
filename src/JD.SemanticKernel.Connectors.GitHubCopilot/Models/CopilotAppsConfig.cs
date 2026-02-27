using System.Text.Json.Serialization;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Models;

/// <summary>
/// Represents the structure of a Copilot <c>apps.json</c> or <c>hosts.json</c> file.
/// Keys are host identifiers (e.g. <c>"github.com"</c> or <c>"github.com:AppId"</c>).
/// </summary>
/// <remarks>
/// The file is a dictionary of host → entry. We deserialize into
/// <c>Dictionary&lt;string, CopilotAppsEntry&gt;</c> directly.
/// </remarks>
public sealed record CopilotAppsEntry
{
    /// <summary>The GitHub username.</summary>
    [JsonPropertyName("user")]
    public string User { get; init; } = string.Empty;

    /// <summary>The long-lived OAuth token (<c>ghu_*</c> or <c>gho_*</c>).</summary>
    [JsonPropertyName("oauth_token")]
    public string OAuthToken { get; init; } = string.Empty;

    /// <summary>Optional GitHub App ID associated with this entry.</summary>
    [JsonPropertyName("githubAppId")]
    public string? GitHubAppId { get; init; }
}
