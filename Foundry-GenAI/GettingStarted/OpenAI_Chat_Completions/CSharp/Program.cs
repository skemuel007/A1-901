using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using DotNetEnv;
using OpenAI.Responses;

#pragma warning disable OPENAI001

// Match the notebook's load_dotenv() behavior by finding the nearest .env file
// in this directory or one of its parents.
Env.TraversePath().Load();

string projectEndpoint = GetRequiredEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
string modelDeploymentName = GetRequiredEnvironmentVariable("MODEL_DEPLOYMENT_NAME");

AIProjectClient projectClient = new(
    endpoint: new Uri(projectEndpoint),
    tokenProvider: new DefaultAzureCredential());

ProjectResponsesClient responsesClient = projectClient.ProjectOpenAIClient
    .GetProjectResponsesClientForModel(modelDeploymentName);

CreateResponseOptions options = new()
{
    Model = modelDeploymentName,
    Instructions = "You are a helpful AI assistant",
    Temperature = 0.7f,
    MaxOutputTokenCount = 1000
};

options.InputItems.Add(
    ResponseItem.CreateUserMessageItem(
        "Can you please tell me something about Microsoft Foundry?"));

ResponseResult response = await responsesClient.CreateResponseAsync(options);

Console.WriteLine($"Response output: {response.GetOutputText()}");

static string GetRequiredEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);

    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException(
            $"Required environment variable '{name}' is not set. " +
            "Add it to the repository's .env file or export it in your shell.")
        : value;
}

#pragma warning restore OPENAI001
