using AIKnowledgeAssistant.Application.DTOs.Documents;
using MediatR;

namespace AIKnowledgeAssistant.Application.Queries.Documents;

public sealed record GetDocumentStatusQuery(
    Guid DocumentId,
    Guid UserId) : IRequest<DocumentStatusResponse>;
