using System.Collections.Generic;

namespace GearZone.Web.Pages.Models
{
    public class HomeHeroSlideViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Eyebrow { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryLabel { get; set; } = string.Empty;
        public string PrimaryHref { get; set; } = string.Empty;
        public string SecondaryLabel { get; set; } = string.Empty;
        public string SecondaryHref { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageAlt { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class HomeServiceStripItemViewModel
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
        public string Tone { get; set; } = "blue";
    }
}
