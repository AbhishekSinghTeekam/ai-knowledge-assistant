using AIKnowledgeAssistant.Domain.Common;

namespace AIKnowledgeAssistant.Domain.Events;

public sealed record DocumentIngested(Guid DocumentId, Guid UserId) : DomainEvent;
