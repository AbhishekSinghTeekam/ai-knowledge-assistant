using FluentValidation;
using System.Text.Json;

namespace AIKnowledgeAssistant.API.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for {Path}: {Errors}",
                context.Request.Path,
                string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));

            await WriteJsonAsync(context, StatusCodes.Status400BadRequest, new
            {
                type = "ValidationError",
                errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Not found for {Path}: {Message}", context.Request.Path, ex.Message);

            await WriteJsonAsync(context, StatusCodes.Status404NotFound, new
            {
                type = "NotFound",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict/operational error for {Path}", context.Request.Path);

            await WriteJsonAsync(context, StatusCodes.Status409Conflict, new
            {
                type = "Conflict",
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access for {Path}", context.Request.Path);

            await WriteJsonAsync(context, StatusCodes.Status401Unauthorized, new
            {
                type = "Unauthorized",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);

            await WriteJsonAsync(context, StatusCodes.Status500InternalServerError, new
            {
                type = "ServerError",
                message = "An unexpected error occurred."
            });
        }
    }

    private static Task WriteJsonAsync(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
