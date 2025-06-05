using System.Net;
using System.Text.Json;
using NotifyHub.Application.Exceptions;
using ApplicationException = System.ApplicationException;

namespace NotifyHub.Api.Infrastructure;

public class ErrorHandlerMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = error switch
            {
                ObjectNotFoundException => (int)HttpStatusCode.NotFound,
                NotValidException => (int)HttpStatusCode.BadRequest,
                ObjectAlreadyExistException => (int)HttpStatusCode.Conflict,
                CommandValidationException => (int)HttpStatusCode.BadRequest,
                ApplicationException => (int)HttpStatusCode.BadRequest,
                Domain.Exceptions.CommandValidationException => (int)HttpStatusCode.BadRequest,
                Domain.Exceptions.DomainException => (int)HttpStatusCode.BadRequest,
                TaskCanceledException => (int)HttpStatusCode.GatewayTimeout,
                not null => (int)HttpStatusCode.InternalServerError,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var result = JsonSerializer.Serialize(new { error.Message });
            if (response.StatusCode == 500)
                result = JsonSerializer.Serialize(new { error.Message, error.StackTrace });

            await response.WriteAsync(result);
        }
    }
}