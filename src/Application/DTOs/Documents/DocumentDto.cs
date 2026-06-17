namespace AIKnowledgeAssistant.Application.DTOs.Documents;

public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string Status,
    DateTime UploadedAt,
    DateTime? ProcessedAt,
    int ChunkCount);

public sealed record DocumentListResponse(IReadOnlyList<DocumentDto> Documents);

public sealed record DocumentStatusResponse(
    Guid Id,
    string FileName,
    string Status,
    DateTime UploadedAt,
    DateTime? ProcessedAt);
