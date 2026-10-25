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
var mcpServerName = Environment.GetEnvironmentVariable("MCP_SERVER_NAME")
    ?? throw new InvalidOperationException("MCP_SERVER_NAME not set");

Console.WriteLine($"Foundry Project Endpoint: {endpoint}");
Console.WriteLine($"Model Deployment Name: {modelDeployment}");
Console.WriteLine($"MCP Server Name: {mcpServerName}");

var projectClient = new AIProjectClient(endpoint: new Uri(endpoint), tokenProvider: new DefaultAzureCredential());

// Find the MCP server connection ID by name
string connectionId = "";
foreach (var connection in projectClient.Connections.GetConnections(connectionType: null, defaultConnection: null))
{
    if (connection.Name == mcpServerName)
    {
        connectionId = connection.Id;
        break;
    }
}

if (string.IsNullOrEmpty(connectionId))
    throw new InvalidOperationException($"MCP server connection '{mcpServerName}' not found");

Console.WriteLine($"MCP Server Connection ID: {connectionId}");

// Create MCP tool using the connection ID
var mcpTool = new McpTool("microsoft_learn_server", new Uri("https://learn.microsoft.com/api/mcp"))
{
    ToolCallApprovalPolicy = new McpToolCallApprovalPolicy(DefaultMcpToolCallApprovalPolicy.NeverRequireApproval)
};

// Create agent with MCP tool
const string agentName = "MCP-Agent";

var agentDefinition = new DeclarativeAgentDefinition(modelDeployment)
{
    Instructions = "You are an intelligent assistant that can interact with the Microsoft Learn MCP server to provide users with relevant learning resources and information about Microsoft technologies.",
    Tools = { mcpTool }
};

var versionOptions = new ProjectsAgentVersionCreationOptions(agentDefinition);
ProjectsAgentVersion agentVersion = await projectClient.AgentAdministrationClient.CreateAgentVersionAsync(agentName, versionOptions);
Console.WriteLine($"Created MCP Agent (id: {agentVersion.Id}, name: {agentVersion.Name}, version: {agentVersion.Version})");

// Create conversation
var projectOpenAI = projectClient.ProjectOpenAIClient;
var conversationResult = await projectOpenAI.GetProjectConversationsClient().CreateProjectConversationAsync();
var conversationId = conversationResult.Value.Id;
Console.WriteLine($"Created conversation with id: {conversationId}");

// Chat loop
var agentRef = new Azure.AI.Extensions.OpenAI.AgentReference(agentName, agentVersion.Version);
var responsesClient = projectOpenAI.GetProjectResponsesClientForAgent(agentRef, conversationId);

string userQuery = "Can you please help me with the latest AI Foundry SDK code samples?";
Console.WriteLine($"\nUser: {userQuery}");

ResponseResult response = await responsesClient.CreateResponseAsync(userQuery);
Console.WriteLine($"\nAgent: {response.GetOutputText()}");
