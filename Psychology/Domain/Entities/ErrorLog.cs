namespace Psychology.Domain.Entities
{
    public class ErrorLog: BaseEntity
    {
        // Exception
        public string ExceptionType { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? StackTrace { get; set; }
        public string? Source { get; set; }

        // HTTP
        public string? Url { get; set; }
        public string? Method { get; set; }
        public string? QueryString { get; set; }
        public int? StatusCode { get; set; }

        // Request context
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public string? HeadersJson { get; set; }    // store a compact JSON of headers
        public string? BodyPreview { get; set; }    // e.g., first 4KB, if safe

        // Correlation
        public string? CorrelationId { get; set; }
    }
}
