using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Queries.Documents;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class GetDocumentStatusQueryHandler
    : IRequestHandler<GetDocumentStatusQuery, DocumentStatusResponse>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentStatusQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentStatusResponse> Handle(
        GetDocumentStatusQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(
            request.DocumentId, cancellationToken);

        if (document is null)
            throw new KeyNotFoundException($"Document '{request.DocumentId}' was not found.");

        if (document.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have access to this document.");

        return new DocumentStatusResponse(
            document.Id,
            document.FileName,
            document.Status.ToString(),
            document.UploadedAt,
            document.ProcessedAt);
    }
}
