using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Queries.Documents;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class GetDocumentsQueryHandler
    : IRequestHandler<GetDocumentsQuery, DocumentListResponse>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentListResponse> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        var dtos = documents.Select(d => new DocumentDto(
            d.Id,
            d.FileName,
            d.ContentType,
            d.FileSizeBytes,
            d.Status.ToString(),
            d.UploadedAt,
            d.ProcessedAt,
            d.Chunks.Count)).ToList();

        return new DocumentListResponse(dtos);
    }
}
