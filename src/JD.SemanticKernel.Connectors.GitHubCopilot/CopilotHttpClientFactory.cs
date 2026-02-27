using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Creates pre-configured <see cref="HttpClient"/> instances that inject GitHub Copilot
/// authentication headers on every request.
///
/// <para>
/// Use this factory when you want to bring your own OpenAI SDK integration but still
/// benefit from automatic Copilot session resolution.  For example:
/// </para>
/// <code>
/// var httpClient = CopilotHttpClientFactory.Create();
/// // Use with any OpenAI-compatible client pointed at Copilot's endpoint
/// </code>
/// <para>
/// <strong>Important:</strong> The returned <see cref="HttpClient"/> owns the
/// underlying <see cref="CopilotSessionHttpHandler"/> and
/// <see cref="CopilotSessionProvider"/>. Dispose the <see cref="HttpClient"/>
/// when you are done to release all unmanaged resources.
/// </para>
/// </summary>
public static class CopilotHttpClientFactory
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> wired with <see cref="CopilotSessionHttpHandler"/>
    /// using auto-resolved credentials (config file → environment variables).
    /// </summary>
    public static HttpClient Create() => Create(configure: null);

    /// <summary>
    /// Creates an <see cref="HttpClient"/> authenticated with the supplied <paramref name="oauthToken"/>.
    /// </summary>
    /// <param name="oauthToken">
    /// A GitHub OAuth token (<c>ghu_*</c> or <c>gho_*</c>).
    /// </param>
    public static HttpClient Create(string oauthToken) =>
        Create(o => o.OAuthToken = oauthToken);

    /// <summary>
    /// Creates an <see cref="HttpClient"/> with options configured by <paramref name="configure"/>.
    /// </summary>
    /// <param name="configure">
    /// Optional delegate to customise <see cref="CopilotSessionOptions"/> before the
    /// <see cref="CopilotSessionProvider"/> is constructed.
    /// </param>
    public static HttpClient Create(Action<CopilotSessionOptions>? configure)
    {
        var options = new CopilotSessionOptions();
        configure?.Invoke(options);

        var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance);

        return new HttpClient(new CopilotSessionHttpHandler(
            provider, options.DangerouslyDisableSslValidation, options.EditorVersion));
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by an existing <paramref name="provider"/>.
    /// Use this overload when the provider is already registered in a DI container.
    /// </summary>
    /// <param name="provider">A pre-configured session provider.</param>
    /// <param name="dangerouslyDisableSslValidation">
    /// When <see langword="true"/>, disables SSL/TLS certificate validation.
    /// Intended only for enterprise proxies with self-signed certificates.
    /// </param>
    public static HttpClient Create(
        CopilotSessionProvider provider,
        bool dangerouslyDisableSslValidation = false) =>
        new HttpClient(new CopilotSessionHttpHandler(
            provider ?? throw new ArgumentNullException(nameof(provider)),
            dangerouslyDisableSslValidation));
}
