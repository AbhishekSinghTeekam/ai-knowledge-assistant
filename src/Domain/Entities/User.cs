using AIKnowledgeAssistant.Domain.Common;

namespace AIKnowledgeAssistant.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; protected set; } = string.Empty;
    public string Email { get; protected set; } = string.Empty;
    public string PasswordHash { get; protected set; } = string.Empty;
    public DateTime CreatedAt { get; protected set; }

    private readonly List<Document> _documents = new();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private readonly List<Conversation> _conversations = new();
    public IReadOnlyCollection<Conversation> Conversations => _conversations.AsReadOnly();

    protected User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        return new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePasswordHash(string newHash) => PasswordHash = newHash;
}
