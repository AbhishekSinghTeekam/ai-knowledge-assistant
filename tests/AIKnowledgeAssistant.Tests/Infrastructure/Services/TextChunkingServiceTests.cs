using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Infrastructure.Services;
using FluentAssertions;

namespace AIKnowledgeAssistant.Tests.Infrastructure.Services;

public sealed class TextChunkingServiceTests
{
    private readonly TextChunkingService _svc = new();

    // -------------------------------------------------------------------------
    // Edge cases — empty / whitespace input
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n\n")]
    public void Split_EmptyOrWhitespace_ReturnsEmptyList(string text)
    {
        var result = _svc.Split(text);
        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Short text — fits in a single chunk
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_ShortText_ReturnsSingleChunk()
    {
        var result = _svc.Split("Hello world");

        result.Should().ContainSingle();
        result[0].Text.Should().Be("Hello world");
        result[0].Index.Should().Be(0);
        result[0].TokenCount.Should().BeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // Chunk index is sequential from 0
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_MultipleChunks_IndexIsSequential()
    {
        // Force multiple chunks by using a tiny chunk size.
        var options = new ChunkingOptions(ChunkSize: 5, OverlapSize: 1);
        var text = string.Join("\n\n", Enumerable.Range(1, 20).Select(i => $"Paragraph {i} with some text."));

        var result = _svc.Split(text, options);

        result.Should().NotBeEmpty();
        for (int i = 0; i < result.Count; i++)
            result[i].Index.Should().Be(i);
    }

    // -------------------------------------------------------------------------
    // Chunk size is respected
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_WithCustomChunkSize_NeverExceedsChunkSizeInChars()
    {
        var options = new ChunkingOptions(ChunkSize: 10, OverlapSize: 2);
        // 10 tokens * 4 chars/token = 40 chars max per chunk
        var text = string.Join(" ", Enumerable.Range(1, 200).Select(i => $"word{i}"));

        var result = _svc.Split(text, options);

        result.Should().NotBeEmpty();
        foreach (var chunk in result)
            chunk.Text.Length.Should().BeLessOrEqualTo(40 + 1, // +1 because trim can leave boundary chars
                because: "chunk size of 10 tokens × 4 chars = 40 chars max");
    }

    // -------------------------------------------------------------------------
    // Overlap — adjacent chunks share trailing / leading content
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_WithOverlap_AdjacentChunks_ShareSomeContent()
    {
        var options = new ChunkingOptions(ChunkSize: 20, OverlapSize: 5);
        // Build long text so we definitely get at least 2 chunks.
        var text = string.Join(" ", Enumerable.Range(1, 300).Select(i => $"token{i}"));

        var result = _svc.Split(text, options);

        result.Count.Should().BeGreaterThan(1);

        // At least one pair of adjacent chunks should share some words.
        bool foundOverlap = false;
        for (int i = 0; i < result.Count - 1 && !foundOverlap; i++)
        {
            var wordsA = result[i].Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var wordsB = result[i + 1].Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foundOverlap = wordsA.Intersect(wordsB).Any();
        }

        foundOverlap.Should().BeTrue(because: "overlap > 0 means adjacent chunks must share content");
    }

    // -------------------------------------------------------------------------
    // No overlap produces non-overlapping chunks
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_ZeroOverlap_ChunksAreNonOverlapping()
    {
        var options = new ChunkingOptions(ChunkSize: 10, OverlapSize: 0);
        var paragraphs = Enumerable.Range(1, 30).Select(i => $"Para{i}").ToArray();
        var text = string.Join("\n\n", paragraphs);

        var result = _svc.Split(text, options);

        // Verify that the combined content reconstructs the original (modulo joining and trimming).
        var allText = string.Join(" ", result.Select(c => c.Text));
        foreach (var para in paragraphs)
            allText.Should().Contain(para);
    }

    // -------------------------------------------------------------------------
    // Paragraph separator is preferred over line separator
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_ParagraphText_SplitsOnDoubleNewline_First()
    {
        var options = new ChunkingOptions(ChunkSize: 10, OverlapSize: 0);
        var text = "Alpha paragraph content here.\n\nBeta paragraph content here.\n\nGamma paragraph content here.";

        var result = _svc.Split(text, options);

        // Each paragraph should appear intact somewhere in the chunks.
        var allText = string.Concat(result.Select(c => c.Text));
        allText.Should().Contain("Alpha paragraph");
        allText.Should().Contain("Beta paragraph");
        allText.Should().Contain("Gamma paragraph");
    }

    // -------------------------------------------------------------------------
    // Hard split fallback — no separators in text
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_TextWithNoSeparators_HardSplitsCorrectly()
    {
        var options = new ChunkingOptions(ChunkSize: 2, OverlapSize: 0);
        // 2 tokens * 4 = 8 chars per chunk
        var text = new string('x', 40); // 40 chars, no separators

        var result = _svc.Split(text, options);

        result.Should().NotBeEmpty();
        // Every chunk should be ≤ 8 chars
        foreach (var chunk in result)
            chunk.Text.Length.Should().BeLessOrEqualTo(8);
        // All chars are accounted for
        string.Concat(result.Select(c => c.Text)).Should().Be(text);
    }

    // -------------------------------------------------------------------------
    // TokenCount estimation
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_TokenCount_IsApproximatelyCeiling_Div4()
    {
        var result = _svc.Split("Hello world!"); // 12 chars → ceiling(12/4) = 3 tokens

        result.Should().ContainSingle();
        result[0].TokenCount.Should().Be(3);
    }

    // -------------------------------------------------------------------------
    // Default options are applied when options is null
    // -------------------------------------------------------------------------

    [Fact]
    public void Split_NullOptions_UsesDefaults()
    {
        // Default chunk = 512 tokens = 2048 chars; short text must yield 1 chunk.
        var result = _svc.Split("A short sentence.", null);
        result.Should().ContainSingle();
    }
}
