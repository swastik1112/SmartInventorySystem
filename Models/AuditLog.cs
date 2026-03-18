namespace SmartInventorySystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }   // ← change from string to string? (nullable)

        public ApplicationUser? User { get; set; }   // ← navigation property (optional but useful)

        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; } = string.Empty;
    }
}
