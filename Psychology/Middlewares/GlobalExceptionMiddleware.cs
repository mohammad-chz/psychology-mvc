using Microsoft.AspNetCore.Mvc;
using Psychology.Application.Interfaces;
using Psychology.Domain.Entities;
using Psychology.Infrastructure.Persistence;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Psychology.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IServiceScopeFactory scopeFactory,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = Activity.Current?.Id ?? context.TraceIdentifier;

                // 1) Log to console/file sinks via ILogger
                _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

                // 2) Persist compact details into DB
                await LogToDatabaseAsync(context, ex, correlationId);

                // 3) Return ProblemDetails (JSON) or HTML
                await WriteProblemResponseAsync(context, ex, correlationId);
            }
        }

        private async Task LogToDatabaseAsync(HttpContext context, Exception ex, string correlationId)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Capture safe request info
                var request = context.Request;

                // Read headers (compact JSON)
                var headers = request.Headers
                    .Where(h => h.Key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) ||
                                h.Key.Equals("Referer", StringComparison.OrdinalIgnoreCase) ||
                                h.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kv => kv.Key, kv => (string)kv.Value);

                var headersJson = JsonSerializer.Serialize(headers);

                // Body preview (optional + safe)
                string? bodyPreview = null;
                if (request.ContentLength is > 0 && request.Body.CanSeek)
                {
                    request.EnableBuffering();
                    request.Body.Position = 0;
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    var raw = await reader.ReadToEndAsync();
                    request.Body.Position = 0;

                    // Truncate to avoid huge/PII logs
                    const int limit = 4096;
                    if (!string.IsNullOrWhiteSpace(raw))
                        bodyPreview = raw.Length <= limit ? raw : raw[..limit] + "…";
                }

                // User info (if authenticated)
                string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                string? userName = context.User.Identity?.Name;

                var log = new ErrorLog
                {
                    ExceptionType = ex.GetType().FullName ?? ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Url = $"{request.Scheme}://{request.Host}{request.Path}",
                    Method = request.Method,
                    QueryString = request.QueryString.HasValue ? request.QueryString.Value : null,
                    StatusCode = 500,
                    UserId = userId,
                    UserName = userName,
                    Ip = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = request.Headers.UserAgent.ToString(),
                    HeadersJson = headersJson,
                    BodyPreview = bodyPreview,
                    CorrelationId = correlationId,
                };

                db.ErrorLogs.Add(log);
                await db.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                // NEVER throw while handling an exception. Just log and continue.
                _logger.LogError(dbEx, "Failed to persist error log.");
            }
        }

        private async Task WriteProblemResponseAsync(HttpContext context, Exception ex, string correlationId)
        {
            // For APIs or AJAX calls we return JSON problem details.
            // For HTML pages, you can redirect to a friendly /Error page if you prefer.

            var accept = context.Request.Headers.Accept.ToString();
            bool wantsJson =
                accept.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if (wantsJson)
            {
                context.Response.ContentType = "application/problem+json";
                var problem = new
                {
                    type = "https://httpstatuses.com/500",
                    title = "An unexpected error occurred.",
                    status = 500,
                    traceId = correlationId,
                    detail = _env.IsDevelopment() ? ex.ToString() : null
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
            else
            {
                // Option A: show a minimal text
                // await context.Response.WriteAsync("خطایی رخ داده است. لطفاً بعداً دوباره تلاش کنید.");

                // Option B: redirect to a friendly MVC error page
                context.Response.Redirect($"/Error/500?traceId={Uri.EscapeDataString(correlationId)}");
            }
        }
    }
}
