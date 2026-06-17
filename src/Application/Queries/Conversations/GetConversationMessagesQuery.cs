using AIKnowledgeAssistant.Application.DTOs.Conversations;
using MediatR;

namespace AIKnowledgeAssistant.Application.Queries.Conversations;

public sealed record GetConversationMessagesQuery(
    Guid ConversationId,
    Guid UserId) : IRequest<ConversationMessagesResponse>;
