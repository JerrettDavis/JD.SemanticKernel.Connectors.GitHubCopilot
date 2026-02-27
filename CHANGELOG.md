# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `JD.SemanticKernel.Connectors.Abstractions` — shared interfaces (`ISessionProvider`,
  `IModelDiscoveryProvider`) and base types for multi-provider Semantic Kernel connectors.
- `JD.SemanticKernel.Connectors.GitHubCopilot` — Semantic Kernel connector leveraging
  GitHub Copilot's local OAuth session for AI model access. Supports automatic token
  discovery, two-step token exchange with caching, model discovery, and SSL bypass.
- `jdcplt` sample CLI — Interactive chat demo with model selection via Copilot auth.
- `CopilotModels` — Well-known model constants for GPT-4o, Claude Sonnet, Gemini, etc.
- `CopilotModelDiscovery` — Runtime model discovery via `/models` endpoint.
- `--insecure` flag across all CLI tools for enterprise SSL bypass.
