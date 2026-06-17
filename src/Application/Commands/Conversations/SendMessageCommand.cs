using AIKnowledgeAssistant.Application.DTOs.Conversations;
using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Conversations;

public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid UserId,
    string Question,
    int TopK = 5) : IRequest<SendMessageResponse>;
