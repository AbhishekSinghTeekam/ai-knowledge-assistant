# AI Knowledge Assistant
### RAG Platform — Enterprise Project Documentation

> **Stack:** ASP.NET Core 8 · Semantic Kernel · Ollama · Qdrant · React · PostgreSQL  
> **Patterns:** Clean Architecture · CQRS · MediatR · JWT · Docker  
> **Cost:** 100% Free & Open Source · Fully Offline

---

## Table of Contents

1. [Project Introduction & Vision](#1-project-introduction--vision)
2. [Scope & Boundaries](#2-scope--boundaries)
3. [Real-World Usage & Business Value](#3-real-world-usage--business-value)
4. [Complete Technology Stack](#4-complete-technology-stack)
5. [Architecture Overview](#5-architecture-overview)
6. [GitHub Repository Structure](#6-github-repository-structure)
7. [Key API Endpoints Reference](#7-key-api-endpoints-reference)

---

## 1. Project Introduction & Vision

### What Is This Project?

The **AI Knowledge Assistant** is a production-grade, enterprise-class **Retrieval-Augmented Generation (RAG)** platform built entirely on free and open-source tools. It allows users to upload documents (PDFs, Word files, plain text), which are automatically chunked, embedded into a vector database, and made queryable through a natural-language chat interface powered by a locally-running Large Language Model (LLM) via **Ollama** — with zero API cost and zero data leaving the machine.

Unlike cloud-based AI assistants (OpenAI, Azure OpenAI), this system runs **entirely offline**. Businesses in healthcare, legal, finance, and defence — domains with strict data privacy requirements — can use this to build internal knowledge bases without any compliance risk.

### Why This Project?

This project addresses every evaluation criterion that a Senior SDE interviewer checks:

- Clean Architecture and separation of concerns
- CQRS and event-driven design
- Containerisation and infrastructure-as-code
- AI/ML integration with real RAG pipelines
- Security with JWT authentication
- Modern, production-quality frontend

It directly maps to the **GenAI + platform engineering** wave being invested in by enterprises in 2025–2026.

### Core Value Proposition

| Pillar | Description |
|---|---|
| **Fully Offline** | Llama 3 / Mistral running via Ollama — no OpenAI key, zero cloud cost |
| **Enterprise Patterns** | Clean Architecture, CQRS/MediatR, repository pattern, domain events |
| **Production-Ready** | Docker Compose, health checks, structured logging (Serilog), JWT auth |
| **Full-Stack Showcase** | React frontend with real-time streaming chat via Server-Sent Events |
| **Demonstrable AI Knowledge** | RAG pipeline, vector embeddings, semantic search |

---

## 2. Scope & Boundaries

### In Scope

- Document ingestion: PDF, DOCX, TXT upload and processing
- Text chunking with configurable chunk size and overlap
- Embedding generation using Ollama (`nomic-embed-text` model)
- Vector storage and similarity search using Qdrant
- Contextual response generation using Llama 3 / Mistral via Ollama
- Conversation history management (per-session chat threads)
- JWT-based user authentication and authorisation
- React frontend with chat UI, document manager, and upload panel
- RESTful API with Swagger / OpenAPI documentation
- Docker Compose deployment (all services containerised)
- PostgreSQL for user/document metadata persistence
- GitHub repository with README and architecture diagrams

### Out of Scope

- Multi-tenant SaaS billing
- Fine-tuning or training custom LLM models
- Production Kubernetes deployment
- Mobile application
- Real-time multi-user collaboration
- Enterprise SSO / Active Directory integration

---

## 3. Real-World Usage & Business Value

RAG platforms are the **#1 enterprise AI use case in 2025–2026**. Every major corporation is building internal knowledge assistants to help employees query internal documentation, legal agreements, HR policies, and technical runbooks — without sending sensitive data to the cloud.

### Use Case Scenarios

**Healthcare / MedTech**  
Query MRI scanner manuals, clinical protocols, regulatory compliance documents, and FDA/CE submissions — all offline, HIPAA/GDPR compliant.

**Legal & Compliance**  
Lawyers query thousands of case files, contracts, and regulatory circulars. RAG enables sub-second semantic search across millions of documents without exposing client data to third-party APIs.

**IT Operations & DevOps**  
Chat with runbooks, incident post-mortems, architecture docs, and Confluence wikis. Reduces Mean Time to Resolve (MTTR) by giving on-call engineers instant contextual answers.

**HR & Onboarding**  
New joiners ask natural-language questions about company policies, benefits, and procedures. Reduces HR ticket volume by 40–60% in real enterprise deployments.

**Product & Engineering Knowledge Base**  
Engineers chat with API docs, Architecture Decision Records (ADRs), and sprint retrospectives. Bridges the knowledge gap between senior engineers and new hires.

---

## 4. Complete Technology Stack

### Backend

| Technology | Role |
|---|---|
| .NET 8 / ASP.NET Core 8 | Web API framework — minimal APIs + controllers |
| C# 12 | Primary programming language |
| Semantic Kernel (Microsoft) | Orchestration framework for LLM, memory, and skills |
| MediatR 12 | CQRS mediator — decouples commands/queries from handlers |
| FluentValidation | Pipeline validation behaviours |
| Serilog + Seq | Structured logging with a free local log viewer UI |
| AutoMapper | Object-to-object mapping |
| Entity Framework Core 8 | ORM for PostgreSQL (metadata / users only) |
| Dapper *(optional)* | Micro-ORM for read-side performance queries |

### AI / ML

| Technology | Role |
|---|---|
| Ollama | Local LLM inference server — runs Llama 3, Mistral, Phi-3 |
| nomic-embed-text | Free text embedding model for vector generation (768-dim) |
| Llama 3 8B / Mistral 7B | Core LLM for answer generation (runs on CPU/GPU locally) |
| Semantic Kernel Memory | Abstraction over vector DB for RAG operations |

### Databases

| Technology | Role |
|---|---|
| Qdrant | High-performance vector database — Docker image, free & open source |
| PostgreSQL 16 | User data, document metadata, conversation history |
| pgAdmin 4 | Free GUI client for PostgreSQL |

### Frontend

| Technology | Role |
|---|---|
| React 18 | SPA framework with functional components and hooks |
| TypeScript | Type-safe frontend development |
| Tailwind CSS | Utility-first CSS framework |
| React Query (TanStack) | Server state management and caching |
| Zustand | Lightweight client state management |
| React Markdown | Render LLM markdown responses in chat |
| Axios | HTTP client for API calls |

### DevOps & Tooling

| Technology | Role |
|---|---|
| Docker Desktop | Containerisation for all services |
| Docker Compose | Multi-service orchestration: API, Qdrant, Postgres, Ollama, React |
| GitHub | Version control and portfolio showcase |
| GitHub Actions (free tier) | CI pipeline — build + test on push |
| VS Code | Primary IDE |
| Swagger / Scalar | API documentation — auto-generated |

### Security

| Technology | Role |
|---|---|
| JWT Bearer Tokens | Stateless authentication |
| BCrypt.Net | Password hashing |
| CORS policy | Locked to React dev server and production origin |
| HTTPS (dev cert) | `dotnet dev-certs https --trust` |

---

## 5. Architecture Overview

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│  Presentation Layer (API)                               │
│  ASP.NET Core · Swagger · JWT Middleware · SSE          │
├─────────────────────────────────────────────────────────┤
│  Application Layer                                      │
│  CQRS Commands/Queries · MediatR Handlers               │
│  FluentValidation Behaviours · DTOs                     │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                   │
│  EF Core + PostgreSQL · Qdrant Adapter                  │
│  Ollama Adapter · Semantic Kernel · JWT Service         │
├─────────────────────────────────────────────────────────┤
│  Domain Layer                                           │
│  Entities · Domain Events · Value Objects               │
│  Repository Interfaces                                  │
└─────────────────────────────────────────────────────────┘
```

#### Domain Layer
- **Entities:** `Document`, `Chunk`, `Conversation`, `Message`, `User`, `ApplicationUser`
- **Domain Events:** `DocumentIngested`, `ChunkEmbedded`
- **Value Objects:** `ChunkId`, `EmbeddingVector`
- Repository interfaces (no implementation in this layer)

#### Application Layer
- **CQRS Commands:** `IngestDocumentCommand`, `SendMessageCommand`
- **CQRS Queries:** `GetConversationsQuery`, `SearchChunksQuery`
- MediatR handlers and pipeline behaviours (validation, logging)
- DTO/ViewModel mapping, `IUnitOfWork` interface

#### Infrastructure Layer
- EF Core + PostgreSQL (`UserRepository`, `DocumentRepository`)
- Qdrant adapter (`IVectorRepository` implementation)
- Ollama adapter (`IEmbeddingService`, `ILlmService` implementation)
- Semantic Kernel registration, JWT token service

#### Presentation Layer (API)
- ASP.NET Core Minimal APIs / Controllers
- Swagger / Scalar UI
- JWT middleware, CORS, rate limiting
- Server-Sent Events endpoint for streaming chat

---

### RAG Pipeline Flow

```
 User
  │
  ▼
[1] INGEST    ──► POST /api/documents (PDF / DOCX / TXT upload)
  │
  ▼
[2] EXTRACT   ──► Text extracted via PdfPig / DocumentFormat.OpenXml
  │
  ▼
[3] CHUNK     ──► Recursive character splitter · 512-token chunks · 50-token overlap
  │
  ▼
[4] EMBED     ──► Ollama (nomic-embed-text) → 768-dimensional vector per chunk
  │
  ▼
[5] STORE     ──► Vectors + metadata → Qdrant · Document metadata → PostgreSQL
  │
  ▼
[6] QUERY     ──► POST /api/chat/message (user question)
  │
  ▼
[7] SEARCH    ──► Question embedded → Qdrant cosine similarity → top-k chunks retrieved
  │
  ▼
[8] AUGMENT   ──► Retrieved chunks + conversation history assembled into LLM prompt
  │
  ▼
[9] GENERATE  ──► Llama 3 via Ollama streams answer → Server-Sent Events to React
  │
  ▼
[10] DISPLAY  ──► React renders markdown in real time · Stores to conversation history
```

---

## 6. GitHub Repository Structure

```
ai-knowledge-assistant/
├── src/
│   ├── Domain/                  # Entities, domain events, interfaces
│   ├── Application/             # CQRS handlers, validators, DTOs
│   ├── Infrastructure/          # EF Core, Qdrant adapter, Ollama adapter
│   └── API/                     # ASP.NET Core project, controllers, middleware
├── frontend/                    # React + TypeScript + Tailwind
│   └── src/
│       ├── components/          # Chat, DocumentList, UploadPanel
│       ├── pages/               # Home, Login, Dashboard
│       ├── hooks/               # useChat, useDocuments
│       └── api/                 # Axios client, API types
├── docker/                      # Individual Dockerfiles
├── docker-compose.yml           # Full stack orchestration
├── docker-compose.dev.yml       # Dev overrides (hot reload)
├── .github/
│   └── workflows/
│       └── ci.yml               # GitHub Actions CI pipeline
├── docs/
│   ├── architecture.png         # Architecture diagram
│   ├── rag-pipeline.png         # RAG flow diagram
│   └── demo.gif                 # Screen recording of app
├── .http/                       # REST Client test files
│   ├── auth.http
│   ├── documents.http
│   └── chat.http
└── README.md
```

> **GitHub Best Practices:** Use semantic commits (`feat:`, `fix:`, `docs:`). Add a detailed README with architecture PNG, tech badges (shields.io), GIF demo, and a "How to Run" section. Add topics: `dotnet`, `rag`, `semantic-kernel`, `ollama`, `qdrant`, `clean-architecture`.

---

## 7. Key API Endpoints Reference

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register new user (name, email, password) | Public |
| `POST` | `/api/auth/login` | Login → returns JWT access token | Public |
| `POST` | `/api/documents` | Upload document (`multipart/form-data`) | Required |
| `GET` | `/api/documents` | List all documents for current user | Required |
| `GET` | `/api/documents/{id}/status` | Get ingestion status (processing / completed / failed) | Required |
| `DELETE` | `/api/documents/{id}` | Delete document + Qdrant vectors | Required |
| `POST` | `/api/conversations` | Create new conversation thread | Required |
| `GET` | `/api/conversations` | List all conversations | Required |
| `POST` | `/api/conversations/{id}/messages` | Send a message → returns full RAG response | Required |
| `GET` | `/api/conversations/{id}/stream` | SSE stream — token-by-token LLM response | Required |
| `GET` | `/api/conversations/{id}/messages` | Get message history for a conversation | Required |
| `GET` | `/api/health` | Health check endpoint for Docker / k8s | Public |

---

## Risk Register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| Ollama too slow on CPU | High | Medium | Use Mistral 7B instead of Llama 3 8B. Enable GPU offload if NVIDIA GPU present. Reduce context window. |
| Qdrant or PostgreSQL Docker issues | Medium | High | Pin Docker image versions. Use health checks with `depends_on`. Keep `docker-compose.dev.yml` for hot-reload. |
| `nomic-embed-text` dimension mismatch | Medium | High | Verify embedding dimensions (768) match Qdrant collection config on first run. Add dimension validation in `OllamaEmbeddingService`. |
| 4-week timeline slipping | Medium | High | Weeks 1–2 are the critical path. React frontend (Week 3) can be simplified if needed. Prioritise working RAG pipeline over UI polish. |
| Large PDF causes memory spike | Low | Medium | Process documents asynchronously with `IHostedService`. Chunk files in streaming batches. Add max file size validation (50 MB). |
| JWT secret key exposure | Low | High | Store JWT secret in environment variable / `.env` file. Never hardcode. Add `.env` to `.gitignore`. |

---

*This document covers sections 1–7 of the full project plan: Introduction, Scope, Business Value, Technology Stack, Architecture, Repository Structure, and API Reference.*
