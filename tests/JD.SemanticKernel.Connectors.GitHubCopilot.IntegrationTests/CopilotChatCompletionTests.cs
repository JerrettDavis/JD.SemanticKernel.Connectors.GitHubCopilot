using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace JD.SemanticKernel.Connectors.GitHubCopilot.IntegrationTests;

/// <summary>
/// Integration tests for chat completion through the Copilot API.
/// Requires a valid Copilot subscription and local credentials.
/// </summary>
public class CopilotChatCompletionTests
{
    [SkippableFact]
    public async Task SimpleChatCompletion_ReturnsResponse()
    {
        IntegrationGuard.EnsureEnabled();

        var kernel = Kernel.CreateBuilder()
            .UseCopilotChatCompletion(CopilotModels.Gpt4o)
            .Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("Reply with exactly: hello world");

        var response = await chatService.GetChatMessageContentsAsync(history);

        Assert.NotEmpty(response);
        Assert.False(string.IsNullOrWhiteSpace(response[0].Content),
            "Chat response should not be empty.");
    }

    [SkippableFact]
    public async Task ChatCompletion_WithClaudeModel_ReturnsResponse()
    {
        IntegrationGuard.EnsureEnabled();

        var kernel = Kernel.CreateBuilder()
            .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet46)
            .Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("Reply with exactly: test passed");

        var response = await chatService.GetChatMessageContentsAsync(history);

        Assert.NotEmpty(response);
        Assert.False(string.IsNullOrWhiteSpace(response[0].Content),
            "Chat response should not be empty.");
    }

    [SkippableFact]
    public async Task ChatCompletion_ViaHttpClientFactory_ReturnsResponse()
    {
        IntegrationGuard.EnsureEnabled();

        using var httpClient = CopilotHttpClientFactory.Create();
        httpClient.BaseAddress = new Uri("https://api.githubcopilot.com");

        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: CopilotModels.Gpt4o,
                apiKey: "copilot-session-managed",
                httpClient: httpClient)
            .Build();

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("Reply with exactly: factory test");

        var response = await chatService.GetChatMessageContentsAsync(history);

        Assert.NotEmpty(response);
        Assert.False(string.IsNullOrWhiteSpace(response[0].Content),
            "Chat response should not be empty.");
    }
}
