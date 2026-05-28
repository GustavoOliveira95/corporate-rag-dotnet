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
                    │  │ Qdrant   │  │  Ollama        │  │
                    │  │ (Vectors)│  │  (llama3.2:3b) │  │
                    │  └──────────┘  └───────────┘  │
                    └───────────────────────────────┘
```

### Ingestion Pipeline

```
PDF/CSV Upload ──▶ DocumentLoaderRouter ──▶ PdfPig or CsvLoader (extract text)
    ──▶ Chunker (500 words / 50 overlap)
    ──▶ Ollama llama3.2:3b (embed each chunk) ──▶ Qdrant (store vectors)
```

### Question-Answering Pipeline

```
Question ──▶ Ollama llama3.2:3b (embed) ──▶ Qdrant (top-5 semantic search)
    ──▶ Context Builder ──▶ Ollama llama3.2:3b (LLM with ChatHistory) ──▶ Answer
```

---

## Tech Stack

| Component | Technology |
|---|---|
| API | .NET 10 Web API |
| AI Orchestration | Semantic Kernel |
| LLM & Embeddings | Ollama + llama3.2:3b (local) |
| Vector Store | Qdrant |
| PDF Extraction | PdfPig |
| CSV Extraction | Built-in (header → value rows) |
| Use Cases | MediatR |
| Tests | xUnit + Moq + FluentAssertions |
| Containers | Docker + Docker Compose |

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v24+)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10) — for local development only
- ~2.5 GB free disk space for the llama3.2:3b model

> **GPU note**: llama3.2:3b runs on CPU by default (~1–3 min/response). For GPU acceleration, edit `docker-compose.yml` and add the appropriate NVIDIA/AMD runtime to the `ollama` service.

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

# 4. Wait for llama3.2:3b to download (~2 GB, first run only)
docker logs -f corporate-rag-ollama-init

# 5. Open Swagger UI
start http://localhost:5000/swagger
```

The `ollama-init` container automatically pulls the llama3.2:3b model and exits. The API waits for Qdrant and Ollama to be healthy before starting.

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/documents/ingest` | Upload and index a PDF or CSV document |
| `GET` | `/api/documents` | List all indexed documents |
| `DELETE` | `/api/documents/{id}` | Remove a document and all its chunks from the index |
| `POST` | `/api/chat/ask` | Ask a question and get an answer grounded in the indexed documents |

Full interactive docs available at `http://localhost:5000/swagger`.

---

## Sample Documents

The `samples/` folder contains optional PDF files for testing the API end-to-end:

| File | Description |
|---|---|
| `samples/employees.pdf` | Directory of 10 fictional employees with name, age, gender, nationality, salary, department and hire date |
| `samples/vacations.pdf` | Vacation schedule for each employee, with start/end dates, number of days and approval status |

---

## Notes

- **Model size**: llama3.2:3b is ~2 GB. Downloaded once and cached in the `ollama_models` Docker volume.
- **CPU performance**: Expect 1–3 minutes per response on CPU. Inference is faster on machines with a dedicated GPU.
- **Conversation history** is stored in-memory and cleared on API restart. For production, replace `InMemoryConversationHistoryService` with a Redis or database-backed implementation.
- **Document registry** is stored as a JSON file in the `data/` volume. For production, replace `JsonFileDocumentRepository` with a SQL or NoSQL database implementation.
