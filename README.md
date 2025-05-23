# InsightFlow

Local-first document intelligence platform built with .NET 8. Upload PDF, DOCX, and TXT files, extract text, generate embeddings, run semantic search, summarize content, and ask questions over your document corpus.

## Features

- Document upload pipeline with validation and storage
- OCR and text extraction with chunking
- ONNX-based embedding generation with caching
- Vector search with nearest-neighbor retrieval
- Hierarchical summarization and keyword extraction
- Retrieval-augmented document Q&A
- Background job processing for indexing workloads
- JWT role-based authentication
- Analytics dashboard and export (PDF, CSV, Markdown)

## Requirements

- .NET 8 SDK
- Optional: Docker for containerized runs

## Quick start

```bash
cp .env.example .env
dotnet restore
dotnet run --project src/InsightFlow.Api
```

Health check: `GET http://localhost:5080/health`  
API health: `GET http://localhost:5080/api/health`

## Solution layout

```
src/InsightFlow.Api          ASP.NET Core host and controllers
src/InsightFlow.Core         domain models and service contracts
src/InsightFlow.Infrastructure persistence, pipelines, integrations
tests/InsightFlow.Tests      unit and integration tests
```

## Configuration

Environment variables are documented in `.env.example`. Key paths:

| Variable | Purpose |
|----------|---------|
| `STORAGE_ROOT` | Uploaded document storage |
| `DATABASE_PATH` | SQLite database file |
| `ONNX_MODEL_PATH` | Embedding model weights |
| `OCR_TESSDATA_PATH` | Tesseract language data |

## Docker

```bash
docker build -t insightflow .
docker run -p 8080:8080 --env-file .env insightflow
```

## License

MIT — see [LICENSE](LICENSE).
