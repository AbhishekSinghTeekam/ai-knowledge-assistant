using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Application.Queries.Documents;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class SearchChunksQueryHandler : IRequestHandler<SearchChunksQuery, SearchChunksResponse>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;

    public SearchChunksQueryHandler(
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository)
    {
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
    }

    public async Task<SearchChunksResponse> Handle(SearchChunksQuery request, CancellationToken cancellationToken)
    {
        var queryEmbedding = await _embeddingService.EmbedAsync(request.QueryText, cancellationToken);

        var vectorResults = await _vectorRepository.SearchAsync(
            queryEmbedding,
            topK: request.TopK,
            cancellationToken: cancellationToken);

        var results = vectorResults.Select(r =>
        {
            r.Metadata.TryGetValue("fileName", out var fileName);
            r.Metadata.TryGetValue("chunkIndex", out var chunkIndexStr);
            int.TryParse(chunkIndexStr, out var chunkIndex);

            return new ChunkSearchResult(
                ChunkId: r.ChunkId,
                DocumentId: r.DocumentId,
                FileName: fileName ?? string.Empty,
                ChunkIndex: chunkIndex,
                Content: r.Content,
                Score: r.Score);
        }).ToList();

        return new SearchChunksResponse(results);
    }
}
