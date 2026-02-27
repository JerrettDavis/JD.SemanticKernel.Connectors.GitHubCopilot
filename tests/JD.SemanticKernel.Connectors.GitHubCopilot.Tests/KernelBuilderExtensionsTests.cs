#if !NETSTANDARD2_0

using Microsoft.SemanticKernel;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.Tests;

/// <summary>
/// Tests for <see cref="KernelBuilderExtensions"/>.
/// Note: KernelBuilderExtensions is behind <c>#if !NETSTANDARD2_0</c>,
/// so these tests only compile on net8.0+ targets.
/// </summary>
public class KernelBuilderExtensionsTests
{
    [Fact]
    public void UseCopilotChatCompletion_ReturnsBuilder()
    {
        var builder = Kernel.CreateBuilder();

        var result = builder.UseCopilotChatCompletion();

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseCopilotChatCompletion_WithModelId_ReturnsBuilder()
    {
        var builder = Kernel.CreateBuilder();

        var result = builder.UseCopilotChatCompletion(CopilotModels.Gpt4o);

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseCopilotChatCompletion_WithConfigure_ReturnsBuilder()
    {
        var builder = Kernel.CreateBuilder();

        var result = builder.UseCopilotChatCompletion(
            configure: o => o.OAuthToken = "ghu_test_kernel");

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseCopilotChatCompletion_DefaultModel_IsCopilotDefault()
    {
        var builder = Kernel.CreateBuilder();

        // Should not throw — uses CopilotModels.Default
        var result = builder.UseCopilotChatCompletion();

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseCopilotChatCompletion_BuildsKernelWithChatService()
    {
        var builder = Kernel.CreateBuilder();
        builder.UseCopilotChatCompletion(
            CopilotModels.Gpt4o,
            o => o.OAuthToken = "ghu_test_build");

        var kernel = builder.Build();

        Assert.NotNull(kernel);
    }

    [Fact]
    public void UseCopilotChatCompletion_WithSslDisabled_ReturnsBuilder()
    {
        var builder = Kernel.CreateBuilder();

        var result = builder.UseCopilotChatCompletion(
            configure: o => o.DangerouslyDisableSslValidation = true);

        Assert.Same(builder, result);
    }

    [Fact]
    public void UseCopilotChatCompletion_WithCustomEndpoint_ReturnsBuilder()
    {
        var builder = Kernel.CreateBuilder();

        var result = builder.UseCopilotChatCompletion(
            configure: o => o.CustomEndpoint = "https://custom.endpoint.com");

        Assert.Same(builder, result);
    }
}

#endif
