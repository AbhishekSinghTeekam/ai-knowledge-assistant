namespace AIKnowledgeAssistant.Domain.Interfaces;

public sealed record VectorSearchResult(
    string VectorId,
    Guid ChunkId,
    Guid DocumentId,
    float Score,
    string Content,
    IReadOnlyDictionary<string, string> Metadata);
