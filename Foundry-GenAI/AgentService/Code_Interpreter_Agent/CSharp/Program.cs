#pragma warning disable OPENAI001

using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Extensions.OpenAI;
using Azure.Identity;
using DotNetEnv;
using OpenAI.Files;
using OpenAI.Responses;
using OpenAI.Containers;

Env.TraversePath().Load();

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT not set");
var modelDeployment = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("MODEL_DEPLOYMENT_NAME not set");

Console.WriteLine($"Foundry Project Endpoint: {endpoint}");
Console.WriteLine($"Model Deployment Name: {modelDeployment}");

var projectClient = new AIProjectClient(endpoint: new Uri(endpoint), tokenProvider: new DefaultAzureCredential());

var projectOpenAI = projectClient.ProjectOpenAIClient;
var filesClient = projectOpenAI.GetProjectFilesClient();

// Upload CSV file for code interpreter
var csvPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "electronics_products.csv"));

Console.WriteLine($"Uploading file: {csvPath}");
var uploadResult = await filesClient.UploadFileAsync(csvPath, FileUploadPurpose.Assistants);
var uploadedFile = uploadResult.Value;
Console.WriteLine($"File uploaded (id: {uploadedFile.Id})");

// Create agent with code interpreter tool
const string agentName = "code-interpreter-agent";

var codeInterpreterTool = new CodeInterpreterTool(
    new CodeInterpreterToolContainer(
        CodeInterpreterToolContainerConfiguration.CreateAutomaticContainerConfiguration([uploadedFile.Id])
    )
);

var agentDefinition = new DeclarativeAgentDefinition(modelDeployment)
{
    Instructions = "You are a helpful AI Assistant with code interpreter capabilities.",
    Tools = { codeInterpreterTool }
};

var versionOptions = new ProjectsAgentVersionCreationOptions(agentDefinition);
ProjectsAgentVersion agentVersion = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(agentName, versionOptions);
Console.WriteLine($"Agent created (id: {agentVersion.Id}, name: {agentVersion.Name}, version: {agentVersion.Version})");

// Create conversation
var conversationResult = await projectOpenAI.GetProjectConversationsClient().CreateProjectConversationAsync();
var conversationId = conversationResult.Value.Id;
Console.WriteLine($"Created conversation with id: {conversationId}");

// Send request to the agent
var agentRef = new Azure.AI.Extensions.OpenAI.AgentReference(agentName, agentVersion.Version);
var responsesClient = projectOpenAI.GetProjectResponsesClientForAgent(agentRef, conversationId);

const string userMessage = "Could you please create a column chart with products on the x-axis and their respective prices on the y-axis?";
Console.WriteLine($"\nUser: {userMessage}");

ResponseResult response = await responsesClient.CreateResponseAsync(userMessage);
Console.WriteLine($"Response completed with id: {response.Id}");

// Print response text
string responseText = response.GetOutputText();
Console.WriteLine($"\nAssistant: {responseText}");

// Extract and download any generated files from annotations
var containerClient = projectOpenAI.GetContainerClient();

foreach (var outputItem in response.OutputItems)
{
    if (outputItem is not MessageResponseItem message) continue;

    foreach (var content in message.Content)
    {
        foreach (var annotation in content.OutputTextAnnotations)
        {
            if (annotation is not ContainerFileCitationMessageAnnotation fileCitation) continue;

            Console.WriteLine($"\nFound generated file: {fileCitation.Filename} (ID: {fileCitation.FileId})");

            var fileContent = await containerClient.DownloadContainerFileAsync(fileCitation.ContainerId, fileCitation.FileId);
            var bytes = fileContent.Value.ToArray();

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileCitation.Filename);
            await File.WriteAllBytesAsync(outputPath, bytes);
            Console.WriteLine($"File saved: {outputPath}");
        }
    }
}
