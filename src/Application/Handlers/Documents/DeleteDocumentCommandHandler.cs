using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Documents;

public sealed class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IVectorRepository _vectorRepository;

    public DeleteDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IVectorRepository vectorRepository)
    {
        _documentRepository = documentRepository;
        _vectorRepository = vectorRepository;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(
            request.DocumentId, cancellationToken);

        if (document is null)
            throw new KeyNotFoundException($"Document '{request.DocumentId}' was not found.");

        if (document.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have access to this document.");

        // Delete vectors first; if this fails the document row is still intact (safe to retry).
        await _vectorRepository.DeleteByDocumentIdAsync(request.DocumentId, cancellationToken);

        await _documentRepository.DeleteAsync(request.DocumentId, cancellationToken);
    }
}
