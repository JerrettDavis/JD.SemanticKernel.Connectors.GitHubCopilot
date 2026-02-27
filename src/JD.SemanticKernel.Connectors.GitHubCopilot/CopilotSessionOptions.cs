using JD.SemanticKernel.Connectors.Abstractions;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Configuration options for GitHub Copilot session authentication.
/// Bind from configuration section <c>"CopilotSession"</c> or configure via
/// <see cref="ServiceCollectionExtensions.AddCopilotAuthentication(Microsoft.Extensions.DependencyInjection.IServiceCollection, Action{CopilotSessionOptions})"/>.
/// </summary>
public sealed class CopilotSessionOptions : SessionOptionsBase
{
    /// <summary>The default configuration section name.</summary>
    public const string SectionName = "CopilotSession";

    /// <summary>
    /// Explicit path to the Copilot token file (<c>apps.json</c> or <c>hosts.json</c>).
    /// When <see langword="null"/>, auto-discovers from standard Copilot config directories.
    /// </summary>
    public string? TokenFilePath { get; set; }

    /// <summary>
    /// The GitHub host to authenticate against. Defaults to <c>"github.com"</c>.
    /// Set this for GitHub Enterprise Server deployments.
    /// </summary>
    public string GitHubHost { get; set; } = "github.com";

    /// <summary>
    /// The default AI model to use for chat completions.
    /// Defaults to <see cref="CopilotModels.Default"/>.
    /// </summary>
    public string DefaultModel { get; set; } = CopilotModels.Default;

    /// <summary>
    /// The <c>Editor-Version</c> header value required by the Copilot API.
    /// Defaults to a compatible VS Code version string.
    /// </summary>
    public string EditorVersion { get; set; } = "vscode/1.104.1";

    /// <summary>
    /// Override URL for the Copilot token exchange endpoint.
    /// Defaults to <c>https://api.github.com/copilot_internal/v2/token</c>.
    /// </summary>
    public string TokenExchangeUrl { get; set; } =
        "https://api.github.com/copilot_internal/v2/token";

    /// <summary>
    /// Override: use an explicit OAuth token instead of reading from the local config file.
    /// Useful for CI/CD environments where the token is injected as a secret.
    /// </summary>
    public string? OAuthToken { get; set; }
}
