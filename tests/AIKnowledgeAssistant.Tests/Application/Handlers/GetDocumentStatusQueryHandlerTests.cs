using AIKnowledgeAssistant.Application.Handlers.Documents;
using AIKnowledgeAssistant.Application.Queries.Documents;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;

namespace AIKnowledgeAssistant.Tests.Application.Handlers;

public sealed class GetDocumentStatusQueryHandlerTests
{
    private readonly IDocumentRepository _documentRepository = Substitute.For<IDocumentRepository>();

    private GetDocumentStatusQueryHandler BuildHandler() => new(_documentRepository);

    private static Document CreateDocument(Guid userId)
        => Document.Create("report.pdf", "application/pdf", 2048, userId);

    [Fact]
    public async Task Handle_ValidRequest_ReturnsMappedStatus()
    {
        var userId = Guid.NewGuid();
        var document = CreateDocument(userId);
        _documentRepository.GetByIdAsync(document.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(document));

        var response = await BuildHandler().Handle(
            new GetDocumentStatusQuery(document.Id, userId),
            CancellationToken.None);

        response.Id.Should().Be(document.Id);
        response.FileName.Should().Be("report.pdf");
        response.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsKeyNotFoundException()
    {
        _documentRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Document?>(null));

        var act = () => BuildHandler().Handle(
            new GetDocumentStatusQuery(Guid.NewGuid(), Guid.NewGuid()),
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
            new GetDocumentStatusQuery(document.Id, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
