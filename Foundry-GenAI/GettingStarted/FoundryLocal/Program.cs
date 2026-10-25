#pragma warning disable OPENAI001

using System.ClientModel;
using Azure.AI.Projects;
using Azure.Identity;
using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using Microsoft.AI.Foundry.Local;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI;

// Sample selector
Console.WriteLine("Azure AI Foundry Local SDK Samples");
Console.WriteLine("===================================");
Console.WriteLine("1. Stream chat response (native SDK)");
Console.WriteLine("2. Quick start: run a model");
Console.WriteLine("3. OpenAI SDK via Foundry Local web service");
Console.WriteLine("4. Generate text embeddings");
Console.WriteLine("5. Call Azure Foundry cloud API");
Console.WriteLine();
Console.Write("Choose a sample (1-5): ");
var choice = Console.ReadLine()?.Trim();

switch (choice)
{
    case "1": await Sample1_StreamChatAsync(); break;
    case "2": await Sample2_QuickStartAsync(); break;
    case "3": await Sample3_OpenAIWebServiceAsync(); break;
    case "4": await Sample4_EmbeddingsAsync(); break;
    case "5": await Sample5_AzureFoundryCloudAsync(); break;
    default:
        Console.WriteLine("Invalid choice.");
        break;
}

// ─────────────────────────────────────────────────────────────────────────────
// Sample 1: Initialize Foundry Local SDK and stream a chat response
// ─────────────────────────────────────────────────────────────────────────────
static async Task Sample1_StreamChatAsync()
{
    Console.WriteLine("\n[Sample 1] Streaming chat response via native Foundry Local SDK");

    var config = new Configuration
    {
        AppName = "foundry-local-samples",
        LogLevel = LogLevel.Warning
    };

    await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
    var mgr = FoundryLocalManager.Instance;

    Console.WriteLine("Discovering and registering execution providers...");
    await mgr.DownloadAndRegisterEpsAsync(
        progressCallback: (epName, percent) => Console.Write($"\r  {epName}: {percent:P0}   "),
        ct: null);
    Console.WriteLine();

    var catalog = await mgr.GetCatalogAsync();
    var model = await catalog.GetModelAsync("phi-4-mini");

    if (!await model.IsCachedAsync())
    {
        Console.WriteLine($"Downloading {model.Alias}...");
        await model.DownloadAsync(
            downloadProgress: p => Console.Write($"\r  {p:P0}   "),
            ct: null);
        Console.WriteLine();
    }

    Console.WriteLine($"Loading {model.Alias}...");
    await model.LoadAsync();

    var chatClient = await model.GetChatClientAsync();

    List<ChatMessage> messages =
    [
        ChatMessage.FromSystem("You are a helpful assistant."),
        ChatMessage.FromUser("Why is the sky blue? Answer in two sentences.")
    ];

    Console.WriteLine("\nAssistant: ");
    await foreach (var chunk in chatClient.CompleteChatStreamingAsync(messages, CancellationToken.None))
    {
        var delta = chunk.Choices?.FirstOrDefault()?.Delta?.Content;
        if (!string.IsNullOrEmpty(delta))
            Console.Write(delta);
    }
    Console.WriteLine();

    await model.UnloadAsync();
    mgr.Shutdown();
}

// ─────────────────────────────────────────────────────────────────────────────
// Sample 2: Quick start — run a model with a single completion
// ─────────────────────────────────────────────────────────────────────────────
static async Task Sample2_QuickStartAsync()
{
    Console.WriteLine("\n[Sample 2] Quick start: run a model with a single chat completion");

    await FoundryLocalManager.CreateAsync(
        new Configuration { AppName = "foundry-local-samples", LogLevel = LogLevel.Warning },
        NullLogger.Instance);

    var manager = FoundryLocalManager.Instance;

    Console.WriteLine("Registering execution providers...");
    await manager.DownloadAndRegisterEpsAsync(ct: null);

    var catalog = await manager.GetCatalogAsync();
    var model = await catalog.GetModelAsync("phi-4-mini");

    if (!await model.IsCachedAsync())
    {
        Console.WriteLine($"Downloading {model.Alias}...");
        await model.DownloadAsync(
            downloadProgress: p => Console.Write($"\r  {p:P0}   "),
            ct: null);
        Console.WriteLine();
    }

    await model.LoadAsync();

    var chatClient = await model.GetChatClientAsync();
    var response = await chatClient.CompleteChatAsync(
    [
        ChatMessage.FromSystem("You are a helpful assistant."),
        ChatMessage.FromUser("Explain async/await in C# in two sentences.")
    ]);

    Console.WriteLine("\nAssistant: " + response.Choices?.FirstOrDefault()?.Message?.Content);

    await model.UnloadAsync();
    manager.Shutdown();
}

