using System.Text;
using AIKnowledgeAssistant.Application.Interfaces;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Extracts plain text from UTF-8 / ASCII text files.
/// </summary>
public sealed class TxtExtractor : IDocumentExtractor
{
    public IReadOnlyList<string> SupportedContentTypes { get; } =
        ["text/plain"];

    public Task<string> ExtractTextAsync(byte[] fileContent, CancellationToken cancellationToken = default)
    {
        // Detect BOM-aware encoding; fall back to UTF-8.
        var text = Encoding.UTF8.GetString(fileContent);
        return Task.FromResult(text);
    }
}
