using AIKnowledgeAssistant.Domain.Entities;

namespace AIKnowledgeAssistant.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    DateTime GetExpiry();
}
