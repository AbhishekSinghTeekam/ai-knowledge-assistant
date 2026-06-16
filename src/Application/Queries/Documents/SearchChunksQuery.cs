using AIKnowledgeAssistant.Application.DTOs.Documents;
using MediatR;

namespace AIKnowledgeAssistant.Application.Queries.Documents;

public sealed record SearchChunksQuery(
    string QueryText,
    int TopK = 5) : IRequest<SearchChunksResponse>;
