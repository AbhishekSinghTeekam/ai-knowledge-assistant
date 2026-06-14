using AIKnowledgeAssistant.Domain.Common;

namespace AIKnowledgeAssistant.Domain.Events;

public sealed record ChunkEmbedded(Guid ChunkId, Guid DocumentId, string VectorId) : DomainEvent;
