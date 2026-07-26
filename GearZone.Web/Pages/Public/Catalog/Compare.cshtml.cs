using System.Linq;
using System.Threading.Tasks;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class CompareModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the catalog service in-process.
        private readonly IApiClient _api;

        public CompareModel(IApiClient api)
        {
            _api = api;
        }

        public ProductComparisonDto? ComparisonData { get; private set; }

        public async Task<IActionResult> OnGetAsync(int categoryId, string ids, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return RedirectToPage("/Index");
            }

            var productIds = ids.Split(',')
                                .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
                                .Where(id => id != Guid.Empty)
                                .ToList();

            if (!productIds.Any())
            {
                return RedirectToPage("/Index");
            }

            ComparisonData = await _api.GetAsync<ProductComparisonDto>(
                $"/api/products/compare?categoryId={categoryId}&ids={Uri.EscapeDataString(string.Join(",", productIds))}", ct);

            if (ComparisonData == null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}
