namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// Well-known model identifiers available through the GitHub Copilot API.
/// These models are accessed through Copilot's multi-model gateway.
/// Use <see cref="CopilotModelDiscovery"/> to discover all available models at runtime.
/// </summary>
public static class CopilotModels
{
    // ── OpenAI (via Azure OpenAI) ───────────────────────────────────

    /// <summary>GPT-4o — OpenAI's multimodal model.</summary>
    public const string Gpt4o = "gpt-4o";

    /// <summary>GPT-4.1 — latest GPT-4 series model.</summary>
    public const string Gpt41 = "gpt-4.1";

    /// <summary>GPT-4o mini — compact, cost-efficient model.</summary>
    public const string Gpt4oMini = "gpt-4o-mini";

    /// <summary>GPT-5 mini — fast general-purpose reasoning model.</summary>
    public const string Gpt5Mini = "gpt-5-mini";

    // ── OpenAI (direct) ─────────────────────────────────────────────

    /// <summary>GPT-5.1 — advanced reasoning model.</summary>
    public const string Gpt51 = "gpt-5.1";

    /// <summary>GPT-5.1-Codex — code-specialized model.</summary>
    public const string Gpt51Codex = "gpt-5.1-codex";

    /// <summary>GPT-5.1-Codex-Max — extended context code model.</summary>
    public const string Gpt51CodexMax = "gpt-5.1-codex-max";

    /// <summary>GPT-5.1-Codex-Mini — lightweight code model.</summary>
    public const string Gpt51CodexMini = "gpt-5.1-codex-mini";

    /// <summary>GPT-5.2 — complex reasoning and analysis model.</summary>
    public const string Gpt52 = "gpt-5.2";

    /// <summary>GPT-5.2-Codex — code-specialized GPT-5.2 variant.</summary>
    public const string Gpt52Codex = "gpt-5.2-codex";

    /// <summary>GPT-5.3-Codex — latest code-specialized model.</summary>
    public const string Gpt53Codex = "gpt-5.3-codex";

    // ── Anthropic ──────────────────────────────────────────────────

    /// <summary>Claude Opus 4.6 — Anthropic's most powerful model.</summary>
    public const string ClaudeOpus46 = "claude-opus-4.6";

    /// <summary>Claude Opus 4.6 (fast mode) — faster variant of Opus 4.6.</summary>
    public const string ClaudeOpus46Fast = "claude-opus-4.6-fast";

    /// <summary>Claude Opus 4.5 — previous generation flagship model.</summary>
    public const string ClaudeOpus45 = "claude-opus-4.5";

    /// <summary>Claude Sonnet 4.6 — latest balanced model.</summary>
    public const string ClaudeSonnet46 = "claude-sonnet-4.6";

    /// <summary>Claude Sonnet 4.5 — previous generation balanced model.</summary>
    public const string ClaudeSonnet45 = "claude-sonnet-4.5";

    /// <summary>Claude Sonnet 4 — reliable completions and reasoning.</summary>
    public const string ClaudeSonnet4 = "claude-sonnet-4";

    /// <summary>Claude Haiku 4.5 — fast, lightweight model for simple tasks.</summary>
    public const string ClaudeHaiku45 = "claude-haiku-4.5";

    // ── Google ─────────────────────────────────────────────────────

    /// <summary>Gemini 2.5 Pro — Google's capable model.</summary>
    public const string Gemini25Pro = "gemini-2.5-pro";

    /// <summary>Gemini 3 Pro (Preview) — advanced reasoning across long contexts.</summary>
    public const string Gemini3Pro = "gemini-3-pro-preview";

    /// <summary>Gemini 3.1 Pro — latest Gemini model.</summary>
    public const string Gemini31Pro = "gemini-3.1-pro-preview";

    /// <summary>Gemini 3 Flash (Preview) — fast Gemini model.</summary>
    public const string Gemini3Flash = "gemini-3-flash-preview";

    // ── xAI ────────────────────────────────────────────────────────

    /// <summary>Grok Code Fast 1 — xAI's code-specialized model.</summary>
    public const string GrokCodeFast1 = "grok-code-fast-1";

    /// <summary>
    /// The recommended default model (<see cref="ClaudeSonnet46"/>).
    /// </summary>
    public const string Default = ClaudeSonnet46;
}
