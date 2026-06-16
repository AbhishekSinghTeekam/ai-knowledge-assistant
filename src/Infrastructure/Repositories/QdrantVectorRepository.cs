using AIKnowledgeAssistant.Domain.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AIKnowledgeAssistant.Infrastructure.Repositories;

/// <summary>
/// Implements <see cref="IVectorRepository"/> using the Qdrant vector database
/// via the official Qdrant .NET gRPC client.
/// Vectors are compared with cosine similarity.
/// Each point payload stores the chunk text and all caller-supplied metadata.
/// </summary>
public sealed class QdrantVectorRepository : IVectorRepository
{
    private readonly QdrantClient _client;
    private readonly QdrantOptions _options;
    private readonly ILogger<QdrantVectorRepository> _logger;

    private const string ChunkIdField = "chunk_id";
    private const string DocumentIdField = "document_id";
    private const string ContentField = "content";

    public QdrantVectorRepository(
        QdrantClient client,
        IOptions<QdrantOptions> options,
        ILogger<QdrantVectorRepository> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> UpsertAsync(
        Guid chunkId,
        float[] embedding,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        var pointId = chunkId.ToString();

        var point = new PointStruct
        {
            Id = new PointId { Uuid = pointId },
            Vectors = embedding,  // implicit float[] → Vectors conversion
        };

        // Always persist chunk_id in payload for reliable round-trip retrieval.
        point.Payload[ChunkIdField] = new Value { StringValue = chunkId.ToString() };

        foreach (var (key, value) in metadata)
        {
            point.Payload[key] = new Value { StringValue = value };
        }

        await _client.UpsertAsync(
            _options.CollectionName,
            [point],
            cancellationToken: cancellationToken);

        _logger.LogDebug("Upserted vector for chunk {ChunkId}", chunkId);

        return pointId;
    }

    /// <inheritdoc/>
    /// <remarks>Results are ordered by descending cosine similarity score.</remarks>
    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var results = await _client.SearchAsync(
            _options.CollectionName,
            queryEmbedding,
            limit: (ulong)topK,
            payloadSelector: true,
            cancellationToken: cancellationToken);

        return results.Select(MapToSearchResult).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = DocumentIdField,
                Match = new Match { Keyword = documentId.ToString() },
            },
        });

        await _client.DeleteAsync(
            _options.CollectionName,
            filter,
            cancellationToken: cancellationToken);

        _logger.LogDebug("Deleted vectors for document {DocumentId}", documentId);
    }

    /// <inheritdoc/>
    public async Task EnsureCollectionExistsAsync(
        int vectorDimensions,
        CancellationToken cancellationToken = default)
    {
        var exists = await _client.CollectionExistsAsync(
            _options.CollectionName,
            cancellationToken);

        if (exists)
            return;

        await _client.CreateCollectionAsync(
            _options.CollectionName,
            new VectorParams
            {
                Size = (ulong)vectorDimensions,
                Distance = Distance.Cosine,
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Created Qdrant collection '{Collection}' with {Dimensions} dimensions (cosine similarity)",
            _options.CollectionName,
            vectorDimensions);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static VectorSearchResult MapToSearchResult(ScoredPoint point)
    {
        var vectorId = point.Id.HasUuid ? point.Id.Uuid : point.Id.Num.ToString();

        var chunkId = point.Payload.TryGetValue(ChunkIdField, out var chunkVal)
            ? Guid.Parse(chunkVal.StringValue)
            : Guid.Empty;

        var documentId = point.Payload.TryGetValue(DocumentIdField, out var docVal)
            ? Guid.Parse(docVal.StringValue)
            : Guid.Empty;

        var content = point.Payload.TryGetValue(ContentField, out var contentVal)
            ? contentVal.StringValue
            : string.Empty;

        var metadata = point.Payload
            .Where(kvp => kvp.Key != ChunkIdField)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.StringValue);

        return new VectorSearchResult(vectorId, chunkId, documentId, point.Score, content, metadata);
    }
}
