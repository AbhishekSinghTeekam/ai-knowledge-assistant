using AIKnowledgeAssistant.Application.DTOs.Auth;
using MediatR;

namespace AIKnowledgeAssistant.Application.Commands.Auth;

public sealed record RegisterCommand(string Name, string Email, string Password) : IRequest<AuthResponse>;