// ─────────────────────────────────────────────────────────────────────────────
// Sample 3: Use OpenAI SDK against the Foundry Local web service
// ─────────────────────────────────────────────────────────────────────────────
static async Task Sample3_OpenAIWebServiceAsync()
{
    Console.WriteLine("\n[Sample 3] OpenAI SDK via Foundry Local web service");

    var config = new Configuration
    {
        AppName = "foundry-local-samples",
        LogLevel = LogLevel.Warning,
        Web = new Configuration.WebService { Urls = "http://127.0.0.1:52495" }
    };

    await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
    var mgr = FoundryLocalManager.Instance;

    await mgr.DownloadAndRegisterEpsAsync(ct: null);

    var catalog = await mgr.GetCatalogAsync();
    var model = await catalog.GetModelAsync("phi-4-mini");

    if (!await model.IsCachedAsync())
    {
        Console.WriteLine($"Downloading {model.Alias}...");
        await model.DownloadAsync(
            downloadProgress: p => Console.Write($"\r  {p:P0}   "),
            ct: null);
        Console.WriteLine();
    }

    await model.LoadAsync();
    await mgr.StartWebServiceAsync();

    var baseUrl = config.Web.Urls.TrimEnd('/') + "/v1";
    Console.WriteLine($"Web service started at {baseUrl}");

    var openAIClient = new OpenAIClient(
        new ApiKeyCredential("not-needed"),
        new OpenAIClientOptions { Endpoint = new Uri(baseUrl) });

    var chatClient = openAIClient.GetChatClient(model.Id);

    Console.WriteLine("\nAssistant (streaming): ");
    var completionUpdates = chatClient.CompleteChatStreaming("Why is the sky blue?");
    foreach (var update in completionUpdates)
    {
        foreach (var part in update.ContentUpdate)
            Console.Write(part.Text);
    }
    Console.WriteLine();

    await mgr.StopWebServiceAsync();
    await model.UnloadAsync();
    mgr.Shutdown();
}

// ─────────────────────────────────────────────────────────────────────────────
// Sample 4: Generate text embeddings locally
// ─────────────────────────────────────────────────────────────────────────────
static async Task Sample4_EmbeddingsAsync()
{
    Console.WriteLine("\n[Sample 4] Generate text embeddings locally");

    var config = new Configuration
    {
        AppName = "foundry-local-samples",
        LogLevel = LogLevel.Warning
    };

    await FoundryLocalManager.CreateAsync(config, NullLogger.Instance);
    var mgr = FoundryLocalManager.Instance;

    await mgr.DownloadAndRegisterEpsAsync(ct: null);

    var catalog = await mgr.GetCatalogAsync();
    var model = await catalog.GetModelAsync("qwen3-embedding-0.6b");

    if (!await model.IsCachedAsync())
    {
        Console.WriteLine($"Downloading {model.Alias}...");
        await model.DownloadAsync(
            downloadProgress: p => Console.Write($"\r  {p:P0}   "),
            ct: null);
        Console.WriteLine();
    }

    await model.LoadAsync();

    var embeddingClient = await model.GetEmbeddingClientAsync();

    // Single embedding
    var singleResponse = await embeddingClient.GenerateEmbeddingAsync("The quick brown fox jumps over the lazy dog");
    var embedding = singleResponse.Data?.FirstOrDefault()?.Embedding;
    if (embedding != null)
    {
        Console.WriteLine($"Dimensions: {embedding.Count}");
        Console.WriteLine($"First 5 values: [{string.Join(", ", embedding.Take(5).Select(v => v.ToString("F4")))}]");
    }

    // Batch embeddings
    string[] inputs =
    [
        "Machine learning is a subset of artificial intelligence",
        "The capital of France is Paris",
        "Rust is a systems programming language"
    ];

    var batchResponse = await embeddingClient.GenerateEmbeddingsAsync(inputs);
    Console.WriteLine($"\nBatch embeddings generated: {batchResponse.Data?.Count ?? 0} vectors");
    if (batchResponse.Data != null)
    {
        for (int i = 0; i < batchResponse.Data.Count; i++)
            Console.WriteLine($"  [{i}] dims={batchResponse.Data[i].Embedding?.Count}  input=\"{inputs[i]}\"");
    }

    await model.UnloadAsync();
    mgr.Shutdown();
}

// ─────────────────────────────────────────────────────────────────────────────
// Sample 5: Call Azure Foundry cloud API with DefaultAzureCredential
// ─────────────────────────────────────────────────────────────────────────────
static async Task Sample5_AzureFoundryCloudAsync()
{
    Console.WriteLine("\n[Sample 5] Azure Foundry cloud API with DefaultAzureCredential");

    var foundryProjectEndpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
    var modelDeployment = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME") ?? "gpt-5.1";

    if (string.IsNullOrEmpty(foundryProjectEndpoint))
    {
        Console.WriteLine("Set FOUNDRY_PROJECT_ENDPOINT environment variable to run this sample.");
        Console.WriteLine("Example: export FOUNDRY_PROJECT_ENDPOINT=https://<your-project>.services.ai.azure.com/api/projects/<name>");
        return;
    }

    AIProjectClient projectClient = new(new Uri(foundryProjectEndpoint), new DefaultAzureCredential());

    var responseClient = projectClient.ProjectOpenAIClient.GetProjectResponsesClientForModel(modelDeployment);
    var response = await responseClient.CreateResponseAsync("What is the size of France in square miles?");

    Console.WriteLine("\nAssistant: " + response.Value.GetOutputText());
}
