namespace GearZone.Web.Pages.Shared.Models
{
    public class ReviewStarsViewModel
    {
        public decimal Rating { get; set; }

        public int MaxStars { get; set; } = 5;

        public int SizePx { get; set; } = 16;

        public string AriaLabel { get; set; } = string.Empty;
    }
}
