# Chat Completions — C# Console

This console application is the C# equivalent of
`../chatCompletions.ipynb`. It authenticates with Microsoft Entra ID, connects
to a Microsoft Foundry project, and sends the same request through the OpenAI
Responses API.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- A Microsoft Foundry project with a deployed model

## Configure and run

From the repository root, create `.env` if it does not already exist:

```bash
cp .env.example .env
```

Set these values in `.env`:

```dotenv
FOUNDRY_PROJECT_ENDPOINT=https://<resource>.services.ai.azure.com/api/projects/<project>
MODEL_DEPLOYMENT_NAME=<model-deployment-name>
```

Authenticate and run the application:

```bash
az login
dotnet run --project Foundry-GenAI/GettingStarted/OpenAI_Chat_Completions/CSharp/ChatCompletions.csproj
```

`DefaultAzureCredential` can also use other supported credentials, such as
managed identity, when the application runs in Azure.
