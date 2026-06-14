using AIKnowledgeAssistant.Application.Interfaces;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Resolves an <see cref="IDocumentExtractor"/> by MIME content type.
/// All registered extractors are injected via DI.
/// </summary>
public sealed class DocumentExtractorFactory : IDocumentExtractorFactory
{
    private readonly IReadOnlyDictionary<string, IDocumentExtractor> _extractors;

    public DocumentExtractorFactory(IEnumerable<IDocumentExtractor> extractors)
    {
        _extractors = extractors
            .SelectMany(e => e.SupportedContentTypes.Select(ct => (ContentType: ct, Extractor: e)))
            .ToDictionary(x => x.ContentType, x => x.Extractor, StringComparer.OrdinalIgnoreCase);
    }

    public IDocumentExtractor GetExtractor(string contentType)
    {
        // Strip optional parameters such as "; charset=utf-8"
        var baseType = contentType.Split(';')[0].Trim();

        if (_extractors.TryGetValue(baseType, out var extractor))
            return extractor;

        throw new NotSupportedException(
            $"No document extractor is registered for content type '{contentType}'.");
    }
}
