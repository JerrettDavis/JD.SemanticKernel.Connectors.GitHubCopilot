# Kernel Builder Integration

The `UseCopilotChatCompletion()` extension method is the primary way to integrate
the Copilot connector with Semantic Kernel.

> [!NOTE]
> Kernel builder extensions require **net8.0** or later. On `netstandard2.0`, use the
> `CopilotHttpClientFactory` directly.

## Basic usage

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion()
    .Build();
```

## With model selection

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(CopilotModels.ClaudeSonnet4)
    .Build();
```

## With custom options

```csharp
var kernel = Kernel.CreateBuilder()
    .UseCopilotChatCompletion(
        modelId: CopilotModels.ClaudeSonnet46,
        configure: options =>
        {
            options.EditorVersion = "vscode/1.104.1";
            options.DangerouslyDisableSslValidation = true;
        })
    .Build();
```

## Service Collection integration

For DI-heavy applications:

```csharp
services.AddCopilotSessionProvider(options =>
{
    options.DefaultModel = CopilotModels.ClaudeSonnet4;
});
```

This registers `CopilotSessionProvider` as a singleton in the container.

## How it works

Under the hood, `UseCopilotChatCompletion()`:

1. Creates a `CopilotSessionProvider` with the specified options
2. Creates an `HttpClient` via `CopilotHttpClientFactory.Create()` with the auth handler
3. Registers SK's built-in `AddOpenAIChatCompletion()` with the custom `HttpClient`
4. Sets the `BaseAddress` to the Copilot API endpoint

Since the Copilot API is **OpenAI-compatible**, no custom `IChatCompletionService` is needed.
