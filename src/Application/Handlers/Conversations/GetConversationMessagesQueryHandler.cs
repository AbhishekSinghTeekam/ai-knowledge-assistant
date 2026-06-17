using AIKnowledgeAssistant.Application.DTOs.Conversations;
using AIKnowledgeAssistant.Application.Queries.Conversations;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Conversations;

public sealed class GetConversationMessagesQueryHandler
    : IRequestHandler<GetConversationMessagesQuery, ConversationMessagesResponse>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationMessagesQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationMessagesResponse> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _conversationRepository.GetByIdWithMessagesAsync(
            request.ConversationId, cancellationToken);

        if (conversation is null)
            throw new KeyNotFoundException($"Conversation '{request.ConversationId}' was not found.");

        if (conversation.UserId != request.UserId)
            throw new UnauthorizedAccessException("You do not have access to this conversation.");

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(m.Id, m.Role.ToString(), m.Content, m.CreatedAt))
            .ToList();

        return new ConversationMessagesResponse(conversation.Id, conversation.Title, messages);
    }
}
