## AI-901: Azure AI Fundamentals

![AI-901 course thumbnail](./Assets/AI-901-course-thumbnail.png)

This repository contains hands-on Jupyter notebooks for Azure AI Speech,
Language, Vision, Content Understanding, Azure OpenAI, and Microsoft Foundry.

## Prerequisites

- [Conda](https://docs.conda.io/projects/conda/en/latest/user-guide/install/index.html)
  (Miniconda, Anaconda, or Miniforge)
- Access to the Azure resources used by the notebook you want to run
- For notebooks that use `DefaultAzureCredential`, an authenticated Azure CLI
  session (`az login`) or another supported Azure identity

## Create the Conda environment

From the repository root, run:

```bash
conda env create -f environment.yml
conda activate ai-901
python -m ipykernel install --user --name ai-901 --display-name "Python (ai-901)"
```

If the environment already exists, update it after pulling changes:

```bash
conda env update -f environment.yml --prune
conda activate ai-901
```

The environment uses Python 3.11 and contains the dependencies required by all
notebooks in this repository.

## Configure credentials

Create a local `.env` file from the supplied template:

```bash
cp .env.example .env
```

Fill in only the values needed for the notebook you plan to run. The notebooks
use `python-dotenv`, which searches the notebook directory and its parent
directories, so the single `.env` file at the repository root is sufficient.

The `.env` file is ignored by Git. Do not commit API keys or other secrets.

## Run the notebooks

With the environment active, start Jupyter from the repository root:

```bash
jupyter lab
```

Open a notebook and select **Python (ai-901)** as its kernel. Run its cells in
order.

Some notebooks contain a `%pip install` cell for standalone use. Skip that cell
when using this Conda environment. In particular, the notebooks contain two
different OpenAI package pins; the shared environment already provides the
newer compatible version.

To stop Jupyter, press `Ctrl+C` in its terminal. To leave the environment, run:

```bash
conda deactivate
```

## Troubleshooting

- **Kernel not listed:** rerun the `python -m ipykernel install ...` command
  while `ai-901` is active, then refresh Jupyter.
- **Authentication failed:** run `az login` for notebooks using
  `DefaultAzureCredential`, or check the relevant key and endpoint in `.env`.
- **Environment changed after running a notebook:** a `%pip install` cell was
  probably executed. Restore it with
  `conda env update -f environment.yml --prune`.
- **Conda creates an incomplete environment on macOS:** retry environment
  creation with package-file copying enabled:
  `CONDA_ALWAYS_COPY=true conda env create -f environment.yml`.
