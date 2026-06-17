namespace AIKnowledgeAssistant.Application.DTOs.Conversations;

public sealed record SendMessageResponse(
    Guid ConversationId,
    Guid UserMessageId,
    Guid AssistantMessageId,
    string Answer,
    int ContextChunksUsed,
    DateTime CreatedAt);
