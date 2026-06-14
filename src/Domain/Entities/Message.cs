using AIKnowledgeAssistant.Domain.Common;
using AIKnowledgeAssistant.Domain.Enums;

namespace AIKnowledgeAssistant.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public Conversation Conversation { get; private set; } = null!;

    private Message() { }

    public static Message Create(Guid conversationId, MessageRole role, string content)
    {
        return new Message
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
    }
}
