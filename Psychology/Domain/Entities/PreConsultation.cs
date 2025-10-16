using System.ComponentModel.DataAnnotations;

namespace Psychology.Domain.Entities
{
    public class PreConsultation : BaseEntity
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;

        [Required, MaxLength(50)]
        public string TherapyType { get; set; } = default!; // e.g., "فردی", "زوجی", "خانوادگی"

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsDone { get; set; } = false;

        // Optional assigned therapist (if applicable)
        public string? AssignedTherapistId { get; set; }
        public ApplicationUser? AssignedTherapist { get; set; }
    }
}
