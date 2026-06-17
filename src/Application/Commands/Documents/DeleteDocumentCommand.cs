using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Documents;

public sealed record DeleteDocumentCommand(
    Guid DocumentId,
    Guid UserId) : IRequest;
