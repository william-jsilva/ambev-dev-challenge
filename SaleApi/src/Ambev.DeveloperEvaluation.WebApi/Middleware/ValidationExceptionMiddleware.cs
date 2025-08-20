using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

/// <summary>
/// Middleware to handle validation exceptions globally.
/// </summary>
public class ValidationExceptionMiddleware
{
    private readonly static JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;

    /// <inheritdoc />
    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to handle validation exceptions.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (DomainException ex)
        {
            await HandleCustomExceptionAsync(context, ex, StatusCodes.Status422UnprocessableEntity);
        }
        catch (InvalidOperationException ex)
        {
            await HandleCustomExceptionAsync(context, ex, StatusCodes.Status422UnprocessableEntity);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleCustomExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (ArgumentException ex)
        {
            await HandleCustomExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            await HandleCustomExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Handles validation exceptions by returning a standardized JSON response.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = new ApiResponse
        {
            Success = false,
            Message = "Validation Failed",
            Errors = exception.Errors
                .Select(error => (ValidationErrorDetail)error)
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private static Task HandleCustomExceptionAsync(HttpContext context, Exception exception, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiResponse
        {
            Success = false,
            Message = exception.Message
        };

        // ToDo: log the exception details for debugging purposes

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
