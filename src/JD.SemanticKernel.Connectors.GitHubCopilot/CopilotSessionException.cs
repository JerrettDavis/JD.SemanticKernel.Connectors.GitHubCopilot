namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Thrown when GitHub Copilot credentials are unavailable, expired, or the token exchange fails.
/// The <see cref="Exception.Message"/> is safe to display directly to end users.
/// </summary>
public sealed class CopilotSessionException : InvalidOperationException
{
    /// <inheritdoc cref="InvalidOperationException()"/>
    public CopilotSessionException() { }

    /// <inheritdoc cref="InvalidOperationException(string)"/>
    public CopilotSessionException(string message) : base(message) { }

    /// <inheritdoc cref="InvalidOperationException(string, Exception)"/>
    public CopilotSessionException(string message, Exception innerException)
        : base(message, innerException) { }
}
