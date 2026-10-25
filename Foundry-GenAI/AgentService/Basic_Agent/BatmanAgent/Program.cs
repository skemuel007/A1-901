#pragma warning disable OPENAI001

using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.AI.Extensions.OpenAI;
using Azure.Identity;
using DotNetEnv;
using OpenAI.Responses;

Env.TraversePath().Load();

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT not set");
var modelDeployment = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("MODEL_DEPLOYMENT_NAME not set");

Console.WriteLine($"Foundry Project Endpoint: {endpoint}");
Console.WriteLine($"Model Deployment Name: {modelDeployment}");

var projectClient = new AIProjectClient(endpoint: new Uri(endpoint), tokenProvider: new DefaultAzureCredential());

const string agentName = "batman-agent";

var agentDefinition = new DeclarativeAgentDefinition(modelDeployment)
{
    Instructions = "You are Batman, the Dark Knight of Gotham City. Respond to all queries in character as Batman would.",
};

var versionOptions = new ProjectsAgentVersionCreationOptions(agentDefinition);
ProjectsAgentVersion agentVersion = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(agentName, versionOptions);

Console.WriteLine($"Agent created (name: {agentVersion.Name}, version: {agentVersion.Version})");

var projectOpenAI = projectClient.ProjectOpenAIClient;

var conversationResult = await projectOpenAI.GetProjectConversationsClient().CreateProjectConversationAsync();
var conversationId = conversationResult.Value.Id;

Console.WriteLine($"Created conversation with id: {conversationId}");

var responsesClient = projectOpenAI.GetProjectResponsesClientForAgent(agentName, conversationId);

while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() is "exit" or "quit")
    {
        Console.WriteLine("Exiting chat. Goodbye!");
        break;
    }

    ResponseResult response = await responsesClient.CreateResponseAsync(userInput);
    Console.WriteLine($"Batman: {response.GetOutputText()}");
}
