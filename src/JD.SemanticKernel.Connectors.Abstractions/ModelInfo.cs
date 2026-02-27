namespace JD.SemanticKernel.Connectors.Abstractions;

/// <summary>
/// Describes an AI model available from a provider.
/// </summary>
/// <param name="Id">The model identifier used in API requests (e.g. <c>"gpt-4o"</c>).</param>
/// <param name="Name">Human-readable display name.</param>
/// <param name="Provider">The upstream provider (e.g. <c>"openai"</c>, <c>"anthropic"</c>).</param>
public sealed record ModelInfo(string Id, string Name, string? Provider = null);
