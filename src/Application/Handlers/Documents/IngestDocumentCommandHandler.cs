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

    public IngestDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentExtractorFactory extractorFactory,
        ITextChunkingService chunkingService)
    {
        _documentRepository = documentRepository;
        _extractorFactory = extractorFactory;
        _chunkingService = chunkingService;
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

        var chunks = _chunkingService.Split(rawText);

        // TODO: Generate embeddings for each chunk via IEmbeddingService
        // TODO: Persist chunks to IDocumentRepository and vectors to IVectorRepository
        // TODO: Raise DocumentIngested domain event via document.MarkAsCompleted()

        await _documentRepository.UpdateAsync(document, cancellationToken);

        return new IngestDocumentResponse(
            document.Id,
            document.FileName,
            document.Status.ToString(),
            document.UploadedAt);
    }
}
