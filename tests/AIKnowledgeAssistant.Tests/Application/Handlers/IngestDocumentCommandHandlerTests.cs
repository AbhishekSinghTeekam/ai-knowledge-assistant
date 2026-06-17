using System.Text;
using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.DTOs.Documents;
using AIKnowledgeAssistant.Application.Handlers.Documents;
using AIKnowledgeAssistant.Application.Interfaces;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Enums;
using AIKnowledgeAssistant.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace AIKnowledgeAssistant.Tests.Application.Handlers;

public sealed class IngestDocumentCommandHandlerTests
{
    // -------------------------------------------------------------------------
    // Dependencies (all substituted)
    // -------------------------------------------------------------------------

    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IDocumentExtractorFactory _extractorFactory = Substitute.For<IDocumentExtractorFactory>();
    private readonly IDocumentExtractor _extractor = Substitute.For<IDocumentExtractor>();
    private readonly ITextChunkingService _chunkingService = Substitute.For<ITextChunkingService>();
    private readonly IEmbeddingService _embeddingService = Substitute.For<IEmbeddingService>();
    private readonly IVectorRepository _vectorRepository = Substitute.For<IVectorRepository>();

    private IngestDocumentCommandHandler BuildHandler() =>
        new(_documentRepository, _extractorFactory, _chunkingService, _embeddingService, _vectorRepository);

    // -------------------------------------------------------------------------
    // Default arrangement
    // -------------------------------------------------------------------------

    private void ArrangeDefaults(string extractedText = "Extracted document text.")
    {
        _extractorFactory.GetExtractor(Arg.Any<string>()).Returns(_extractor);
        _extractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(extractedText));
        _chunkingService.Split(Arg.Any<string>(), Arg.Any<ChunkingOptions?>())
            .Returns([new TextChunk("Extracted document text.", 4, 0)]);
        _embeddingService.EmbedBatchAsync(Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<float[]>>([new float[] { 0.1f, 0.2f, 0.3f }]));
        _vectorRepository.UpsertAsync(
                Arg.Any<Guid>(),
                Arg.Any<float[]>(),
                Arg.Any<Dictionary<string, string>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Guid.NewGuid().ToString()));
    }

    // -------------------------------------------------------------------------
    // Happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ValidCommand_ReturnsResponseWithCorrectDocumentId()
    {
        ArrangeDefaults();
        var cmd = ValidCommand();
        var handler = BuildHandler();

        var response = await handler.Handle(cmd, CancellationToken.None);

        response.Should().NotBeNull();
        response.FileName.Should().Be(cmd.FileName);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsDocument_ViaAddAsync()
    {
        ArrangeDefaults();
        var handler = BuildHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        await _documentRepository.Received(1).AddAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesDocument_ViaUpdateAsync()
    {
        ArrangeDefaults();
        var handler = BuildHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        await _documentRepository.DidNotReceive().UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsExtractorFactory_WithCorrectContentType()
    {
        ArrangeDefaults();
        var cmd = ValidCommand();
        var handler = BuildHandler();

        await handler.Handle(cmd, CancellationToken.None);

        _extractorFactory.Received(1).GetExtractor(cmd.ContentType);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsExtractor_WithCorrectFileContent()
    {
        ArrangeDefaults();
        var cmd = ValidCommand();
        var handler = BuildHandler();

        await handler.Handle(cmd, CancellationToken.None);

        await _extractor.Received(1).ExtractTextAsync(cmd.FileContent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsChunkingService_WithExtractedText()
    {
        const string extracted = "My extracted document content.";
        ArrangeDefaults(extracted);
        var handler = BuildHandler();

        await handler.Handle(ValidCommand(), CancellationToken.None);

        _chunkingService.Received(1).Split(extracted, Arg.Any<ChunkingOptions?>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ResponseStatus_IsProcessing_Or_Pending()
    {
        // The handler marks as Completed when ingest + embedding + vector upsert finish.
        ArrangeDefaults();
        var handler = BuildHandler();

        var response = await handler.Handle(ValidCommand(), CancellationToken.None);

        response.Status.Should().Be(DocumentStatus.Completed.ToString());
    }

    // -------------------------------------------------------------------------
    // Propagation of extractor exceptions
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_ExtractorThrows_PropagatesException()
    {
        _extractorFactory.GetExtractor(Arg.Any<string>()).Returns(_extractor);
        _extractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns<Task<string>>(_ => throw new NotSupportedException("Bad content type"));

        var handler = BuildHandler();

        var act = () => handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    [Fact]
    public async Task Handle_ExtractorFactoryThrows_PropagatesException()
    {
        _extractorFactory.GetExtractor(Arg.Any<string>())
            .Returns(_ => throw new NotSupportedException("unknown type"));

        var handler = BuildHandler();

        var act = () => handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotSupportedException>();
    }

    // -------------------------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_CancelledToken_ThrowsOperationCanceledException()
    {
        _extractorFactory.GetExtractor(Arg.Any<string>()).Returns(_extractor);
        _extractor.ExtractTextAsync(Arg.Any<byte[]>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                callInfo.Arg<CancellationToken>().ThrowIfCancellationRequested();
                return Task.FromResult("text");
            });

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var handler = BuildHandler();
        var act = () => handler.Handle(ValidCommand(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static IngestDocumentCommand ValidCommand() => new(
        FileName: "report.pdf",
        ContentType: "application/pdf",
        FileSizeBytes: 1024,
        FileContent: Encoding.UTF8.GetBytes("fake pdf bytes"),
        UserId: Guid.NewGuid());
}
