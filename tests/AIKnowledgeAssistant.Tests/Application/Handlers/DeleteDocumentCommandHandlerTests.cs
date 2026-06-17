using AIKnowledgeAssistant.Application.Commands.Documents;
using AIKnowledgeAssistant.Application.Handlers.Documents;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;

namespace AIKnowledgeAssistant.Tests.Application.Handlers;

public sealed class DeleteDocumentCommandHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();
    private readonly IVectorRepository _vectorRepository = Substitute.For<IVectorRepository>();

    private DeleteDocumentCommandHandler BuildHandler() =>
        new(_documentRepository, _vectorRepository);

    private static Document CreateDocument(Guid userId)
    {
        return Document.Create("file.pdf", "application/pdf", 1024, userId);
    }

    [Fact]
    public async Task Handle_ValidCommand_DeletesVectorsThenDocument()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(document));

        await BuildHandler().Handle(new DeleteDocumentCommand(document.Id, userId), CancellationToken.None);

        await _vectorRepository.Received(1)
            .DeleteByDocumentIdAsync(document.Id, Arg.Any<CancellationToken>());
        await _documentRepository.Received(1)
            .DeleteAsync(document.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsKeyNotFoundException()
    {
        _documentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(null));

        var act = () => BuildHandler().Handle(
            new DeleteDocumentCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsUnauthorizedAccessException()
    {
        var document = CreateDocument(Guid.NewGuid());
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(document));

        var act = () => BuildHandler().Handle(
            new DeleteDocumentCommand(document.Id, Guid.NewGuid() /* different user */),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_VectorDeletionFails_DocumentIsNotDeleted()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(document));
        _vectorRepository.DeleteByDocumentIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Qdrant unavailable"));

        var act = () => BuildHandler().Handle(
            new DeleteDocumentCommand(document.Id, userId),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _documentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
