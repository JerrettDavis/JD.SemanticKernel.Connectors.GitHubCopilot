using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.IntegrationTests;

/// <summary>
/// Helper that checks whether integration tests should run.
/// Set <c>COPILOT_INTEGRATION_TESTS=true</c> to enable.
/// </summary>
internal static class IntegrationGuard
{
    public static bool IsEnabled =>
        string.Equals(
            Environment.GetEnvironmentVariable("COPILOT_INTEGRATION_TESTS"),
            "true",
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Skips the test unless <c>COPILOT_INTEGRATION_TESTS=true</c>.
    /// Call at the start of every integration test.
    /// </summary>
    public static void EnsureEnabled() =>
        Skip.IfNot(IsEnabled, "Integration tests disabled. Set COPILOT_INTEGRATION_TESTS=true to run.");
}
