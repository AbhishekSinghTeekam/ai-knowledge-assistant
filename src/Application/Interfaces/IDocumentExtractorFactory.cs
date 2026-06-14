namespace AIKnowledgeAssistant.Application.Interfaces;

/// <summary>
/// Resolves the correct <see cref="IDocumentExtractor"/> for a given MIME content type.
/// </summary>
public interface IDocumentExtractorFactory
{
    /// <summary>
    /// Returns the extractor registered for <paramref name="contentType"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown when no extractor is registered for the supplied content type.
    /// </exception>
    IDocumentExtractor GetExtractor(string contentType);
}
