using System.ComponentModel.DataAnnotations;

namespace Psychology.Dtos
{
    public class RegisterDto
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, MinLength(6)] public string Password { get; set; } = default!;
        [Required] public string FullName { get; set; } = default!;
        public bool IsTherapist { get; set; }
        public string? Specialty { get; set; }
    }
}
