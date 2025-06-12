using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Log the request
                _logger.LogInformation(
                    "HTTP {Method} {Path} started",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                // Log the response
                stopwatch.Stop();
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                // Log the exception (the actual exception will be logged by the GlobalExceptionHandlingMiddleware)
                stopwatch.Stop();
                _logger.LogWarning(
                    "HTTP {Method} {Path} failed in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);

                throw; // Re-throw to let the GlobalExceptionHandlingMiddleware handle it
            }
        }
    }
}