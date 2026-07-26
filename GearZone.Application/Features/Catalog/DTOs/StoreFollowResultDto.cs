namespace GearZone.Application.Features.Catalog.DTOs
{
    /// <summary>Result of toggling a store follow.</summary>
    public class StoreFollowResultDto
    {
        public bool IsFollowing { get; set; }
        public int FollowerCount { get; set; }
    }
}
