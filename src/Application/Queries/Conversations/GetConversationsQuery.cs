using AIKnowledgeAssistant.Application.DTOs.Conversations;
using MediatR;

namespace AIKnowledgeAssistant.Application.Queries.Conversations;

public sealed record GetConversationsQuery(Guid UserId) : IRequest<ConversationListResponse>;
