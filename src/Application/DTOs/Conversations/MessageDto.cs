namespace AIKnowledgeAssistant.Application.DTOs.Conversations;

public sealed record MessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);

public sealed record ConversationMessagesResponse(
    Guid ConversationId,
    string Title,
    IReadOnlyList<MessageDto> Messages);
