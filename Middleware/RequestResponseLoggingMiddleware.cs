using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserApiProject.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Log the request
                context.Request.EnableBuffering();
                var requestBody = await ReadStreamAsync(context.Request.Body);
                _logger.LogInformation($"HTTP Request: {context.Request.Method} {context.Request.Path} {requestBody}");
                context.Request.Body.Position = 0;

                // Capture and log the response
                var originalResponseBodyStream = context.Response.Body;
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation($"HTTP Response: {context.Response.StatusCode} {responseBody}");

                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context);
            }
        }

        private async Task<string> ReadStreamAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        private async Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorResponse = new
            {
                error = "Internal server error."
            };

            var errorJson = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(errorJson);
        }
    }
}