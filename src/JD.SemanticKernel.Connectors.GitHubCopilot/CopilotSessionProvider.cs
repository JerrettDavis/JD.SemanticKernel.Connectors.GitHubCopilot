using System.Net.Http.Headers;
using System.Text.Json;
using JD.SemanticKernel.Connectors.Abstractions;
using JD.SemanticKernel.Connectors.GitHubCopilot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Resolves GitHub Copilot API credentials through a two-step flow:
/// <list type="number">
///   <item><description>Read local OAuth token from <c>apps.json</c> / <c>hosts.json</c></description></item>
///   <item><description>Exchange for a short-lived Copilot API token (~30 min TTL)</description></item>
/// </list>
/// Implements <see cref="ISessionProvider"/> for multi-provider scenarios.
/// </summary>
public sealed class CopilotSessionProvider : ISessionProvider, IDisposable
{
    private readonly CopilotSessionOptions _options;
    private readonly ILogger<CopilotSessionProvider> _logger;
    private readonly HttpClient _exchangeClient;
    private readonly bool _ownsHttpClient;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private volatile CopilotTokenResponse? _cached;

    /// <summary>
    /// Initialises the provider with DI-injected options and logger.
    /// </summary>
    public CopilotSessionProvider(
        IOptions<CopilotSessionOptions> options,
        ILogger<CopilotSessionProvider> logger)
        : this(options, logger, null)
    {
    }

    /// <summary>
    /// Initialises the provider with an explicit <see cref="HttpClient"/> for token exchange.
    /// Primarily used for unit testing.
    /// </summary>
    internal CopilotSessionProvider(
        IOptions<CopilotSessionOptions> options,
        ILogger<CopilotSessionProvider> logger,
        HttpClient? exchangeClient)
    {
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (exchangeClient is not null)
        {
            _exchangeClient = exchangeClient;
            _ownsHttpClient = false;
        }
        else
        {
            var handler = CopilotSessionHttpHandler.CreateInnerHandler(_options.DangerouslyDisableSslValidation);
#if NET5_0_OR_GREATER
            handler.CheckCertificateRevocationList = !_options.DangerouslyDisableSslValidation;
#endif
            _exchangeClient = new HttpClient(handler);
            _ownsHttpClient = true;
        }
    }

    /// <summary>
    /// Returns the current API endpoint base URL from the last token exchange,
    /// or the default <c>https://api.githubcopilot.com</c>.
    /// </summary>
    public string ApiEndpoint =>
        _cached?.Endpoints?.Api ?? "https://api.githubcopilot.com";

