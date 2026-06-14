namespace AIKnowledgeAssistant.Application.DTOs.Auth;

public sealed record AuthResponse(string Token, string Email, string Name, DateTime ExpiresAt);
