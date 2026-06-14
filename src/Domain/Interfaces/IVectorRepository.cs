namespace AIKnowledgeAssistant.Domain.Interfaces;

public interface IVectorRepository
{
    Task<string> UpsertAsync(
        Guid chunkId,
        float[] embedding,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK = 5,
        CancellationToken cancellationToken = default);

    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task EnsureCollectionExistsAsync(int vectorDimensions, CancellationToken cancellationToken = default);
}
