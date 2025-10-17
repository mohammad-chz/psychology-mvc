namespace Psychology.Application.Emails.PreConsultation
{
    public class PreConsultationEmailModel
    {
        public required string Email { get; init; }
        public required string PhoneNumber { get; init; }
        public required string CountryPrefix { get; init; } // e.g., +98
        public required string Date { get; init; }
        public required string Time { get; init; }
    }
}
