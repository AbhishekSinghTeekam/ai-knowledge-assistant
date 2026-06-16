using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Queries.Documents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIKnowledgeAssistant.API.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "text/plain",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    ];

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Uploads a document (PDF, TXT, DOCX), extracts text, chunks it,
    /// generates embeddings via Ollama and stores vectors in Qdrant.
    /// </summary>
    [HttpPost("ingest")]
    [ProducesResponseType(typeof(IngestDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSizeBytes)]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Ingest(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file supplied.");

        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest($"Unsupported content type '{file.ContentType}'. Allowed: pdf, txt, docx.");

        if (file.Length > MaxFileSizeBytes)
            return BadRequest($"File exceeds the 50 MB limit.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
            return Unauthorized();

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, cancellationToken);
            fileBytes = ms.ToArray();
        }

        var command = new IngestDocumentCommand(
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length,
            FileContent: fileBytes,
            UserId: parsedUserId);

        var response = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(Ingest), new { id = response.DocumentId }, response);
    }

    /// <summary>
    /// Performs semantic search across all ingested document chunks using vector similarity.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchChunksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required.");

        if (topK is < 1 or > 20)
            return BadRequest("'topK' must be between 1 and 20.");

        var query = new SearchChunksQuery(q, topK);
        var response = await _mediator.Send(query, cancellationToken);

        return Ok(response);
    }
}
