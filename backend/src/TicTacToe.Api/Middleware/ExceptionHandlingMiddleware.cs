using System.Net;
using System.Text.Json;
using TicTacToe.Domain.Exceptions;

namespace TicTacToe.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            GameNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            InvalidMoveException => (HttpStatusCode.BadRequest, exception.Message),
            InvalidGameOperationException => (HttpStatusCode.Conflict, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Only truly unexpected failures are logged as errors and hide their
            // detail from the client - the domain exceptions above are expected,
            // everyday outcomes (an invalid move, a stale id) and are safe to
            // return to the caller as-is.
            _logger.LogError(exception, "Unhandled exception while processing {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = JsonSerializer.Serialize(new { status = (int)statusCode, title });
        await context.Response.WriteAsync(body);
    }
}
