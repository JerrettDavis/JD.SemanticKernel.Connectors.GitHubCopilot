using System.Net.Http.Headers;
using System.Text.Json;
using JD.SemanticKernel.Connectors.Abstractions;
using Microsoft.Extensions.Logging;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Discovers available models from the Copilot API's <c>/models</c> endpoint.
/// </summary>
public sealed class CopilotModelDiscovery : IModelDiscoveryProvider
{
    private readonly CopilotSessionProvider _provider;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CopilotModelDiscovery> _logger;

    /// <summary>Initialises model discovery with a session provider and HTTP client.</summary>
    public CopilotModelDiscovery(
        CopilotSessionProvider provider,
        HttpClient httpClient,
        ILogger<CopilotModelDiscovery> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModelInfo>> DiscoverModelsAsync(CancellationToken ct = default)
    {
        var token = await _provider.GetApiTokenAsync(ct).ConfigureAwait(false);
        var endpoint = _provider.ApiEndpoint.TrimEnd('/');

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

#if NETSTANDARD2_0
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(json);
#else
        var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
#endif

        var models = new List<ModelInfo>();

        if (doc.RootElement.TryGetProperty("data", out var data))
        {
            foreach (var model in data.EnumerateArray())
            {
                var id = model.GetProperty("id").GetString() ?? string.Empty;
                var name = id; // Copilot API may not have a separate display name
                string? provider = null;

                if (model.TryGetProperty("vendor", out var vendor))
                    provider = vendor.GetString();

                models.Add(new ModelInfo(id, name, provider));
            }
        }

        _logger.LogInformation("Discovered {Count} models from Copilot API", models.Count);
        return models;
    }
}
