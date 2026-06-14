using AIKnowledgeAssistant.Infrastructure.Services;
using FluentAssertions;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Fonts.Standard14Fonts;

namespace AIKnowledgeAssistant.Tests.Infrastructure.Services;

public sealed class PdfExtractorTests
{
    private readonly PdfExtractor _extractor = new();

    // -------------------------------------------------------------------------
    // SupportedContentTypes
    // -------------------------------------------------------------------------

    [Fact]
    public void SupportedContentTypes_ContainsApplicationPdf()
    {
        _extractor.SupportedContentTypes.Should().Contain("application/pdf");
    }

    // -------------------------------------------------------------------------
    // ExtractTextAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExtractTextAsync_ValidPdf_WithText_ReturnsExtractedText()
    {
        var bytes = BuildPdf("Hello from PDF");

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("Hello from PDF");
    }

    [Fact]
    public async Task ExtractTextAsync_MultiPagePdf_CombinesAllPageText()
    {
        var bytes = BuildPdf("Page one content", "Page two content");

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Contain("Page one content");
        result.Should().Contain("Page two content");
    }

    [Fact]
    public async Task ExtractTextAsync_InvalidBytes_ThrowsException()
    {
        var invalidBytes = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        var act = () => _extractor.ExtractTextAsync(invalidBytes);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ExtractTextAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Build a large enough PDF to make iteration observable.
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        for (int i = 0; i < 5; i++)
        {
            var p = builder.AddPage(PageSize.A4);
            p.AddText($"Page {i}", 12, new UglyToad.PdfPig.Core.PdfPoint(10, 700), font);
        }
        var bytes = builder.Build();

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _extractor.ExtractTextAsync(bytes, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // Builder helper
    // -------------------------------------------------------------------------

    private static byte[] BuildPdf(params string[] pageTexts)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        foreach (var text in pageTexts)
        {
            var page = builder.AddPage(PageSize.A4);
            page.AddText(text, 12, new UglyToad.PdfPig.Core.PdfPoint(10, 700), font);
        }

        return builder.Build();
    }
}
