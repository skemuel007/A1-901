# Image Generation — C# Console

This console application is the C# equivalent of `../imageGeneration.ipynb`.
It authenticates with Microsoft Entra ID, calls the OpenAI-compatible v1
inference endpoint for the image model deployed in Microsoft Foundry, and saves
the generated 1024×1024 image as `output.png`.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- A Microsoft Foundry project with a supported image model deployment
- Access to the image model used by the deployment

## Configure the application

Set these values in `Foundry-GenAI/GettingStarted/Image_Generation/.env`:

```dotenv
IMAGE_MODEL_ENDPOINT=https://<resource>.services.ai.azure.com/openai/v1
IMAGE_MODEL_DEPLOYMENT_NAME=<image-model-deployment-name>
```

You can also use the Azure OpenAI-compatible hostname:

```dotenv
IMAGE_MODEL_ENDPOINT=https://<resource>.openai.azure.com/openai/v1
```

Do not set `IMAGE_MODEL_ENDPOINT` to the Foundry project endpoint ending in
`/api/projects/<project>`. The standalone Images API is an inference API, and
using the project endpoint with `GetImageClient` results in HTTP 404.

The application uses Microsoft Entra authentication, so
`IMAGE_MODEL_API_KEY` and `IMAGE_FOUNDRY_PROJECT_ENDPOINT` are not required by
this console app.

## Run

From the repository root:

```bash
az login
dotnet run --project Foundry-GenAI/GettingStarted/Image_Generation/CSharp/ImageGeneration.csproj
```

The generated file is written to `output.png` in the directory from which the
command is run. Image generation is a billable Azure operation.
