using Azure;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using DotNetEnv;

Env.TraversePath().Load();

var endpoint = Environment.GetEnvironmentVariable("AZURE_LANGUAGE_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_LANGUAGE_ENDPOINT not set");

var client = new TextAnalyticsClient(new Uri(endpoint), new DefaultAzureCredential());

// Load all review files from the reviews/ folder next to the project root
var reviewsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "reviews"));
var documents = new Dictionary<string, string>();

foreach (var file in Directory.GetFiles(reviewsDir, "*.txt").OrderBy(f => f))
{
    documents[Path.GetFileName(file)] = File.ReadAllText(file, System.Text.Encoding.UTF8);
}

Console.WriteLine($"Loaded {documents.Count} review(s) from: {reviewsDir}");
var texts = documents.Values.ToList();
var names = documents.Keys.ToList();

// ─── Key Phrase Extraction ───────────────────────────────────────────────────
Console.WriteLine("\n=== Key Phrase Extraction ===");
var keyPhrasesResponse = await client.ExtractKeyPhrasesBatchAsync(texts);
foreach (var (result, idx) in keyPhrasesResponse.Value.Select((r, i) => (r, i)))
{
    if (result.HasError)
    {
        Console.WriteLine($"Error in '{names[idx]}': {result.Error.ErrorCode} - {result.Error.Message}");
        continue;
    }
    Console.WriteLine($"Key phrases in '{names[idx]}':");
    foreach (var phrase in result.KeyPhrases)
        Console.WriteLine($"  - {phrase}");
}

// ─── Linked Entity Recognition ───────────────────────────────────────────────
Console.WriteLine("\n=== Linked Entities ===");
var linkedResponse = await client.RecognizeLinkedEntitiesBatchAsync(texts);
foreach (var (result, idx) in linkedResponse.Value.Select((r, i) => (r, i)))
{
    if (result.HasError)
    {
        Console.WriteLine($"Error in '{names[idx]}': {result.Error.ErrorCode} - {result.Error.Message}");
        continue;
    }
    Console.WriteLine($"Linked entities in '{names[idx]}':");
    foreach (var entity in result.Entities)
        Console.WriteLine($"  - Name: {entity.Name}, ID: {entity.DataSourceEntityId}, URL: {entity.Url}, Source: {entity.DataSource}");
}

// ─── Named Entity Recognition ────────────────────────────────────────────────
Console.WriteLine("\n=== Named Entity Recognition (NER) ===");
var nerResponse = await client.RecognizeEntitiesBatchAsync(texts);
foreach (var (result, idx) in nerResponse.Value.Select((r, i) => (r, i)))
{
    if (result.HasError)
    {
        Console.WriteLine($"Error in '{names[idx]}': {result.Error.ErrorCode} - {result.Error.Message}");
        continue;
    }
    foreach (var entity in result.Entities)
        Console.WriteLine($"Entity in '{names[idx]}': {entity.Text}, Category: {entity.Category}, Subcategory: {entity.SubCategory}");
}

// ─── PII Detection ───────────────────────────────────────────────────────────
Console.WriteLine("\n=== PII Detection ===");
var piiResponse = await client.RecognizePiiEntitiesBatchAsync(texts);
foreach (var (result, idx) in piiResponse.Value.Select((r, i) => (r, i)))
{
    if (result.HasError)
    {
        Console.WriteLine($"Error in '{names[idx]}': {result.Error.ErrorCode} - {result.Error.Message}");
        continue;
    }
    foreach (var entity in result.Entities)
        Console.WriteLine($"PII Entity in '{names[idx]}': {entity.Text}, Type: {entity.Category}");
}

// ─── Sentiment Analysis with Opinion Mining ──────────────────────────────────
Console.WriteLine("\n=== Sentiment Analysis ===");
var sentimentResponse = await client.AnalyzeSentimentBatchAsync(
    texts,
    options: new AnalyzeSentimentOptions { IncludeOpinionMining = true });

foreach (var (result, idx) in sentimentResponse.Value.Select((r, i) => (r, i)))
{
    if (result.HasError)
    {
        Console.WriteLine($"Error in document {idx}: {result.Error}");
        continue;
    }

    Console.WriteLine($"\nText: {texts[idx]}");
    Console.WriteLine($"Overall Sentiment: {result.DocumentSentiment.Sentiment}");
    Console.WriteLine($"Positive Score: {result.DocumentSentiment.ConfidenceScores.Positive:F2}");
    Console.WriteLine($"Neutral Score:  {result.DocumentSentiment.ConfidenceScores.Neutral:F2}");
    Console.WriteLine($"Negative Score: {result.DocumentSentiment.ConfidenceScores.Negative:F2}");

    foreach (var sentence in result.DocumentSentiment.Sentences)
    {
        Console.WriteLine($"\tSentence: {sentence.Text}");
        Console.WriteLine($"\tSentence Sentiment: {sentence.Sentiment}");
        Console.WriteLine($"\tScores: Pos={sentence.ConfidenceScores.Positive:F2}, Neu={sentence.ConfidenceScores.Neutral:F2}, Neg={sentence.ConfidenceScores.Negative:F2}");
    }
}
