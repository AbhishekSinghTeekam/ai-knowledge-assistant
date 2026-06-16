namespace AIKnowledgeAssistant.Application.DTOs.Documents;

public sealed record ChunkSearchResult(
    Guid ChunkId,
    Guid DocumentId,
    string FileName,
    int ChunkIndex,
    string Content,
    float Score);

public sealed record SearchChunksResponse(IReadOnlyList<ChunkSearchResult> Results);
