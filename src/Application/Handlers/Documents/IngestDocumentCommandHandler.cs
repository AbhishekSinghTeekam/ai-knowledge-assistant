using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class IngestDocumentCommandHandler : IRequestHandler<IngestDocumentCommand, IngestDocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;

    public IngestDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
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

        // TODO: Extract text content from FileContent based on ContentType
        // TODO: Split extracted text into chunks
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
