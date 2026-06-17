using AIKnowledgeAssistant.Application.Handlers.Conversations;
using AIKnowledgeAssistant.Application.Queries.Conversations;
using AIKnowledgeAssistant.Domain.Entities;
using AIKnowledgeAssistant.Domain.Interfaces;

namespace AIKnowledgeAssistant.Tests.Application.Handlers;

public sealed class GetConversationsQueryHandlerTests
{
    private readonly IConversationRepository _repository = Substitute.For<IConversationRepository>();

    private GetConversationsQueryHandler BuildHandler() => new(_repository);

    [Fact]
    public async Task Handle_ReturnsConversationsMappedCorrectly()
    {
        var userId = Guid.NewGuid();
        var conversation = Conversation.Create("Test", userId);
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Conversation>>(new[] { conversation }));

        var response = await BuildHandler().Handle(new GetConversationsQuery(userId), CancellationToken.None);

        response.Conversations.Should().HaveCount(1);
        response.Conversations[0].Title.Should().Be("Test");
    }

    [Fact]
    public async Task Handle_NoConversations_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Conversation>>(Array.Empty<Conversation>()));

        var response = await BuildHandler().Handle(new GetConversationsQuery(userId), CancellationToken.None);

        response.Conversations.Should().BeEmpty();
    }
}
