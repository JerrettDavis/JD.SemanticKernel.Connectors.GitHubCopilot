namespace JD.SemanticKernel.Connectors.Abstractions;

/// <summary>
/// Represents a resolved authentication credential with optional expiry.
/// </summary>
/// <param name="Token">The bearer or API token value.</param>
/// <param name="ExpiresAt">When the token expires, or <see langword="null"/> for non-expiring tokens.</param>
public sealed record SessionCredentials(string Token, DateTimeOffset? ExpiresAt = null)
{
    /// <summary>
    /// Returns <see langword="true"/> if the credential has a known expiry that has passed
    /// (with a 30-second safety margin for clock skew).
    /// </summary>
    public bool IsExpired =>
        ExpiresAt.HasValue && DateTimeOffset.UtcNow.AddSeconds(30) >= ExpiresAt.Value;
}
