namespace JD.SemanticKernel.Connectors.Abstractions;

/// <summary>
/// Base configuration options shared across all session-based AI connectors.
/// </summary>
public abstract class SessionOptionsBase
{
    /// <summary>
    /// When <see langword="true"/>, disables SSL/TLS certificate validation on outgoing HTTP
    /// requests.  This is intended <b>only</b> for enterprise environments behind TLS-intercepting
    /// proxies or self-signed certificates.
    /// <para>
    /// <b>Warning:</b> enabling this option exposes all traffic to potential
    /// man-in-the-middle attacks.  Do not enable in production.
    /// </para>
    /// </summary>
    public bool DangerouslyDisableSslValidation { get; set; }

    /// <summary>
    /// Optional custom API endpoint override.
    /// When <see langword="null"/>, the provider's default endpoint is used.
    /// </summary>
    public string? CustomEndpoint { get; set; }
}
