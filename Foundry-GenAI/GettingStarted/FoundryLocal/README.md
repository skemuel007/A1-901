# Azure AI Foundry Local SDK — C# Samples

Five runnable C# samples covering the Foundry Local SDK and Azure Foundry cloud API.

| # | Sample | What it demonstrates |
|---|--------|----------------------|
| 1 | Stream chat response | Native SDK, streaming tokens with `CompleteChatStreamingAsync` |
| 2 | Quick start | Single blocking completion with `CompleteChatAsync` |
| 3 | OpenAI SDK via web service | Foundry Local as an OpenAI-compatible HTTP endpoint |
| 4 | Text embeddings | `GenerateEmbeddingAsync` / `GenerateEmbeddingsAsync` |
| 5 | Azure Foundry cloud API | `AIProjectClient` with `DefaultAzureCredential` |

---

## Prerequisites

### Required for all samples (1–4)

| Tool | Install |
|------|---------|
| .NET 10 SDK | https://dotnet.microsoft.com/download |
| **Foundry Local** desktop app | https://aka.ms/foundry-local — install and launch it once before running any sample |

Foundry Local manages model downloads and hardware execution providers (CPU/GPU). The app must be running in the background when you execute samples 1–4.

### Required for sample 5 only

- An **Azure AI Foundry project** endpoint  
- Azure CLI logged in: `az login`  
- The identity used must have the **Azure AI User** role on the Foundry project

---

## Setup

### 1. Clone / open the repository

```bash
git clone https://github.com/skemuel007/A1-901.git
cd A1-901/Foundry-GenAI/GettingStarted/FoundryLocal
```

### 2. Restore packages

```bash
dotnet restore
```

### 3. (Samples 1–4) Start Foundry Local

Open the **Foundry Local** desktop app and confirm it is running. On first launch it downloads the model catalog and registers execution providers automatically.

### 4. (Sample 5 only) Set environment variable

```bash
# macOS / Linux
export FOUNDRY_PROJECT_ENDPOINT="https://<resource>.services.ai.azure.com/api/projects/<project-name>"
export MODEL_DEPLOYMENT_NAME="gpt-5.1"   # or your deployment name

# Windows (PowerShell)
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project-name>"
$env:MODEL_DEPLOYMENT_NAME    = "gpt-5.1"
```

Then authenticate:

```bash
az login
```

---

## Running the samples

```bash
dotnet run
```

The app shows a numbered menu. Type the number and press Enter.

```
Azure AI Foundry Local SDK Samples
===================================
1. Stream chat response (native SDK)
2. Quick start: run a model
3. OpenAI SDK via Foundry Local web service
4. Generate text embeddings
5. Call Azure Foundry cloud API

Choose a sample (1-5):
```

### First run note

Samples 1–4 download the model on first use (~700 MB for `phi-4-mini`, ~400 MB for `qwen3-embedding-0.6b`). Progress is printed to the console. Subsequent runs use the cached model.

---

## Sample details

### Sample 1 — Streaming chat (native SDK)

Uses `FoundryLocalManager` directly with `CompleteChatStreamingAsync`. Tokens stream to the console as they are generated.

```
Assistant:
The sky appears blue because of a phenomenon called Rayleigh scattering...
```

### Sample 2 — Quick start

A minimal blocking call. Useful for scripts or batch workloads where streaming is not needed.

### Sample 3 — OpenAI SDK via web service

Starts the Foundry Local HTTP server on `http://127.0.0.1:52495` and connects with the `OpenAI` NuGet package, which means any existing code that targets the OpenAI REST API works unchanged against a local model.

### Sample 4 — Text embeddings

Loads `qwen3-embedding-0.6b` and generates embedding vectors for a single input and a batch of three strings. Prints dimension count and the first five float values.

### Sample 5 — Azure Foundry cloud API

Calls your Foundry project's hosted deployment over HTTPS using `DefaultAzureCredential` (works with `az login`, managed identity, or environment variables). No local model download required.

---

## Project structure

```
FoundryLocal/
├── FoundryLocalSamples.csproj   # project file
├── Program.cs                   # all five samples
└── README.md                    # this file
```

## Key packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AI.Foundry.Local` | 2.2.1 | Foundry Local SDK |
| `Azure.AI.Projects` | 3.0.0-beta.5 | Azure Foundry cloud client (sample 5) |
| `Azure.Identity` | 1.21.0 | `DefaultAzureCredential` (sample 5) |
| `OpenAI` | 2.14.0 | OpenAI-compatible client (sample 3) |

> `Betalgo.Ranul.OpenAI` is pulled in transitively by `Microsoft.AI.Foundry.Local` and provides the `ChatMessage` / `ChatCompletionCreateResponse` types used in samples 1–2.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `FoundryLocalManager.Instance` is null | Call `await FoundryLocalManager.CreateAsync(...)` before accessing `Instance` |
| Model download hangs | Check that Foundry Local app is running and you have a network connection |
| Sample 3: web service port conflict | Edit `config.Web.Urls` in `Program.cs` to a free port |
| Sample 5: 401 Unauthorized | Run `az login` and confirm the identity has **Azure AI User** role on the project |
| `NETSDK1047` build error | Run `dotnet restore` before `dotnet build` |
