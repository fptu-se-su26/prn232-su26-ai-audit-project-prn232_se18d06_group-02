using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Catalog.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Public.Catalog
{
    public class CompareModel : PageModel
    {
        private readonly ICatalogService _catalogService;

        public CompareModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public ProductComparisonDto? ComparisonData { get; private set; }

        public async Task<IActionResult> OnGetAsync(int categoryId, string ids)
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

            ComparisonData = await _catalogService.GetProductComparisonAsync(categoryId, productIds);

            if (ComparisonData == null)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
}
