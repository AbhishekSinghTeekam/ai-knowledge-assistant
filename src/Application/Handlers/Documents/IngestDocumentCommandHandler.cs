using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class IngestDocumentCommandHandler : IRequestHandler<IngestDocumentCommand, IngestDocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentExtractorFactory _extractorFactory;
    private readonly ITextChunkingService _chunkingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;

    public IngestDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentExtractorFactory extractorFactory,
        ITextChunkingService chunkingService,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository)
    {
        _documentRepository = documentRepository;
        _extractorFactory = extractorFactory;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
    }

    public async Task<IngestDocumentResponse> Handle(IngestDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = Document.Create(
            request.FileName,
            request.ContentType,
            request.FileSizeBytes,
            request.UserId);

        await _documentRepository.AddAsync(document, cancellationToken);

        document.MarkAsProcessing();

        var extractor = _extractorFactory.GetExtractor(request.ContentType);
        var rawText = await extractor.ExtractTextAsync(request.FileContent, cancellationToken);

        var textChunks = _chunkingService.Split(rawText);

        // Generate embeddings for all chunks in a single batch call
        var texts = textChunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingService.EmbedBatchAsync(texts, cancellationToken);

        await _vectorRepository.EnsureCollectionExistsAsync(
            vectorDimensions: embeddings[0].Length,
            cancellationToken: cancellationToken);

        for (int i = 0; i < textChunks.Count; i++)
        {
            var chunk = Chunk.Create(
                document.Id,
                textChunks[i].Text,
                textChunks[i].Index,
                textChunks[i].TokenCount);

            var vectorId = await _vectorRepository.UpsertAsync(
                chunkId: chunk.Id,
                embedding: embeddings[i],
                metadata: new Dictionary<string, string>
                {
                    ["documentId"] = document.Id.ToString(),
                    ["fileName"] = document.FileName,
                    ["chunkIndex"] = chunk.ChunkIndex.ToString()
                },
                cancellationToken: cancellationToken);

            chunk.SetVectorId(vectorId);
            document.AddChunk(chunk);
        }

        document.MarkAsCompleted();
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return new IngestDocumentResponse(
            document.Id,
            document.FileName,
            document.Status.ToString(),
            document.UploadedAt);
    }
}
