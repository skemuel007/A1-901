# CLAUDE.md — AI-901 Azure AI Fundamentals Repo

Guidance for future sessions working in this repository.

## Repo structure

Each Azure service area has a folder. Inside each, Python Jupyter notebooks are the primary artifacts. C# console apps live in `CSharp/` subfolders beside each notebook and mirror its behaviour exactly.

```
<ServiceArea>/
├── <Topic>/
│   ├── <topic>.ipynb        # Python notebook (source of truth for behaviour)
│   ├── .env                 # env vars for both Python and C# (gitignored)
│   └── CSharp/
│       ├── <Project>.csproj
│       ├── Program.cs
│       └── README.md
```

## C# project conventions

### Target framework
All projects use `net10.0`.

### Standard NuGet packages
| Package | Version | Used when |
|---------|---------|-----------|
| `Azure.AI.Projects` | 3.0.0-beta.5 | Azure AI Foundry agent/project API |
| `Azure.Identity` | 1.21.0 | `DefaultAzureCredential` (always preferred over API keys) |
| `DotNetEnv` | 3.2.0 | Loading `.env` files |
| `Azure.AI.TextAnalytics` | 5.3.0 | Azure Language Service |
| `Microsoft.AI.Foundry.Local` | 2.2.1 | Foundry Local SDK (requires `<RuntimeIdentifiers>`) |
| `OpenAI` | 2.14.0 | OpenAI-compatible client |

### `.env` loading
```csharp
Env.TraversePath().Load();
```
`TraversePath()` walks parent directories to find `.env` — place it at the service folder level (e.g. `Text-Analysis/.env`), not inside `CSharp/`. This is required because `.env` files are gitignored and permission-denied for direct Read.

### Authentication
Always use `DefaultAzureCredential`. Never hardcode or read API keys in C# code. Run `az login` before executing.

### Native packages (Foundry Local)
When a project references `Microsoft.AI.Foundry.Local`, add:
```xml
<RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64</RuntimeIdentifiers>
```
Then run `dotnet restore` before `dotnet build`.

## Git rules

- Never commit `.env` files. Both `.env` and `**/.env` are in `.gitignore`.
- Never add `--no-verify` to skip hooks.
- No co-author lines unless the commit attribution reminder in the session says to add them.
- Push to `origin` (remote named `origin` points to `https://github.com/skemuel007/A1-901.git`).

## Known SDK quirks (Azure.AI.Projects 3.0.0-beta.5)

- `CodeInterpreterTool` for responses is `OpenAI.Responses.CodeInterpreterTool`, **not** `CodeInterpreterToolboxTool` (which is a `ToolboxTool`, not a `ResponseTool`).
- `FileUploadPurpose` is in `OpenAI.Files` namespace — import explicitly.
- `AgentReference` is ambiguous — use fully qualified `Azure.AI.Extensions.OpenAI.AgentReference`.
- `GetProjectResponsesClientForModel` / `GetProjectResponsesClientForAgent` return a client; response is `ClientResult<ResponseResult>` — call `.Value` to unwrap, then `.GetOutputText()`.
- Suppress `OPENAI001` warning with `#pragma warning disable OPENAI001` at top of file when using experimental APIs.

## Running a project

From the repo root:
```bash
az login
dotnet run --project <ServiceArea>/<Topic>/CSharp/<Project>.csproj
```

## Adding a new C# project

1. Create `CSharp/` folder beside the `.ipynb`.
2. Create `<Name>.csproj` targeting `net10.0` with needed packages.
3. Create `Program.cs` mirroring the notebook behaviour.
4. Use `Env.TraversePath().Load()` and `DefaultAzureCredential`.
5. Add `README.md` with: Azure portal resource creation steps, role assignment, `.env` variables, `az login` + `dotnet run` commands, expected output, troubleshooting table.
6. Test with `dotnet build` before committing.
