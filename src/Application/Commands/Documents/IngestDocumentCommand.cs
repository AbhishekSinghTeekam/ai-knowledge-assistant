using AIKnowledgeAssistant.Application.DTOs.Documents;
using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Documents;

public sealed record IngestDocumentCommand(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    byte[] FileContent,
    Guid UserId) : IRequest<IngestDocumentResponse>;
