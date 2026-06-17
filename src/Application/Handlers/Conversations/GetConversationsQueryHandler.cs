using AIKnowledgeAssistant.Application.DTOs.Conversations;
using AIKnowledgeAssistant.Application.Queries.Conversations;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Conversations;

public sealed class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, ConversationListResponse>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationsQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<ConversationListResponse> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await _conversationRepository.GetByUserIdAsync(
            request.UserId, cancellationToken);

        var dtos = conversations.Select(c => new ConversationDto(
            c.Id,
            c.Title,
            c.CreatedAt,
            c.UpdatedAt,
            c.Messages.Count)).ToList();

        return new ConversationListResponse(dtos);
    }
}
