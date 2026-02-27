#if !NETSTANDARD2_0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// <see cref="IKernelBuilder"/> extensions for wiring GitHub Copilot authentication into
/// Semantic Kernel using the OpenAI-compatible chat completions endpoint.
/// </summary>
public static class KernelBuilderExtensions
{
    /// <summary>
    /// Registers an OpenAI chat completion service backed by GitHub Copilot session authentication.
    /// Credentials are resolved automatically from Copilot's local config files,
    /// environment variables, or the options delegate — in that priority order.
    /// </summary>
    /// <param name="builder">The kernel builder to configure.</param>
    /// <param name="modelId">
    /// The model to target. Defaults to <see cref="CopilotModels.Default"/>
    /// (<c>gpt-4o</c>). See <see cref="CopilotModels"/> for well-known identifiers.
    /// </param>
    /// <param name="configure">
    /// Optional delegate for fine-grained control over <see cref="CopilotSessionOptions"/>,
    /// e.g. setting a custom <see cref="CopilotSessionOptions.TokenFilePath"/>.
    /// </param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// // Minimal — reads from local Copilot config
    /// var kernel = Kernel.CreateBuilder()
    ///     .UseCopilotChatCompletion()
    ///     .Build();
    ///
    /// // With explicit model
    /// var kernel = Kernel.CreateBuilder()
    ///     .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet4)
    ///     .Build();
    ///
    /// // With SSL bypass for enterprise
    /// var kernel = Kernel.CreateBuilder()
    ///     .UseCopilotChatCompletion(configure: o => o.DangerouslyDisableSslValidation = true)
    ///     .Build();
    /// </code>
    /// </example>
    public static IKernelBuilder UseCopilotChatCompletion(
        this IKernelBuilder builder,
        string modelId = CopilotModels.Default,
        Action<CopilotSessionOptions>? configure = null)
    {
        var options = new CopilotSessionOptions();
        configure?.Invoke(options);

        var provider = new CopilotSessionProvider(
            Options.Create(options),
            NullLogger<CopilotSessionProvider>.Instance);

        var handler = new CopilotSessionHttpHandler(
            provider, options.DangerouslyDisableSslValidation, options.EditorVersion);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.CustomEndpoint ?? "https://api.githubcopilot.com"),
        };

        // Register OpenAI-compatible chat completion pointed at Copilot's endpoint
        builder.AddOpenAIChatCompletion(
            modelId: modelId,
            apiKey: "copilot-session-managed",
            httpClient: httpClient);

        return builder;
    }
}

#endif
