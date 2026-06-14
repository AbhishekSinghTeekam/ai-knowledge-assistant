using AIKnowledgeAssistant.Infrastructure.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentAssertions;

namespace AIKnowledgeAssistant.Tests.Infrastructure.Services;

public sealed class DocxExtractorTests
{
    private readonly DocxExtractor _extractor = new();

    // -------------------------------------------------------------------------
    // SupportedContentTypes
    // -------------------------------------------------------------------------

    [Fact]
    public void SupportedContentTypes_ContainsDocxMimeType()
    {
        _extractor.SupportedContentTypes
            .Should().Contain("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
    }

    // -------------------------------------------------------------------------
    // ExtractTextAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExtractTextAsync_SingleParagraph_ReturnsParagraphText()
    {
        var bytes = BuildDocx("Hello from DOCX");

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("Hello from DOCX");
    }

    [Fact]
    public async Task ExtractTextAsync_MultipleParagraphs_ContainsAllText()
    {
        var bytes = BuildDocx("First paragraph.", "Second paragraph.", "Third paragraph.");

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("First paragraph.");
        result.Should().Contain("Second paragraph.");
        result.Should().Contain("Third paragraph.");
    }

    [Fact]
    public async Task ExtractTextAsync_SingleCellTable_ContainsCellText()
    {
        var bytes = BuildDocxWithTable(new[] { new[] { "CellA", "CellB" } });

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("CellA");
        result.Should().Contain("CellB");
    }

    [Fact]
    public async Task ExtractTextAsync_MultiRowTable_ContainsAllCells()
    {
        var bytes = BuildDocxWithTable(new[]
        {
            new[] { "Row1Col1", "Row1Col2" },
            new[] { "Row2Col1", "Row2Col2" }
        });

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("Row1Col1").And.Contain("Row2Col2");
    }

    [Fact]
    public async Task ExtractTextAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var bytes = BuildDocx(string.Join(" ", Enumerable.Repeat("word", 5000)));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _extractor.ExtractTextAsync(bytes, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // Test DOCX builders
    // -------------------------------------------------------------------------

    private static byte[] BuildDocx(params string[] paragraphTexts)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            var body = new Body();

            foreach (var text in paragraphTexts)
                body.AppendChild(new Paragraph(new Run(new Text(text))));

            mainPart.Document = new Document(body);
            mainPart.Document.Save();
        } // flush & close the zip before reading bytes

        return stream.ToArray();
    }

    private static byte[] BuildDocxWithTable(string[][] rows)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            var body = new Body();
            var table = new Table();

            foreach (var row in rows)
            {
                var tableRow = new TableRow();
                foreach (var cellText in row)
                    tableRow.AppendChild(new TableCell(new Paragraph(new Run(new Text(cellText)))));
                table.AppendChild(tableRow);
            }

            body.AppendChild(table);
            mainPart.Document = new Document(body);
            mainPart.Document.Save();
        } // flush & close the zip before reading bytes

        return stream.ToArray();
    }
}