    /// <inheritdoc/>
    public async Task<SessionCredentials> GetCredentialsAsync(CancellationToken ct = default)
    {
        var token = await GetApiTokenAsync(ct).ConfigureAwait(false);
        return new SessionCredentials(token.Token, token.ExpiresAtUtc);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAuthenticatedAsync(CancellationToken ct = default)
    {
        try
        {
            await GetApiTokenAsync(ct).ConfigureAwait(false);
            return true;
        }
        catch (CopilotSessionException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a valid Copilot API token, exchanging or refreshing as needed.
    /// </summary>
    /// <exception cref="CopilotSessionException">
    /// Thrown when no valid OAuth token is found or the token exchange fails.
    /// </exception>
    public async Task<CopilotTokenResponse> GetApiTokenAsync(CancellationToken ct = default)
    {
        var snapshot = _cached;
        if (snapshot is not null && DateTimeOffset.UtcNow.AddSeconds(60) < snapshot.ExpiresAtUtc)
            return snapshot;

        await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock.
            if (_cached is not null && DateTimeOffset.UtcNow.AddSeconds(60) < _cached.ExpiresAtUtc)
                return _cached;

            var oauthToken = await ResolveOAuthTokenAsync(ct).ConfigureAwait(false);
            var apiToken = await ExchangeTokenAsync(oauthToken, ct).ConfigureAwait(false);
            _cached = apiToken;

            _logger.LogInformation(
                "Exchanged Copilot token (expires {ExpiresAt}, sku: {Sku}, endpoint: {Endpoint})",
                apiToken.ExpiresAtUtc,
                apiToken.Sku ?? "unknown",
                apiToken.Endpoints?.Api ?? "default");

            return apiToken;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>Clears the cached API token, forcing re-exchange on next call.</summary>
    internal void InvalidateCache() => _cached = null;

    private async Task<string> ResolveOAuthTokenAsync(CancellationToken ct)
    {
        // Priority 1: Explicit OAuth token in options
        if (!string.IsNullOrWhiteSpace(_options.OAuthToken))
        {
            _logger.LogDebug("Using explicit OAuth token from options");
            return _options.OAuthToken!;
        }

        // Priority 2: Environment variable
        var envToken = Environment.GetEnvironmentVariable("GITHUB_COPILOT_TOKEN");
        if (!string.IsNullOrWhiteSpace(envToken))
        {
            _logger.LogDebug("Using GITHUB_COPILOT_TOKEN environment variable");
            return envToken!;
        }

        // Priority 3: Local config file
        return await ReadOAuthTokenFromFileAsync(ct).ConfigureAwait(false);
    }

    private async Task<string> ReadOAuthTokenFromFileAsync(CancellationToken ct)
    {
        var paths = ResolveTokenFilePaths();

        foreach (var path in paths)
        {
            _logger.LogDebug("Checking Copilot token file: {Path}", path);

            if (!File.Exists(path))
                continue;

            try
            {
                Dictionary<string, CopilotAppsEntry>? entries;
#if NETSTANDARD2_0
                var json = await Task
                    .Run(() => File.ReadAllText(path), ct)
                    .ConfigureAwait(false);
                entries = JsonSerializer
                    .Deserialize<Dictionary<string, CopilotAppsEntry>>(json);
#else
                await using var stream = File.OpenRead(path);
                entries = await JsonSerializer
                    .DeserializeAsync<Dictionary<string, CopilotAppsEntry>>(
                        stream, cancellationToken: ct)
                    .ConfigureAwait(false);
#endif

                if (entries is null)
                    continue;

                // Find entry matching our target host
                foreach (var kvp in entries)
                {
                    if (kvp.Key.StartsWith(_options.GitHubHost, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(kvp.Value.OAuthToken))
                    {
                        _logger.LogDebug(
                            "Found Copilot token for {Host} (user: {User}) in {Path}",
                            kvp.Key, kvp.Value.User, path);
                        return kvp.Value.OAuthToken;
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                _logger.LogDebug("Token file not found at {Path}", path);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Permission denied reading {Path}", path);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "I/O error reading {Path}", path);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Malformed JSON in {Path}", path);
            }
        }

        throw new CopilotSessionException(
            "No GitHub Copilot credentials found. " +
            "Install GitHub Copilot CLI and sign in, or set the GITHUB_COPILOT_TOKEN environment variable.");
    }

    private async Task<CopilotTokenResponse> ExchangeTokenAsync(string oauthToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _options.TokenExchangeUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("token", oauthToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Editor-Version", _options.EditorVersion);
        request.Headers.TryAddWithoutValidation("User-Agent", "GitHubCopilotChat/0.1");

        HttpResponseMessage response;
        try
        {
            response = await _exchangeClient.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new CopilotSessionException(
                $"Failed to contact Copilot token exchange endpoint at {_options.TokenExchangeUrl}. " +
                "Check your network connection and proxy settings.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(
#if !NETSTANDARD2_0
                ct
#endif
            ).ConfigureAwait(false);

            throw new CopilotSessionException(
                $"Copilot token exchange failed with HTTP {(int)response.StatusCode}: {body}. " +
                "Your Copilot subscription may have expired or the OAuth token may be invalid.");
        }

#if NETSTANDARD2_0
        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var tokenResponse = JsonSerializer.Deserialize<CopilotTokenResponse>(responseJson);
#else
        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        var tokenResponse = await JsonSerializer
            .DeserializeAsync<CopilotTokenResponse>(stream, cancellationToken: ct)
            .ConfigureAwait(false);
#endif

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.Token))
            throw new CopilotSessionException(
                "Copilot token exchange returned an empty or invalid response.");

        return tokenResponse;
    }

    private IEnumerable<string> ResolveTokenFilePaths()
    {
        if (!string.IsNullOrWhiteSpace(_options.TokenFilePath))
        {
            yield return _options.TokenFilePath!;
            yield break;
        }

        // Windows: %LOCALAPPDATA%\github-copilot\
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrEmpty(localAppData))
        {
            yield return Path.Combine(localAppData, "github-copilot", "apps.json");
            yield return Path.Combine(localAppData, "github-copilot", "hosts.json");
        }

        // Linux/macOS: ~/.config/github-copilot/
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home))
        {
            yield return Path.Combine(home, ".config", "github-copilot", "apps.json");
            yield return Path.Combine(home, ".config", "github-copilot", "hosts.json");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cacheLock.Dispose();
        if (_ownsHttpClient)
            _exchangeClient.Dispose();
    }
}
