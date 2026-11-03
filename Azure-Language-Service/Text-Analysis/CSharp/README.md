# Text Analysis — C# Console

C# equivalent of `../text-analysis.ipynb`. Runs five Azure Language Service analyses (key phrase extraction, linked entity recognition, NER, PII detection, sentiment + opinion mining) against all `.txt` files in `../reviews/`.

## Prerequisites

| Tool | Install |
|------|---------|
| .NET 10 SDK | https://dotnet.microsoft.com/download |
| Azure CLI | https://learn.microsoft.com/cli/azure/install-azure-cli |
| Azure Language Service resource | see setup below |

## Azure Setup

### 1. Create a Language Service resource

1. Go to [Azure Portal](https://portal.azure.com) → **Create a resource** → search **Language service**
2. Click **Create** → choose **Custom features** or accept defaults → **Continue to create your resource**
3. Fill in:
   - **Subscription** — your subscription
   - **Resource group** — new or existing
   - **Region** — e.g. `East US`
   - **Name** — unique name, e.g. `my-language-svc`
   - **Pricing tier** — `Free F0` (up to 5,000 transactions/month) or `Standard S`
4. **Review + create** → **Create**. Wait for deployment to complete.

### 2. Get the endpoint

1. Go to the resource → **Keys and Endpoint** (left menu)
2. Copy **Endpoint** — it looks like `https://<name>.cognitiveservices.azure.com/`

### 3. Assign role to your identity

The app uses `DefaultAzureCredential` (no API key needed). Your Azure identity must have the **Cognitive Services User** role on the resource.

```bash
# Get the resource ID
az cognitiveservices account show \
  --name <resource-name> \
  --resource-group <resource-group> \
  --query id -o tsv

# Assign the role (replace <resource-id> and <your-email>)
az role assignment create \
  --role "Cognitive Services User" \
  --assignee <your-email> \
  --scope <resource-id>
```

### 4. Configure environment variables

Edit `Azure-Language-Service/Text-Analysis/.env`:

```dotenv
AZURE_LANGUAGE_ENDPOINT=https://<name>.cognitiveservices.azure.com/
```

The `.env` file is gitignored — never commit secrets.

## Run

```bash
az login
dotnet run --project Azure-Language-Service/Text-Analysis/CSharp/TextAnalysis.csproj
```

## Expected output

```
Loaded 5 review(s) from: .../reviews

=== Key Phrase Extraction ===
Key phrases in 'review1.txt':
  - comfortable hotel
  - good location
  ...

=== Linked Entities ===
Linked entities in 'review1.txt':
  - Name: London, ID: Q84, URL: https://en.wikipedia.org/wiki/London, Source: Wikipedia
  ...

=== Named Entity Recognition (NER) ===
Entity in 'review1.txt': London, Category: Location, Subcategory: City
...

=== PII Detection ===
(no PII in sample reviews — output varies with document content)

=== Sentiment Analysis ===
Text: ...
Overall Sentiment: Positive
Positive Score: 0.98
Neutral Score:  0.01
Negative Score: 0.01
  Sentence: ...
  Sentence Sentiment: Positive
  ...
```

## Project structure

```
Text-Analysis/
├── CSharp/
│   ├── TextAnalysis.csproj
│   ├── Program.cs
│   └── README.md         # this file
├── reviews/              # .txt review files read at runtime
├── text-analysis.ipynb   # Python notebook equivalent
└── .env                  # AZURE_LANGUAGE_ENDPOINT (gitignored)
```

## Key packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Azure.AI.TextAnalytics` | 5.3.0 | Language Service client |
| `Azure.Identity` | 1.21.0 | `DefaultAzureCredential` |
| `DotNetEnv` | 3.2.0 | Load `.env` file |

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `AZURE_LANGUAGE_ENDPOINT not set` | Add endpoint to `.env` in `Text-Analysis/` folder |
| `AuthenticationFailedException` | Run `az login` |
| `403 Forbidden` | Assign **Cognitive Services User** role (step 3 above) |
| `reviews/` directory not found | Run from repo root or use `--project` flag as shown |
| `Free tier quota exceeded` | Upgrade to Standard S tier or wait for monthly reset |
