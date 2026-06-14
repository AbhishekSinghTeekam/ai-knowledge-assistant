---
description: "Use when creating, moving, or updating any file in this project. Enforces the folder structure defined in Documentation.md for the AI Knowledge Assistant RAG platform."
applyTo: "**"
---

# AI Knowledge Assistant — Project Structure

Always place files in the correct layer according to the structure below. Never create files outside their designated folders.

## Canonical Folder Map

```
ai-knowledge-assistant/
├── src/
│   ├── Domain/                  # Entities, domain events, value objects, repository interfaces
│   ├── Application/             # CQRS commands/queries, MediatR handlers, DTOs, validators
│   ├── Infrastructure/          # EF Core, PostgreSQL repos, Qdrant adapter, Ollama adapter, JWT service
│   └── API/                     # ASP.NET Core controllers, middleware, Program.cs, Swagger config
├── frontend/
│   └── src/
│       ├── components/          # Reusable React components (Chat, DocumentList, UploadPanel, etc.)
│       ├── pages/               # Route-level pages (Home, Login, Dashboard)
│       ├── hooks/               # Custom React hooks (useChat, useDocuments, etc.)
│       └── api/                 # Axios client, API type definitions
├── docker/                      # Individual Dockerfiles (one per service)
├── docker-compose.yml           # Full stack orchestration
├── docker-compose.dev.yml       # Dev overrides with hot reload
├── .github/
│   ├── workflows/
│   │   └── ci.yml               # GitHub Actions CI pipeline
│   └── instructions/            # Copilot instruction files
├── docs/                        # Architecture diagrams, RAG pipeline diagram, demo GIF
├── .http/                       # REST Client test files (auth.http, documents.http, chat.http)
└── README.md
```

## Layer Rules

### Domain (`src/Domain/`)
- Contains: `Entities/`, `Events/`, `ValueObjects/`, `Interfaces/`
- Entities: `Document`, `Chunk`, `Conversation`, `Message`, `User`, `ApplicationUser`
- Domain Events: `DocumentIngested`, `ChunkEmbedded`
- Value Objects: `ChunkId`, `EmbeddingVector`
- Repository interfaces live here — **no implementations**
- **No dependency** on Application, Infrastructure, or API layers

### Application (`src/Application/`)
- Contains: `Commands/`, `Queries/`, `Handlers/`, `DTOs/`, `Validators/`, `Behaviours/`
- Commands: `IngestDocumentCommand`, `SendMessageCommand`
- Queries: `GetConversationsQuery`, `SearchChunksQuery`
- MediatR pipeline behaviours (validation, logging) go in `Behaviours/`
- References Domain only — **no Infrastructure or API references**

### Infrastructure (`src/Infrastructure/`)
- Contains: `Persistence/`, `Repositories/`, `Services/`, `Migrations/`
- `Persistence/`: EF Core `DbContext`, entity configurations
- `Repositories/`: `UserRepository`, `DocumentRepository`, `IVectorRepository` implementation
- `Services/`: `OllamaEmbeddingService`, `OllamaLlmService`, `JwtTokenService`
- References Application and Domain — **no API references**

### API (`src/API/`)
- Contains: `Controllers/`, `Middleware/`, `Extensions/`
- Controllers: `AuthController`, `DocumentsController`, `ConversationsController`
- `Program.cs` wires up DI, middleware, Swagger, CORS, JWT
- References all other layers

### Frontend (`frontend/src/`)
- `components/`: Small, reusable UI building blocks
- `pages/`: Top-level route components only
- `hooks/`: Custom hooks that encapsulate API calls or state logic
- `api/`: Only Axios client config and TypeScript type definitions for API contracts

## File Placement Rules

- **New C# entity** → `src/Domain/Entities/`
- **New domain event** → `src/Domain/Events/`
- **New repository interface** → `src/Domain/Interfaces/`
- **New MediatR command/query** → `src/Application/Commands/` or `src/Application/Queries/`
- **New MediatR handler** → `src/Application/Handlers/`
- **New DTO** → `src/Application/DTOs/`
- **New FluentValidation validator** → `src/Application/Validators/`
- **New EF Core migration** → `src/Infrastructure/Migrations/`
- **New repository implementation** → `src/Infrastructure/Repositories/`
- **New external service adapter** → `src/Infrastructure/Services/`
- **New API controller** → `src/API/Controllers/`
- **New middleware** → `src/API/Middleware/`
- **New React component** → `frontend/src/components/`
- **New page/route** → `frontend/src/pages/`
- **New custom hook** → `frontend/src/hooks/`
- **New API type or client config** → `frontend/src/api/`
- **New Dockerfile** → `docker/` folder (not inside layer folders)
- **New REST test file** → `.http/`
- **New architecture or diagram asset** → `docs/`
