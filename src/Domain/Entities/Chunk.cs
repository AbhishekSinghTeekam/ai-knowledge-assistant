using AIKnowledgeAssistant.Domain.Common;
using AIKnowledgeAssistant.Domain.Events;

namespace AIKnowledgeAssistant.Domain.Entities;

public class Chunk : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public int ChunkIndex { get; private set; }
    public int TokenCount { get; private set; }
    public string VectorId { get; private set; } = string.Empty;

    public Document Document { get; private set; } = null!;

    private Chunk() { }

    public static Chunk Create(Guid documentId, string content, int chunkIndex, int tokenCount)
    {
        return new Chunk
        {
            DocumentId = documentId,
            Content = content,
            ChunkIndex = chunkIndex,
            TokenCount = tokenCount
        };
    }

    public void SetVectorId(string vectorId)
    {
        VectorId = vectorId;
        AddDomainEvent(new ChunkEmbedded(Id, DocumentId, vectorId));
    }
}
