# AI Knowledge Assistant
### RAG Platform — Enterprise Project Documentation

> **Stack:** ASP.NET Core 8 · Ollama · Qdrant · React · PostgreSQL  
> **Patterns:** Clean Architecture · CQRS · MediatR · JWT · Docker  
> **Cost:** 100% Free & Open Source · Fully Offline

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Scope & Boundaries](#2-scope--boundaries)
3. [Technology Stack](#3-technology-stack)
4. [Architecture Overview](#4-architecture-overview)
5. [Repository Structure](#5-repository-structure)
6. [API Endpoints](#6-api-endpoints)
7. [Implementation Status](#7-implementation-status)

---

## 1. Project Overview

The **AI Knowledge Assistant** is a production-grade, full-stack **Retrieval-Augmented Generation (RAG)** platform built entirely on free, open-source technology. It enables users to upload internal documents (PDF, DOCX, TXT), which are automatically chunked, embedded via a local AI model, and stored in a vector database. Users can then query their knowledge base through a natural-language chat interface — powered by a locally-running LLM — with answers grounded in their actual documents.

The system runs **100% offline**. No data leaves the machine, no API keys are required, and there is zero ongoing cloud cost. This makes it directly applicable to regulated industries — healthcare, legal, finance, and defence — where data sovereignty is non-negotiable.

### Why It's Technically Challenging

This is not a tutorial project. It combines several distinct engineering disciplines into a single, cohesive system:

- **AI / ML pipeline** — text extraction, chunking strategy, vector embedding, cosine similarity search, and context-augmented LLM prompting (the full RAG loop)
- **Clean Architecture + CQRS** — strict layer separation with MediatR, domain events, and FluentValidation pipeline behaviours
- **Real-time streaming** — token-by-token LLM output delivered to the React frontend via Server-Sent Events
- **Multi-service infrastructure** — Docker Compose orchestrating a .NET API, React SPA, PostgreSQL, Qdrant vector DB, Ollama, and Seq log viewer
- **Security** — stateless JWT authentication, BCrypt password hashing, CORS policy, secrets via environment variables

### Business Value

RAG platforms are the **#1 enterprise AI investment in 2025–2026**. This project mirrors what companies like Microsoft, Google, and hundreds of enterprises are building internally — allowing employees to query proprietary documents without exposing data to third-party AI APIs.

| Pillar | Detail |
|---|---|
| **Fully Offline** | Llama 3 / Mistral via Ollama — no OpenAI key, zero cloud cost |
| **Enterprise Patterns** | Clean Architecture, CQRS/MediatR, repository pattern, domain events |
| **Production-Ready** | Docker Compose, Serilog structured logging (Seq), JWT auth, Swagger |
| **Full-Stack** | React + TypeScript SPA with real-time streaming chat (SSE) |
| **Demonstrable AI Depth** | End-to-end RAG pipeline: chunking → embedding → vector search → generation |

---

## 2. Scope & Boundaries

**In Scope**
- Document ingestion: PDF, DOCX, TXT — chunked, embedded, stored in Qdrant
- Embedding via Ollama (`nomic-embed-text`), generation via Llama 3 / Mistral
- Conversation history, JWT auth, Swagger/OpenAPI, Docker Compose deployment

**Out of Scope**
- Multi-tenant billing, fine-tuning, Kubernetes, mobile, SSO, real-time collaboration

---

## 3. Technology Stack

| Layer | Technology | Role |
|---|---|---|
| **Backend** | ASP.NET Core 8 + C# 12 | Web API |
| | MediatR 12 + FluentValidation | CQRS pipeline |
| | EF Core 8 | ORM (PostgreSQL) |
| | Serilog + Seq | Structured logging |
| **AI / ML** | Ollama | Local LLM inference (Llama 3, Mistral) |
| | nomic-embed-text | Text embeddings (768-dim) |
| **Document Parsing** | PdfPig | PDF text extraction (free, no native deps) |
| | DocumentFormat.OpenXml | DOCX text + table extraction |
| **Databases** | PostgreSQL 17 | Relational metadata store |
| | Qdrant | Vector database |
| **Frontend** | React 18 + TypeScript + Vite | SPA |
| | Tailwind CSS, TanStack Query, Zustand, Axios | UI / state / HTTP |
| **DevOps** | Docker Compose | Multi-service orchestration |
| | GitHub Actions | CI pipeline |
| **Security** | JWT Bearer + BCrypt.Net | Auth + password hashing |
| **Testing** | xUnit + FluentAssertions + NSubstitute | Unit test suite |

---

## 4. Architecture Overview

```
┌──────────────────────────────────────────┐
│  API Layer  (ASP.NET Core · Swagger · SSE)│
├──────────────────────────────────────────┤
│  Application  (CQRS · MediatR · DTOs)    │
├──────────────────────────────────────────┤
│  Infrastructure  (EF Core · Qdrant       │
│                   Ollama · JWT)           │
├──────────────────────────────────────────┤
│  Domain  (Entities · Events · Interfaces)│
└──────────────────────────────────────────┘
```

**RAG Pipeline**

```
Upload ──► Extract text ──► Chunk (512 tokens, 50 overlap)
      ──► Embed (nomic-embed-text) ──► Store in Qdrant + PostgreSQL

Query  ──► Embed question ──► Qdrant similarity search
      ──► Augment prompt ──► Llama 3 generates answer ──► SSE stream to React
```

---

## 5. Repository Structure

```
ai-knowledge-assistant/
├── src/
│   ├── Domain/           # Entities, enums, domain events, value objects, repository interfaces
│   ├── Application/      # CQRS commands/queries, handlers, DTOs, validators, behaviours
│   │   └── Interfaces/   # IDocumentExtractor, IDocumentExtractorFactory, ITextChunkingService
│   ├── Infrastructure/   # EF Core, migrations, repositories, Ollama/Qdrant services, JWT
│   │   └── Services/     # PdfExtractor, TxtExtractor, DocxExtractor, DocumentExtractorFactory
│   │                     # TextChunkingService, JwtTokenService, BcryptPasswordHasher
│   └── API/              # Controllers, middleware, Program.cs
├── frontend/src/         # React + TypeScript (components, pages, hooks, api/)
├── tests/
│   └── AIKnowledgeAssistant.Tests/   # xUnit unit tests (93 tests, 0 failures)
│       ├── Domain/Entities/          # Document, Chunk entity tests
│       ├── Application/Validators/   # Register, Login, IngestDocument validator tests
│       ├── Application/Handlers/     # IngestDocumentCommandHandler tests
│       └── Infrastructure/Services/  # Extractor + factory + chunking tests
├── docker/               # api.Dockerfile, frontend.Dockerfile
├── docker-compose.yml    # postgres · qdrant · seq · api · frontend
└── AIKnowledgeAssistant.sln
```

---

## 6. API Endpoints

| Method | Endpoint | Description | Auth | Status |
|---|---|---|---|---|
| `POST` | `/api/auth/register` | Register new user | Public | ✅ |
| `POST` | `/api/auth/login` | Login → JWT token | Public | ✅ |
| `POST` | `/api/documents/ingest` | Upload & ingest document (PDF, DOCX, TXT) | Required | ✅ |
| `GET` | `/api/documents/search?q=...&topK=5` | Semantic similarity search over chunks | Required | ✅ |
| `GET` | `/api/documents` | List documents | Required | 🔲 |
| `GET` | `/api/documents/{id}/status` | Ingestion status | Required | 🔲 |
| `DELETE` | `/api/documents/{id}` | Delete document + vectors | Required | 🔲 |
| `POST` | `/api/conversations` | Create conversation | Required | 🔲 |
| `GET` | `/api/conversations` | List conversations | Required | 🔲 |
| `POST` | `/api/conversations/{id}/messages` | Send message (RAG response) | Required | 🔲 |
| `GET` | `/api/conversations/{id}/stream` | SSE token stream | Required | 🔲 |
| `GET` | `/api/conversations/{id}/messages` | Message history | Required | 🔲 |
| `GET` | `/api/health` | Health check | Public | 🔲 |

---

## 7. Implementation Status

| Area | Status |
|---|---|
| Domain layer (entities, events, value objects, interfaces) | ✅ Done |
| Application — Auth commands, handlers, DTOs, validators | ✅ Done |
| Application — MediatR behaviours (logging, validation) | ✅ Done |
| Application — `IDocumentExtractor`, `IDocumentExtractorFactory` interfaces | ✅ Done |
| Application — `ITextChunkingService` interface + `ChunkingOptions` / `TextChunk` records | ✅ Done |
| Application — `IngestDocumentCommandHandler` (extract → chunk → embed → store) | ✅ Done |
| Application — `SearchChunksQuery` + `SearchChunksQueryHandler` (semantic search) | ✅ Done |
| Application — `SearchChunksResponse`, `ChunkSearchResult` DTOs | ✅ Done |
| Infrastructure — `JwtTokenService`, `BcryptPasswordHasher` | ✅ Done |
| Infrastructure — `AppDbContext`, EF configurations, migration | ✅ Done |
| Infrastructure — `UserRepository`, `DocumentRepository`, `ConversationRepository` | ✅ Done |
| Infrastructure — `PdfExtractor` (PdfPig), `TxtExtractor`, `DocxExtractor` (OpenXml) | ✅ Done |
| Infrastructure — `DocumentExtractorFactory` (MIME-type resolver) | ✅ Done |
| Infrastructure — `TextChunkingService` (recursive char splitter, 512 tokens / 50 overlap) | ✅ Done |
| Infrastructure — `OllamaEmbeddingService` (batch embedding via `nomic-embed-text`) | ✅ Done |
| Infrastructure — `QdrantVectorRepository` (upsert, search, delete, collection init) | ✅ Done |
| API — `AuthController`, `ValidationExceptionMiddleware`, `Program.cs` | ✅ Done |
| API — `DocumentsController` — `POST /api/documents/ingest` | ✅ Done |
| API — `DocumentsController` — `GET /api/documents/search` | ✅ Done |
| Docker Compose (postgres, qdrant, seq, api, frontend profiles) | ✅ Done |
| REST test file (`.http/AIKnowledgeAssistant.API.http`) | ✅ Done |
| Frontend — Vite scaffold, Axios `apiClient.ts` | ✅ Done |
| Tests — xUnit project (domain, validators, handler, extractors, chunker) | ✅ Done |
| API — `ConversationsController` (create, list, messages, SSE stream) | 🔲 Pending |
| Infrastructure — `OllamaLlmService` (chat generation / streaming) | 🔲 Pending |
| Application — `SendMessageCommand` handler (RAG prompt assembly + generation) | 🔲 Pending |
| Application — `GetConversationsQuery` handler | 🔲 Pending |
| Frontend UI — Chat, DocumentList, UploadPanel, Login/Register pages, hooks | 🔲 Pending |
| CI pipeline (GitHub Actions), architecture diagrams | 🔲 Pending |
