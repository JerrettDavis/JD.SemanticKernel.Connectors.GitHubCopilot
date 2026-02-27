using System.Net.Http.Headers;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// A <see cref="DelegatingHandler"/> that injects GitHub Copilot API authentication
/// headers into every outgoing HTTP request.
///
/// <para>
/// Injects the following headers on each request:
/// <list type="bullet">
///   <item><description><c>Authorization: Bearer {copilot_api_token}</c></description></item>
///   <item><description><c>Editor-Version: vscode/{version}</c></description></item>
///   <item><description><c>User-Agent: GitHubCopilotChat/0.1</c></description></item>
///   <item><description><c>Copilot-Integration-Id: vscode-chat</c></description></item>
/// </list>
/// </para>
///
/// <para>On 401 responses, the handler invalidates the cached token and retries once.</para>
/// </summary>
public sealed class CopilotSessionHttpHandler : DelegatingHandler
{
    private readonly CopilotSessionProvider _provider;
    private readonly string _editorVersion;

    /// <summary>
    /// Initialises a new handler backed by the given <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The session provider that resolves credentials.</param>
    /// <param name="dangerouslyDisableSslValidation">
    /// When <see langword="true"/>, disables SSL/TLS certificate validation.
    /// Intended only for enterprise proxies with self-signed certificates.
    /// </param>
    /// <param name="editorVersion">The <c>Editor-Version</c> header value.</param>
    public CopilotSessionHttpHandler(
        CopilotSessionProvider provider,
        bool dangerouslyDisableSslValidation = false,
        string editorVersion = "vscode/1.104.1")
        : base(CreateInnerHandler(dangerouslyDisableSslValidation))
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _editorVersion = editorVersion;
    }

    /// <summary>
    /// Initialises a handler with an explicit inner handler — intended for unit testing.
    /// </summary>
    internal CopilotSessionHttpHandler(
        CopilotSessionProvider provider,
        HttpMessageHandler innerHandler,
        string editorVersion = "vscode/1.104.1")
        : base(innerHandler ?? throw new ArgumentNullException(nameof(innerHandler)))
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _editorVersion = editorVersion;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await ApplyAuthHeadersAsync(request, cancellationToken).ConfigureAwait(false);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // On 401, invalidate cache and retry once
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _provider.InvalidateCache();
            await ApplyAuthHeadersAsync(request, cancellationToken).ConfigureAwait(false);
            response.Dispose();
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private async Task ApplyAuthHeadersAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var token = await _provider.GetApiTokenAsync(ct).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        SetOrReplace(request, "Editor-Version", _editorVersion);
        SetOrReplace(request, "User-Agent", "GitHubCopilotChat/0.1");
        SetOrReplace(request, "Copilot-Integration-Id", "vscode-chat");
    }

    internal static HttpClientHandler CreateInnerHandler(bool disableSsl)
    {
        var handler = new HttpClientHandler();
        if (disableSsl)
        {
#pragma warning disable MA0039 // Intentional: enterprise proxy support requires bypassing SSL validation
#if NET5_0_OR_GREATER
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
#else
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
#endif
#pragma warning restore MA0039
        }

        return handler;
    }

    private static void SetOrReplace(HttpRequestMessage request, string name, string value)
    {
        request.Headers.Remove(name);
        request.Headers.TryAddWithoutValidation(name, value);
    }
}
