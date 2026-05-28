# corporate-rag-dotnet

A corporate chatbot that answers questions based on internal company documents using **RAG (Retrieval-Augmented Generation)** — fully local, no cloud required.

## What is RAG?

RAG combines two AI capabilities:
1. **Retrieval** — finds the most relevant passages from your document library using semantic search
2. **Generation** — feeds those passages as context to an LLM, which generates a grounded, accurate answer

This prevents hallucination: the model can only answer from what's actually in your documents.

---

## Architecture

```
                        ┌─────────────────────────────────────────┐
                        │              Clean Architecture           │
  ┌──────────┐          ├──────────┬──────────────┬───────────────┤
  │  Client  │──HTTP──▶ │   Api    │ Application  │    Domain     │
  └──────────┘          │          │  (MediatR)   │  (Entities)   │
                        └────┬─────┴──────┬───────┴───────────────┘
                             │            │
                    ┌────────▼────────────▼────────┐
                    │         Infrastructure         │
                    │  ┌──────────┐  ┌───────────┐  │
                    │  │ Qdrant   │  │  Ollama   │  │
                    │  │ (Vectors)│  │  (phi3)   │  │
                    │  └──────────┘  └───────────┘  │
                    └───────────────────────────────┘
```

### Ingestion Pipeline

```
PDF Upload ──▶ PdfPig (extract text) ──▶ Chunker (500 words / 50 overlap)
    ──▶ Ollama phi3 (embed each chunk) ──▶ Qdrant (store vectors)
```

### Question-Answering Pipeline

```
Question ──▶ Ollama phi3 (embed) ──▶ Qdrant (top-5 semantic search)
    ──▶ Context Builder ──▶ Ollama phi3 (LLM with ChatHistory) ──▶ Answer
```

---

## Tech Stack

| Component | Technology |
|---|---|
| API | .NET 10 Web API |
| AI Orchestration | Semantic Kernel |
| LLM & Embeddings | Ollama + phi3 (local) |
| Vector Store | Qdrant |
| PDF Extraction | PdfPig |
| Use Cases | MediatR |
| Tests | xUnit + Moq + FluentAssertions |
| Containers | Docker + Docker Compose |

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v24+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10) — for local development only
- ~3 GB free disk space for the phi3 model

> **GPU note**: phi3 runs on CPU by default (~10–30 s/response). For GPU acceleration, edit `docker-compose.yml` and add the appropriate NVIDIA/AMD runtime to the `ollama` service.

---

## Running Locally

```bash
# 1. Clone the repository
git clone https://github.com/your-org/corporate-rag-dotnet
cd corporate-rag-dotnet

# 2. Copy env file (optional — defaults work out of the box)
cp .env.example .env

# 3. Start all services
docker-compose up -d

# 4. Wait for phi3 to download (~2 GB, first run only)
docker logs -f corporate-rag-ollama-init

# 5. Open Swagger UI
start http://localhost:5000/swagger
```

The `ollama-init` container automatically pulls the phi3 model and exits. The API waits for Qdrant and Ollama to be healthy before starting.

---

## API Usage

### Upload a document

```bash
curl -X POST http://localhost:5000/api/documents/ingest \
  -F "file=@/path/to/company-policy.pdf"
```

Response:
```json
{ "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "chunkCount": 42 }
```

### Ask a question

```bash
curl -X POST http://localhost:5000/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"question": "What is the remote work policy?", "conversationId": "session-1"}'
```

Response:
```json
{
  "answer": "According to the Remote Work Policy document, employees may work remotely up to 3 days per week...",
  "conversationId": "session-1"
}
```

Omit `conversationId` to start a new conversation — a UUID is generated automatically.

### List documents

```bash
curl http://localhost:5000/api/documents
```

### Delete a document

```bash
curl -X DELETE http://localhost:5000/api/documents/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

## Running Tests

```bash
# Unit tests (no external dependencies)
dotnet test tests/UnitTests

# Integration tests (requires Qdrant + Ollama running)
docker-compose up -d qdrant ollama
dotnet test tests/IntegrationTests
```

---

## Sample Documents

The `samples/` folder contains two ready-to-use PDF files for testing the ingestion pipeline end-to-end:

| File | Description |
|---|---|
| `samples/employees.pdf` | Directory of 10 fictional employees with name, age, gender, nationality, salary, department and hire date |
| `samples/vacations.pdf` | Vacation schedule for each employee, with start/end dates, number of days and approval status |

Ingest them right after starting the stack:

```bash
curl -X POST http://localhost:5000/api/documents/ingest -F "file=@samples/employees.pdf"
curl -X POST http://localhost:5000/api/documents/ingest -F "file=@samples/vacations.pdf"
```

Then try asking questions like:

```bash
curl -X POST http://localhost:5000/api/chat/ask \
  -H "Content-Type: application/json" \
  -d '{"question": "Who is the Tech Lead and what is their salary?", "conversationId": "demo"}'
```

---

## Configuration

All settings can be overridden via environment variables:

| Variable | Default | Description |
|---|---|---|
| `Ollama__Endpoint` | `http://localhost:11434` | Ollama API base URL |
| `Qdrant__Host` | `localhost` | Qdrant hostname |
| `Qdrant__Port` | `6333` | Qdrant gRPC port |
| `DataPath` | `<app>/data` | Directory for the document registry JSON |
| `APP_PORT` | `5000` | Host port mapped to the API container |

---

## Notes

- **Model size**: phi3 (phi3:mini) is ~2.3 GB. Downloaded once and cached in the `ollama_models` Docker volume.
- **CPU performance**: Expect 10–30 seconds per response on a modern CPU. Inference is faster on machines with more RAM.
- **Security vulnerability**: `Microsoft.SemanticKernel.Core` 1.28.0 has a known advisory ([GHSA-2ww3-72rp-wpp4](https://github.com/advisories/GHSA-2ww3-72rp-wpp4)). Update the SK version in `Infrastructure.csproj` when a patched release is available.
- **Conversation history** is stored in-memory and cleared on API restart. For production, replace `InMemoryConversationHistoryService` with a Redis or database-backed implementation.
- **Document registry** is stored as a JSON file in the `data/` volume. For production, replace `JsonFileDocumentRepository` with a SQL or NoSQL database implementation.
