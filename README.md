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

## Authentication

Default local users (change in production):

| User | Password | Roles |
|------|----------|-------|
| admin | admin123 | Admin, Analyst |
| analyst | analyst123 | Analyst |
| viewer | viewer123 | Viewer |

```bash
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Use the returned bearer token for protected endpoints.

## API overview

| Endpoint | Description |
|----------|-------------|
| `POST /api/documents` | Upload PDF, DOCX, or TXT |
| `POST /api/documents/{id}/extraction` | Extract text and chunks |
| `POST /api/documents/{id}/embeddings` | Generate embeddings |
| `POST /api/search` | Semantic vector search |
| `POST /api/documents/{id}/summary` | Summarize document |
| `POST /api/qa/ask` | Ask a question over indexed content |
| `POST /api/jobs` | Queue background indexing jobs |
| `GET /api/analytics` | Dashboard metrics |
| `GET /api/export/documents/{id}/pdf` | PDF report export |

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
| `JWT_SECRET` | Signing key for bearer tokens |
| `JWT_ISSUER` / `JWT_AUDIENCE` | Token validation metadata |

## Docker

```bash
docker build -t insightflow .
docker run -p 8080:8080 --env-file .env insightflow
```

## License

MIT — see [LICENSE](LICENSE).
