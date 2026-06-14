using System.Text;
using AIKnowledgeAssistant.Application.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AIKnowledgeAssistant.Infrastructure.Services;

/// <summary>
/// Extracts plain text from DOCX files using DocumentFormat.OpenXml.
/// Paragraphs are separated by newlines; tables are walked cell-by-cell.
/// </summary>
public sealed class DocxExtractor : IDocumentExtractor
{
    public IReadOnlyList<string> SupportedContentTypes { get; } =
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"];

    public Task<string> ExtractTextAsync(byte[] fileContent, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(fileContent, writable: false);
        using var wordDoc = WordprocessingDocument.Open(stream, isEditable: false);

        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body is null)
            return Task.FromResult(string.Empty);

        var sb = new StringBuilder();

        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (element is Paragraph paragraph)
            {
                sb.AppendLine(paragraph.InnerText);
            }
            else if (element is Table table)
            {
                foreach (var row in table.Elements<TableRow>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var cells = row.Elements<TableCell>().Select(c => c.InnerText);
                    sb.AppendLine(string.Join('\t', cells));
                }
            }
        }

        return Task.FromResult(sb.ToString());
    }
}
