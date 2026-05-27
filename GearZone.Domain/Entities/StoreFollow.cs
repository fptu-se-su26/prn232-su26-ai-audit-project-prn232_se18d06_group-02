namespace GearZone.Domain.Entities
{
    public class StoreFollow : Entity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public Guid StoreId { get; set; }
        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; } = null!;
        public Store Store { get; set; } = null!;
    }
}
