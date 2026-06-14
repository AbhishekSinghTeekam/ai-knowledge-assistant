namespace AIKnowledgeAssistant.Application.DTOs.Documents;

public sealed record IngestDocumentResponse(
    Guid DocumentId,
    string FileName,
    string Status,
    DateTime UploadedAt);
