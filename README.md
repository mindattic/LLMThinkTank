# LLM Think Tank

LLM Think Tank is a .NET MAUI + Blazor WebView app for running multi-participant discussions across multiple LLM providers.

The app is organized around:

- **Providers**: per-provider auth configuration (API keys, model selection, etc.) stored as JSON.
- **Personalities**: reusable participant templates (provider + display name + personality markdown + optional custom instructions/biases).
- **Conversations (Tabs)**: each tab is a separate discussion containing a topic/prompt, selected participants, message history, and status events.

## Persistence model

All user data is stored under the per-user Windows Local AppData folder:

- `%LOCALAPPDATA%\MindAttic\LLMThinkTank\Settings.json`
  - Provider auth JSON blobs
  - Personalities/templates
  - Conversation tabs (id/title/topic/participants)
  - In-memory conversation message/status state

- `%LOCALAPPDATA%\MindAttic\LLMThinkTank\Conversations\<chatId>\`
  - `chat.json` (append-only structured log of chat events)
  - `<participantId>.md` (participant “perspective” markdown)

- `%LOCALAPPDATA%\MindAttic\LLMThinkTank\Personalities\`
  - `OpenAI.md`, `Claude.md`, `Gemini.md`, `DeepSeek.md` (baseline personality docs)

### What is restored on restart

On app start:

- Conversation tabs and participants are restored from `Settings.json`.
- Message history is restored from `Conversations/<chatId>/chat.json`.
- Status events are restored from `Settings.json`.

## Key components

- `Services/SettingsService.cs`
  - Reads/writes `Settings.json`
  - Manages Provider auth configs, templates, conversation persistence, and theme.

- `Services/ChatConversationsService.cs`
  - Manages conversation tabs and active conversation
  - Persists mutations via `SettingsService`

- `Services/ChatLogService.cs` (`ChatStorage`)
  - Writes chat event logs and participant perspective markdown under `Conversations/<chatId>/`
  - Loads turns for UI restoration.

- `Components/Pages/Settings.razor`
  - Provider auth editor (manual Save + debounced autosave)
  - Personality editor (includes Custom Instructions/biases)

- `Components/Pages/Arena.razor`
  - Main Think Tank conversation UI (tabs, topic, participants, run loop)

## Running

Open the solution in Visual Studio and run the `LLMThinkTank` target for your platform (Windows, Android, iOS, etc.).

## Unit tests

A test project exists at `LLMThinkTank.UnitTests` using NUnit.

Note: because the app is a MAUI head project, most testable logic should ideally live in a separate class library (`.Core`) for clean unit testing without XAML build steps.

## Security

Do not commit API keys. Provider auth JSON is stored in Local AppData and should remain local.
