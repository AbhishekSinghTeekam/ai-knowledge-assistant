using System.Text;
using AIKnowledgeAssistant.Infrastructure.Services;
using FluentAssertions;

namespace AIKnowledgeAssistant.Tests.Infrastructure.Services;

public sealed class TxtExtractorTests
{
    private readonly TxtExtractor _extractor = new();

    // -------------------------------------------------------------------------
    // SupportedContentTypes
    // -------------------------------------------------------------------------

    [Fact]
    public void SupportedContentTypes_ContainsTextPlain()
    {
        _extractor.SupportedContentTypes.Should().Contain("text/plain");
    }

    // -------------------------------------------------------------------------
    // ExtractTextAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExtractTextAsync_ValidUtf8_ReturnsDecodedString()
    {
        var expected = "Hello, World! This is a test document.";
        var bytes = Encoding.UTF8.GetBytes(expected);

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task ExtractTextAsync_EmptyBytes_ReturnsEmptyString()
    {
        var result = await _extractor.ExtractTextAsync([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExtractTextAsync_MultilineText_PreservesNewlines()
    {
        var text = "Line one\nLine two\nLine three";
        var bytes = Encoding.UTF8.GetBytes(text);

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Be(text);
    }

    [Fact]
    public async Task ExtractTextAsync_UnicodeCharacters_ArePreserved()
    {
        var text = "Ärger über Ångström: 日本語テスト";
        var bytes = Encoding.UTF8.GetBytes(text);

        var result = await _extractor.ExtractTextAsync(bytes);

        result.Should().Be(text);
    }

    [Fact]
    public async Task ExtractTextAsync_RespectsCancel_OnAlreadyCancelledToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // TxtExtractor is synchronous internally — it should not observe the token
        // but must at least not throw for this trivial case.
        var bytes = Encoding.UTF8.GetBytes("some text");
        var result = await _extractor.ExtractTextAsync(bytes, cts.Token);

        result.Should().Be("some text");
    }
}
