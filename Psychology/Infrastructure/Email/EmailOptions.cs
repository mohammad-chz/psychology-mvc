namespace Psychology.Infrastructure.Email
{
    public class EmailOptions
    {
        public string FromEmail { get; set; } = default!;
        public string FromName { get; set; } = "SENS Clinic";

        public string Host { get; set; } = default!;
        public int Port { get; set; } = 587;

        // "StartTls" (default for 587), "SslOnConnect" (for 465), or "Auto"
        public string Security { get; set; } = "StartTls";

        public string? User { get; set; }
        public string? Pass { get; set; }

        public string[] AdminRecipients { get; set; } = Array.Empty<string>();

        // Only for debugging self-signed servers. Leave false in production.
        public bool SkipCertificateValidation { get; set; } = false;
    }
}
