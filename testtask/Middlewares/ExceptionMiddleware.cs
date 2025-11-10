using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Text.Json;

namespace testtask.Middlewares
{
    public class ExceptionMiddleware
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;

            switch (exception)
            {
                case ArgumentOutOfRangeException:
                case ArgumentException:
                case ValidationException:
                    code = HttpStatusCode.BadRequest;
                    break;
                case DuplicateNameException:
                    code = HttpStatusCode.Conflict;
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    break;
            }

            var response = new
            {
                error = exception.Message
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
