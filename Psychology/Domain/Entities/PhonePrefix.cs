namespace Psychology.Domain.Entities
{
    public class PhonePrefix: BaseEntity
    {
        public string CountryName { get; set; } = string.Empty; // e.g., "ایران"
        public string Prefix { get; set; } = string.Empty;      // e.g., "+98"
        public int ExpectedLength { get; set; }                 // digits count for the local part (without prefix)
        public bool IsActive { get; set; } = true;
    }
}
