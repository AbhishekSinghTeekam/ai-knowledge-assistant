using AIKnowledgeAssistant.Application.DTOs.Auth;
using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
