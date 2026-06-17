using AIKnowledgeAssistant.Application.DTOs.Documents;
using MediatR;

namespace AIKnowledgeAssistant.Application.Queries.Documents;

public sealed record GetDocumentsQuery(Guid UserId) : IRequest<DocumentListResponse>;
