using AIKnowledgeAssistant.Application.Handlers.Conversations;
using AIKnowledgeAssistant.Application.Queries.Conversations;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;

namespace AIKnowledgeAssistant.Tests.Application.Handlers;

public sealed class GetConversationMessagesQueryHandlerTests
{
    private readonly IConversationRepository _repository = Substitute.For<IConversationRepository>();

    private GetConversationMessagesQueryHandler BuildHandler() => new(_repository);

    [Fact]
    public async Task Handle_ValidRequest_ReturnsMappedMessages()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.Create("Chat", userId);
        _repository.GetByIdWithMessagesAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Conversation?>(conversation));

        var response = await BuildHandler().Handle(
            new GetConversationMessagesQuery(conversation.Id, userId),
            CancellationToken.None);

        response.ConversationId.Should().Be(conversation.Id);
        response.Title.Should().Be("Chat");
        response.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ConversationNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();
        _repository.GetByIdWithMessagesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Conversation?>(null));

        var act = () => BuildHandler().Handle(
            new GetConversationMessagesQuery(Guid.NewGuid(), userId),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsUnauthorizedAccessException()
    {
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var conversation = Conversation.Create("Chat", ownerId);
        _repository.GetByIdWithMessagesAsync(conversation.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Conversation?>(conversation));

        var act = () => BuildHandler().Handle(
            new GetConversationMessagesQuery(conversation.Id, requestingUserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
