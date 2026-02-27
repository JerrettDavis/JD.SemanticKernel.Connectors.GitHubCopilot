namespace JD.SemanticKernel.Connectors.Abstractions;

/// <summary>
/// Provides authentication credentials for an AI provider session.
/// Implementations read local credential stores, exchange tokens, and cache results.
/// </summary>
public interface ISessionProvider
{
    /// <summary>
    /// Returns a bearer token suitable for authenticating API requests.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A token string (API key, bearer token, or similar credential).</returns>
    Task<SessionCredentials> GetCredentialsAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks whether valid credentials are available without throwing.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><see langword="true"/> if credentials can be resolved; otherwise <see langword="false"/>.</returns>
    Task<bool> IsAuthenticatedAsync(CancellationToken ct = default);
}
