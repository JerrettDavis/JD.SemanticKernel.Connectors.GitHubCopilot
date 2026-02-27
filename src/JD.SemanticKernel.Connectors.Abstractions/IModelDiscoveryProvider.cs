namespace JD.SemanticKernel.Connectors.Abstractions;

/// <summary>
/// Discovers available AI models from a provider's API.
/// </summary>
public interface IModelDiscoveryProvider
{
    /// <summary>
    /// Returns the list of models currently available through this provider.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<ModelInfo>> DiscoverModelsAsync(CancellationToken ct = default);
}
