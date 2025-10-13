namespace Psychology.Domain.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }

        public bool IsDeleted { get; set; } // soft delete

        // Persian (Shamsi) text fields
        public string? CreateDate { get; set; }   // yyyy/MM/dd
        public string? CreateTime { get; set; }   // HHmm
        public string? UpdateDate { get; set; }   // yyyy/MM/dd
        public string? UpdateTime { get; set; }   // HHmm

        // UTC timestamps
        public DateTime CreateDateAndTime { get; set; }
        public DateTime UpdateDateAndTime { get; set; }
    }
}
