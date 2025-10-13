using Microsoft.AspNetCore.Identity;
using Psychology.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Psychology.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        // Domain-specific flags
        public bool IsTherapist { get; set; }

        public Gender? Gender { get; set; }
    }
}
