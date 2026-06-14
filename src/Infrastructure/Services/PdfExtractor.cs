using System.Text;
using AIKnowledgeAssistant.Application.Interfaces;
using UglyToad.PdfPig;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Extracts plain text from PDF files using PdfPig.
/// </summary>
public sealed class PdfExtractor : IDocumentExtractor
{
    public IReadOnlyList<string> SupportedContentTypes { get; } =
        ["application/pdf"];

    public Task<string> ExtractTextAsync(byte[] fileContent, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        using var document = PdfDocument.Open(fileContent);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(page.Text);
        }

        return Task.FromResult(sb.ToString());
    }
}
