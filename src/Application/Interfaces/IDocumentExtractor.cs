namespace AIKnowledgeAssistant.Application.Interfaces;

/// <summary>
/// Extracts plain text from raw document bytes for a specific file format.
/// </summary>
public interface IDocumentExtractor
{
    /// <summary>MIME content types this extractor handles (e.g. "application/pdf").</summary>
    IReadOnlyList<string> SupportedContentTypes { get; }

    /// <summary>Extracts all text content from <paramref name="fileContent"/>.</summary>
    Task<string> ExtractTextAsync(byte[] fileContent, CancellationToken cancellationToken = default);
}
