using System.CommandLine;
using JD.SemanticKernel.Connectors.GitHubCopilot;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var modelOption = new Option<string>("--model", "-m")
{
    Description = "AI model to use (e.g. gpt-4o, claude-sonnet-4, gemini-2.0-flash)",
    DefaultValueFactory = _ => CopilotModels.Default,
};

var promptOption = new Option<string?>("--prompt", "-p")
{
    Description = "Single prompt to send (non-interactive mode)",
};

var listModelsOption = new Option<bool>("--list-models")
{
    Description = "List available models and exit",
};

var insecureOption = new Option<bool>("--insecure")
{
    Description = "Disable SSL certificate validation (enterprise proxies)",
};

var systemOption = new Option<string?>("--system", "-s")
{
    Description = "System prompt to set context for the conversation",
};

var rootCommand = new RootCommand("Interactive AI chat powered by GitHub Copilot")
{
    modelOption,
    promptOption,
    listModelsOption,
    insecureOption,
    systemOption,
};

rootCommand.SetAction(async (parseResult, ct) =>
{
    var model = parseResult.GetValue(modelOption)!;
    var prompt = parseResult.GetValue(promptOption);
    var listModels = parseResult.GetValue(listModelsOption);
    var insecure = parseResult.GetValue(insecureOption);
    var systemPrompt = parseResult.GetValue(systemOption);

    if (listModels)
    {
        Console.WriteLine("Discovering available models...");
        try
        {
            var options = new CopilotSessionOptions { DangerouslyDisableSslValidation = insecure };
            using var provider = new CopilotSessionProvider(
                Microsoft.Extensions.Options.Options.Create(options),
                Microsoft.Extensions.Logging.Abstractions.NullLogger<CopilotSessionProvider>.Instance);

            using var httpClient = CopilotHttpClientFactory.Create(provider, insecure);
            var discovery = new CopilotModelDiscovery(
                provider, httpClient,
                Microsoft.Extensions.Logging.Abstractions.NullLogger<CopilotModelDiscovery>.Instance);

            var models = await discovery.DiscoverModelsAsync(ct);
            Console.WriteLine($"\nFound {models.Count} models:");
            foreach (var m in models)
                Console.WriteLine($"  {m.Id,-35} {m.Provider ?? ""}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
        return;
    }

    var kernel = Kernel.CreateBuilder()
        .UseCopilotChatCompletion(model, o => o.DangerouslyDisableSslValidation = insecure)
        .Build();

    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var history = new ChatHistory();

    if (!string.IsNullOrWhiteSpace(systemPrompt))
        history.AddSystemMessage(systemPrompt);

    if (!string.IsNullOrWhiteSpace(prompt))
    {
        // Single-shot mode
        history.AddUserMessage(prompt);
        Console.WriteLine($"[{model}] Thinking...");
        var response = await chatService.GetChatMessageContentAsync(history, cancellationToken: ct);
        Console.WriteLine(response.Content);
        return;
    }

    // Interactive mode
    Console.WriteLine($"GitHub Copilot Chat (model: {model})");
    Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

    while (!ct.IsCancellationRequested)
    {
        Console.Write("You: ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            continue;

        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
            input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        history.AddUserMessage(input);

        try
        {
            Console.Write($"\n[{model}]: ");
            var response = await chatService.GetChatMessageContentAsync(history, cancellationToken: ct);
            Console.WriteLine(response.Content);
            Console.WriteLine();

            if (response.Content is not null)
                history.AddAssistantMessage(response.Content);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nError: {ex.Message}\n");
        }
    }
});

try
{
    return await rootCommand.Parse(args).InvokeAsync();
}
catch (CopilotSessionException ex)
{
    Console.Error.WriteLine($"Authentication error: {ex.Message}");
    return 1;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Network error: {ex.Message}");
    return 1;
}
