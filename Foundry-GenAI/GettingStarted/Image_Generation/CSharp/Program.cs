using Azure.Identity;
using DotNetEnv;
using OpenAI;
using OpenAI.Images;
using System.ClientModel;
using System.ClientModel.Primitives;

#pragma warning disable OPENAI001

string envPath = FindImageGenerationEnvironmentFile();
Env.Load(envPath);

Uri modelEndpoint = GetOpenAIV1Endpoint(
    GetRequiredEnvironmentVariable("IMAGE_MODEL_ENDPOINT"));
string modelDeploymentName = GetRequiredEnvironmentVariable("IMAGE_MODEL_DEPLOYMENT_NAME");

BearerTokenPolicy tokenPolicy = new(
    new DefaultAzureCredential(),
    "https://ai.azure.com/.default");

OpenAIClient openAIClient = new(
    authenticationPolicy: tokenPolicy,
    options: new OpenAIClientOptions
    {
        Endpoint = modelEndpoint
    });

ImageClient imageClient = openAIClient.GetImageClient(modelDeploymentName);

const string prompt =
    "a spaceman floating in space with earth in the background, digital art";

ImageGenerationOptions options = new()
{
    Size = GeneratedImageSize.W1024xH1024,
    ResponseFormat = GeneratedImageFormat.Bytes
};

try
{
    GeneratedImage generatedImage = await imageClient.GenerateImageAsync(prompt, options);

    string outputPath = Path.GetFullPath("output.png");
    await File.WriteAllBytesAsync(outputPath, generatedImage.ImageBytes.ToArray());

    Console.WriteLine($"Image saved to: {outputPath}");
}
catch (ClientResultException exception) when (exception.Status == 404)
{
    Console.Error.WriteLine(
        "Image generation returned 404. IMAGE_MODEL_ENDPOINT must be the OpenAI v1 " +
        "inference endpoint ending in '/openai/v1', not the Foundry project endpoint " +
        "ending in '/api/projects/<project>'.");
    Environment.ExitCode = 1;
}

static string FindImageGenerationEnvironmentFile()
{
    DirectoryInfo? directory = new(Directory.GetCurrentDirectory());

    while (directory is not null)
    {
        string repositoryRelativePath = Path.Combine(
            directory.FullName,
            "Foundry-GenAI",
            "GettingStarted",
            "Image_Generation",
            ".env");

        if (File.Exists(repositoryRelativePath))
        {
            return repositoryRelativePath;
        }

        if (directory.Name == "Image_Generation")
        {
            string localPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(localPath))
            {
                return localPath;
            }
        }

        directory = directory.Parent;
    }

    throw new FileNotFoundException(
        "Could not find Foundry-GenAI/GettingStarted/Image_Generation/.env.");
}

static string GetRequiredEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);

    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException(
            $"Required environment variable '{name}' is not set in the Image Generation .env file.")
        : value;
}

static Uri GetOpenAIV1Endpoint(string configuredEndpoint)
{
    if (!Uri.TryCreate(configuredEndpoint, UriKind.Absolute, out Uri? endpoint) ||
        (endpoint.Scheme != Uri.UriSchemeHttps && endpoint.Scheme != Uri.UriSchemeHttp))
    {
        throw new InvalidOperationException(
            "IMAGE_MODEL_ENDPOINT must be an absolute HTTP or HTTPS URL.");
    }

    string path = endpoint.AbsolutePath.TrimEnd('/');
    if (!path.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "IMAGE_MODEL_ENDPOINT must end in '/openai/v1'. Do not use the " +
            "Foundry project endpoint ending in '/api/projects/<project>'.");
    }

    return new Uri(endpoint.ToString().TrimEnd('/') + "/");
}

#pragma warning restore OPENAI001
