# CopilotChat CLI Sample

An interactive chat CLI built on the Copilot connector, demonstrating real-world usage
of `CopilotSessionProvider`, model selection, and the Semantic Kernel chat completion API.

## Installation

```bash
dotnet tool install -g JD.Tools.CopilotChat
```

## Usage

### Interactive chat

```bash
jdcplt
```

Starts an interactive session with `gpt-4o` (default model). Type messages and receive
AI responses. Type `exit` or `quit` to end the session.

### Single-shot prompt

```bash
jdcplt -p "Explain the SOLID principles"
```

### With a specific model

```bash
jdcplt -m claude-sonnet-4 -p "Write a haiku about C#"
```

### With a system prompt

```bash
jdcplt -s "You are a senior .NET architect" -p "Review this design pattern"
```

### List available models

```bash
jdcplt --list-models
```

### Enterprise networks (SSL bypass)

```bash
jdcplt --insecure
```

## Architecture

```
Program.cs
  → System.CommandLine CLI parser
    → CopilotSessionProvider (reads local credentials)
    → CopilotHttpClientFactory (creates auth-injecting HttpClient)
    → Kernel.CreateBuilder().AddOpenAIChatCompletion(...)
      → ChatCompletionService.GetChatMessageContentAsync()
```

## Source

See [`samples/CopilotChat/Program.cs`](https://github.com/JerrettDavis/JD.SemanticKernel.Connectors.GitHubCopilot/blob/main/samples/CopilotChat/Program.cs)
for the full implementation.
