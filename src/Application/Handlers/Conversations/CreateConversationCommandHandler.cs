using AIKnowledgeAssistant.Application.Commands.Conversations;
using AIKnowledgeAssistant.Application.DTOs.Conversations;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;
using MediatR;

namespace AIKnowledgeAssistant.Application.Handlers.Conversations;

public sealed class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, CreateConversationResponse>
{
    private readonly IConversationRepository _conversationRepository;

    public CreateConversationCommandHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<CreateConversationResponse> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var title = string.IsNullOrWhiteSpace(request.Title)
            ? $"Conversation {DateTime.UtcNow:yyyy-MM-dd HH:mm}"
            : request.Title.Trim();

        var conversation = Conversation.Create(title, request.UserId);
        await _conversationRepository.AddAsync(conversation, cancellationToken);

        return new CreateConversationResponse(conversation.Id, conversation.Title, conversation.CreatedAt);
    }
}
