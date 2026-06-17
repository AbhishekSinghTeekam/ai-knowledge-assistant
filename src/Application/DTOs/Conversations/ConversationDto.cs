namespace AIKnowledgeAssistant.Application.DTOs.Conversations;

public sealed record ConversationDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int MessageCount);

public sealed record ConversationListResponse(IReadOnlyList<ConversationDto> Conversations);

public sealed record CreateConversationResponse(
    Guid Id,
    string Title,
    DateTime CreatedAt);
