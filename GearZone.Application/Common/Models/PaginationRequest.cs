namespace GearZone.Application.Common.Models
{
    public class PaginationRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Column name to sort by (e.g. "stock", "price", "createdAt").
        /// Null or empty means use the default ordering of each feature.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction: "asc", "desc", or null/empty (reset to default).
        /// </summary>
        public string? SortDirection { get; set; }
    }
}
