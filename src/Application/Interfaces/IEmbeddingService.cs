namespace AIKnowledgeAssistant.Application.Interfaces;

/// <summary>
/// Generates dense vector embeddings for text using a configured embedding model.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Generates an embedding vector for a single text.</summary>
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embedding vectors for a batch of texts.
    /// Results are returned in the same order as <paramref name="texts"/>.
    /// </summary>
    Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}
