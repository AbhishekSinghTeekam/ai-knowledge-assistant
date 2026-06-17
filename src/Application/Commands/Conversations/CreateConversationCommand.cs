using AIKnowledgeAssistant.Application.DTOs.Conversations;
using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Conversations;

public sealed record CreateConversationCommand(
    Guid UserId,
    string Title) : IRequest<CreateConversationResponse>;
