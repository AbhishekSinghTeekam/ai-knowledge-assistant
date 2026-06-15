using AIKnowledgeAssistant.Domain.Common;
using AIKnowledgeAssistant.Domain.Enums;
using AIKnowledgeAssistant.Domain.Events;

namespace AIKnowledgeAssistant.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public DocumentStatus Status { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public User User { get; private set; } = null!;

    private readonly List<Chunk> _chunks = new();
    public IReadOnlyCollection<Chunk> Chunks => _chunks.AsReadOnly();

    private Document() { }

    public static Document Create(string fileName, string contentType, long fileSizeBytes, Guid userId)
    {
        return new Document
        {
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            UserId = userId,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void AddChunk(Chunk chunk) => _chunks.Add(chunk);

    public void MarkAsProcessing() => Status = DocumentStatus.Processing;

    public void MarkAsCompleted()
    {
        Status = DocumentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
        AddDomainEvent(new DocumentIngested(Id, UserId));
    }

    public void MarkAsFailed() => Status = DocumentStatus.Failed;
}
