namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Well-known model identifiers available through the GitHub Copilot API.
/// These models are accessed through Copilot's multi-model gateway.
/// </summary>
public static class CopilotModels
{
    // ── OpenAI ─────────────────────────────────────────────────────

    /// <summary>GPT-4o — OpenAI's flagship multimodal model.</summary>
    public const string Gpt4o = "gpt-4o";

    /// <summary>GPT-4.1 — latest GPT-4 series model.</summary>
    public const string Gpt41 = "gpt-4.1";

    /// <summary>o4-mini — OpenAI's compact reasoning model.</summary>
    public const string O4Mini = "o4-mini";

    // ── Anthropic ──────────────────────────────────────────────────

    /// <summary>Claude Sonnet 4 — Anthropic's balanced model.</summary>
    public const string ClaudeSonnet4 = "claude-sonnet-4";

    /// <summary>Claude Sonnet 3.5 — previous generation balanced model.</summary>
    public const string ClaudeSonnet35 = "claude-3.5-sonnet";

    // ── Google ─────────────────────────────────────────────────────

    /// <summary>Gemini 2.0 Flash — Google's fast model.</summary>
    public const string Gemini20Flash = "gemini-2.0-flash";

    /// <summary>Gemini 2.5 Pro — Google's capable model (preview).</summary>
    public const string Gemini25Pro = "gemini-2.5-pro";

    /// <summary>
    /// The recommended default model (<see cref="Gpt4o"/>).
    /// </summary>
    public const string Default = Gpt4o;
}
