using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JD.SemanticKernel.Connectors.GitHubCopilot;

/// <summary>
/// <see cref="IServiceCollection"/> extensions for registering GitHub Copilot authentication services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="CopilotSessionProvider"/> and binds
    /// <see cref="CopilotSessionOptions"/> from the <c>"CopilotSession"</c> configuration section.
    /// </summary>
    public static IServiceCollection AddCopilotAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
#if NETSTANDARD2_0
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));
#else
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
#endif

        services.Configure<CopilotSessionOptions>(
            configuration.GetSection(CopilotSessionOptions.SectionName));

        services.AddSingleton<CopilotSessionProvider>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="CopilotSessionProvider"/> and configures
    /// <see cref="CopilotSessionOptions"/> via the provided <paramref name="configure"/> delegate.
    /// </summary>
    public static IServiceCollection AddCopilotAuthentication(
        this IServiceCollection services,
        Action<CopilotSessionOptions>? configure = null)
    {
#if NETSTANDARD2_0
        if (services is null) throw new ArgumentNullException(nameof(services));
#else
        ArgumentNullException.ThrowIfNull(services);
#endif

        if (configure is not null)
            services.Configure<CopilotSessionOptions>(configure);
        else
            services.Configure<CopilotSessionOptions>(static _ => { });

        services.AddSingleton<CopilotSessionProvider>();
        return services;
    }
}
